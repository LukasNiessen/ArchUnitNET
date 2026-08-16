using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// How to format violations in test result messages.
/// </summary>
public enum FormatStyle
{
    /// <summary>
    /// Compact summary: "✓ No violations" or "✗ 3 violations"
    /// </summary>
    Compact,

    /// <summary>
    /// Detailed list showing first 10 violations with "... and N more" suffix.
    /// </summary>
    Detailed,

    /// <summary>
    /// Group violations by type with category counts.
    /// </summary>
    Grouped
}

/// <summary>
/// Factory for creating test results from rule violations.
/// Uses ViolationFactory for consistent violation formatting.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Create a test result from violations.
    /// Returns a result with a formatted message suitable for test frameworks.
    /// </summary>
    /// <param name="violations">List of violations from rule check</param>
    /// <param name="ruleName">Name of the rule that was checked</param>
    /// <param name="colored">Whether to include ANSI colour codes in output</param>
    /// <param name="style">How to format the violations (Compact, Detailed, Grouped)</param>
    /// <returns>Test result with pass/fail status and formatted message</returns>
    public static TestResult CreateFromViolations(
        IReadOnlyList<Violation> violations,
        string ruleName,
        bool colored = false,
        FormatStyle style = FormatStyle.Detailed)
    {
        if (violations == null)
            throw new ArgumentNullException(nameof(violations));

        if (violations.Count == 0)
        {
            var successMsg = Colours.Colorize("✓ Rule passed", Colours.Success, colored);
            return new TestResult(
                Passed: true,
                Message: $"Rule '{ruleName}' passed",
                FormattedMessage: successMsg,
                ViolationCount: 0
            );
        }

        var formattedMessage = style switch
        {
            FormatStyle.Compact => FormatCompact(violations, colored),
            FormatStyle.Grouped => FormatGrouped(violations, colored),
            _ => FormatDetailed(violations, colored)
        };

        return new TestResult(
            Passed: false,
            Message: $"Rule '{ruleName}' failed with {violations.Count} violation(s):",
            FormattedMessage: formattedMessage,
            ViolationCount: violations.Count
        );
    }

    /// <summary>
    /// Format violations in detailed style: full list with first 10 + truncation message.
    /// </summary>
    private static string FormatDetailed(IReadOnlyList<Violation> violations, bool colored)
    {
        var messages = new List<string>();

        var header = Colours.Colorize($"✗ {violations.Count} violation(s) found:", Colours.Error, colored);
        messages.Add(header);
        messages.Add("");

        for (int i = 0; i < Math.Min(violations.Count, 10); i++)
        {
            messages.Add(ViolationFactory.FormatViolation(violations[i], i + 1, colored));
            if (i < Math.Min(violations.Count, 10) - 1)
                messages.Add("");
        }

        if (violations.Count > 10)
        {
            var suffix = Colours.Colorize(
                $"... and {violations.Count - 10} more violation(s)",
                Colours.Muted,
                colored);
            messages.Add("");
            messages.Add(suffix);
        }

        return string.Join("\n", messages);
    }

    /// <summary>
    /// Format violations grouped by type.
    /// </summary>
    private static string FormatGrouped(IReadOnlyList<Violation> violations, bool colored)
    {
        var grouped = violations.GroupBy(v => v.GetType().Name).ToList();
        var lines = new List<string>();

        var header = Colours.Colorize(
            $"✗ {violations.Count} violation(s) in {grouped.Count} categories:",
            Colours.Error,
            colored);
        lines.Add(header);
        lines.Add("");

        int violationIndex = 1;
        foreach (var group in grouped)
        {
            var categoryName = Colours.Colorize($"  {group.Key}", Colours.Info, colored);
            lines.Add($"{categoryName} ({group.Count()})");

            foreach (var violation in group)
            {
                var msg = violation.ToString() ?? "No message";
                lines.Add($"    {violationIndex}. {msg}");
                violationIndex++;
            }

            lines.Add("");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Format violations in compact summary style.
    /// </summary>
    private static string FormatCompact(IReadOnlyList<Violation> violations, bool colored)
    {
        var fileViolations = 0;
        var otherViolations = 0;

        foreach (var v in violations)
        {
            if (v.GetType().Name.Contains("File"))
                fileViolations++;
            else
                otherViolations++;
        }

        var parts = new List<string>();

        if (fileViolations > 0)
            parts.Add($"{fileViolations} file violation{(fileViolations != 1 ? "s" : "")}");

        if (otherViolations > 0)
            parts.Add($"{otherViolations} other violation{(otherViolations != 1 ? "s" : "")}");

        var summary = string.Join(", ", parts);
        return Colours.Colorize($"✗ {summary}", Colours.Error, colored);
    }
}

/// <summary>
/// Result of a rule check suitable for test frameworks.
/// </summary>
public record TestResult(
    bool Passed,
    string Message,
    string FormattedMessage = "",
    int ViolationCount = 0
)
{
    public bool Failed => !Passed;
}
