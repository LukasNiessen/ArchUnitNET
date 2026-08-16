using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Layers.Common;
using ArchUnitNet.Layers.Projection;

namespace ArchUnitNet.Layers.FluentApi;

/// <summary>
/// Entry point for layer-based architecture rules.
/// Example: ProjectLayers().DefinedBy("src/{Layer}/**").Where(Layer("Presentation")).MayOnlyDependOn(Layer("Business"))
/// </summary>
public class LayersConditionBuilder
{
    private readonly Graph _graph;
    private string? _layerPattern;

    public LayersConditionBuilder(Graph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    /// <summary>
    /// Define how layers are extracted from file paths.
    /// Use {Layer} placeholder to mark the layer extraction point.
    /// Example: "src/{Layer}/**" extracts "Presentation", "Business", "Data" from path structure.
    /// </summary>
    public LayersConditionBuilder DefinedBy(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _layerPattern = pattern;
        return this;
    }

    /// <summary>
    /// Specify constraints for a particular layer.
    /// Example: .Where(Layer("Presentation")) transitions to constraint builder.
    /// </summary>
    public LayersWhereClause Where(Layer layer)
    {
        if (_layerPattern == null)
            throw new InvalidOperationException("Layer pattern must be defined first via DefinedBy()");

        if (layer == null)
            throw new ArgumentNullException(nameof(layer));

        var projector = new LayerProjector(_layerPattern);
        var constraint = new LayerConstraint(_graph, projector, layer.Name);

        return new LayersWhereClause(constraint);
    }
}

/// <summary>
/// Clause for specifying what a layer may or may not depend on.
/// Example: .MayOnlyDependOn(Layer("Business"), Layer("Common"))
/// </summary>
public class LayersWhereClause
{
    private readonly LayerConstraint _constraint;

    public LayersWhereClause(LayerConstraint constraint)
    {
        _constraint = constraint ?? throw new ArgumentNullException(nameof(constraint));
    }

    /// <summary>
    /// Specify that this layer may only depend on the given layers.
    /// If no layers specified, the layer is sealed (no external dependencies allowed).
    /// Example: .MayOnlyDependOn(Layer("Business"), Layer("Common"))
    /// Example: .MayOnlyDependOn() // Sealed layer - no external dependencies
    /// </summary>
    public LayerConstraint MayOnlyDependOn(params Layer[] allowedLayers)
    {
        if (allowedLayers == null)
            throw new ArgumentNullException(nameof(allowedLayers));

        var allowedLayerNames = allowedLayers.Select(l => l.Name).ToList();
        _constraint.SetAllowedLayers(allowedLayerNames);
        return _constraint;
    }

    /// <summary>
    /// Specify that this layer may not depend on the given layers.
    /// Blocklist is evaluated before allowlist (more restrictive).
    /// Example: .MayNotDependOn(Layer("Data"), Layer("External"))
    /// </summary>
    public LayerConstraint MayNotDependOn(params Layer[] forbiddenLayers)
    {
        if (forbiddenLayers == null)
            throw new ArgumentNullException(nameof(forbiddenLayers));

        var forbiddenLayerNames = forbiddenLayers.Select(l => l.Name).ToList();
        _constraint.SetForbiddenLayers(forbiddenLayerNames);
        return _constraint;
    }
}

/// <summary>
/// Factory methods for convenient layer rule construction.
/// </summary>
public static class ProjectLayers
{
    /// <summary>
    /// Create a layer-based architecture rule builder.
    /// </summary>
    public static LayersConditionBuilder From(Graph graph)
    {
        return new LayersConditionBuilder(graph);
    }
}
