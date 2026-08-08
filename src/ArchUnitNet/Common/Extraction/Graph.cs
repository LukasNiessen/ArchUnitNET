namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// A graph is a collection of dependency edges.
/// In ArchUnitTS: type Graph = Edge[]
///
/// This is the raw, unprocessed dependency graph extracted from source code.
/// All downstream operations (projections, assertions, reporting) work with graphs.
/// </summary>
public class Graph
{
    private readonly List<Edge> _edges;

    public Graph(IEnumerable<Edge>? edges = null)
    {
        _edges = new List<Edge>(edges ?? Enumerable.Empty<Edge>());
    }

    /// <summary>
    /// All edges in the graph.
    /// </summary>
    public IReadOnlyList<Edge> Edges => _edges.AsReadOnly();

    /// <summary>
    /// Add an edge to the graph.
    /// </summary>
    public void Add(Edge edge)
    {
        _edges.Add(edge);
    }

    /// <summary>
    /// Validate all edges in the graph.
    /// Throws if any edge is invalid.
    /// </summary>
    public void Validate()
    {
        foreach (var edge in _edges)
        {
            edge.Validate();
        }
    }

    /// <summary>
    /// Merge another graph into this one.
    /// </summary>
    public void Merge(Graph other)
    {
        _edges.AddRange(other.Edges);
    }

    /// <summary>
    /// Get all unique node labels (both sources and targets).
    /// </summary>
    public IReadOnlySet<string> GetNodes()
    {
        var nodes = new HashSet<string>();
        foreach (var edge in _edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }
        return nodes;
    }

    /// <summary>
    /// Filter edges by predicate.
    /// </summary>
    public Graph Where(Func<Edge, bool> predicate)
    {
        return new Graph(_edges.Where(predicate));
    }
}
