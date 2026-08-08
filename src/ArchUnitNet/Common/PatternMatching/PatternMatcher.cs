namespace ArchUnitNet.Common.PatternMatching;

/// <summary>
/// Flexible pattern matcher supporting both glob and regex patterns.
/// Combines pattern type with optional include/exclude filters.
/// </summary>
public class PatternMatcher
{
    private readonly GlobPattern? _globPattern;
    private readonly RegexPattern? _regexPattern;
    private readonly GlobFilter? _globFilter;

    public PatternMatcher(string pattern, bool isRegex = false, string[]? exclude = null)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentNullException(nameof(pattern));

        if (isRegex)
        {
            _regexPattern = new RegexPattern(pattern);
        }
        else
        {
            _globPattern = new GlobPattern(pattern);
            if (exclude != null && exclude.Length > 0)
            {
                _globFilter = new GlobFilter(pattern, exclude);
            }
        }
    }

    public bool Matches(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (_globFilter != null)
            return _globFilter.Matches(path);

        if (_globPattern != null)
            return _globPattern.Matches(path);

        if (_regexPattern != null)
            return _regexPattern.Matches(path);

        return false;
    }
}
