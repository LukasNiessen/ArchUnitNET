using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// Represents a dependency edge in the code graph.
/// An edge connects a source file to a target file, capturing how one depends on the other.
///
/// In ArchUnitTS: type Edge = { source, target, external, importKinds }
/// </summary>
public record Edge(
    /// <summary>
    /// Normalized path to the source file (the file that imports).
    /// Example: "src/Common/Error.cs"
    /// </summary>
    string Source,

    /// <summary>
    /// Normalized path to the target file (the file being imported).
    /// Example: "src/Files/FluentApi.cs"
    /// </summary>
    string Target,

    /// <summary>
    /// Whether this is an external dependency (outside the project).
    /// External examples: Microsoft.CodeAnalysis, System.Collections.Generic
    /// </summary>
    bool External,

    /// <summary>
    /// The kinds of imports used (Using, StaticUsing, GlobalUsing, etc).
    /// Multiple kinds can be combined if the same target is imported multiple ways.
    /// </summary>
    IReadOnlyList<ImportKind> ImportKinds)
{
    /// <summary>
    /// Validate that Edge has consistent data.
    /// Throws if source/target are empty or null.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Source))
            throw new ArgumentException("Edge source cannot be null or empty", nameof(Source));

        if (string.IsNullOrWhiteSpace(Target))
            throw new ArgumentException("Edge target cannot be null or empty", nameof(Target));

        if (ImportKinds == null || ImportKinds.Count == 0)
            throw new ArgumentException("Edge must have at least one ImportKind", nameof(ImportKinds));
    }

    /// <summary>
    /// Check if this is a self-edge (source == target).
    /// Self-edges represent files with no imports (they still appear as nodes in the graph).
    /// </summary>
    public bool IsSelfEdge => Source == Target;
}
