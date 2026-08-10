using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Defines file dependency rule: which files should (not) depend on which.
/// Example: .DependOnFiles().InPath("src/Models/**")
/// </summary>
public class FileDependencyCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly string _sourcePattern;
    private readonly bool _negated;
    private PatternMatcher? _targetMatcher;

    public FileDependencyCondition(Graph graph, PatternMatcher sourceMatcher, bool negated, string sourcePattern = "")
    {
        _graph = graph;
        _sourceMatcher = sourceMatcher;
        _sourcePattern = sourcePattern;
        _negated = negated;
    }

    public FileDependencyCondition InPath(string pattern)
    {
        _targetMatcher = new PatternMatcher(pattern);
        return this;
    }

    public FileDependencyCondition InFolder(string folder)
    {
        var pattern = $"{folder}/**";
        return InPath(pattern);
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (_targetMatcher == null)
            throw new InvalidOperationException("Must call InPath() or InFolder() first");

        options ??= new CheckOptions();

        var violations = new List<Violation>();
        var projectedEdges = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Empty-test guard: fail if no files match the pattern (unless explicitly allowed)
        var matchingEdges = projectedEdges
            .Where(e => _sourceMatcher.Matches(e.Source))
            .ToList();

        if (matchingEdges.Count == 0 && !options.AllowEmptyTests)
        {
            violations.Add(new MatchingFilesViolation(
                _sourcePattern,
                $"No files matched pattern '{_sourcePattern}' - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var edge in matchingEdges)
        {
            bool targetMatches = _targetMatcher.Matches(edge.Target);

            bool violates = targetMatches;
            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? "forbidden dependency"
                    : "expected dependency not found";
                violations.Add(ViolatingFileDependency.Create(edge, reason));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
