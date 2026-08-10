using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// Framework-agnostic assertion helper for architecture testing.
/// Abstracts the pattern: build rule → check → assert.
/// Used by xUnit, NUnit, MSTest adapters without framework coupling.
/// </summary>
public static class AssertHelper
{
    /// <summary>
    /// Assert that a rule passes (no violations).
    /// Throws AssertException if violations found.
    /// </summary>
    public static async Task PassesAsync(Checkable rule, CheckOptions? options = null, string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count == 0)
            return;

        var header = message ?? "Architecture rule failed";
        var report = ViolationFormatter.FormatReport(violations, message, colored: false);

        throw new AssertException($"{header}:\n{report}");
    }

    /// <summary>
    /// Assert that a rule fails (has violations).
    /// Throws AssertException if no violations found.
    /// </summary>
    public static async Task FailsAsync(Checkable rule, CheckOptions? options = null, string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count > 0)
            return;

        var msg = message ?? "Expected rule to fail but it passed";
        throw new AssertException(msg);
    }

    /// <summary>
    /// Assert that a rule fails with exactly N violations.
    /// </summary>
    public static async Task FailsWithAsync(Checkable rule, int expectedCount, CheckOptions? options = null, string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count == expectedCount)
            return;

        var msg = message ?? $"Expected {expectedCount} violation(s) but found {violations.Count}";
        throw new AssertException(msg);
    }

    /// <summary>
    /// Assert that a rule produces violations containing specific text.
    /// </summary>
    public static async Task FailsWithMessageContainingAsync(
        Checkable rule,
        string expectedText,
        CheckOptions? options = null,
        string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count == 0)
            throw new AssertException("Expected violations but found none");

        var allMessages = string.Join("\n", violations.Select(v => v.ToString()));

        if (allMessages.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
            return;

        var msg = message ?? $"Expected violation message to contain '{expectedText}'";
        throw new AssertException($"{msg}\n\nActual violations:\n{allMessages}");
    }

    /// <summary>
    /// Custom exception for architecture test failures.
    /// Framework adapters catch this and convert to their native exception.
    /// </summary>
    public class AssertException : Exception
    {
        public AssertException(string message) : base(message) { }
        public AssertException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
