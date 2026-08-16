using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// Framework-agnostic async assertion helper for architecture testing.
/// Abstracts the pattern: build rule → check → assert.
/// Used by xUnit, NUnit, MSTest adapters without framework coupling.
///
/// ASYNC ONLY: All methods are async (return Task).
/// For synchronous assertions or zero-configuration fallback, use ArchAssert class instead.
///
/// Usage comparison:
/// - ArchAssert.Passes(rule) - Synchronous, no framework setup, simplest option
/// - AssertHelper.PassesAsync(rule) - Async, used by framework adapters internally
/// - rule.Should().PassAsync() - Fluent async, via framework-specific adapters (xUnit, NUnit, MSTest)
///
/// See also: ArchAssert class for the simple, configuration-free alternative.
/// </summary>
public static class AssertHelper
{
    /// <summary>
    /// Assert that a rule passes (no violations).
    /// Throws AssertException if violations found.
    /// Uses ResultFactory for consistent formatting across all frameworks.
    /// </summary>
    public static async Task PassesAsync(Checkable rule, CheckOptions? options = null, string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count == 0)
            return;

        // Use ResultFactory for consistent violation formatting
        var result = ResultFactory.CreateFromViolations(
            violations,
            ruleName: message ?? "Architecture Rule",
            colored: false,
            style: FormatStyle.Detailed);

        throw new AssertException(result.FormattedMessage);
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
    /// Uses ResultFactory for detailed violation formatting in error messages.
    /// </summary>
    public static async Task FailsWithAsync(Checkable rule, int expectedCount, CheckOptions? options = null, string? message = null)
    {
        var violations = await rule.CheckAsync(options);

        if (violations.Count == expectedCount)
            return;

        // Format violations for detailed error message
        var result = ResultFactory.CreateFromViolations(
            violations,
            ruleName: message ?? "Architecture Rule",
            colored: false,
            style: FormatStyle.Detailed);

        var errorMsg = $"Expected {expectedCount} violation(s) but found {violations.Count}\n\n{result.FormattedMessage}";
        throw new AssertException(errorMsg);
    }

    /// <summary>
    /// Assert that a rule produces violations containing specific text.
    /// Uses ResultFactory for formatted violation output in error messages.
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

        // Format violations for detailed error message
        var result = ResultFactory.CreateFromViolations(
            violations,
            ruleName: message ?? "Architecture Rule",
            colored: false,
            style: FormatStyle.Detailed);

        var errorMsg = $"Expected violation message to contain '{expectedText}'\n\n{result.FormattedMessage}";
        throw new AssertException(errorMsg);
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
