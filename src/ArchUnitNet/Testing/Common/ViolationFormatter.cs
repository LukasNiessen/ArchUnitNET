using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// Formats violations for display with colors, structure, and readability.
/// Used by test framework adapters to produce user-friendly error messages.
/// </summary>
public static class ViolationFormatter
{
    /// <summary>
    /// Format a single violation for console output.
    /// </summary>
    public static string Format(Violation violation, int index = 0, bool colored = true)
    {
        var prefix = colored ? "[✗]" : "[✗]";
        var number = index > 0 ? $"{index}. " : "";

        var type = violation.GetType().Name;
        var typeLabel = colored ? type : type;

        var message = violation.ToString() ?? "No message";

        return $"{prefix} {number}{typeLabel}\n  {message}";
    }

    /// <summary>
    /// Format multiple violations into a readable report.
    /// </summary>
    public static string FormatReport(IEnumerable<Violation> violations, string? ruleName = null, bool colored = true)
    {
        var violationList = violations.ToList();

        if (violationList.Count == 0)
        {
            return "✓ All checks passed!";
        }

        var lines = new List<string>();

        if (ruleName != null)
        {
            lines.Add($"Rule: {ruleName}");
        }

        lines.Add($"{violationList.Count} violation(s) found:");
        lines.Add("");

        for (int i = 0; i < violationList.Count; i++)
        {
            lines.Add(Format(violationList[i], i + 1, colored));
            if (i < violationList.Count - 1)
                lines.Add("");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Format violations grouped by type.
    /// </summary>
    public static string FormatGrouped(IEnumerable<Violation> violations, bool colored = true)
    {
        var violationList = violations.ToList();

        if (violationList.Count == 0)
        {
            return "✓ All checks passed!";
        }

        var grouped = violationList.GroupBy(v => v.GetType().Name).ToList();
        var lines = new List<string>();

        lines.Add($"{violationList.Count} violation(s) in {grouped.Count} categories:");
        lines.Add("");

        int violationIndex = 1;
        foreach (var group in grouped)
        {
            lines.Add($"  {group.Key} ({group.Count()})");

            foreach (var violation in group)
            {
                var message = violation.ToString() ?? "No message";
                lines.Add($"    {violationIndex}. {message}");
                violationIndex++;
            }

            lines.Add("");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Format violations in a compact summary format.
    /// </summary>
    public static string FormatSummary(IEnumerable<Violation> violations, bool colored = true)
    {
        var violationList = violations.ToList();
        var fileViolations = violationList.OfType<ViolatingFileDependency>().Count();
        var otherViolations = violationList.Count - fileViolations;

        var parts = new List<string>();

        if (fileViolations > 0)
            parts.Add($"{fileViolations} dependency violation{(fileViolations != 1 ? "s" : "")}");

        if (otherViolations > 0)
            parts.Add($"{otherViolations} other violation{(otherViolations != 1 ? "s" : "")}");

        if (parts.Count == 0)
        {
            return "✓ No violations";
        }

        var summary = string.Join(", ", parts);
        return $"✗ {summary}";
    }
}
