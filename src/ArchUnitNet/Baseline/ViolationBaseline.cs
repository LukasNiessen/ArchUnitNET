using ArchUnitNet.Common.Assertion;
using Newtonsoft.Json.Linq;

namespace ArchUnitNet.Baseline;

/// <summary>
/// Manages violation baselines for gradual technical debt remediation.
/// Allows suppressing known violations while preventing new ones.
/// </summary>
public class ViolationBaseline
{
    private readonly HashSet<string> _suppressedViolations = new();
    private readonly HashSet<string> _violationPatterns = new();
    private DateTime _createdAt;
    private string? _description;

    public ViolationBaseline()
    {
        _createdAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Add a violation to the baseline (suppress it).
    /// </summary>
    public void Add(Violation violation, string? reason = null)
    {
        var hash = GetViolationHash(violation);
        _suppressedViolations.Add(hash);
    }

    /// <summary>
    /// Add a violation pattern to suppress by text matching.
    /// </summary>
    public void AddPattern(string pattern)
    {
        _violationPatterns.Add(pattern);
    }

    /// <summary>
    /// Check if a violation is suppressed by baseline.
    /// </summary>
    public bool IsSuppressed(Violation violation)
    {
        var hash = GetViolationHash(violation);
        if (_suppressedViolations.Contains(hash))
            return true;

        var text = violation.ToString() ?? "";
        return _violationPatterns.Any(pattern =>
            text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Filter violations to only return non-suppressed ones.
    /// </summary>
    public List<Violation> FilterNewViolations(IEnumerable<Violation> violations)
    {
        return violations.Where(v => !IsSuppressed(v)).ToList();
    }

    /// <summary>
    /// Get baseline statistics.
    /// </summary>
    public BaselineStatistics GetStatistics(IEnumerable<Violation> allViolations)
    {
        var allList = allViolations.ToList();
        var suppressedList = allList.Where(IsSuppressed).ToList();
        var newViolations = allList.Where(v => !IsSuppressed(v)).ToList();

        return new BaselineStatistics
        {
            TotalViolations = allList.Count,
            SuppressedViolations = suppressedList.Count,
            NewViolations = newViolations.Count,
            CreatedAt = _createdAt,
            SuppressedCount = _suppressedViolations.Count,
            PatternCount = _violationPatterns.Count,
        };
    }

    /// <summary>
    /// Export baseline to JSON.
    /// </summary>
    public string ExportToJson()
    {
        var obj = new JObject
        {
            ["createdAt"] = _createdAt.ToString("O"),
            ["description"] = _description ?? "",
            ["suppressedViolations"] = new JArray(_suppressedViolations.OrderBy(x => x)),
            ["violationPatterns"] = new JArray(_violationPatterns.OrderBy(x => x)),
        };

        return obj.ToString(Newtonsoft.Json.Formatting.Indented);
    }

    /// <summary>
    /// Import baseline from JSON.
    /// </summary>
    public static ViolationBaseline ImportFromJson(string json)
    {
        try
        {
            var obj = JObject.Parse(json);
            var baseline = new ViolationBaseline();

            if (obj["createdAt"] != null)
                baseline._createdAt = obj["createdAt"]!.Value<DateTime>();

            if (obj["description"] != null)
                baseline._description = obj["description"]!.Value<string>();

            if (obj["suppressedViolations"] is JArray violationsArray)
            {
                foreach (var violation in violationsArray)
                {
                    baseline._suppressedViolations.Add(violation.Value<string>() ?? "");
                }
            }

            if (obj["violationPatterns"] is JArray patternsArray)
            {
                foreach (var pattern in patternsArray)
                {
                    baseline._violationPatterns.Add(pattern.Value<string>() ?? "");
                }
            }

            return baseline;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to import baseline from JSON", ex);
        }
    }

    /// <summary>
    /// Save baseline to file.
    /// </summary>
    public async Task SaveToFileAsync(string filePath)
    {
        var json = ExportToJson();
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Load baseline from file.
    /// </summary>
    public static async Task<ViolationBaseline> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Baseline file not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);
        return ImportFromJson(json);
    }

    /// <summary>
    /// Set baseline description.
    /// </summary>
    public ViolationBaseline WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Get count of suppressed violations.
    /// </summary>
    public int GetSuppressedCount() => _suppressedViolations.Count;

    /// <summary>
    /// Get count of suppression patterns.
    /// </summary>
    public int GetPatternCount() => _violationPatterns.Count;

    /// <summary>
    /// Clear all suppressions.
    /// </summary>
    public void Clear()
    {
        _suppressedViolations.Clear();
        _violationPatterns.Clear();
    }

    private static string GetViolationHash(Violation violation)
    {
        var text = violation.ToString() ?? "";
        return ComputeHash(text);
    }

    private static string ComputeHash(string text)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
            return System.Convert.ToBase64String(hash);
        }
    }
}

/// <summary>
/// Statistics about a baseline.
/// </summary>
public class BaselineStatistics
{
    public int TotalViolations { get; set; }
    public int SuppressedViolations { get; set; }
    public int NewViolations { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SuppressedCount { get; set; }
    public int PatternCount { get; set; }

    /// <summary>
    /// Calculate improvement (fewer violations than baseline).
    /// </summary>
    public int GetImprovement()
    {
        return SuppressedViolations - NewViolations;
    }

    /// <summary>
    /// Get summary text.
    /// </summary>
    public string GetSummary()
    {
        if (NewViolations == 0)
            return "✓ No new violations (baseline clean)";

        if (GetImprovement() > 0)
            return $"↓ Improved by {GetImprovement()} violations";

        if (NewViolations > SuppressedViolations)
            return $"↑ Regressed by {NewViolations - SuppressedViolations} violations";

        return $"{NewViolations} new violation(s) found";
    }

    /// <summary>
    /// Check if baseline is clean (no new violations).
    /// </summary>
    public bool IsClean => NewViolations == 0;

    /// <summary>
    /// Check if there's been improvement.
    /// </summary>
    public bool IsImproved => GetImprovement() > 0;
}

/// <summary>
/// Extension methods for baseline management.
/// </summary>
public static class BaselineExtensions
{
    /// <summary>
    /// Filter violations against a baseline.
    /// </summary>
    public static List<Violation> WithoutBaseline(
        this IEnumerable<Violation> violations,
        ViolationBaseline baseline)
    {
        return baseline.FilterNewViolations(violations);
    }

    /// <summary>
    /// Create baseline from current violations.
    /// </summary>
    public static ViolationBaseline CreateBaseline(this IEnumerable<Violation> violations)
    {
        var baseline = new ViolationBaseline();
        foreach (var violation in violations)
        {
            baseline.Add(violation);
        }
        return baseline;
    }

    /// <summary>
    /// Save baseline with description.
    /// </summary>
    public static async Task SaveBaselineAsync(
        this IEnumerable<Violation> violations,
        string filePath,
        string description)
    {
        var baseline = violations.CreateBaseline();
        baseline.WithDescription(description);
        await baseline.SaveToFileAsync(filePath);
    }

    /// <summary>
    /// Compare current violations against baseline.
    /// </summary>
    public static BaselineStatistics CompareWithBaseline(
        this IEnumerable<Violation> violations,
        ViolationBaseline baseline)
    {
        return baseline.GetStatistics(violations);
    }
}
