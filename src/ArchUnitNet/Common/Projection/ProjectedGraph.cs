using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection.Cycles;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// A projected graph containing relabeled/transformed edges.
/// Supports cycle detection, node extraction, and dependency queries.
///
/// Key feature: Preserves raw edges for violation tracing.
/// When reporting violations, traces back to original file paths.
/// </summary>
public class ProjectedGraph
{
    private readonly List<ProjectedEdge> _edges;
    private readonly Graph _underlyingGraph;

    /// <summary>
    /// Create a projected graph from projected edges.
    /// Also maintains the underlying raw graph for cycle detection.
    /// </summary>
    public ProjectedGraph(IEnumerable<ProjectedEdge> edges)
    {
        _edges = new List<ProjectedEdge>(edges ?? Enumerable.Empty<ProjectedEdge>());

        // Reconstruct underlying raw graph from projected edges' raw edges
        var allRawEdges = _edges
            .SelectMany(pe => pe.RawEdges)
            .ToArray();
        _underlyingGraph = new Graph(allRawEdges);
    }

    /// <summary>
    /// Get all projected edges in this graph.
    /// </summary>
    public IReadOnlyList<ProjectedEdge> Edges => _edges.AsReadOnly();

    /// <summary>
    /// Extract all unique nodes (sources and targets) from projected edges.
    /// </summary>
    public List<string> ExtractAllNodes()
    {
        var nodes = new HashSet<string>();

        foreach (var edge in _edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }

        return nodes.ToList();
    }

    /// <summary>
    /// Extract nodes that depend on the given target.
    /// </summary>
    public List<string> ExtractDependents(string target)
    {
        return _edges
            .Where(e => e.Target == target)
            .Select(e => e.Source)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Extract nodes that the given source depends on.
    /// </summary>
    public List<string> ExtractDependencies(string source)
    {
        return _edges
            .Where(e => e.Source == source)
            .Select(e => e.Target)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Count how many edges point to this node (incoming dependencies).
    /// </summary>
    public int ExtractInDegree(string node)
    {
        return _edges.Count(e => e.Target == node);
    }

    /// <summary>
    /// Count how many edges originate from this node (outgoing dependencies).
    /// </summary>
    public int ExtractOutDegree(string node)
    {
        return _edges.Count(e => e.Source == node);
    }

    /// <summary>
    /// Check if a node is isolated (no incoming or outgoing edges).
    /// </summary>
    public bool IsIsolated(string node)
    {
        return ExtractInDegree(node) == 0 && ExtractOutDegree(node) == 0;
    }

    /// <summary>
    /// Find all strongly connected components (SCCs) in this graph.
    /// An SCC is a maximal group of nodes that can reach each other.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindStronglyConnectedComponents()
    {
        var tarjan = new TarjanSCC(_underlyingGraph);
        return tarjan.FindSCCs();
    }

    /// <summary>
    /// Find all SCCs that contain cycles (size > 1 or self-loop).
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindCyclicSCCs()
    {
        var tarjan = new TarjanSCC(_underlyingGraph);
        return tarjan.FindCyclicSCCs();
    }

    /// <summary>
    /// Find all elementary cycles in this graph.
    /// An elementary cycle has no repeated vertices except start/end.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> FindAllCycles()
    {
        var johnson = new JohnsonsCycles(_underlyingGraph);
        return johnson.FindAllCycles();
    }

    /// <summary>
    /// Check if this graph contains any cycles.
    /// </summary>
    public bool HasCycles()
    {
        var cyclicSccs = FindCyclicSCCs();
        return cyclicSccs.Count > 0;
    }

    /// <summary>
    /// Get the count of edges in this graph.
    /// </summary>
    public int EdgeCount => _edges.Count;

    /// <summary>
    /// Get the count of nodes in this graph.
    /// </summary>
    public int NodeCount => ExtractAllNodes().Count;
}
