namespace ArchUnitNet.Common.PatternMatching;

/// <summary>
/// Combines include and exclude glob patterns.
/// Matching logic: must match include pattern AND NOT match any exclude pattern.
/// </summary>
public class GlobFilter
{
    private readonly GlobPattern? _include;
    private readonly List<GlobPattern> _excludes;

    public GlobFilter(string? include = null, string[]? exclude = null)
    {
        _include = include == null ? null : new GlobPattern(include);
        _excludes = (exclude ?? Array.Empty<string>())
            .Select(e => new GlobPattern(e))
            .ToList();
    }

    public bool Matches(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var includeMatches = _include?.Matches(path) ?? true;
        var excludeMatches = _excludes.Any(e => e.Matches(path));

        return includeMatches && !excludeMatches;
    }

    public override string ToString()
    {
        var inc = _include?.ToString() ?? "*";
        var exc = _excludes.Count > 0 ? $" (except {string.Join(", ", _excludes)})" : "";
        return $"{inc}{exc}";
    }
}
