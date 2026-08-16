using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Projection.Cycles;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Cycle detection rule: files should have no circular dependencies.
/// Uses Johnson's algorithm to find all elementary cycles in the filtered graph.
/// Reports violations for each cycle found as a readable path.
///
/// Note: Positive mood only - only .Should().HaveNoCycles() is supported.
/// Cycles violate the rule; no cycles = rule passes.
/// </summary>
public class FileIndependenceCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly bool _negated;

    public FileIndependenceCondition(Graph graph, PatternMatcher fileMatcher, bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _fileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        _negated = negated;

        // Issue #18: Positive mood only
        if (_negated)
            throw new InvalidOperationException("HaveNoCycles() supports only positive mood. Use .Should().HaveNoCycles(), not .ShouldNot()");
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        var violations = new List<Violation>();

        // Filter edges to only include matching files
        var filteredEdges = _graph.Edges
            .Where(e => _fileMatcher.Matches(e.Source) || _fileMatcher.Matches(e.Target))
            .ToList();

        if (filteredEdges.Count == 0)
        {
            // No matching files = no cycles possible = rule passes
            return await Task.FromResult(violations.AsReadOnly());
        }

        // Create filtered graph for cycle detection
        var filteredGraph = new Graph(filteredEdges.ToArray());

        // Find all elementary cycles using Johnson's algorithm
        var cycleFinder = new JohnsonsCycles(filteredGraph);
        var allCycles = cycleFinder.FindAllCycles();

        // Each cycle found = violation
        // Cycles are reported as readable paths: "A → B → C → A"
        foreach (var cycle in allCycles)
        {
            violations.Add(CyclicDependency.Create(cycle));
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
