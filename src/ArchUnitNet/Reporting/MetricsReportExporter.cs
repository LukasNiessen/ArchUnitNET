using System.Text;
using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Reporting;

/// <summary>
/// Exports code metrics reports in HTML format for visualization.
/// Includes charts, tables, and interactive visualizations.
/// </summary>
public class MetricsReportExporter
{
    private readonly IEnumerable<Violation> _violations;
    private readonly string _projectName;
    private readonly Dictionary<string, object> _customData;

    public MetricsReportExporter(IEnumerable<Violation> violations, string projectName = "Code Metrics Analysis")
    {
        _violations = violations ?? throw new ArgumentNullException(nameof(violations));
        _projectName = projectName;
        _customData = new Dictionary<string, object>();
    }

    /// <summary>
    /// Add custom data to the report.
    /// </summary>
    public MetricsReportExporter WithData(string key, object value)
    {
        _customData[key] = value;
        return this;
    }

    /// <summary>
    /// Export metrics as an interactive HTML report.
    /// </summary>
    public async Task<string> ExportAsHtmlAsync()
    {
        var violations = _violations.ToList();
        var severityStats = CalculateSeverityStats(violations);
        var typeStats = CalculateTypeStats(violations);

        var html = GenerateHtmlReport(violations, severityStats, typeStats);

        return await Task.FromResult(html);
    }

    /// <summary>
    /// Export and save to file.
    /// </summary>
    public async Task ExportToFileAsync(string filePath)
    {
        var html = await ExportAsHtmlAsync();
        await File.WriteAllTextAsync(filePath, html);
    }

