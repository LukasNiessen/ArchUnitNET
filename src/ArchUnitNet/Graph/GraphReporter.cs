using System.Text;
using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.GraphReporting;

/// <summary>
/// Exports dependency graphs in multiple formats.
/// Supports: Mermaid, DOT, D2, CSV, JSON, HTML
/// </summary>
public class GraphReporter
{
    private readonly IReadOnlyList<Edge> _edges;
    private bool _includeExternalDependencies = false;
    private int? _collapseToFolderDepth;
    private string? _focusOn;

    public GraphReporter(IReadOnlyList<Edge> edges)
    {
        _edges = edges ?? throw new ArgumentNullException(nameof(edges));
    }

    /// <summary>
    /// Include external dependencies (NuGet packages, system libraries) in the output.
    /// </summary>
    public GraphReporter IncludeExternalDependencies()
    {
        _includeExternalDependencies = true;
        return this;
    }

    /// <summary>
    /// Collapse paths to a specific folder depth.
    /// Example: depth=2 converts "src/Feature/Component.cs" to "src/Feature"
    /// </summary>
    public GraphReporter CollapseToFolderDepth(int depth)
    {
        if (depth < 1)
            throw new ArgumentException("Folder depth must be at least 1", nameof(depth));

        _collapseToFolderDepth = depth;
        return this;
    }

    /// <summary>
    /// Focus the graph on a specific file or folder.
    /// Shows only dependencies related to this target.
    /// </summary>
    public GraphReporter FocusOn(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        _focusOn = path;
        return this;
    }

    /// <summary>
    /// Export to Mermaid diagram format.
    /// </summary>
    public async Task<string> ExportToMermaidAsync()
    {
        var filteredEdges = FilterEdges();
        var nodes = ExtractNodes(filteredEdges);

        var mermaid = new StringBuilder();
        mermaid.AppendLine("graph TD");

        foreach (var edge in filteredEdges)
        {
            var source = SanitizeNodeId(edge.Source);
            var target = SanitizeNodeId(edge.Target);
            var label = edge.External ? "[external]" : "";

            mermaid.AppendLine($"    {source} -->|{label}| {target}");
        }

        return await Task.FromResult(mermaid.ToString());
    }

    /// <summary>
    /// Export to Graphviz DOT format.
    /// </summary>
    public async Task<string> ExportToDOTAsync()
    {
        var filteredEdges = FilterEdges();

        var dot = new StringBuilder();
        dot.AppendLine("digraph Dependencies {");
        dot.AppendLine("    rankdir=LR;");
        dot.AppendLine("    node [shape=box];");

        foreach (var edge in filteredEdges)
        {
            var source = SanitizeNodeId(edge.Source);
            var target = SanitizeNodeId(edge.Target);
            var style = edge.External ? "[color=gray, style=dashed]" : "";

            dot.AppendLine($"    {source} -> {target} {style};");
        }

        dot.AppendLine("}");

        return await Task.FromResult(dot.ToString());
    }

    /// <summary>
    /// Export to D2 diagram format.
    /// </summary>
    public async Task<string> ExportToD2Async()
    {
        var filteredEdges = FilterEdges();

        var d2 = new StringBuilder();

        foreach (var edge in filteredEdges)
        {
            var source = edge.Source;
            var target = edge.Target;

            if (edge.External)
            {
                d2.AppendLine($"{source} -> {target} {{style: dashed}}");
            }
            else
            {
                d2.AppendLine($"{source} -> {target}");
            }
        }

        return await Task.FromResult(d2.ToString());
    }

    /// <summary>
    /// Export to CSV format (source,target,external).
    /// </summary>
    public async Task<string> ExportToCSVAsync()
    {
        var filteredEdges = FilterEdges();

        var csv = new StringBuilder();
        csv.AppendLine("Source,Target,External,ImportKinds");

        foreach (var edge in filteredEdges)
        {
            var importKinds = string.Join("|", edge.ImportKinds);
            csv.AppendLine($"\"{edge.Source}\",\"{edge.Target}\",{edge.External},{importKinds}");
        }

        return await Task.FromResult(csv.ToString());
    }

