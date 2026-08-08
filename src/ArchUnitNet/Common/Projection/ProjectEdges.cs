using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// Transforms and filters edges in dependency graphs.
/// Groups edges by source→target, aggregating import kinds.
/// </summary>
public static class ProjectEdges
{
    /// <summary>
    /// Group edges by (source, target) pair, combining import kinds.
    /// Multiple edges between same source/target are merged into one.
    /// </summary>
    public static List<Edge> GroupBySourceAndTarget(Graph graph)
    {
        var grouped = graph.Edges
            .GroupBy(e => (e.Source, e.Target, e.External))
            .Select(g => new Edge(
                Source: g.Key.Source,
                Target: g.Key.Target,
                External: g.Key.External,
                ImportKinds: g.SelectMany(e => e.ImportKinds).Distinct().ToList()
            ))
            .ToList();

        return grouped;
    }

    /// <summary>
    /// Filter edges by target dependency name/path.
    /// </summary>
    public static List<Edge> FilterByTarget(List<Edge> edges, string target)
    {
        return edges.Where(e => e.Target == target).ToList();
    }

    /// <summary>
    /// Filter edges by source file path.
    /// </summary>
    public static List<Edge> FilterBySource(List<Edge> edges, string source)
    {
        return edges.Where(e => e.Source == source).ToList();
    }

    /// <summary>
    /// Remove all external dependencies, keeping only internal edges.
    /// </summary>
    public static List<Edge> RemoveExternalDependencies(List<Edge> edges)
    {
        return edges.Where(e => !e.External).ToList();
    }

    /// <summary>
    /// Remove all internal dependencies, keeping only external edges.
    /// </summary>
    public static List<Edge> RemoveInternalDependencies(List<Edge> edges)
    {
        return edges.Where(e => e.External).ToList();
    }
}
