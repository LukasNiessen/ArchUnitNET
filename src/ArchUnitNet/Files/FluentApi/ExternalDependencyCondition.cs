using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Defines rules for external (NuGet) module dependencies.
/// Example: files should depend on Newtonsoft.Json but not on legacy packages.
/// </summary>
public class ExternalDependencyCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly bool _negated;
    private PatternMatcher? _targetMatcher;

    public ExternalDependencyCondition(Graph graph, PatternMatcher sourceMatcher, bool negated)
    {
        _graph = graph;
        _sourceMatcher = sourceMatcher;
        _negated = negated;
    }

    /// <summary>
    /// Match external modules by name pattern (e.g., "Newtonsoft.*", "Microsoft.Extensions.*")
    /// </summary>
    public ExternalDependencyCondition Matching(string modulePattern)
    {
        _targetMatcher = new PatternMatcher(modulePattern);
        return this;
    }

    /// <summary>
    /// Match specific external module by exact name.
    /// </summary>
    public ExternalDependencyCondition Named(string moduleName)
    {
        _targetMatcher = new PatternMatcher(moduleName);
        return this;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (_targetMatcher == null)
            throw new InvalidOperationException("Must call Matching() or Named() first");

        var violations = new List<Violation>();

        // Filter for external dependencies only
        var externalEdges = _graph.Edges
            .Where(e => e.External)
            .ToList();

        foreach (var edge in externalEdges)
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
                    ? "forbidden external dependency"
                    : "expected external dependency not found";
                violations.Add(ViolatingFileDependency.Create(edge, reason));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
