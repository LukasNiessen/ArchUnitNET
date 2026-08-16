using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.Projection.Cycles;
using ArchUnitNet.Slices.Assertion;
using ArchUnitNet.Slices.Common;
using ArchUnitNet.Slices.Projection;

namespace ArchUnitNet.Slices.FluentApi;

/// <summary>
/// Entry point for slice-based architecture rules.
/// Defines how files are mapped to logical slices based on patterns.
/// </summary>
public class SliceConditionBuilder
{
    private string? _slicePattern;
    private IEnumerable<Edge>? _edges;

    /// <summary>
    /// Define slices based on a file path pattern.
    /// Example: "src/{Slice}/**/*.cs" extracts slices like "Feature1", "Feature2"
    /// </summary>
    public SliceConditionBuilder DefinedBy(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _slicePattern = pattern;
        return this;
    }

    /// <summary>
    /// Provide the edges to analyze (dependency graph).
    /// </summary>
    public SliceConditionBuilder WithEdges(IEnumerable<Edge> edges)
    {
        _edges = edges ?? throw new ArgumentNullException(nameof(edges));
        return this;
    }

    /// <summary>
    /// Transition to positive condition builder (should adhere, should contain, etc.).
    /// </summary>
    public PositiveSliceCondition Should()
    {
        if (string.IsNullOrEmpty(_slicePattern))
            throw new InvalidOperationException("Slice pattern must be defined first via DefinedBy()");

        return new PositiveSliceCondition(_slicePattern, _edges);
    }

    /// <summary>
    /// Transition to negative condition builder (should not depend on, etc.).
    /// </summary>
    public NegativeSliceCondition ShouldNot()
    {
        if (string.IsNullOrEmpty(_slicePattern))
            throw new InvalidOperationException("Slice pattern must be defined first via DefinedBy()");

        return new NegativeSliceCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Builder for positive slice conditions (should adhere, should contain).
/// </summary>
public class PositiveSliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal PositiveSliceCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    /// <summary>
    /// Slices should adhere to defined architecture (no cycles, no unexpected dependencies).
    /// </summary>
    public AdhereToSlicesCondition AdhereToDefinedSlices()
    {
        return new AdhereToSlicesCondition(_slicePattern, _edges);
    }

    /// <summary>
    /// Slices should follow a specific dependency structure.
    /// </summary>
    public DependencyPatternCondition FollowPattern(string dependencyPattern)
    {
        if (string.IsNullOrEmpty(dependencyPattern))
            throw new ArgumentException("Dependency pattern cannot be null or empty", nameof(dependencyPattern));

        return new DependencyPatternCondition(_slicePattern, _edges, dependencyPattern);
    }

    /// <summary>
    /// Slices should not have any cyclic dependencies.
    /// </summary>
    public NoCyclicSlicesCondition BeAcyclic()
    {
        return new NoCyclicSlicesCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Builder for negative slice conditions (should not).
/// </summary>
public class NegativeSliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal NegativeSliceCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    /// <summary>
    /// Slices should not have any cyclic dependencies.
    /// </summary>
    public NoCyclicSlicesCondition HaveCycles()
    {
        return new NoCyclicSlicesCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Marker interface for conditions that can be checked.
/// </summary>
public interface ISliceCondition
{
}

/// <summary>
/// Represents a condition where slices should adhere to defined architecture.
/// </summary>
public class AdhereToSlicesCondition : ISliceCondition, Checkable
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal AdhereToSlicesCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        if (_edges == null || !_edges.Any())
        {
            // Empty-test guard: fail if no edges unless explicitly allowed
            if (!options.AllowEmptyTests)
            {
                violations.Add(ViolatingSliceEdge.CreateEmptyTestViolation(_slicePattern));
            }
            return await Task.FromResult(violations.AsReadOnly());
        }

        var projector = new SliceProjector(_slicePattern);
        var architecture = projector.Project(_edges);

        // Check that all slice dependencies are acyclic (basic adherence)
        var sliceDependencyGraph = BuildSliceDependencyGraph(architecture);

        if (HasCycles(sliceDependencyGraph))
        {
            // Find and report all cyclic dependencies
            var cycles = FindCycles(sliceDependencyGraph);
            foreach (var cycle in cycles)
            {
                for (int i = 0; i < cycle.Count; i++)
                {
                    var source = cycle[i];
                    var target = cycle[(i + 1) % cycle.Count];
                    var deps = architecture.GetDependenciesFrom(source)
                        .Where(d => d.TargetSlice == target)
                        .FirstOrDefault();

                    if (deps != null)
                    {
                        violations.Add(ViolatingSliceEdge.CreateCyclicSliceDependency(
                            deps.SourceSlice,
                            deps.TargetSlice,
                            deps.SourceFile,
                            deps.TargetFile
                        ));
                    }
                }
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    private Dictionary<string, HashSet<string>> BuildSliceDependencyGraph(SliceArchitecture architecture)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        foreach (var slice in architecture.Slices.Keys)
        {
            graph[slice] = new HashSet<string>();
        }

        foreach (var dep in architecture.Dependencies)
        {
            graph[dep.SourceSlice].Add(dep.TargetSlice);
        }

        return graph;
    }

    private bool HasCycles(Dictionary<string, HashSet<string>> graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                if (HasCycleDFS(node, graph, visited, recursionStack))
                    return true;
            }
        }
        return false;
    }

    private bool HasCycleDFS(string node, Dictionary<string, HashSet<string>> graph, HashSet<string> visited, HashSet<string> recursionStack)
    {
        visited.Add(node);
        recursionStack.Add(node);

        if (graph.ContainsKey(node))
        {
            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasCycleDFS(neighbor, graph, visited, recursionStack))
                        return true;
                }
                else if (recursionStack.Contains(neighbor))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(node);
        return false;
    }

    private List<List<string>> FindCycles(Dictionary<string, HashSet<string>> graph)
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>();
        var path = new List<string>();

        foreach (var start in graph.Keys)
        {
            if (!visited.Contains(start))
            {
                FindCyclesDFS(start, start, graph, visited, path, cycles);
            }
        }

        return cycles;
    }

