using ArchUnitNet.Common.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// Extracts using statements from a Roslyn syntax tree.
/// Handles: using, static using, global using, alias using, extern alias.
///
/// For each using, returns the imported namespace/type name (not the resolved file path).
/// Resolution (mapping names to file paths or external packages) happens later.
/// </summary>
public class UsingStatementExtractor : CSharpSyntaxWalker
{
    private readonly List<ImportedNamespace> _imports = new();

    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var name = node.Name?.ToString() ?? "";

        // Skip empty or global namespace
        if (string.IsNullOrWhiteSpace(name))
            return;

        var kind = DetermineImportKind(node);
        var isGlobal = node.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
        _imports.Add(new ImportedNamespace(name, kind, isGlobal));

        base.VisitUsingDirective(node);
    }

    public IReadOnlyList<ImportedNamespace> GetImports() => _imports.AsReadOnly();

    private static ImportKind DetermineImportKind(UsingDirectiveSyntax node)
    {
        if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
            return ImportKind.StaticUsing;

        if (node.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
            return ImportKind.GlobalUsing;

        if (node.Alias != null)
            return ImportKind.AliasUsing;

        return ImportKind.Using;
    }
}

/// <summary>
/// Represents an imported namespace from a using statement.
/// Example: "System.Collections" from "using System.Collections;"
/// </summary>
public record ImportedNamespace(
    /// <summary>The imported namespace or type name.</summary>
    string Name,

    /// <summary>Kind of import (Using, StaticUsing, etc).</summary>
    ImportKind Kind,

    /// <summary>Whether this is a global using directive.</summary>
    bool IsGlobal);
