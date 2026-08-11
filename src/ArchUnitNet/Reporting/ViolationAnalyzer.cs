using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Reporting;

/// <summary>
/// Analyzes and groups violations for reporting and filtering.
/// Provides utilities to understand violation patterns and severity.
/// </summary>
public class ViolationAnalyzer
{
    private readonly IEnumerable<Violation> _violations;

    public ViolationAnalyzer(IEnumerable<Violation> violations)
    {
        _violations = violations ?? throw new ArgumentNullException(nameof(violations));
    }

    /// <summary>
    /// Group violations by type.
    /// </summary>
    public ILookup<string, Violation> GroupByType()
    {
        return _violations.ToLookup(v => v.GetType().Name);
    }

    /// <summary>
    /// Group violations by severity level.
    /// </summary>
    public IDictionary<string, List<Violation>> GroupBySeverity()
    {
        var groups = new Dictionary<string, List<Violation>>
        {
            ["Critical"] = new(),
            ["Error"] = new(),
            ["Warning"] = new(),
            ["Info"] = new(),
        };

        foreach (var violation in _violations)
        {
            var severity = GetViolationSeverity(violation);
            if (groups.ContainsKey(severity))
            {
                groups[severity].Add(violation);
            }
        }

        return groups;
    }

    /// <summary>
    /// Group violations by source file/module.
    /// </summary>
    public ILookup<string, Violation> GroupBySource()
    {
        return _violations.ToLookup(v => ExtractSource(v) ?? "Unknown");
    }

    /// <summary>
    /// Group violations by category (violation type).
    /// </summary>
    public IDictionary<string, int> GetViolationCounts()
    {
        return _violations
            .GroupBy(v => v.GetType().Name)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Get violations by type filter.
    /// </summary>
    public List<Violation> FilterByType<T>() where T : Violation
    {
        return _violations.OfType<T>().Cast<Violation>().ToList();
    }

    /// <summary>
    /// Get violations matching a text pattern.
    /// </summary>
    public List<Violation> FilterByText(string pattern)
    {
        return _violations
            .Where(v => v.ToString()?.Contains(pattern, StringComparison.OrdinalIgnoreCase) ?? false)
            .ToList();
    }

    /// <summary>
    /// Get violations from a specific source.
    /// </summary>
    public List<Violation> FilterBySource(string sourcePattern)
    {
        return _violations
            .Where(v => ExtractSource(v)?.Contains(sourcePattern, StringComparison.OrdinalIgnoreCase) ?? false)
            .ToList();
    }

    /// <summary>
    /// Get violations between two files/modules.
    /// </summary>
    public List<Violation> FilterBySourceAndTarget(string source, string target)
    {
        return _violations
            .Where(v => IsViolationBetween(v, source, target))
            .ToList();
    }

    /// <summary>
    /// Get summary statistics about violations.
    /// </summary>
    public ViolationStatistics GetStatistics()
    {
        var violations = _violations.ToList();
        var byType = GroupByType();
        var bySeverity = GroupBySeverity();

        return new ViolationStatistics
        {
            TotalViolations = violations.Count,
            ViolationsByType = byType.ToDictionary(g => g.Key, g => g.Count()),
            ViolationsBySeverity = bySeverity.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count),
            CriticalCount = bySeverity["Critical"].Count,
            ErrorCount = bySeverity["Error"].Count,
            WarningCount = bySeverity["Warning"].Count,
            InfoCount = bySeverity["Info"].Count,
            AffectedSources = GetAffectedSources(violations).Count(),
        };
    }

    /// <summary>
    /// Get violations sorted by severity (critical first).
    /// </summary>
    public List<Violation> GetSortedBySeverity()
    {
        var severityOrder = new Dictionary<string, int>
        {
            ["Critical"] = 0,
            ["Error"] = 1,
            ["Warning"] = 2,
            ["Info"] = 3,
        };

        return _violations
            .OrderBy(v => severityOrder.GetValueOrDefault(GetViolationSeverity(v), 99))
            .ToList();
    }

    /// <summary>
    /// Get violations sorted by frequency (most common first).
    /// </summary>
    public List<Violation> GetSortedByFrequency()
    {
        var counts = GetViolationCounts();
        return _violations
            .OrderByDescending(v => counts.ContainsKey(v.GetType().Name) ? counts[v.GetType().Name] : 0)
            .ToList();
    }

    /// <summary>
    /// Find violations related to a specific path.
    /// </summary>
    public List<Violation> FindRelatedViolations(string pathPattern)
    {
        return _violations
            .Where(v => IsViolationRelatedToPath(v, pathPattern))
            .ToList();
    }

