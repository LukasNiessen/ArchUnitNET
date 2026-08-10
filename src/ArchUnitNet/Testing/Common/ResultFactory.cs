using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// Factory for creating test results from rule violations.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Create a test result from violations.
    /// Returns a result with a formatted message suitable for test frameworks.
    /// </summary>
    public static TestResult CreateFromViolations(IReadOnlyList<Violation> violations, string ruleName)
    {
        if (violations == null)
            throw new ArgumentNullException(nameof(violations));

        if (violations.Count == 0)
        {
            return new TestResult(
                Passed: true,
                Message: $"Rule '{ruleName}' passed",
                ViolationCount: 0
            );
        }

        var messages = new List<string>
        {
            $"Rule '{ruleName}' failed with {violations.Count} violation(s):",
            ""
        };

        for (int i = 0; i < Math.Min(violations.Count, 10); i++)
        {
            messages.Add($"  {i + 1}. {violations[i]}");
        }

        if (violations.Count > 10)
        {
            messages.Add($"  ... and {violations.Count - 10} more violations");
        }

        return new TestResult(
            Passed: false,
            Message: string.Join(Environment.NewLine, messages),
            ViolationCount: violations.Count
        );
    }
}

/// <summary>
/// Result of a rule check suitable for test frameworks.
/// </summary>
public record TestResult(
    bool Passed,
    string Message,
    int ViolationCount
)
{
    public bool Failed => !Passed;
}
