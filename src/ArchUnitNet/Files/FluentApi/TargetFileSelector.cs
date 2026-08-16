using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Builder for selecting target files in dependency rules.
/// Supports chainable selectors with AND logic for combining conditions.
/// Example: .DependOnFiles().InFolder("src/Models").And().HaveName("*.cs")
/// </summary>
public class TargetFileSelector
{
    private readonly List<PatternMatcher> _pathMatchers = new();
    private readonly List<PatternMatcher> _folderMatchers = new();
    private readonly List<PatternMatcher> _nameMatchers = new();
    private readonly List<PatternMatcher> _exceptions = new();

    /// <summary>
    /// Select files matching a path pattern.
    /// Example: .InPath("src/Models/**")
    /// Can be called multiple times - all patterns combined with OR.
    /// </summary>
    public TargetFileSelector InPath(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _pathMatchers.Add(new PatternMatcher(pattern));
        return this;
    }

    /// <summary>
    /// Select files in a specific folder.
    /// Example: .InFolder("src/Models")
    /// Can be called multiple times - all folders combined with OR.
    /// </summary>
    public TargetFileSelector InFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
            throw new ArgumentException("Folder cannot be null or empty", nameof(folder));

        var pattern = $"{folder}/**";
        _folderMatchers.Add(new PatternMatcher(pattern));
        return this;
    }

    /// <summary>
    /// Select files matching a name pattern (filename only, not full path).
    /// Example: .HaveName("*.Service.cs")
    /// Can be called multiple times - all patterns combined with OR.
    /// </summary>
    public TargetFileSelector HaveName(string namePattern)
    {
        if (string.IsNullOrWhiteSpace(namePattern))
            throw new ArgumentException("Name pattern cannot be null or empty", nameof(namePattern));

        _nameMatchers.Add(new PatternMatcher(namePattern));
        return this;
    }

    /// <summary>
    /// Exclude files matching an exception pattern.
    /// Example: .Except("**/Legacy/**")
    /// Exceptions are applied after all other selectors with OR logic.
    /// </summary>
    public TargetFileSelector Except(string exceptionPattern)
    {
        if (string.IsNullOrWhiteSpace(exceptionPattern))
            throw new ArgumentException("Exception pattern cannot be null or empty", nameof(exceptionPattern));

        _exceptions.Add(new PatternMatcher(exceptionPattern));
        return this;
    }

    /// <summary>
    /// Syntax sugar for chaining multiple selectors.
    /// Returns this for method chaining.
    /// Example: .InFolder("src/Models").And().HaveName("*.cs")
    /// </summary>
    public TargetFileSelector And()
    {
        // This is just a syntax sugar method that returns self
        // It makes the fluent API more readable
        return this;
    }

    /// <summary>
    /// Check if a file matches this selector's criteria.
    /// Combines all selectors with AND logic:
    /// - Paths combined with OR
    /// - Folders combined with OR
    /// - Names combined with OR
    /// - Exceptions combined with OR (then excluded)
    /// All groups combined with AND.
    /// </summary>
    public bool Matches(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        // Check exceptions first - if any exception matches, exclude this file
        if (_exceptions.Count > 0 && _exceptions.Any(e => e.Matches(filePath)))
            return false;

        // If no selectors defined, nothing matches (must define at least one)
        if (_pathMatchers.Count == 0 && _folderMatchers.Count == 0 && _nameMatchers.Count == 0)
            return false;

        // Path matchers - OR logic (any match = passes)
        bool pathMatches = _pathMatchers.Count == 0 || _pathMatchers.Any(p => p.Matches(filePath));
        if (!pathMatches)
            return false;

        // Folder matchers - OR logic (any match = passes)
        bool folderMatches = _folderMatchers.Count == 0 || _folderMatchers.Any(f => f.Matches(filePath));
        if (!folderMatches)
            return false;

        // Name matchers - OR logic (any match = passes)
        if (_nameMatchers.Count > 0)
        {
            var fileName = Path.GetFileName(filePath);
            bool nameMatches = _nameMatchers.Any(n => n.Matches(fileName));
            if (!nameMatches)
                return false;
        }

        // All selectors passed - file matches
        return true;
    }

    /// <summary>
    /// Check if any selectors have been defined.
    /// </summary>
    public bool HasSelectors => _pathMatchers.Count > 0 || _folderMatchers.Count > 0 || _nameMatchers.Count > 0;

    /// <summary>
    /// Get a readable description of the selector for error messages.
    /// </summary>
    public string GetDescription()
    {
        var parts = new List<string>();

        if (_pathMatchers.Count > 0)
            parts.Add($"path patterns");
        if (_folderMatchers.Count > 0)
            parts.Add($"folders");
        if (_nameMatchers.Count > 0)
            parts.Add($"names");

        var description = string.Join(" and ", parts);

        if (_exceptions.Count > 0)
            description += $" (except {_exceptions.Count} patterns)";

        return description;
    }
}
