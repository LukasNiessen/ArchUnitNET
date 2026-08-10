using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArchUnitNet.Metrics.Common;

namespace ArchUnitNet.Metrics.Extraction;

/// <summary>
/// Extracts class information from multiple C# files or syntax trees.
/// Batch processor for large codebases.
/// </summary>
public class ClassInfoBatchExtractor
{
    private readonly List<ClassInfo> _extractedClasses = new();

    /// <summary>
    /// Extract from a C# source file.
    /// </summary>
    public void ExtractFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        if (!filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("File must be a C# source file (.cs)");

        var source = File.ReadAllText(filePath);
        ExtractFromSource(source, filePath);
    }

    /// <summary>
    /// Extract from C# source code string.
    /// </summary>
    public void ExtractFromSource(string source, string? filePath = null)
    {
        try
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            ExtractFromSyntaxTree(root, filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse C# source from {filePath ?? "unknown"}. Error: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract from a syntax tree.
    /// </summary>
    public void ExtractFromSyntaxTree(CompilationUnitSyntax root, string? filePath = null)
    {
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

        foreach (var classDeclaration in classDeclarations)
        {
            var extractor = new ClassInfoExtractor(classDeclaration, filePath);
            var classInfo = extractor.Extract();
            _extractedClasses.Add(classInfo);
        }
    }

    /// <summary>
    /// Extract from multiple files.
    /// </summary>
    public void ExtractFromDirectory(string directoryPath, bool recursive = true)
    {
        if (!Directory.Exists(directoryPath))
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

        var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var csFiles = Directory.GetFiles(directoryPath, "*.cs", searchOption);

        foreach (var filePath in csFiles)
        {
            try
            {
                ExtractFromFile(filePath);
            }
            catch (Exception ex)
            {
                // Log and continue with next file
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to extract from {filePath}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Get all extracted class information.
    /// </summary>
    public IReadOnlyList<ClassInfo> GetExtractedClasses() => _extractedClasses.AsReadOnly();

    /// <summary>
    /// Get extracted class by name, or null if not found.
    /// </summary>
    public ClassInfo? GetClass(string className) => _extractedClasses.FirstOrDefault(c => c.Name == className);

    /// <summary>
    /// Clear all extracted data.
    /// </summary>
    public void Clear() => _extractedClasses.Clear();

    /// <summary>
    /// Get summary statistics about extracted classes.
    /// </summary>
    public ExtractionSummary GetSummary()
    {
        return new ExtractionSummary(
            ClassCount: _extractedClasses.Count,
            TotalMethods: _extractedClasses.Sum(c => c.MethodCount),
            TotalFields: _extractedClasses.Sum(c => c.FieldCount),
            AverageMethodsPerClass: _extractedClasses.Count > 0 ? _extractedClasses.Average(c => c.MethodCount) : 0
        );
    }
}

/// <summary>
/// Summary statistics for batch extraction.
/// </summary>
public record ExtractionSummary(
    int ClassCount,
    int TotalMethods,
    int TotalFields,
    double AverageMethodsPerClass
);
