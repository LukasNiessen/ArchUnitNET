using DotNet.Globbing;

namespace ArchUnitNet.Common.PatternMatching;

/// <summary>
/// Glob pattern matching for file paths.
/// Examples: "src/**/*.cs", "tests/*/Error.cs", "**/{internal}/**"
/// </summary>
public class GlobPattern
{
    private readonly Glob _glob;
    private readonly string _pattern;

    public GlobPattern(string pattern)
    {
        _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        _glob = Glob.Parse(_pattern);
    }

    public bool Matches(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return _glob.IsMatch(path);
    }

    public override string ToString() => _pattern;
}