    /// <summary>
    /// Export to JSON format.
    /// </summary>
    public async Task<string> ExportToJSONAsync()
    {
        var filteredEdges = FilterEdges();

        var json = new StringBuilder();
        json.AppendLine("{");
        json.AppendLine("  \"nodes\": [");

        var nodes = ExtractNodes(filteredEdges).ToList();
        for (int i = 0; i < nodes.Count; i++)
        {
            var comma = i < nodes.Count - 1 ? "," : "";
            json.AppendLine($"    {{ \"id\": \"{nodes[i]}\" }}{comma}");
        }

        json.AppendLine("  ],");
        json.AppendLine("  \"edges\": [");

        for (int i = 0; i < filteredEdges.Count; i++)
        {
            var edge = filteredEdges[i];
            var comma = i < filteredEdges.Count - 1 ? "," : "";
            json.AppendLine($"    {{");
            json.AppendLine($"      \"source\": \"{edge.Source}\",");
            json.AppendLine($"      \"target\": \"{edge.Target}\",");
            json.AppendLine($"      \"external\": {(edge.External ? "true" : "false")}");
            json.AppendLine($"    }}{comma}");
        }

        json.AppendLine("  ]");
        json.AppendLine("}");

        return await Task.FromResult(json.ToString());
    }

    /// <summary>
    /// Export to HTML with embedded SVG visualization.
    /// </summary>
    public async Task<string> ExportToHTMLAsync()
    {
        var mermaidDiagram = await ExportToMermaidAsync();

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("  <title>Dependency Graph</title>");
        html.AppendLine("  <script src=\"https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js\"></script>");
        html.AppendLine("  <style>");
        html.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("    .mermaid { border: 1px solid #ccc; padding: 20px; margin: 20px 0; }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("  <h1>Dependency Graph</h1>");
        html.AppendLine("  <div class=\"mermaid\">");
        html.Append(mermaidDiagram);
        html.AppendLine("  </div>");
        html.AppendLine("  <script>mermaid.contentLoaderInit();</script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    /// <summary>
    /// Export to a specific format.
    /// </summary>
    public async Task<string> ExportAsync(GraphExportFormat format)
    {
        return format switch
        {
            GraphExportFormat.Mermaid => await ExportToMermaidAsync(),
            GraphExportFormat.DOT => await ExportToDOTAsync(),
            GraphExportFormat.D2 => await ExportToD2Async(),
            GraphExportFormat.CSV => await ExportToCSVAsync(),
            GraphExportFormat.JSON => await ExportToJSONAsync(),
            GraphExportFormat.HTML => await ExportToHTMLAsync(),
            _ => throw new ArgumentException($"Unknown format: {format}", nameof(format))
        };
    }

    /// <summary>
    /// Export to file asynchronously.
    /// </summary>
    public async Task ExportToFileAsync(GraphExportFormat format, string filePath)
    {
        var content = await ExportAsync(format);
        await File.WriteAllTextAsync(filePath, content);
    }

    private IReadOnlyList<Edge> FilterEdges()
    {
        var edges = _edges.ToList();

        // Filter external dependencies
        if (!_includeExternalDependencies)
        {
            edges = edges.Where(e => !e.External).ToList();
        }

        // Apply focus filter
        if (!string.IsNullOrEmpty(_focusOn))
        {
            edges = edges.Where(e =>
                e.Source.Contains(_focusOn, StringComparison.OrdinalIgnoreCase) ||
                e.Target.Contains(_focusOn, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }

        // Apply folder depth collapse
        if (_collapseToFolderDepth.HasValue)
        {
            edges = edges.Select(e => new Edge(
                CollapsePath(e.Source, _collapseToFolderDepth.Value),
                CollapsePath(e.Target, _collapseToFolderDepth.Value),
                e.External,
                e.ImportKinds
            )).Distinct().ToList();
        }

        return edges.AsReadOnly();
    }

    private static IEnumerable<string> ExtractNodes(IEnumerable<Edge> edges)
    {
        var nodes = new HashSet<string>();

        foreach (var edge in edges)
        {
            nodes.Add(edge.Source);
            nodes.Add(edge.Target);
        }

        return nodes.OrderBy(n => n);
    }

    private static string CollapsePath(string path, int depth)
    {
        var parts = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length <= depth)
            return path;

        return string.Join("/", parts.Take(depth));
    }

    private static string SanitizeNodeId(string nodeName)
    {
        // Remove special characters and replace with underscores for safe node IDs
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            nodeName,
            @"[^\w\s]",
            "_"
        );

        return $"\"{sanitized}\"";
    }
}

/// <summary>
/// Supported graph export formats.
/// </summary>
public enum GraphExportFormat
{
    Mermaid,
    DOT,
    D2,
    CSV,
    JSON,
    HTML
}
