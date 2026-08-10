using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArchUnitNet.Metrics.Extraction;

/// <summary>
/// Analyzes method bodies to find field accesses.
/// Walks the syntax tree looking for member access expressions.
/// </summary>
internal class FieldAccessAnalyzer : CSharpSyntaxWalker
{
    private readonly HashSet<string> _accessedFields = new();
    private readonly HashSet<string> _allFieldNames;

    public FieldAccessAnalyzer(IEnumerable<string> fieldNames)
    {
        _allFieldNames = new HashSet<string>(fieldNames);
    }

    /// <summary>
    /// Get all fields accessed in the analyzed syntax tree.
    /// </summary>
    public IReadOnlySet<string> AccessedFields => (IReadOnlySet<string>)_accessedFields;

    /// <summary>
    /// Visit member access expressions (e.g., this.field or field).
    /// </summary>
    public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var memberName = node.Name.Identifier.Text;

        // Check if this is a field access (not a method call or other member)
        if (_allFieldNames.Contains(memberName))
        {
            _accessedFields.Add(memberName);
        }

        base.VisitMemberAccessExpression(node);
    }

    /// <summary>
    /// Visit simple member access (e.g., field used directly without "this.").
    /// </summary>
    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        var name = node.Identifier.Text;

        // Check if it's a known field (heuristic: assume simple identifiers are fields if they match)
        if (_allFieldNames.Contains(name) && !IsMethodCall(node))
        {
            _accessedFields.Add(name);
        }

        base.VisitIdentifierName(node);
    }

    /// <summary>
    /// Check if an identifier is used as a method call.
    /// Heuristic: if parent is invocation expression, it's a method call.
    /// </summary>
    private static bool IsMethodCall(IdentifierNameSyntax node)
    {
        var parent = node.Parent;
        return parent is InvocationExpressionSyntax or ArgumentListSyntax;
    }
}
