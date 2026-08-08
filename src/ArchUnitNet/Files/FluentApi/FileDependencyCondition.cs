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
    private readonly bool _negated;
    private PatternMatcher? _targetMatcher;

    public FileDependencyCondition(Graph graph, PatternMatcher sourceMatcher, bool negated)
    {
        _graph = graph;
        _sourceMatcher = sourceMatcher;
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

        var violations = new List<Violation>();
        var projectedEdges = ProjectEdges.GroupBySourceAndTarget(_graph);

        foreach (var edge in projectedEdges)
        {
            bool sourceMatches = _sourceMatcher.Matches(edge.Source);
            bool targetMatches = _targetMatcher.Matches(edge.Target);

            if (!sourceMatches)
                continue;

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
