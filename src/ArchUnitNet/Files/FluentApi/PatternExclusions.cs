using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Fluent API for pattern exclusions in file rules.
/// Allows using "except" companion to specify paths that should be excluded from rules.
/// </summary>
public class PatternExclusions
{
    private readonly string _pattern;
    private readonly List<string> _exclusions = new();

    public PatternExclusions(string pattern)
    {
        _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
    }

    /// <summary>
    /// Exclude specific paths from the pattern.
    /// Can be called multiple times to add multiple exclusions.
    /// </summary>
    public PatternExclusions Except(string excludePath)
    {
        if (!string.IsNullOrEmpty(excludePath))
        {
            _exclusions.Add(excludePath);
        }
        return this;
    }

    /// <summary>
    /// Exclude multiple paths at once.
    /// </summary>
    public PatternExclusions ExceptMany(params string[] excludePaths)
    {
        foreach (var path in excludePaths ?? Array.Empty<string>())
        {
            if (!string.IsNullOrEmpty(path))
            {
                _exclusions.Add(path);
            }
        }
        return this;
    }

    /// <summary>
    /// Exclude all test files (*.Tests.cs, *Test.cs).
    /// </summary>
    public PatternExclusions ExceptTests()
    {
        _exclusions.Add("**/*.Tests.cs");
        _exclusions.Add("**/*Test.cs");
        return this;
    }

    /// <summary>
    /// Exclude all generated files (*Generated.cs, *.g.cs).
    /// </summary>
    public PatternExclusions ExceptGenerated()
    {
        _exclusions.Add("**/*Generated.cs");
        _exclusions.Add("**/*.g.cs");
        return this;
    }

    /// <summary>
    /// Exclude internal modules (*/internal/**, */Internal/**).
    /// </summary>
    public PatternExclusions ExceptInternal()
    {
        _exclusions.Add("**/internal/**");
        _exclusions.Add("**/Internal/**");
        return this;
    }

    /// <summary>
    /// Get the pattern matcher with exclusions applied.
    /// </summary>
    public PatternMatcher Build()
    {
        return new PatternMatcher(_pattern, isRegex: false, _exclusions.ToArray());
    }

    /// <summary>
    /// Get the original pattern string.
    /// </summary>
    public string GetPattern() => _pattern;

    /// <summary>
    /// Get all exclusion patterns.
    /// </summary>
    public IReadOnlyList<string> GetExclusions() => _exclusions.AsReadOnly();
}

/// <summary>
/// Extension methods for pattern exclusions.
/// </summary>
public static class PatternExclusionExtensions
{
    /// <summary>
    /// Create a pattern with exclusion support.
    /// </summary>
    public static PatternExclusions Pattern(string pattern)
    {
        return new PatternExclusions(pattern);
    }

    /// <summary>
    /// Convenience method: Create pattern with immediate except call.
    /// Example: Pattern("src/**").Except("src/internal/**")
    /// </summary>
    public static PatternExclusions PatternExcept(string pattern, string excludePath)
    {
        return new PatternExclusions(pattern).Except(excludePath);
    }

    /// <summary>
    /// Create pattern excluding common paths.
    /// </summary>
    public static PatternExclusions PatternExceptInternal(string pattern)
    {
        return new PatternExclusions(pattern).ExceptInternal();
    }

    /// <summary>
    /// Create pattern excluding test files.
    /// </summary>
    public static PatternExclusions PatternExceptTests(string pattern)
    {
        return new PatternExclusions(pattern).ExceptTests();
    }

    /// <summary>
    /// Create pattern excluding generated files.
    /// </summary>
    public static PatternExclusions PatternExceptGenerated(string pattern)
    {
        return new PatternExclusions(pattern).ExceptGenerated();
    }
}

/// <summary>
/// Common exclusion patterns for reuse.
/// </summary>
public static class ExclusionPatterns
{
    /// <summary>
    /// All test files pattern.
    /// </summary>
    public static readonly string[] TestFiles = new[] { "**/*.Tests.cs", "**/*Test.cs", "**/test/**", "**/tests/**" };

    /// <summary>
    /// All generated files pattern.
    /// </summary>
    public static readonly string[] GeneratedFiles = new[] { "**/*Generated.cs", "**/*.g.cs", "**/*.generated.cs" };

    /// <summary>
    /// All internal module patterns.
    /// </summary>
    public static readonly string[] InternalModules = new[] { "**/internal/**", "**/Internal/**", "**/*.Internal/**" };

    /// <summary>
    /// All configuration/settings files.
    /// </summary>
    public static readonly string[] ConfigFiles = new[] { "**/*Config.cs", "**/*Settings.cs", "**/*Options.cs" };

    /// <summary>
    /// All example/sample files.
    /// </summary>
    public static readonly string[] ExampleFiles = new[] { "**/*Example.cs", "**/*Sample.cs", "**/examples/**", "**/samples/**" };
}
