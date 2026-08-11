using System.Text.Json;

namespace ArchUnitNet.Common.Assertion;

/// <summary>
/// Snapshots the state of architecture rule violations for baseline tracking.
/// Enables gradual technical debt remediation and progress monitoring.
/// </summary>
public record RuleSnapshot(
    string RuleName,
    DateTime CreatedAt,
    int ViolationCount,
    IReadOnlyList<ViolationRecord> Violations,
    Dictionary<string, int> ViolationsByType
)
{
    /// <summary>
    /// Create a snapshot from current violations.
    /// </summary>
    public static RuleSnapshot FromViolations(string ruleName, IEnumerable<Violation> violations)
    {
        var violationList = violations.ToList();
        var violationRecords = violationList
            .Select(v => new ViolationRecord(v.GetType().Name, v.ToString() ?? ""))
            .ToList()
            .AsReadOnly();

        var typeGroups = violationList
            .GroupBy(v => v.GetType().Name)
            .ToDictionary(g => g.Key, g => g.Count());

        return new RuleSnapshot(
            ruleName,
            DateTime.UtcNow,
            violationList.Count,
            violationRecords,
            typeGroups
        );
    }

    /// <summary>
    /// Determine if violations have improved compared to this baseline.
    /// </summary>
    public bool HasImproved(IEnumerable<Violation> currentViolations)
    {
        return currentViolations.Count() < ViolationCount;
    }

    /// <summary>
    /// Determine if violations have regressed compared to this baseline.
    /// </summary>
    public bool HasRegressed(IEnumerable<Violation> currentViolations)
    {
        return currentViolations.Count() > ViolationCount;
    }

    /// <summary>
    /// Get the change in violation count.
    /// Positive = improvement (fewer violations), negative = regression (more violations).
    /// </summary>
    public int GetViolationCountChange(IEnumerable<Violation> currentViolations)
    {
        return ViolationCount - currentViolations.Count();
    }

    /// <summary>
    /// Serialize snapshot to JSON.
    /// </summary>
    public string ToJson()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Deserialize snapshot from JSON.
    /// </summary>
    public static RuleSnapshot? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<RuleSnapshot>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Save snapshot to file.
    /// </summary>
    public async Task SaveToFileAsync(string filePath)
    {
        var json = ToJson();
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Load snapshot from file.
    /// </summary>
    public static async Task<RuleSnapshot?> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        return FromJson(json);
    }

    /// <summary>
    /// Get summary of violations by type.
    /// </summary>
    public string GetSummary()
    {
        var lines = new List<string>
        {
            $"Rule: {RuleName}",
            $"Created: {CreatedAt:yyyy-MM-dd HH:mm:ss}",
            $"Total Violations: {ViolationCount}"
        };

        if (ViolationsByType.Any())
        {
            lines.Add("Violations by Type:");
            foreach (var kvp in ViolationsByType.OrderByDescending(x => x.Value))
            {
                lines.Add($"  {kvp.Key}: {kvp.Value}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>
/// Represents a single violation in a snapshot.
/// </summary>
public record ViolationRecord(string Type, string Message);

/// <summary>
/// Extension methods for rule snapshots.
/// </summary>
public static class RuleSnapshotExtensions
{
    /// <summary>
    /// Create a snapshot of current violations.
    /// </summary>
    public static RuleSnapshot ToSnapshot(this IEnumerable<Violation> violations, string ruleName)
    {
        return RuleSnapshot.FromViolations(ruleName, violations);
    }

    /// <summary>
    /// Compare current violations against a baseline snapshot.
    /// </summary>
    public static RuleComparisonResult CompareAgainstBaseline(
        this IEnumerable<Violation> currentViolations,
        RuleSnapshot baseline)
    {
        var current = currentViolations.ToList();
        return new RuleComparisonResult(
            baseline,
            current.Count,
            baseline.GetViolationCountChange(current),
            baseline.HasImproved(current),
            baseline.HasRegressed(current)
        );
    }
}

/// <summary>
/// Result of comparing current violations against a baseline.
/// </summary>
public record RuleComparisonResult(
    RuleSnapshot Baseline,
    int CurrentViolationCount,
    int ViolationCountChange,
    bool Improved,
    bool Regressed
)
{
    /// <summary>
    /// Get a human-readable comparison summary.
    /// </summary>
    public string GetSummary()
    {
        var status = Improved ? "✓ IMPROVED" : (Regressed ? "✗ REGRESSED" : "= NO CHANGE");
        var change = ViolationCountChange > 0 ? $"+{ViolationCountChange}" : ViolationCountChange.ToString();

        return $"{status} | Baseline: {Baseline.ViolationCount} → Current: {CurrentViolationCount} ({change})";
    }
}
