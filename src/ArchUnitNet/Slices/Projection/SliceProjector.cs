using System.Text.RegularExpressions;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Slices.Common;

namespace ArchUnitNet.Slices.Projection;

/// <summary>
/// Projects files onto logical slices based on a file path pattern.
/// Example: Pattern "src/{Slice}/**/*.cs" extracts slices like "Feature1", "Feature2" from paths.
/// </summary>
public class SliceProjector
{
    private readonly string _pattern;
    private readonly Dictionary<string, Slice> _slices = new();
    private readonly List<SliceDependency> _dependencies = new();

    /// <summary>
    /// Create a slice projector with the given pattern.
    /// Use {SliceName} placeholder to mark the slice extraction point.
    /// Example: "src/{Feature}/**" or "packages/{Package}/**/index.cs"
    /// </summary>
    public SliceProjector(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _pattern = NormalizePattern(pattern);
    }

    /// <summary>
    /// Project edges onto slices and extract slice architecture.
    /// </summary>
    public SliceArchitecture Project(IEnumerable<Edge> edges)
    {
        _slices.Clear();
        _dependencies.Clear();

        // Group files by slice
        var filesBySlice = new Dictionary<string, HashSet<string>>();
        var fileToSlice = new Dictionary<string, string>();

        foreach (var edge in edges)
        {
            var sourceSlice = ExtractSliceName(edge.Source);
            var targetSlice = ExtractSliceName(edge.Target);

            if (sourceSlice != null)
            {
                fileToSlice[edge.Source] = sourceSlice;
                if (!filesBySlice.ContainsKey(sourceSlice))
                    filesBySlice[sourceSlice] = new HashSet<string>();
                filesBySlice[sourceSlice].Add(edge.Source);
            }

            if (targetSlice != null)
            {
                fileToSlice[edge.Target] = targetSlice;
                if (!filesBySlice.ContainsKey(targetSlice))
                    filesBySlice[targetSlice] = new HashSet<string>();
                filesBySlice[targetSlice].Add(edge.Target);
            }
        }

        // Create Slice objects
        foreach (var kvp in filesBySlice)
        {
            _slices[kvp.Key] = new Slice(kvp.Key, kvp.Value.ToList().AsReadOnly());
        }

        // Extract slice dependencies from edge dependencies
        var processedDependencies = new HashSet<string>();
        foreach (var edge in edges)
        {
            if (fileToSlice.TryGetValue(edge.Source, out var sourceSlice) &&
                fileToSlice.TryGetValue(edge.Target, out var targetSlice) &&
                sourceSlice != targetSlice)
            {
                var depKey = $"{sourceSlice}->{targetSlice}";
                if (!processedDependencies.Contains(depKey))
                {
                    _dependencies.Add(new SliceDependency(sourceSlice, targetSlice, edge.Source, edge.Target));
                    processedDependencies.Add(depKey);
                }
            }
        }

        return new SliceArchitecture(_slices, _dependencies.AsReadOnly());
    }

    /// <summary>
    /// Extract the slice name from a file path based on the pattern.
    /// Returns null if the file doesn't match the pattern.
    /// </summary>
    public string? ExtractSliceName(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        // Convert pattern to regex with named capture group
        var regexPattern = ConvertPatternToRegex(_pattern);
        var match = Regex.Match(filePath, regexPattern, RegexOptions.IgnoreCase);

        if (match.Success && match.Groups.TryGetValue("slice", out var sliceGroup))
        {
            return sliceGroup.Value;
        }

        return null;
    }

    /// <summary>
    /// Get the constructed slice architecture.
    /// </summary>
    public SliceArchitecture GetArchitecture() =>
        new SliceArchitecture(_slices, _dependencies.AsReadOnly());

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

        // Replace {Slice} placeholder with named capture group
        escaped = Regex.Replace(escaped, @"\\\{[Ss]lice\\\}", "(?<slice>[^/]+)");

        // Anchor the pattern
        return "^" + escaped + "$";
    }
}
