using System.Text.RegularExpressions;

namespace ArchUnitNet.Common.PatternMatching;

/// <summary>
/// Regex pattern matching for file paths.
/// Example: @"^src/.*\.cs$"
/// </summary>
public class RegexPattern
{
    private readonly Regex _regex;

    public RegexPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentNullException(nameof(pattern));

        try
        {
            _regex = new Regex(pattern, RegexOptions.IgnoreCase);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern: {pattern}", nameof(pattern), ex);
        }
    }

    public bool Matches(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return _regex.IsMatch(path);
    }

    public override string ToString() => _regex.ToString();
}
