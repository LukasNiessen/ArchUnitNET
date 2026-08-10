using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// Extracts and analyzes nodes (files/dependencies) from edges.
/// Computes node properties like in-degree, out-degree, isolation.
/// </summary>
public static class ProjectNodes
{
    /// <summary>
    /// Extract all unique nodes (both sources and targets) from edges.
    /// </summary>
    public static List<string> ExtractAllNodes(Graph graph)
    {
        var nodes = new HashSet<string>();

        foreach (var edge in graph.Edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }

        return nodes.ToList();
    }

    /// <summary>
    /// Extract only internal nodes (files that are sources in the project).
    /// </summary>
    public static List<string> ExtractInternalNodes(Graph graph)
    {
        var internalNodes = new HashSet<string>();

        foreach (var edge in graph.Edges.Where(e => !e.External))
        {
            internalNodes.Add(edge.Source);
            internalNodes.Add(edge.Target);
        }

        return internalNodes.ToList();
    }

    /// <summary>
    /// Extract only external nodes (dependencies outside the project).
    /// </summary>
    public static List<string> ExtractExternalNodes(Graph graph)
    {
        return graph.Edges
            .Where(e => e.External)
            .Select(e => e.Target)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Count how many edges point TO this node (incoming dependencies).
    /// </summary>
    public static int ExtractInDegree(List<Edge> edges, string node)
    {
        return edges.Count(e => e.Target == node);
    }

    /// <summary>
    /// Count how many edges originate FROM this node (outgoing dependencies).
    /// </summary>
    public static int ExtractOutDegree(List<Edge> edges, string node)
    {
        return edges.Count(e => e.Source == node);
    }

    /// <summary>
    /// Check if a node is isolated (no incoming or outgoing edges).
    /// </summary>
    public static bool IsIsolated(List<Edge> edges, string node)
    {
        return ExtractInDegree(edges, node) == 0 && ExtractOutDegree(edges, node) == 0;
    }

    /// <summary>
    /// Extract nodes that depend on the given target.
    /// </summary>
    public static List<string> ExtractDependents(List<Edge> edges, string target)
    {
        return edges
            .Where(e => e.Target == target)
            .Select(e => e.Source)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Extract nodes that the given source depends on.
    /// </summary>
    public static List<string> ExtractDependencies(List<Edge> edges, string source)
    {
        return edges
            .Where(e => e.Source == source)
            .Select(e => e.Target)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// Extract nodes that depend on the given target (Graph overload).
    /// </summary>
    public static List<string> ExtractDependents(Graph graph, string target)
    {
        return ExtractDependents(graph.Edges.ToList(), target);
    }

    /// <summary>
    /// Extract nodes that the given source depends on (Graph overload).
    /// </summary>
    public static List<string> ExtractDependencies(Graph graph, string source)
    {
        return ExtractDependencies(graph.Edges.ToList(), source);
    }

    /// <summary>
    /// Count how many edges point TO this node (Graph overload).
    /// </summary>
    public static int ExtractInDegree(Graph graph, string node)
    {
        return ExtractInDegree(graph.Edges.ToList(), node);
    }

    /// <summary>
    /// Count how many edges originate FROM this node (Graph overload).
    /// </summary>
    public static int ExtractOutDegree(Graph graph, string node)
    {
        return ExtractOutDegree(graph.Edges.ToList(), node);
    }
}
