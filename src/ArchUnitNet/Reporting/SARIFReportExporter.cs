using ArchUnitNet.Common.Assertion;
using Newtonsoft.Json.Linq;

namespace ArchUnitNet.Reporting;

/// <summary>
/// Exports violations in SARIF (Static Analysis Results Format) 2.1.0 for CI/CD integration.
/// SARIF is widely supported by GitHub, Azure DevOps, and other CI/CD platforms.
/// </summary>
public class SARIFReportExporter
{
    private readonly IEnumerable<Violation> _violations;
    private readonly string _projectName;

    public SARIFReportExporter(IEnumerable<Violation> violations, string projectName = "ArchUnit Analysis")
    {
        _violations = violations ?? throw new ArgumentNullException(nameof(violations));
        _projectName = projectName ?? "ArchUnit Analysis";
    }

    /// <summary>
    /// Export violations to SARIF 2.1.0 JSON format.
    /// </summary>
    public async Task<string> ExportAsync()
    {
        var violations = _violations.ToList();

        var results = new JArray();

        for (int i = 0; i < violations.Count; i++)
        {
            var violation = violations[i];
            var result = new JObject
            {
                ["ruleId"] = GenerateRuleId(violation),
                ["ruleIndex"] = i,
                ["kind"] = "fail",
                ["level"] = "error",
                ["message"] = new JObject
                {
                    ["text"] = violation.ToString() ?? "Architecture violation detected",
                },
                ["locations"] = new JArray(new JObject
                {
                    ["physicalLocation"] = new JObject
                    {
                        ["artifactLocation"] = new JObject
                        {
                            ["uri"] = GetViolationUri(violation),
                        },
                        ["region"] = new JObject
                        {
                            ["startLine"] = 1,
                            ["startColumn"] = 1,
                        },
                    },
                }),
            };

            results.Add(result);
        }

        var sarifLog = new JObject
        {
            ["version"] = "2.1.0",
            ["$schema"] = "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/master/Schemata/sarif-schema-2.1.0.json",
            ["runs"] = new JArray(new JObject
            {
                ["tool"] = new JObject
                {
                    ["driver"] = new JObject
                    {
                        ["name"] = "ArchUnit",
                        ["version"] = GetVersionNumber(),
                        ["informationUri"] = "https://github.com/LukasNiessen/ArchUnitNET",
                        ["rules"] = GenerateRules(violations),
                    },
                },
                ["results"] = results,
                ["properties"] = new JObject
                {
                    ["projectName"] = _projectName,
                    ["totalViolations"] = violations.Count,
                    ["analysisTimestamp"] = DateTime.UtcNow.ToString("O"),
                },
            }),
        };

        return await Task.FromResult(sarifLog.ToString(Newtonsoft.Json.Formatting.Indented));
    }

    /// <summary>
    /// Export violations to SARIF file.
    /// </summary>
    public async Task ExportToFileAsync(string filePath)
    {
        var content = await ExportAsync();
        await File.WriteAllTextAsync(filePath, content);
    }

    private JArray GenerateRules(IEnumerable<Violation> violations)
    {
        var rules = new JArray();
        var ruleIdSet = new HashSet<string>();

        foreach (var violation in violations)
        {
            var ruleId = GenerateRuleId(violation);

            if (!ruleIdSet.Contains(ruleId))
            {
                ruleIdSet.Add(ruleId);

                var rule = new JObject
                {
                    ["id"] = ruleId,
                    ["shortDescription"] = new JObject
                    {
                        ["text"] = GetRuleDescription(violation),
                    },
                    ["fullDescription"] = new JObject
                    {
                        ["text"] = GetRuleFullDescription(violation),
                    },
                    ["defaultConfiguration"] = new JObject
                    {
                        ["level"] = "error",
                    },
                    ["helpUri"] = $"https://github.com/LukasNiessen/ArchUnitNET/wiki/{ruleId}",
                };

                rules.Add(rule);
            }
        }

        return rules;
    }

    private string GenerateRuleId(Violation violation)
    {
        var violationType = violation.GetType().Name;
        return violationType
            .Replace("Violation", "")
            .Replace("Dependency", "Dep")
            .ToUpper();
    }

    private string GetRuleDescription(Violation violation)
    {
        var violationType = violation.GetType().Name;
        return violationType switch
        {
            nameof(ArchUnitNet.Files.Assertion.ViolatingFileDependency) => "File Dependency Violation",
            nameof(ArchUnitNet.Files.Assertion.CyclicDependency) => "Cyclic Dependency Detected",
            _ => $"{violationType} Violation",
        };
    }

    private string GetRuleFullDescription(Violation violation)
    {
        var violationType = violation.GetType().Name;
        return violationType switch
        {
            nameof(ArchUnitNet.Files.Assertion.ViolatingFileDependency) =>
                "A file violates the defined dependency rules by depending on a forbidden module or layer.",
            nameof(ArchUnitNet.Files.Assertion.CyclicDependency) =>
                "A cyclic dependency was detected in the architecture. This creates tight coupling and can lead to maintenance issues.",
            _ => $"Architecture violation of type {violationType} was detected.",
        };
    }

    private string GetViolationUri(Violation violation)
    {
        if (violation is ArchUnitNet.Files.Assertion.ViolatingFileDependency dep)
        {
            return dep.Source ?? "unknown";
        }

        if (violation is ArchUnitNet.Files.Assertion.CyclicDependency cycle)
        {
            return cycle.Cycle.FirstOrDefault() ?? "unknown";
        }

        return "unknown";
    }

    private string GetVersionNumber()
    {
        var version = typeof(ArchUnit).Assembly.GetName().Version;
        return version?.ToString() ?? "1.0.0";
    }
}

/// <summary>
/// Extension methods for SARIF export.
/// </summary>
public static class SARIFExportExtensions
{
    /// <summary>
    /// Export violations to SARIF format.
    /// </summary>
    public static async Task<string> ExportToSARIFAsync(
        this IEnumerable<Violation> violations,
        string projectName = "ArchUnit Analysis")
    {
        var exporter = new SARIFReportExporter(violations, projectName);
        return await exporter.ExportAsync();
    }

    /// <summary>
    /// Export violations to SARIF file.
    /// </summary>
    public static async Task ExportToSARIFFileAsync(
        this IEnumerable<Violation> violations,
        string filePath,
        string projectName = "ArchUnit Analysis")
    {
        var exporter = new SARIFReportExporter(violations, projectName);
        await exporter.ExportToFileAsync(filePath);
    }
}
