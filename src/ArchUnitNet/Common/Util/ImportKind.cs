namespace ArchUnitNet.Common.Util;

/// <summary>
/// Describes the kind of C# import statement.
/// Used to categorize edges in the dependency graph.
/// </summary>
[Flags]
public enum ImportKind
{
    /// <summary>Regular using statement: using System;</summary>
    Using = 1,

    /// <summary>Static using: using static System.Console;</summary>
    StaticUsing = 2,

    /// <summary>Global using (C# 10+): global using System;</summary>
    GlobalUsing = 4,

    /// <summary>Alias using: using str = System.String;</summary>
    AliasUsing = 8,

    /// <summary>Extern alias: extern alias MyAssembly;</summary>
    ExternAlias = 16,
}

/// <summary>
/// Utility methods for ImportKind.
/// </summary>
public static class ImportKindExtensions
{
    /// <summary>
    /// Get a human-readable description of the import kind.
    /// </summary>
    public static string GetDescription(this ImportKind kind) =>
        kind switch
        {
            ImportKind.Using => "using statement",
            ImportKind.StaticUsing => "static using",
            ImportKind.GlobalUsing => "global using",
            ImportKind.AliasUsing => "alias using",
            ImportKind.ExternAlias => "extern alias",
            _ => "unknown import kind",
        };

    /// <summary>
    /// Get all individual import kinds (excluding combined flags).
    /// </summary>
    public static IEnumerable<ImportKind> GetIndividualKinds(this ImportKind kinds)
    {
        if (kinds.HasFlag(ImportKind.Using))
            yield return ImportKind.Using;
        if (kinds.HasFlag(ImportKind.StaticUsing))
            yield return ImportKind.StaticUsing;
        if (kinds.HasFlag(ImportKind.GlobalUsing))
            yield return ImportKind.GlobalUsing;
        if (kinds.HasFlag(ImportKind.AliasUsing))
            yield return ImportKind.AliasUsing;
        if (kinds.HasFlag(ImportKind.ExternAlias))
            yield return ImportKind.ExternAlias;
    }
}