    private void FindCyclesDFS(string current, string start, Dictionary<string, HashSet<string>> graph, HashSet<string> visited, List<string> path, List<List<string>> cycles)
    {
        visited.Add(current);
        path.Add(current);

        if (graph.ContainsKey(current))
        {
            foreach (var neighbor in graph[current])
            {
                if (neighbor == start && path.Count > 1)
                {
                    cycles.Add(new List<string>(path));
                }
                else if (!visited.Contains(neighbor))
                {
                    FindCyclesDFS(neighbor, start, graph, visited, path, cycles);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
    }
}

/// <summary>
/// Represents a dependency pattern condition between slices.
/// </summary>
public class DependencyPatternCondition : ISliceCondition, Checkable
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;
    private readonly string _dependencyPattern;

    internal DependencyPatternCondition(string slicePattern, IEnumerable<Edge>? edges, string dependencyPattern)
    {
        _slicePattern = slicePattern;
        _edges = edges;
        _dependencyPattern = dependencyPattern;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        if (_edges == null || !_edges.Any())
        {
            // Empty-test guard: fail if no edges unless explicitly allowed
            if (!options.AllowEmptyTests)
            {
                violations.Add(ViolatingSliceEdge.CreateEmptyTestViolation(_slicePattern));
            }
            return await Task.FromResult(violations.AsReadOnly());
        }

        var projector = new SliceProjector(_slicePattern);
        var architecture = projector.Project(_edges);

        // Parse pattern (simple format: "Layer1 -> Layer2 -> Layer3")
        var allowedPairs = ParseDependencyPattern(_dependencyPattern);

        // Check that all actual dependencies match the allowed pattern
        foreach (var dep in architecture.Dependencies)
        {
            var pair = (dep.SourceSlice, dep.TargetSlice);
            if (!allowedPairs.Contains(pair))
            {
                violations.Add(ViolatingSliceEdge.CreateUnexpectedDependency(
                    dep.SourceSlice,
                    dep.TargetSlice,
                    dep.SourceFile,
                    dep.TargetFile
                ));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    private HashSet<(string source, string target)> ParseDependencyPattern(string pattern)
    {
        var allowed = new HashSet<(string source, string target)>();

        // Simple pattern parser: "Layer1 -> Layer2 -> Layer3"
        var layers = pattern.Split("->")
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();

        for (int i = 0; i < layers.Count - 1; i++)
        {
            allowed.Add((layers[i], layers[i + 1]));
        }

        return allowed;
    }
}

/// <summary>
/// Represents a no-cycles condition between slices.
/// </summary>
public class NoCyclicSlicesCondition : ISliceCondition, Checkable
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal NoCyclicSlicesCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        if (_edges == null || !_edges.Any())
        {
            // Empty-test guard: fail if no edges unless explicitly allowed
            if (!options.AllowEmptyTests)
            {
                violations.Add(ViolatingSliceEdge.CreateEmptyTestViolation(_slicePattern));
            }
            return await Task.FromResult(violations.AsReadOnly());
        }

        var projector = new SliceProjector(_slicePattern);
        var architecture = projector.Project(_edges);

        // Build slice dependency graph and detect cycles
        var sliceDependencyGraph = BuildSliceDependencyGraph(architecture);

        if (HasCycles(sliceDependencyGraph))
        {
            var cycles = FindCycles(sliceDependencyGraph);
            foreach (var cycle in cycles)
            {
                for (int i = 0; i < cycle.Count; i++)
                {
                    var source = cycle[i];
                    var target = cycle[(i + 1) % cycle.Count];
                    var deps = architecture.GetDependenciesFrom(source)
                        .Where(d => d.TargetSlice == target)
                        .FirstOrDefault();

                    if (deps != null)
                    {
                        violations.Add(ViolatingSliceEdge.CreateCyclicSliceDependency(
                            deps.SourceSlice,
                            deps.TargetSlice,
                            deps.SourceFile,
                            deps.TargetFile
                        ));
                    }
                }
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    private Dictionary<string, HashSet<string>> BuildSliceDependencyGraph(SliceArchitecture architecture)
    {
        var graph = new Dictionary<string, HashSet<string>>();
        foreach (var slice in architecture.Slices.Keys)
        {
            graph[slice] = new HashSet<string>();
        }

        foreach (var dep in architecture.Dependencies)
        {
            graph[dep.SourceSlice].Add(dep.TargetSlice);
        }

        return graph;
    }

    private bool HasCycles(Dictionary<string, HashSet<string>> graph)
    {
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                if (HasCycleDFS(node, graph, visited, recursionStack))
                    return true;
            }
        }
        return false;
    }

    private bool HasCycleDFS(string node, Dictionary<string, HashSet<string>> graph, HashSet<string> visited, HashSet<string> recursionStack)
    {
        visited.Add(node);
        recursionStack.Add(node);

        if (graph.ContainsKey(node))
        {
            foreach (var neighbor in graph[node])
            {
                if (!visited.Contains(neighbor))
                {
                    if (HasCycleDFS(neighbor, graph, visited, recursionStack))
                        return true;
                }
                else if (recursionStack.Contains(neighbor))
                {
                    return true;
                }
            }
        }

        recursionStack.Remove(node);
        return false;
    }

    private List<List<string>> FindCycles(Dictionary<string, HashSet<string>> graph)
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>();
        var path = new List<string>();

        foreach (var start in graph.Keys)
        {
            if (!visited.Contains(start))
            {
                FindCyclesDFS(start, start, graph, visited, path, cycles);
            }
        }

        return cycles;
    }

    private void FindCyclesDFS(string current, string start, Dictionary<string, HashSet<string>> graph, HashSet<string> visited, List<string> path, List<List<string>> cycles)
    {
        visited.Add(current);
        path.Add(current);

        if (graph.ContainsKey(current))
        {
            foreach (var neighbor in graph[current])
            {
                if (neighbor == start && path.Count > 1)
                {
                    cycles.Add(new List<string>(path));
                }
                else if (!visited.Contains(neighbor))
                {
                    FindCyclesDFS(neighbor, start, graph, visited, path, cycles);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
    }
}