    private string GenerateHtmlReport(List<Violation> violations, Dictionary<string, int> severityStats, Dictionary<string, int> typeStats)
    {
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"UTF-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        html.AppendLine($"  <title>{HtmlEncode(_projectName)} - Metrics Report</title>");
        html.AppendLine("  <script src=\"https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js\"></script>");
        html.AppendLine("  <style>");
        html.AppendLine(GetCssStyles());
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Header
        html.AppendLine("  <div class=\"header\">");
        html.AppendLine($"    <h1>📊 {HtmlEncode(_projectName)}</h1>");
        html.AppendLine("    <p class=\"subtitle\">Code Metrics & Quality Analysis Report</p>");
        html.AppendLine("  </div>");

        // Summary Statistics
        html.AppendLine("  <div class=\"summary\">");
        html.AppendLine($"    <div class=\"stat-box\">");
        html.AppendLine($"      <div class=\"stat-value\">{violations.Count}</div>");
        html.AppendLine($"      <div class=\"stat-label\">Total Violations</div>");
        html.AppendLine($"    </div>");
        html.AppendLine($"    <div class=\"stat-box\">");
        html.AppendLine($"      <div class=\"stat-value\">{severityStats.Keys.Count}</div>");
        html.AppendLine($"      <div class=\"stat-label\">Severity Levels</div>");
        html.AppendLine($"    </div>");
        html.AppendLine($"    <div class=\"stat-box\">");
        html.AppendLine($"      <div class=\"stat-value\">{typeStats.Keys.Count}</div>");
        html.AppendLine($"      <div class=\"stat-label\">Violation Types</div>");
        html.AppendLine($"    </div>");
        html.AppendLine($"    <div class=\"stat-box\">");
        html.AppendLine($"      <div class=\"stat-value\">{DateTime.Now:yyyy-MM-dd HH:mm:ss}</div>");
        html.AppendLine($"      <div class=\"stat-label\">Analysis Time</div>");
        html.AppendLine($"    </div>");
        html.AppendLine("  </div>");

        // Charts
        html.AppendLine("  <div class=\"charts\">");
        html.AppendLine("    <div class=\"chart-container\">");
        html.AppendLine("      <h2>Severity Distribution</h2>");
        html.AppendLine("      <canvas id=\"severityChart\"></canvas>");
        html.AppendLine("    </div>");
        html.AppendLine("    <div class=\"chart-container\">");
        html.AppendLine("      <h2>Violation Types</h2>");
        html.AppendLine("      <canvas id=\"typeChart\"></canvas>");
        html.AppendLine("    </div>");
        html.AppendLine("  </div>");

        // Violations Table
        html.AppendLine("  <div class=\"violations-section\">");
        html.AppendLine("    <h2>Violations Details</h2>");
        html.AppendLine("    <table class=\"violations-table\">");
        html.AppendLine("      <thead>");
        html.AppendLine("        <tr>");
        html.AppendLine("          <th>Type</th>");
        html.AppendLine("          <th>Message</th>");
        html.AppendLine("          <th>Count</th>");
        html.AppendLine("        </tr>");
        html.AppendLine("      </thead>");
        html.AppendLine("      <tbody>");

        foreach (var type in typeStats.OrderByDescending(x => x.Value))
        {
            var typeViolations = violations.Where(v => v.GetType().Name == type.Key).ToList();
            html.AppendLine("        <tr>");
            html.AppendLine($"          <td><span class=\"type-badge\">{HtmlEncode(type.Key)}</span></td>");
            html.AppendLine($"          <td>{HtmlEncode(typeViolations.FirstOrDefault()?.ToString() ?? "")}</td>");
            html.AppendLine($"          <td class=\"count\">{type.Value}</td>");
            html.AppendLine("        </tr>");
        }

        html.AppendLine("      </tbody>");
        html.AppendLine("    </table>");
        html.AppendLine("  </div>");

        // Chart Data Script
        html.AppendLine("  <script>");
        html.AppendLine("    // Severity Chart");
        html.AppendLine("    const severityCtx = document.getElementById('severityChart').getContext('2d');");
        html.AppendLine("    new Chart(severityCtx, {");
        html.AppendLine("      type: 'doughnut',");
        html.Append("      data: { labels: [");
        html.Append(string.Join(",", severityStats.Keys.Select(k => $"'{HtmlEncode(k)}'")));
        html.AppendLine("],");
        html.Append("        datasets: [{ data: [");
        html.Append(string.Join(",", severityStats.Values));
        html.AppendLine("],");
        html.AppendLine("          backgroundColor: ['#28a745', '#ffc107', '#dc3545', '#6c757d'],");
        html.AppendLine("          borderColor: '#fff',");
        html.AppendLine("          borderWidth: 2");
        html.AppendLine("        }]");
        html.AppendLine("      },");
        html.AppendLine("      options: { responsive: true, maintainAspectRatio: true }");
        html.AppendLine("    });");

        html.AppendLine("    // Type Chart");
        html.AppendLine("    const typeCtx = document.getElementById('typeChart').getContext('2d');");
        html.AppendLine("    new Chart(typeCtx, {");
        html.AppendLine("      type: 'bar',");
        html.Append("      data: { labels: [");
        html.Append(string.Join(",", typeStats.Keys.Select(k => $"'{HtmlEncode(k)}'")));
        html.AppendLine("],");
        html.Append("        datasets: [{ label: 'Violations', data: [");
        html.Append(string.Join(",", typeStats.Values));
        html.AppendLine("],");
        html.AppendLine("          backgroundColor: '#007bff',");
        html.AppendLine("          borderColor: '#0056b3',");
        html.AppendLine("          borderWidth: 1");
        html.AppendLine("        }]");
        html.AppendLine("      },");
        html.AppendLine("      options: {");
        html.AppendLine("        responsive: true,");
        html.AppendLine("        indexAxis: 'y',");
        html.AppendLine("        scales: { x: { beginAtZero: true } }");
        html.AppendLine("      }");
        html.AppendLine("    });");
        html.AppendLine("  </script>");

        // Footer
        html.AppendLine("  <div class=\"footer\">");
        html.AppendLine("    <p>Generated by ArchUnitNET - Code Quality & Architecture Testing</p>");
        html.AppendLine("  </div>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private string GetCssStyles()
    {
        return @"
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
          font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
          background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
          min-height: 100vh;
          padding: 20px;
        }
        .header {
          background: white;
          padding: 40px;
          border-radius: 8px;
          margin-bottom: 30px;
          box-shadow: 0 4px 6px rgba(0,0,0,0.1);
          text-align: center;
        }
        .header h1 { color: #333; font-size: 2.5em; margin-bottom: 10px; }
        .subtitle { color: #666; font-size: 1.2em; }
        .summary {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 20px;
          margin-bottom: 30px;
        }
        .stat-box {
          background: white;
          padding: 30px;
          border-radius: 8px;
          text-align: center;
          box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }
        .stat-value {
          font-size: 2.5em;
          font-weight: bold;
          color: #667eea;
          margin-bottom: 10px;
        }
        .stat-label {
          color: #666;
          font-size: 0.9em;
          text-transform: uppercase;
          letter-spacing: 1px;
        }
        .charts {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
          gap: 20px;
          margin-bottom: 30px;
        }
        .chart-container {
          background: white;
          padding: 20px;
          border-radius: 8px;
          box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        }
        .chart-container h2 {
          margin-bottom: 20px;
          color: #333;
          font-size: 1.3em;
        }
        .violations-section {
          background: white;
          padding: 20px;
          border-radius: 8px;
          box-shadow: 0 4px 6px rgba(0,0,0,0.1);
          margin-bottom: 30px;
        }
        .violations-section h2 {
          color: #333;
          margin-bottom: 20px;
          font-size: 1.3em;
        }
        .violations-table {
          width: 100%;
          border-collapse: collapse;
        }
        .violations-table thead {
          background: #f8f9fa;
          border-bottom: 2px solid #dee2e6;
        }
        .violations-table th {
          padding: 12px;
          text-align: left;
          font-weight: 600;
          color: #333;
        }
        .violations-table td {
          padding: 12px;
          border-bottom: 1px solid #dee2e6;
        }
        .violations-table tr:hover {
          background: #f9f9f9;
        }
        .type-badge {
          display: inline-block;
          padding: 4px 8px;
          background: #e7f3ff;
          color: #007bff;
          border-radius: 4px;
          font-weight: 600;
          font-size: 0.85em;
        }
        .count {
          font-weight: 600;
          color: #667eea;
        }
        .footer {
          text-align: center;
          color: white;
          margin-top: 40px;
          font-size: 0.9em;
        }
        @media (max-width: 768px) {
          .header { padding: 20px; }
          .header h1 { font-size: 1.5em; }
          .charts { grid-template-columns: 1fr; }
          .violations-table { font-size: 0.9em; }
        }
";
    }

    private Dictionary<string, int> CalculateSeverityStats(List<Violation> violations)
    {
        var stats = new Dictionary<string, int>
        {
            ["Critical"] = violations.Count(v => v.GetType().Name == "CyclicDependency"),
            ["Error"] = violations.Count(v => v.GetType().Name == "ViolatingFileDependency"),
            ["Warning"] = violations.Count(v => !new[] { "CyclicDependency", "ViolatingFileDependency" }.Contains(v.GetType().Name)),
        };

        return stats.Where(x => x.Value > 0).ToDictionary(x => x.Key, x => x.Value);
    }

    private Dictionary<string, int> CalculateTypeStats(List<Violation> violations)
    {
        return violations
            .GroupBy(v => v.GetType().Name)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private string HtmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

/// <summary>
/// Extension methods for metrics reporting.
/// </summary>
public static class MetricsReportingExtensions
{
    /// <summary>
    /// Export violations as HTML metrics report.
    /// </summary>
    public static async Task<string> ExportMetricsReportAsync(
        this IEnumerable<Violation> violations,
        string projectName = "Code Metrics Analysis")
    {
        var exporter = new MetricsReportExporter(violations, projectName);
        return await exporter.ExportAsHtmlAsync();
    }

    /// <summary>
    /// Export and save metrics report to file.
    /// </summary>
    public static async Task ExportMetricsReportToFileAsync(
        this IEnumerable<Violation> violations,
        string filePath,
        string projectName = "Code Metrics Analysis")
    {
        var exporter = new MetricsReportExporter(violations, projectName);
        await exporter.ExportToFileAsync(filePath);
    }
}
