using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.GraphReporting;

/// <summary>
/// Builds and configures dependency graphs for visualization and analysis.
/// Provides a fluent API for graph customization and export.
/// </summary>
public class ProjectGraphBuilder
{
    private readonly List<Edge> _edges = new();
    private GraphReporter? _reporter;

    /// <summary>
    /// Add edges (dependencies) to the graph.
    /// </summary>
    public ProjectGraphBuilder AddEdges(IEnumerable<Edge> edges)
    {
        if (edges == null)
            throw new ArgumentNullException(nameof(edges));

        _edges.AddRange(edges);
        _reporter = null; // Invalidate cached reporter

        return this;
    }

    /// <summary>
    /// Add a single edge to the graph.
    /// </summary>
    public ProjectGraphBuilder AddEdge(Edge edge)
    {
        if (edge == null)
            throw new ArgumentNullException(nameof(edge));

        _edges.Add(edge);
        _reporter = null; // Invalidate cached reporter

        return this;
    }

    /// <summary>
    /// Get the underlying GraphReporter for customization.
    /// </summary>
    public GraphReporter GetReporter()
    {
        _reporter ??= new GraphReporter(_edges.AsReadOnly());
        return _reporter;
    }

    /// <summary>
    /// Include external dependencies in the graph.
    /// </summary>
    public ProjectGraphBuilder IncludeExternalDependencies()
    {
        GetReporter().IncludeExternalDependencies();
        return this;
    }

    /// <summary>
    /// Collapse paths to a specific folder depth.
    /// </summary>
    public ProjectGraphBuilder CollapseToFolderDepth(int depth)
    {
        GetReporter().CollapseToFolderDepth(depth);
        return this;
    }

    /// <summary>
    /// Focus the graph on a specific file or folder.
    /// </summary>
    public ProjectGraphBuilder FocusOn(string path)
    {
        GetReporter().FocusOn(path);
        return this;
    }

    /// <summary>
    /// Export the graph to Mermaid format.
    /// </summary>
    public async Task<string> ExportToMermaidAsync()
    {
        return await GetReporter().ExportToMermaidAsync();
    }

    /// <summary>
    /// Export the graph to DOT format.
    /// </summary>
    public async Task<string> ExportToDOTAsync()
    {
        return await GetReporter().ExportToDOTAsync();
    }

    /// <summary>
    /// Export the graph to D2 format.
    /// </summary>
    public async Task<string> ExportToD2Async()
    {
        return await GetReporter().ExportToD2Async();
    }

    /// <summary>
    /// Export the graph to CSV format.
    /// </summary>
    public async Task<string> ExportToCSVAsync()
    {
        return await GetReporter().ExportToCSVAsync();
    }

    /// <summary>
    /// Export the graph to JSON format.
    /// </summary>
    public async Task<string> ExportToJSONAsync()
    {
        return await GetReporter().ExportToJSONAsync();
    }

    /// <summary>
    /// Export the graph to HTML format.
    /// </summary>
    public async Task<string> ExportToHTMLAsync()
    {
        return await GetReporter().ExportToHTMLAsync();
    }

    /// <summary>
    /// Export the graph to a specific format.
    /// </summary>
    public async Task<string> ExportAsync(GraphExportFormat format)
    {
        return await GetReporter().ExportAsync(format);
    }

    /// <summary>
    /// Export the graph to a file.
    /// </summary>
    public async Task ExportToFileAsync(GraphExportFormat format, string filePath)
    {
        await GetReporter().ExportToFileAsync(format, filePath);
    }

    /// <summary>
    /// Get the number of nodes in the graph.
    /// </summary>
    public int GetNodeCount()
    {
        var nodes = new HashSet<string>();
        foreach (var edge in _edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }
        return nodes.Count;
    }

    /// <summary>
    /// Get the number of edges in the graph.
    /// </summary>
    public int GetEdgeCount() => _edges.Count;

    /// <summary>
    /// Get all edges in the graph.
    /// </summary>
    public IReadOnlyList<Edge> GetEdges() => _edges.AsReadOnly();
}
