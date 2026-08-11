using ArchUnitNet;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Presets;

namespace ArchUnitNet.Dogfood;

/// <summary>
/// Architecture validation rules for ArchUnitNET itself.
/// Demonstrates best practices and validates our own architecture.
/// </summary>
public static class ArchitectureRules
{
    /// <summary>
    /// Build all architecture rules that ArchUnitNET should follow.
    /// </summary>
    public static IEnumerable<Checkable> GetAllRules()
    {
        var projectPath = GetProjectPath();

        return new Checkable[]
        {
            // Core layering: Common is independent
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Common/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Files/**"),

            // Files depends on Common, not on Slices or Metrics
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Files/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Slices/**"),

            // Slices depends on Common and Projection, not on Files or Metrics
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Slices/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Metrics/**"),

            // Metrics depends on Common and Extraction
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Metrics/**")
                .Should()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Common/**"),

            // No circular dependencies in core
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Common/**")
                .Should()
                .HaveNoCycles(),

            // No circular dependencies in Files
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Files/**")
                .Should()
                .HaveNoCycles(),

            // No circular dependencies in entire library
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/**")
                .Should()
                .HaveNoCycles(),

            // Testing namespace should only depend on core
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Testing/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Metrics/**"),

            // Reporting should not depend on specific rule implementations
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Reporting/**")
                .Should()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Common/**"),

            // Presets should compose rules, not implement them
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Presets/**")
                .Should()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/**"),

            // Configuration should be independent
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Configuration/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Files/**"),

            // Performance should only depend on Common
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Performance/**")
                .Should()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Common/**"),

            // Baseline should only depend on Common
            ArchUnit.ProjectFiles(projectPath)
                .InPath("src/ArchUnitNet/Baseline/**")
                .Should()
                .DependOnFiles()
                .InPath("src/ArchUnitNet/Common/**"),
        };
    }

    /// <summary>
    /// Validate all architecture rules.
    /// </summary>
    public static async Task<ArchitectureValidationResult> ValidateAsync()
    {
        var result = new ArchitectureValidationResult();
        var rules = GetAllRules().ToList();

        Console.WriteLine($"🏗️  Validating {rules.Count} architecture rules for ArchUnitNET...");
        Console.WriteLine();

        foreach (var (index, rule) in rules.Select((r, i) => (i + 1, r)))
        {
            try
            {
                var violations = await rule.CheckAsync();
                if (violations.Count == 0)
                {
                    Console.WriteLine($"  ✓ Rule {index}/{rules.Count} passed");
                    result.PassedRules++;
                }
                else
                {
                    Console.WriteLine($"  ✗ Rule {index}/{rules.Count} failed with {violations.Count} violation(s)");
                    result.FailedRules++;
                    result.TotalViolations += violations.Count;
                    result.AllViolations.AddRange(violations);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠ Rule {index}/{rules.Count} errored: {ex.Message}");
                result.ErroredRules++;
            }
        }

        Console.WriteLine();
        Console.WriteLine(result.GetSummary());

        return result;
    }

    private static string GetProjectPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = Path.Combine(currentDir, "../../..");
        return Path.GetFullPath(projectRoot);
    }
}

/// <summary>
/// Result of architecture validation.
/// </summary>
public class ArchitectureValidationResult
{
    public int PassedRules { get; set; }
    public int FailedRules { get; set; }
    public int ErroredRules { get; set; }
    public int TotalViolations { get; set; }
    public List<Common.Assertion.Violation> AllViolations { get; set; } = new();

    /// <summary>
    /// Check if validation passed.
    /// </summary>
    public bool IsPassed => FailedRules == 0 && ErroredRules == 0;

    /// <summary>
    /// Get summary text.
    /// </summary>
    public string GetSummary()
    {
        if (IsPassed)
        {
            return $"✅ All architecture rules passed! ({PassedRules} rules validated)";
        }

        var parts = new List<string>();
        if (PassedRules > 0)
            parts.Add($"{PassedRules} passed");
        if (FailedRules > 0)
            parts.Add($"{FailedRules} failed ({TotalViolations} violations)");
        if (ErroredRules > 0)
            parts.Add($"{ErroredRules} errored");

        return $"❌ Architecture validation failed: {string.Join(", ", parts)}";
    }
}
