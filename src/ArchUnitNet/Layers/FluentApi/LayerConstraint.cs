using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Layers.Assertion;
using ArchUnitNet.Layers.Common;
using ArchUnitNet.Layers.Projection;

namespace ArchUnitNet.Layers.FluentApi;

/// <summary>
/// Constraint on a single layer: what other layers it may or may not depend on.
/// Implements blocklist/allowlist evaluation with proper precedence.
/// </summary>
public class LayerConstraint : Checkable
{
    private readonly Graph _graph;
    private readonly LayerProjector _projector;
    private readonly string _layerName;
    private readonly List<string> _allowedLayers = new();
    private readonly List<string> _forbiddenLayers = new();

    public LayerConstraint(Graph graph, LayerProjector projector, string layerName)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));
        if (projector == null)
            throw new ArgumentNullException(nameof(projector));
        if (string.IsNullOrEmpty(layerName))
            throw new ArgumentException("Layer name cannot be null or empty", nameof(layerName));

        _graph = graph;
        _projector = projector;
        _layerName = layerName;
    }

    /// <summary>
    /// Set the allowlist of layers this layer may depend on.
    /// If an allowlist is specified and the dependency target is not in it, it's a violation.
    /// Empty allowlist means sealed layer (no external dependencies).
    /// </summary>
    public void SetAllowedLayers(IEnumerable<string> allowedLayers)
    {
        _allowedLayers.Clear();
        _allowedLayers.AddRange(allowedLayers ?? new List<string>());
    }

    /// <summary>
    /// Set the blocklist of layers this layer may not depend on.
    /// Blocklist is evaluated before allowlist (more restrictive).
    /// </summary>
    public void SetForbiddenLayers(IEnumerable<string> forbiddenLayers)
    {
        _forbiddenLayers.Clear();
        _forbiddenLayers.AddRange(forbiddenLayers ?? new List<string>());
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        // Project graph to layer architecture
        var layerArchitecture = _projector.Project(_graph.Edges);

        // Get dependencies from this layer
        var dependencies = layerArchitecture.GetDependenciesFrom(_layerName);

        // Empty-test guard: if no layers exist at all
        if (layerArchitecture.LayerCount == 0 && !options.AllowEmptyTests)
        {
            violations.Add(ViolatingLayerDependency.CreateEmptyTestViolation("Layer pattern"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        // If this layer doesn't exist but others do, it's not an empty-test guard
        // (it just means this specific layer has no files matching the pattern)
        if (layerArchitecture.GetLayer(_layerName) == null)
        {
            // Layer doesn't exist - no violations possible
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var dependency in dependencies)
        {
            // Intra-layer dependencies are always allowed
            if (dependency.SourceLayer == dependency.TargetLayer)
                continue;

            // Check blocklist first (most restrictive)
            if (_forbiddenLayers.Contains(dependency.TargetLayer))
            {
                violations.Add(ViolatingLayerDependency.CreateForbiddenDependency(
                    dependency.SourceLayer,
                    dependency.TargetLayer,
                    dependency.SourceFile,
                    dependency.TargetFile));
                continue;
            }

            // Check allowlist (if specified)
            if (_allowedLayers.Count > 0 && !_allowedLayers.Contains(dependency.TargetLayer))
            {
                // Special case: sealed layer (allowlist is empty or contains only the layer itself)
                if (_allowedLayers.Count == 1 && _allowedLayers[0] == _layerName)
                {
                    violations.Add(ViolatingLayerDependency.CreateSealedLayerViolation(
                        dependency.SourceLayer,
                        dependency.TargetLayer,
                        dependency.SourceFile,
                        dependency.TargetFile));
                }
                else
                {
                    violations.Add(ViolatingLayerDependency.CreateNotAllowedDependency(
                        dependency.SourceLayer,
                        dependency.TargetLayer,
                        dependency.SourceFile,
                        dependency.TargetFile,
                        _allowedLayers.ToArray()));
                }
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
