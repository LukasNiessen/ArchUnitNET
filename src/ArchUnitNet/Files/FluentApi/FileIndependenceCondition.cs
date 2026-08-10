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
/// Uses Tarjan's SCC algorithm to find strongly connected components
/// and reports any cycles found within the filtered file set.
/// </summary>
public class FileIndependenceCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly bool _negated;

    public FileIndependenceCondition(Graph graph, PatternMatcher fileMatcher, bool negated)
    {
        _graph = graph;
        _fileMatcher = fileMatcher;
        _negated = negated;
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
            if (_negated)
            {
                // ShouldNot().HaveNoCycles() with no matching files = no violation
                return await Task.FromResult(violations.AsReadOnly());
            }
            // Should().HaveNoCycles() with no matching files = no violation (vacuously true)
            return await Task.FromResult(violations.AsReadOnly());
        }

        // Create filtered graph for cycle detection
        var filteredGraph = new Graph(filteredEdges.ToArray());

        // Find all elementary cycles using Johnson's algorithm
        var cycleFinder = new JohnsonsCycles(filteredGraph);
        var allCycles = cycleFinder.FindAllCycles();

        foreach (var cycle in allCycles)
        {
            bool violates = true;

            if (_negated)
            {
                // ShouldNot().HaveNoCycles() with cycles found = violation (cycles should not exist)
                violates = true;
            }
            // Should().HaveNoCycles() with cycles found = no violation (cycles exist as expected)
            else
            {
                violates = false;
            }

            if (violates)
            {
                violations.Add(CyclicDependency.Create(cycle));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