    /// <summary>
    /// Get violation distribution matrix (source → target counts).
    /// </summary>
    public IDictionary<string, IDictionary<string, int>> GetViolationMatrix()
    {
        var matrix = new Dictionary<string, Dictionary<string, int>>();

        foreach (var violation in _violations)
        {
            var source = ExtractSource(violation) ?? "Unknown";
            var target = ExtractTarget(violation) ?? "Unknown";

            if (!matrix.ContainsKey(source))
                matrix[source] = new Dictionary<string, int>();

            if (!matrix[source].ContainsKey(target))
                matrix[source][target] = 0;

            matrix[source][target]++;
        }

        return matrix.ToDictionary(
            kvp => kvp.Key,
            kvp => (IDictionary<string, int>)kvp.Value
        );
    }

    private string GetViolationSeverity(Violation violation)
    {
        return violation.GetType().Name switch
        {
            "CyclicDependency" => "Critical",
            "ViolatingFileDependency" => "Error",
            _ => "Warning",
        };
    }

    private string? ExtractSource(Violation violation)
    {
        if (violation is ArchUnitNet.Files.Assertion.ViolatingFileDependency dep)
            return dep.Source;

        if (violation is ArchUnitNet.Files.Assertion.CyclicDependency cycle)
            return cycle.Cycle.FirstOrDefault();

        return null;
    }

    private string? ExtractTarget(Violation violation)
    {
        if (violation is ArchUnitNet.Files.Assertion.ViolatingFileDependency dep)
            return dep.Target;

        if (violation is ArchUnitNet.Files.Assertion.CyclicDependency cycle)
            return cycle.Cycle.Count > 1 ? cycle.Cycle[1] : null;

        return null;
    }

    private IEnumerable<string> GetAffectedSources(List<Violation> violations)
    {
        return violations
            .Select(v => ExtractSource(v))
            .Where(s => s != null)
            .Distinct()
            .OfType<string>();
    }

    private bool IsViolationBetween(Violation violation, string source, string target)
    {
        if (violation is ArchUnitNet.Files.Assertion.ViolatingFileDependency dep)
        {
            return dep.Source.Contains(source, StringComparison.OrdinalIgnoreCase) &&
                   dep.Target.Contains(target, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private bool IsViolationRelatedToPath(Violation violation, string pathPattern)
    {
        var source = ExtractSource(violation);
        var target = ExtractTarget(violation);

        return (source?.Contains(pathPattern, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (target?.Contains(pathPattern, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}

/// <summary>
/// Statistics about violations in an analysis.
/// </summary>
public class ViolationStatistics
{
    public int TotalViolations { get; set; }
    public Dictionary<string, int> ViolationsByType { get; set; } = new();
    public Dictionary<string, int> ViolationsBySeverity { get; set; } = new();
    public int CriticalCount { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public int AffectedSources { get; set; }

    /// <summary>
    /// Check if analysis has any critical violations.
    /// </summary>
    public bool HasCriticalViolations => CriticalCount > 0;

    /// <summary>
    /// Get pass/fail status based on critical violations.
    /// </summary>
    public bool IsPassingBuild => !HasCriticalViolations;

    /// <summary>
    /// Get summary text.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string>();

        if (CriticalCount > 0)
            parts.Add($"{CriticalCount} critical");
        if (ErrorCount > 0)
            parts.Add($"{ErrorCount} error{(ErrorCount != 1 ? "s" : "")}");
        if (WarningCount > 0)
            parts.Add($"{WarningCount} warning{(WarningCount != 1 ? "s" : "")}");

        if (parts.Count == 0)
            return "✓ No violations";

        return $"✗ {string.Join(", ", parts)} ({TotalViolations} total)";
    }
}

/// <summary>
/// Extension methods for violation analysis.
/// </summary>
public static class ViolationAnalysisExtensions
{
    /// <summary>
    /// Analyze violations.
    /// </summary>
    public static ViolationAnalyzer Analyze(this IEnumerable<Violation> violations)
    {
        return new ViolationAnalyzer(violations);
    }

    /// <summary>
    /// Get statistics about violations.
    /// </summary>
    public static ViolationStatistics GetStatistics(this IEnumerable<Violation> violations)
    {
        return new ViolationAnalyzer(violations).GetStatistics();
    }

    /// <summary>
    /// Filter violations by type.
    /// </summary>
    public static List<Violation> OfViolationType<T>(this IEnumerable<Violation> violations) where T : Violation
    {
        return violations.OfType<T>().OfType<Violation>().ToList();
    }

    /// <summary>
    /// Check if violations exceed a threshold.
    /// </summary>
    public static bool ExceedsThreshold(this IEnumerable<Violation> violations, int threshold)
    {
        return violations.Count() > threshold;
    }

    /// <summary>
    /// Check if there are critical violations.
    /// </summary>
    public static bool HasCriticalViolations(this IEnumerable<Violation> violations)
    {
        return violations.Analyze().GetStatistics().HasCriticalViolations;
    }
}
