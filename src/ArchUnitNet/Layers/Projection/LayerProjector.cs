using System.Text.RegularExpressions;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Layers.Common;

namespace ArchUnitNet.Layers.Projection;

/// <summary>
/// Projects files onto logical layers based on a file path pattern.
/// Example: Pattern "src/{Layer}/**" extracts layers like "Presentation", "Business", "Data" from paths.
/// </summary>
public class LayerProjector
{
    private readonly string _pattern;
    private readonly Dictionary<string, Layer> _layers = new();
    private readonly List<LayerDependency> _dependencies = new();
    private readonly Dictionary<string, List<string>> _layerFiles = new();

    /// <summary>
    /// Create a layer projector with the given pattern.
    /// Use {Layer} placeholder to mark the layer extraction point.
    /// Example: "src/{Layer}/**" or "packages/{Layer}/src/**"
    /// </summary>
    public LayerProjector(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _pattern = NormalizePattern(pattern);
    }

    /// <summary>
    /// Project edges onto layers and extract layer architecture.
    /// </summary>
    public LayerArchitecture Project(IEnumerable<Edge> edges)
    {
        _layers.Clear();
        _dependencies.Clear();
        _layerFiles.Clear();

        // Group files by layer
        var filesByLayer = new Dictionary<string, HashSet<string>>();
        var fileToLayer = new Dictionary<string, string>();

        foreach (var edge in edges)
        {
            var sourceLayer = ExtractLayerName(edge.Source);
            var targetLayer = ExtractLayerName(edge.Target);

            if (sourceLayer != null)
            {
                fileToLayer[edge.Source] = sourceLayer;
                if (!filesByLayer.ContainsKey(sourceLayer))
                    filesByLayer[sourceLayer] = new HashSet<string>();
                filesByLayer[sourceLayer].Add(edge.Source);
            }

            if (targetLayer != null)
            {
                fileToLayer[edge.Target] = targetLayer;
                if (!filesByLayer.ContainsKey(targetLayer))
                    filesByLayer[targetLayer] = new HashSet<string>();
                filesByLayer[targetLayer].Add(edge.Target);
            }
        }

        // Create Layer objects and LayerFiles mapping
        foreach (var kvp in filesByLayer)
        {
            _layers[kvp.Key] = new Layer(kvp.Key);
            _layerFiles[kvp.Key] = kvp.Value.ToList();
        }

        // Extract inter-layer dependencies from edge dependencies
        var processedDependencies = new HashSet<string>();
        foreach (var edge in edges)
        {
            if (fileToLayer.TryGetValue(edge.Source, out var sourceLayer) &&
                fileToLayer.TryGetValue(edge.Target, out var targetLayer) &&
                sourceLayer != targetLayer)
            {
                var depKey = $"{sourceLayer}->{targetLayer}";
                if (!processedDependencies.Contains(depKey))
                {
                    _dependencies.Add(new LayerDependency(sourceLayer, targetLayer, edge.Source, edge.Target));
                    processedDependencies.Add(depKey);
                }
            }
        }

        // Build read-only layer files dictionary
        var readOnlyLayerFiles = new Dictionary<string, IReadOnlyList<string>>();
        foreach (var kvp in _layerFiles)
        {
            readOnlyLayerFiles[kvp.Key] = kvp.Value.AsReadOnly();
        }

        return new LayerArchitecture(_layers, _dependencies.AsReadOnly(), readOnlyLayerFiles);
    }

    /// <summary>
    /// Extract the layer name from a file path based on the pattern.
    /// Returns null if the file doesn't match the pattern.
    /// </summary>
    public string? ExtractLayerName(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        // Convert pattern to regex with named capture group
        var regexPattern = ConvertPatternToRegex(_pattern);
        var match = Regex.Match(filePath, regexPattern, RegexOptions.IgnoreCase);

        if (match.Success && match.Groups.TryGetValue("layer", out var layerGroup))
        {
            return layerGroup.Value;
        }

        return null;
    }

    /// <summary>
    /// Get the constructed layer architecture.
    /// </summary>
    public LayerArchitecture GetArchitecture() =>
        new LayerArchitecture(_layers, _dependencies.AsReadOnly(),
            _layerFiles.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.AsReadOnly() as IReadOnlyList<string>));

    private string NormalizePattern(string pattern)
    {
        // Normalize path separators to forward slashes for consistent matching
        var normalized = pattern.Replace("\\", "/");
        return normalized;
    }

    private string ConvertPatternToRegex(string pattern)
    {
        // Escape the pattern for regex use
        var escaped = Regex.Escape(pattern);

        // Replace escaped glob patterns with regex equivalents
        escaped = escaped
            .Replace(@"\*\*/", "(.*/)?") // {dir}/** -> matches any depth
            .Replace(@"\*", "[^/]*") // * -> matches any char except /
            .Replace(@"\?", ".") // ? -> matches single char
            .Replace(@"\[", "[")  // restore [ ] for char classes
            .Replace(@"\]", "]");

        // Replace {Layer} placeholder with named capture group
        escaped = Regex.Replace(escaped, @"\\\{[Ll]ayer\\\}", "(?<layer>[^/]+)");

        // Anchor the pattern
        return "^" + escaped + "$";
    }
}
