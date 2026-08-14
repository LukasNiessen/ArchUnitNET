using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Common.Projection;

/// <summary>
/// Represents a projected/transformed edge in a dependency graph.
/// A projected edge may have relabeled source/target (via MapFunction) but preserves original raw edges for tracing.
/// </summary>
public record ProjectedEdge(
    /// <summary>
    /// Transformed/relabeled source identifier (may be different from raw edge source).
    /// Example: If mapping files to folders, this could be "src/common" instead of "src/common/Error.cs"
    /// </summary>
    string Source,

    /// <summary>
    /// Transformed/relabeled target identifier (may be different from raw edge target).
    /// </summary>
    string Target,

    /// <summary>
    /// Whether this dependency is external (NuGet package, System.* namespace, etc).
    /// </summary>
    bool External = false,

    /// <summary>
    /// Combined import kinds from all raw edges that created this projected edge.
    /// </summary>
    IReadOnlyList<ImportKind> ImportKinds = null!,

    /// <summary>
    /// Original raw edges that were projected/transformed into this edge.
    /// CRITICAL: Preserved for violation messages - allows tracing back to exact file/line.
    /// Never null or empty - at least one raw edge always exists.
    /// </summary>
    Edge[] RawEdges = null!)
{
    /// <summary>
    /// Create a projected edge from a single raw edge with no transformation.
    /// Used when applying identity mapping (pass-through).
    /// </summary>
    public static ProjectedEdge FromRawEdge(Edge rawEdge)
    {
        rawEdge.Validate();
        return new ProjectedEdge(
            Source: rawEdge.Source,
            Target: rawEdge.Target,
            External: rawEdge.External,
            ImportKinds: rawEdge.ImportKinds,
            RawEdges: new[] { rawEdge }
        );
    }

    /// <summary>
    /// Create a projected edge from multiple raw edges.
    /// Combines multiple edges into one by using provided relabeled source/target.
    /// Merges all ImportKinds from raw edges.
    /// </summary>
    /// <param name="projectedSource">Relabeled source (result of applying MapFunction)</param>
    /// <param name="projectedTarget">Relabeled target (result of applying MapFunction)</param>
    /// <param name="rawEdges">Original edges that were transformed into this projection</param>
    /// <returns>New projected edge with preserved raw edges</returns>
    public static ProjectedEdge FromRawEdges(
        string projectedSource,
        string projectedTarget,
        params Edge[] rawEdges)
    {
        if (string.IsNullOrWhiteSpace(projectedSource))
            throw new ArgumentException("Projected source cannot be null or empty", nameof(projectedSource));

        if (string.IsNullOrWhiteSpace(projectedTarget))
            throw new ArgumentException("Projected target cannot be null or empty", nameof(projectedTarget));

        if (rawEdges == null || rawEdges.Length == 0)
            throw new ArgumentException("At least one raw edge must be provided", nameof(rawEdges));

        foreach (var edge in rawEdges)
            edge.Validate();

        var isExternal = rawEdges.First().External;
        var mergedImportKinds = rawEdges
            .SelectMany(e => e.ImportKinds)
            .Distinct()
            .ToList();

        return new ProjectedEdge(
            Source: projectedSource,
            Target: projectedTarget,
            External: isExternal,
            ImportKinds: mergedImportKinds,
            RawEdges: rawEdges
        );
    }

    /// <summary>
    /// Debug representation showing projected labels and raw edge count.
    /// </summary>
    public override string ToString()
    {
        var externalMarker = External ? " [external]" : "";
        var rawEdgeInfo = RawEdges.Length > 1 ? $" ({RawEdges.Length} raw edges)" : "";
        return $"{Source} → {Target}{externalMarker}{rawEdgeInfo}";
    }

    /// <summary>
    /// Check if this projected edge is a self-edge (source == target after projection).
    /// </summary>
    public bool IsSelfEdge => Source == Target;
}
