using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// Delegate for transforming/filtering edges during graph projection.
/// MapFunctions enable each module (Files, Metrics, Slices) to have its own view of the dependency graph.
///
/// A MapFunction:
/// - Takes a raw Edge as input
/// - Returns a ProjectedEdge (transformed with relabeled source/target), or null if filtering out
/// - Can be chained or composed with other MapFunctions
///
/// Example: Transform file paths to folder paths: "src/Dashboard.cs" → "src"
/// </summary>
public delegate ProjectedEdge? MapFunction(Edge edge);

/// <summary>
/// Built-in MapFunction implementations for common filtering/transformation patterns.
/// </summary>
public static class MapFunctions
{
    /// <summary>
    /// Identity mapping: pass through all edges unchanged.
    /// Source and target labels remain exactly as in raw edges.
    /// </summary>
    /// <param name="edge">Raw edge to map</param>
    /// <returns>Projected edge with unchanged labels and preserved raw edge</returns>
    public static ProjectedEdge? Identity(Edge edge)
    {
        return ProjectedEdge.FromRawEdge(edge);
    }

    /// <summary>
    /// Filter out self-edges (A → A), keep all others.
    /// Self-edges represent files with internal/circular imports.
    /// </summary>
    /// <param name="edge">Raw edge to filter</param>
    /// <returns>Projected edge if not self-edge, null if filtering out</returns>
    public static ProjectedEdge? PerEdge(Edge edge)
    {
        if (edge.IsSelfEdge)
            return null;

        return ProjectedEdge.FromRawEdge(edge);
    }

    /// <summary>
    /// Keep only internal dependencies (within the project), filter out external.
    /// External example: imports from System.*, NuGet packages, etc.
    /// </summary>
    /// <param name="edge">Raw edge to filter</param>
    /// <returns>Projected edge if internal, null if external</returns>
    public static ProjectedEdge? PerInternalEdge(Edge edge)
    {
        if (edge.External)
            return null;

        return ProjectedEdge.FromRawEdge(edge);
    }

    /// <summary>
    /// Keep only external dependencies (outside the project), filter out internal.
    /// External example: imports from System.*, NuGet packages, etc.
    /// </summary>
    /// <param name="edge">Raw edge to filter</param>
    /// <returns>Projected edge if external, null if internal</returns>
    public static ProjectedEdge? PerExternalEdge(Edge edge)
    {
        if (!edge.External)
            return null;

        return ProjectedEdge.FromRawEdge(edge);
    }

    /// <summary>
    /// Compose two MapFunctions: apply first, then second.
    /// If first returns null, skip to next edge without calling second.
    /// </summary>
    /// <param name="first">First mapping to apply</param>
    /// <param name="second">Second mapping to apply to result of first</param>
    /// <returns>Composed MapFunction</returns>
    public static MapFunction Compose(MapFunction first, MapFunction second)
    {
        return edge =>
        {
            var intermediate = first(edge);
            if (intermediate == null)
                return null;
            return second(intermediate.RawEdges[0]);
        };
    }
}
