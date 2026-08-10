using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace ArchUnitNet.Reporting;

/// <summary>
/// Exports architecture violations in multiple report formats.
/// Supports HTML, SARIF, and JSON for CI/CD integration.
/// </summary>
public class ViolationReportExporter
{
    private readonly List<Violation> _violations;
    private readonly string _projectName;
    private readonly DateTime _reportDate;

    public ViolationReportExporter(List<Violation> violations, string projectName = "ArchUnitCSharp")
    {
        _violations = violations;
        _projectName = projectName;
        _reportDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Export violations as an interactive HTML report.
    /// </summary>
    public async Task ExportToHTMLAsync(string outputPath)
    {
        var html = GenerateHTMLReport();
        await File.WriteAllTextAsync(outputPath, html, Encoding.UTF8);
    }

    /// <summary>
    /// Export violations in SARIF format (Static Analysis Results Interchange Format).
    /// Compatible with GitHub, Azure DevOps, and other tools.
    /// </summary>
    public async Task ExportToSARIFAsync(string outputPath)
    {
        var sarif = GenerateSARIFReport();
        await File.WriteAllTextAsync(outputPath, sarif, Encoding.UTF8);
    }

    /// <summary>
    /// Export violations as JSON for programmatic processing.
    /// </summary>
    public async Task ExportToJSONAsync(string outputPath)
    {
        var json = GenerateJSONReport();
        await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);
    }

    /// <summary>
    /// Export violations as a detailed text report.
    /// </summary>
    public async Task ExportToTextAsync(string outputPath)
    {
        var text = GenerateTextReport();
        await File.WriteAllTextAsync(outputPath, text, Encoding.UTF8);
    }

