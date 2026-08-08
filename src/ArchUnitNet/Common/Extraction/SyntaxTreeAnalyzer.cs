using ArchUnitNet.Common.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// Analyzes C# files and extracts dependencies from their syntax trees.
/// Works at the file level — for each source file, finds all imports.
///
/// This bridges:
/// - Input: List of source files
/// - Processing: Roslyn syntax tree analysis
/// - Output: List of Edge objects
/// </summary>
public class SyntaxTreeAnalyzer
{
    /// <summary>
    /// Analyze a single C# source file and extract all dependencies.
    /// </summary>
    /// <param name="filePath">Normalized path to the .cs file (e.g., "src/Common/Error.cs")</param>
    /// <param name="fileContent">Content of the C# file</param>
    /// <returns>List of edges from this file to its imports</returns>
    public List<(string ImportedNamespace, ImportKind Kind)> ExtractImportsFromFile(
        string filePath,
        string fileContent)
    {
        if (string.IsNullOrEmpty(fileContent))
            return new List<(string, ImportKind)>();

        try
        {
            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();

            var extractor = new UsingStatementExtractor();
            extractor.Visit(root);

            return extractor.GetImports()
                .Select(import => (import.Name, import.Kind))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse C# file '{filePath}': {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Determine if a namespace is internal (belongs to the project) or external.
    /// Heuristic: if it starts with a known external prefix, it's external.
    /// Example: "System.*", "Microsoft.*" are external.
    /// </summary>
    public bool IsExternalNamespace(string namespaceName)
    {
        if (string.IsNullOrEmpty(namespaceName))
            return false;

        // Common external namespaces
        var externalPrefixes = new[]
        {
            "System",
            "Microsoft",
            "Windows",
            "Newtonsoft",
            "log4net",
            "NUnit",
            "xunit",
            "Moq",
            "FluentAssertions",
        };

        return externalPrefixes.Any(prefix =>
            namespaceName.StartsWith(prefix, StringComparison.Ordinal));
    }
}
