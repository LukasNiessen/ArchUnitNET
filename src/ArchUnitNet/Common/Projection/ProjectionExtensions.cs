using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// Extension methods for projecting graphs via MapFunctions.
/// Enables fluent API for transforming and filtering edges.
/// </summary>
public static class ProjectionExtensions
{
    /// <summary>
    /// Project all edges through a MapFunction, creating a new projected graph.
    /// Edges that return null from the MapFunction are filtered out.
    /// </summary>
    /// <param name="graph">Graph to project</param>
    /// <param name="mapFunction">Function to apply to each edge</param>
    /// <returns>New ProjectedGraph with transformed edges</returns>
    public static ProjectedGraph ProjectEdges(this Graph graph, MapFunction mapFunction)
    {
        if (graph == null)
            throw new ArgumentNullException(nameof(graph));

        if (mapFunction == null)
            throw new ArgumentNullException(nameof(mapFunction));

        var projectedEdges = new List<ProjectedEdge>();

        foreach (var edge in graph.Edges)
        {
            var projected = mapFunction(edge);
            if (projected != null)
            {
                projectedEdges.Add(projected);
            }
        }

        return new ProjectedGraph(projectedEdges);
    }

    /// <summary>
    /// Project edges through the identity mapping (no transformation).
    /// Useful as a base transformation before applying filters.
    /// </summary>
    public static ProjectedGraph ProjectIdentity(this Graph graph)
    {
        return graph.ProjectEdges(MapFunctions.Identity);
    }

    /// <summary>
    /// Project edges, filtering out self-edges.
    /// </summary>
    public static ProjectedGraph ProjectPerEdge(this Graph graph)
    {
        return graph.ProjectEdges(MapFunctions.PerEdge);
    }

    /// <summary>
    /// Project edges, keeping only internal dependencies.
    /// </summary>
    public static ProjectedGraph ProjectInternalOnly(this Graph graph)
    {
        return graph.ProjectEdges(MapFunctions.PerInternalEdge);
    }

    /// <summary>
    /// Project edges, keeping only external dependencies.
    /// </summary>
    public static ProjectedGraph ProjectExternalOnly(this Graph graph)
    {
        return graph.ProjectEdges(MapFunctions.PerExternalEdge);
    }
}
