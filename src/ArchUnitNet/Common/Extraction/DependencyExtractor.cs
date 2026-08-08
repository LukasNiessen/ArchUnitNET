using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// Extracts a dependency graph from a C# project by analyzing source code.
///
/// This is the ONLY C#-specific code in the library.
/// Everything downstream (projection, assertion, reporting) is language-agnostic.
///
/// The extractor:
/// 1. Finds the .csproj file (via project path or auto-discovery)
/// 2. Loads all C# source files
/// 3. Parses each file with Roslyn to find using statements
/// 4. Resolves import targets (internal files or external packages)
/// 5. Returns a graph of Edge objects
///
/// In ArchUnitTS: extractGraph() in src/common/extraction/extract-graph.ts
/// </summary>
public interface IDependencyExtractor
{
    /// <summary>
    /// Extract a dependency graph from a C# project.
    /// </summary>
    /// <param name="projectPath">Path to .csproj file or project directory.
    /// If null, auto-discovers from current directory.
    /// </param>
    /// <returns>A graph of edges representing all dependencies</returns>
    Task<Graph> ExtractGraphAsync(string? projectPath = null);
}

/// <summary>
/// Default implementation of IDependencyExtractor using Roslyn.
/// </summary>
public class DependencyExtractor : IDependencyExtractor
{
    private static readonly Dictionary<string, Graph> ExtractedGraphs = new();
    private readonly IDependencyExtractorLogger _logger;

    public DependencyExtractor(IDependencyExtractorLogger? logger = null)
    {
        _logger = logger ?? new NullExtractorLogger();
    }

    public async Task<Graph> ExtractGraphAsync(string? projectPath = null)
    {
        projectPath = ResolveProjectPath(projectPath);

        // Check cache
        if (ExtractedGraphs.TryGetValue(projectPath, out var cached))
        {
            _logger.LogCacheHit(projectPath);
            return cached;
        }

        _logger.LogExtractionStart(projectPath);

        try
        {
            var graph = await ExtractAsync(projectPath);
            graph.Validate();

            // Cache result
            ExtractedGraphs[projectPath] = graph;

            _logger.LogExtractionComplete(projectPath, graph.Edges.Count);
            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogExtractionError(projectPath, ex);
            throw;
        }
    }

    /// <summary>
    /// Clear the extraction cache (useful when codebase changes during testing).
    /// </summary>
    public static void ClearCache()
    {
        ExtractedGraphs.Clear();
    }

    private static string ResolveProjectPath(string? projectPath)
    {
        if (!string.IsNullOrEmpty(projectPath))
            return PathNormalizer.Normalize(projectPath);

        // Auto-discover: walk up from current directory looking for .csproj
        var current = Environment.CurrentDirectory;
        while (current != null)
        {
            var csproj = Directory.GetFiles(current, "*.csproj").FirstOrDefault();
            if (csproj != null)
                return PathNormalizer.Normalize(csproj);

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException(
            "Could not find .csproj file. Provide explicit project path or run from project directory.");
    }

    private async Task<Graph> ExtractAsync(string projectPath)
    {
        var graph = new Graph();

        // 1. Find all source files in the project
        var sourceFiles = ProjectFileParser.FindSourceFiles(projectPath);
        _logger.LogCacheHit($"Found {sourceFiles.Count} source files");

        if (sourceFiles.Count == 0)
            return graph;

        // 2. Get project directory for relative path calculation
        var projectDir = Path.GetDirectoryName(projectPath)
            ?? throw new InvalidOperationException("Could not determine project directory");

        var analyzer = new SyntaxTreeAnalyzer();

        // 3. Analyze each file for imports
        foreach (var sourceFile in sourceFiles)
        {
            var fullPath = Path.Combine(projectDir, sourceFile);

            if (!File.Exists(fullPath))
                continue;

            try
            {
                var content = await File.ReadAllTextAsync(fullPath);
                var imports = analyzer.ExtractImportsFromFile(sourceFile, content);

                // 4. Create edges from imports
                foreach (var (namespaceName, kind) in imports)
                {
                    // For now: target is the namespace name itself
                    // TODO: Resolve to actual file paths for internal namespaces
                    var isExternal = analyzer.IsExternalNamespace(namespaceName);

                    var edge = new Edge(
                        Source: sourceFile,
                        Target: namespaceName,
                        External: isExternal,
                        ImportKinds: new[] { kind });

                    graph.Add(edge);
                }
            }
            catch (Exception ex)
            {
                _logger.LogExtractionError(sourceFile, ex);
                throw;
            }
        }

        return await Task.FromResult(graph);
    }
}

/// <summary>
/// Logger interface for extraction process.
/// Allows injecting custom logging behavior.
/// </summary>
public interface IDependencyExtractorLogger
{
    void LogExtractionStart(string projectPath);
    void LogExtractionComplete(string projectPath, int edgeCount);
    void LogExtractionError(string projectPath, Exception ex);
    void LogCacheHit(string projectPath);
}

/// <summary>
/// No-op logger (default).
/// </summary>
internal class NullExtractorLogger : IDependencyExtractorLogger
{
    public void LogExtractionStart(string projectPath) { }
    public void LogExtractionComplete(string projectPath, int edgeCount) { }
    public void LogExtractionError(string projectPath, Exception ex) { }
    public void LogCacheHit(string projectPath) { }
}
