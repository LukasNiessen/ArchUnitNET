using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Defines rules for external (NuGet) module dependencies.
/// Supports repeatable .Matching() calls for OR logic between patterns.
/// Example: .Should().DependOnExternalModules().Matching("Newtonsoft.*").Or().Matching("Json.*")
/// </summary>
public class ExternalDependencyCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly bool _negated;
    private readonly List<PatternMatcher> _targetMatchers = new();

    public ExternalDependencyCondition(Graph graph, PatternMatcher sourceMatcher, bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _sourceMatcher = sourceMatcher ?? throw new ArgumentNullException(nameof(sourceMatcher));
        _negated = negated;
    }

    /// <summary>
    /// Match external modules by name pattern.
    /// Can be called multiple times - patterns combined with OR logic.
    /// Example: .Matching("Newtonsoft.*") or .Matching("Json*")
    /// </summary>
    public ExternalDependencyCondition Matching(string modulePattern)
    {
        if (string.IsNullOrWhiteSpace(modulePattern))
            throw new ArgumentException("Module pattern cannot be null or empty", nameof(modulePattern));

        _targetMatchers.Add(new PatternMatcher(modulePattern));
        return this;
    }

    /// <summary>
    /// Match specific external module by exact name.
    /// Can be called multiple times for OR logic.
    /// Example: .Named("Newtonsoft.Json") or .Named("System.Json")
    /// </summary>
    public ExternalDependencyCondition Named(string moduleName)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
            throw new ArgumentException("Module name cannot be null or empty", nameof(moduleName));

        _targetMatchers.Add(new PatternMatcher(moduleName));
        return this;
    }

    /// <summary>
    /// Syntax sugar for chaining multiple Matching() calls with OR logic.
    /// Example: .Matching("Newtonsoft.*").Or().Matching("Json.*")
    /// </summary>
    public ExternalDependencyCondition Or()
    {
        // Returns self for method chaining
        return this;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (_targetMatchers.Count == 0)
            throw new InvalidOperationException("Must call Matching() or Named() at least once");

        options ??= new CheckOptions();
        var violations = new List<Violation>();

        // Filter for external dependencies only
        var externalEdges = _graph.Edges
            .Where(e => e.External)
            .ToList();

        // Empty-test guard: check if source files match
        var matchingSourceEdges = externalEdges
            .Where(e => _sourceMatcher.Matches(e.Source))
            .ToList();

        if (matchingSourceEdges.Count == 0 && !options.AllowEmptyTests)
        {
            violations.Add(new MatchingFilesViolation(
                "file selection",
                $"No files matched the selection pattern - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var edge in externalEdges)
        {
            bool sourceMatches = _sourceMatcher.Matches(edge.Source);
            if (!sourceMatches)
                continue;

            // Check if target matches ANY of the patterns (OR logic)
            bool targetMatches = _targetMatchers.Any(m => m.Matches(edge.Target));

            bool violates = targetMatches;
            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? "forbidden external dependency"
                    : "expected external dependency not found";
                violations.Add(ViolatingFileDependency.Create(edge, reason));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    /// <summary>
    /// Get a readable description of the matchers for error messages.
    /// </summary>
    public string GetDescription()
    {
        var patterns = _targetMatchers.Select((m, i) => $"pattern {i + 1}").ToList();
        return string.Join(" or ", patterns);
    }
}