    private string GenerateHTMLReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"  <title>{_projectName} - Architecture Violations Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    * { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; background: #f5f5f5; }");
        sb.AppendLine("    .container { max-width: 1200px; margin: 0 auto; padding: 20px; }");
        sb.AppendLine("    .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; border-radius: 8px; margin-bottom: 30px; }");
        sb.AppendLine("    .header h1 { font-size: 28px; margin-bottom: 10px; }");
        sb.AppendLine("    .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }");
        sb.AppendLine("    .stat-card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("    .stat-card h3 { color: #667eea; font-size: 14px; margin-bottom: 10px; text-transform: uppercase; }");
        sb.AppendLine("    .stat-card .value { font-size: 32px; font-weight: bold; color: #333; }");
        sb.AppendLine("    .violation { background: white; margin-bottom: 20px; border-left: 4px solid #dc3545; border-radius: 4px; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }");
        sb.AppendLine("    .violation.error { border-left-color: #dc3545; }");
        sb.AppendLine("    .violation.warning { border-left-color: #ffc107; }");
        sb.AppendLine("    .violation-header { background: #f8f9fa; padding: 15px; display: flex; justify-content: space-between; align-items: center; }");
        sb.AppendLine("    .violation-type { font-weight: bold; color: #495057; }");
        sb.AppendLine("    .violation-details { padding: 15px; }");
        sb.AppendLine("    .violation-details p { margin: 8px 0; }");
        sb.AppendLine("    .label { font-weight: 600; color: #667eea; }");
        sb.AppendLine("    code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-family: 'Courier New', monospace; }");
        sb.AppendLine("    .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 14px; text-align: center; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div class=\"container\">");

        // Header
        sb.AppendLine("    <div class=\"header\">");
        sb.AppendLine($"      <h1>{_projectName} Architecture Violations</h1>");
        sb.AppendLine($"      <p>Report generated on {_reportDate:yyyy-MM-dd HH:mm:ss UTC}</p>");
        sb.AppendLine("    </div>");

        // Statistics
        var errorCount = _violations.OfType<TechnicalError>().Count();
        var userErrorCount = _violations.OfType<UserError>().Count();
        var totalCount = _violations.Count;

        sb.AppendLine("    <div class=\"stats\">");
        sb.AppendLine("      <div class=\"stat-card\">");
        sb.AppendLine("        <h3>Total Violations</h3>");
        sb.AppendLine($"        <div class=\"value\">{totalCount}</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"stat-card\">");
        sb.AppendLine("        <h3>Technical Errors</h3>");
        sb.AppendLine($"        <div class=\"value\" style=\"color: #dc3545;\">{errorCount}</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("      <div class=\"stat-card\">");
        sb.AppendLine("        <h3>Validation Errors</h3>");
        sb.AppendLine($"        <div class=\"value\" style=\"color: #ffc107;\">{userErrorCount}</div>");
        sb.AppendLine("      </div>");
        sb.AppendLine("    </div>");

        // Violations
        if (totalCount > 0)
        {
            sb.AppendLine("    <h2>Violations Details</h2>");
            foreach (var violation in _violations)
            {
                var isError = violation is TechnicalError;
                sb.AppendLine($"    <div class=\"violation {(isError ? "error" : "warning")}\">");
                sb.AppendLine("      <div class=\"violation-header\">");
                sb.AppendLine($"        <span class=\"violation-type\">{violation.ViolationType}</span>");
                sb.AppendLine("      </div>");
                sb.AppendLine("      <div class=\"violation-details\">");
                sb.AppendLine($"        <p><span class=\"label\">Type:</span> {violation.GetType().Name}</p>");
                sb.AppendLine($"        <p><span class=\"label\">Message:</span> <code>{HtmlEncode(violation.ToString())}</code></p>");
                sb.AppendLine("      </div>");
                sb.AppendLine("    </div>");
            }
        }
        else
        {
            sb.AppendLine("    <div style=\"background: #d4edda; color: #155724; padding: 20px; border-radius: 8px; margin-top: 20px;\">");
            sb.AppendLine("      <strong>✓ No violations found!</strong> Your architecture is valid.");
            sb.AppendLine("    </div>");
        }

        // Footer
        sb.AppendLine("    <div class=\"footer\">");
        sb.AppendLine("      <p>Generated by ArchUnitCSharp v2.4.0 | <a href='https://github.com/LukasNiessen/ArchUnitNET'>GitHub</a></p>");
        sb.AppendLine("    </div>");

        sb.AppendLine("  </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    private string GenerateSARIFReport()
    {
        // SARIF (Static Analysis Results Interchange Format)
        // See: https://sarifweb.azurewebsites.net/
        var sarif = new
        {
            version = "2.1.0",
            runs = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "ArchUnitCSharp",
                            version = "2.4.0",
                            informationUri = "https://github.com/LukasNiessen/ArchUnitNET",
                            rules = Array.Empty<object>()
                        }
                    },
                    results = _violations.Select((v, i) => new
                    {
                        ruleId = $"ARCH-{(i + 1):D4}",
                        level = v is TechnicalError ? "error" : "warning",
                        message = new { text = v.ToString() },
                        locations = new[]
                        {
                            new
                            {
                                physicalLocation = new
                                {
                                    artifactLocation = new { uri = _projectName }
                                }
                            }
                        }
                    }).ToArray()
                }
            }
        };

        return JsonConvert.SerializeObject(sarif, Formatting.Indented);
    }

    private string GenerateJSONReport()
    {
        var report = new
        {
            project = _projectName,
            timestamp = _reportDate,
            summary = new
            {
                total = _violations.Count,
                errors = _violations.OfType<TechnicalError>().Count(),
                warnings = _violations.OfType<UserError>().Count()
            },
            violations = _violations.Select((v, i) => new
            {
                id = i + 1,
                type = v.ViolationType,
                message = v.ToString(),
                severity = v is TechnicalError ? "error" : "warning"
            }).ToArray()
        };

        return JsonConvert.SerializeObject(report, Formatting.Indented);
    }

    private string GenerateTextReport()
    {
        var sb = new StringBuilder();

        sb.AppendLine("================================================================================");
        sb.AppendLine($"  {_projectName} - Architecture Violations Report");
        sb.AppendLine($"  Generated: {_reportDate:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine("================================================================================");
        sb.AppendLine();

        sb.AppendLine("SUMMARY");
        sb.AppendLine("-------");
        sb.AppendLine($"Total Violations: {_violations.Count}");
        sb.AppendLine($"Errors:           {_violations.OfType<TechnicalError>().Count()}");
        sb.AppendLine($"Warnings:         {_violations.OfType<UserError>().Count()}");
        sb.AppendLine();

        if (_violations.Count > 0)
        {
            sb.AppendLine("DETAILS");
            sb.AppendLine("-------");
            for (int i = 0; i < _violations.Count; i++)
            {
                var violation = _violations[i];
                sb.AppendLine($"[{i + 1}] {violation.ViolationType}");
                sb.AppendLine($"    {violation}");
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("✓ No violations found! Your architecture is valid.");
            sb.AppendLine();
        }

        sb.AppendLine("================================================================================");
        sb.AppendLine("Generated by ArchUnitCSharp v2.4.0");
        sb.AppendLine("https://github.com/LukasNiessen/ArchUnitNET");

        return sb.ToString();
    }

    private static string HtmlEncode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}

/// <summary>
/// Report format options.
/// </summary>
public enum ReportFormat
{
    /// <summary>Interactive HTML report with styling</summary>
    HTML,

    /// <summary>SARIF format for CI/CD integration</summary>
    SARIF,

    /// <summary>JSON format for programmatic processing</summary>
    JSON,

    /// <summary>Plain text format for console output</summary>
    Text
}

/// <summary>
/// Extension methods for easy report exporting.
/// </summary>
public static class ViolationReportingExtensions
{
    /// <summary>
    /// Export violations to a report file.
    /// </summary>
    public static async Task ExportReportAsync(
        this IEnumerable<Violation> violations,
        ReportFormat format,
        string outputPath,
        string projectName = "ArchUnitCSharp")
    {
        var violationList = violations.ToList();
        var exporter = new ViolationReportExporter(violationList, projectName);

        switch (format)
        {
            case ReportFormat.HTML:
                await exporter.ExportToHTMLAsync(outputPath);
                break;

            case ReportFormat.SARIF:
                await exporter.ExportToSARIFAsync(outputPath);
                break;

            case ReportFormat.JSON:
                await exporter.ExportToJSONAsync(outputPath);
                break;

            case ReportFormat.Text:
                await exporter.ExportToTextAsync(outputPath);
                break;

            default:
                throw new ArgumentException($"Unknown report format: {format}");
        }
    }
}
