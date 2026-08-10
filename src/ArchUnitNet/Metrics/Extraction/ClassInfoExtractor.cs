using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArchUnitNet.Metrics.Common;

namespace ArchUnitNet.Metrics.Extraction;

/// <summary>
/// Extracts class structure information from C# syntax trees.
/// Parses fields, methods, and field access patterns for cohesion analysis.
/// </summary>
public class ClassInfoExtractor
{
    private readonly ClassDeclarationSyntax _classDeclaration;
    private readonly string? _filePath;

    public ClassInfoExtractor(ClassDeclarationSyntax classDeclaration, string? filePath = null)
    {
        _classDeclaration = classDeclaration ?? throw new ArgumentNullException(nameof(classDeclaration));
        _filePath = filePath;
    }

    /// <summary>
    /// Extract complete class information including fields, methods, and access patterns.
    /// </summary>
    public ClassInfo Extract()
    {
        var className = _classDeclaration.Identifier.Text;
        var fields = ExtractFields();
        var methods = ExtractMethods(fields.Select(f => f.Name).ToList());

        return new ClassInfo(className, fields, methods);
    }

    private List<FieldInfo> ExtractFields()
    {
        var fields = new List<FieldInfo>();

        foreach (var fieldDeclaration in _classDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var fieldName = variable.Identifier.Text;
                var fieldType = fieldDeclaration.Declaration.Type.ToString();
                var isPublic = fieldDeclaration.Modifiers.Any(m => m.Text == "public");

                fields.Add(new FieldInfo(fieldName, fieldType, isPublic));
            }
        }

        return fields;
    }

    private List<MethodInfo> ExtractMethods(List<string> fieldNames)
    {
        var methods = new List<MethodInfo>();

        foreach (var methodDeclaration in _classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = methodDeclaration.Identifier.Text;
            var body = methodDeclaration.Body;

            if (body == null)
                continue;

            // Analyze field accesses in method body
            var fieldAccessSet = AnalyzeFieldAccesses(body, fieldNames);
            var complexity = EstimateComplexity(body);

            methods.Add(new MethodInfo(methodName, fieldAccessSet, complexity));
        }

        // Also extract auto-property getters/setters
        foreach (var propertyDeclaration in _classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
        {
            var propertyName = propertyDeclaration.Identifier.Text;

            // Check if property has a backing field and extract accesses
            if (propertyDeclaration.AccessorList != null)
            {
                var accessedFields = ExtractPropertyFieldAccesses(propertyDeclaration, fieldNames);
                if (accessedFields.Any())
                {
                    methods.Add(new MethodInfo($"get_{propertyName}", accessedFields));
                }
            }
        }

        return methods;
    }

    private HashSet<string> AnalyzeFieldAccesses(BlockSyntax methodBody, List<string> fieldNames)
    {
        var analyzer = new FieldAccessAnalyzer(fieldNames);
        analyzer.Visit(methodBody);
        return new HashSet<string>(analyzer.AccessedFields);
    }

    private HashSet<string> ExtractPropertyFieldAccesses(PropertyDeclarationSyntax property, List<string> fieldNames)
    {
        var accessedFields = new HashSet<string>();

        if (property.AccessorList == null)
            return accessedFields;

        foreach (var accessor in property.AccessorList.Accessors)
        {
            if (accessor.Body != null)
            {
                var analyzer = new FieldAccessAnalyzer(fieldNames);
                analyzer.Visit(accessor.Body);
                accessedFields.UnionWith(analyzer.AccessedFields);
            }
        }

        return accessedFields;
    }

    private int EstimateComplexity(BlockSyntax body)
    {
        // Cyclomatic complexity: count decision points (if, for, while, case, etc.)
        var complexity = 1; // Base complexity

        // Count if statements
        complexity += body.DescendantNodes().OfType<IfStatementSyntax>().Count();

        // Count loops (for, foreach, while, do-while)
        complexity += body.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += body.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        complexity += body.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += body.DescendantNodes().OfType<DoStatementSyntax>().Count();

        // Count switch cases
        complexity += body.DescendantNodes().OfType<SwitchSectionSyntax>().Count();

        // Count catch blocks
        complexity += body.DescendantNodes().OfType<CatchClauseSyntax>().Count();

        // Count ternary operators
        complexity += body.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();

        return complexity;
    }
}
