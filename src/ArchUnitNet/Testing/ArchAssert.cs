using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing.Common;

namespace ArchUnitNet.Testing;

/// <summary>
/// Framework-agnostic architecture assertions.
/// Zero-configuration fallback that works with NUnit, xUnit, MSTest, or any test framework.
///
/// This is the simplest way to write architecture tests without framework setup.
/// For async or fluent API, use framework-specific adapters (xUnit.Should(), NUnit.Should(), etc.).
/// </summary>
public static class ArchAssert
{
    /// <summary>
    /// Assert that an architecture rule passes (produces no violations).
    /// Framework-agnostic, zero-configuration fallback method.
    /// Works with NUnit, xUnit, MSTest, and any other test framework.
    ///
    /// Example:
    /// <code>
    /// var rule = ProjectFiles().InPath("src/**").Should().HaveNoCycles();
    /// ArchAssert.Passes(rule);
    /// </code>
    /// </summary>
    /// <param name="rule">The architecture rule to check</param>
    /// <param name="options">Optional check options (e.g., AllowEmptyTests)</param>
    /// <param name="message">Optional custom message for the rule name in error output</param>
    /// <exception cref="AssertHelper.AssertException">Thrown if rule fails (has violations)</exception>
    public static void Passes(Checkable rule, CheckOptions? options = null, string? message = null)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        try
        {
            // Block on async call - acceptable in test context
            var violations = rule.CheckAsync(options).Result;

            if (violations.Count > 0)
            {
                // Format violations using Issue #24 pipeline
                var formatted = ResultFactory.CreateFromViolations(
                    violations,
                    ruleName: message ?? "Architecture Rule",
                    colored: false,
                    style: FormatStyle.Detailed);

                throw new AssertHelper.AssertException(formatted.FormattedMessage);
            }
        }
        catch (AggregateException ex) when (ex.InnerException != null)
        {
            // Unwrap task exceptions for clearer error messages
            throw new AssertHelper.AssertException(
                $"Architecture rule check failed: {ex.InnerException.Message}",
                ex.InnerException);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails (produces violations).
    /// Useful for testing rule correctness with intentional violations.
    /// </summary>
    /// <param name="rule">The architecture rule to check</param>
    /// <param name="options">Optional check options</param>
    /// <param name="message">Optional custom message for the rule name</param>
    /// <exception cref="AssertHelper.AssertException">Thrown if rule passes (no violations)</exception>
    public static void Fails(Checkable rule, CheckOptions? options = null, string? message = null)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        try
        {
            var violations = rule.CheckAsync(options).Result;

            if (violations.Count == 0)
            {
                throw new AssertHelper.AssertException(
                    message ?? "Expected architecture rule to fail but it passed");
            }
        }
        catch (AggregateException ex) when (ex.InnerException != null)
        {
            throw new AssertHelper.AssertException(
                $"Architecture rule check failed: {ex.InnerException.Message}",
                ex.InnerException);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails with exactly N violations.
    /// Useful for regression testing - verify that a known set of violations is present.
    /// </summary>
    /// <param name="rule">The architecture rule to check</param>
    /// <param name="expectedCount">Expected number of violations</param>
    /// <param name="options">Optional check options</param>
    /// <param name="message">Optional custom message for the rule name</param>
    /// <exception cref="AssertHelper.AssertException">Thrown if violation count doesn't match</exception>
    public static void FailsWith(
        Checkable rule,
        int expectedCount,
        CheckOptions? options = null,
        string? message = null)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        if (expectedCount < 0)
            throw new ArgumentException("Expected count must be non-negative", nameof(expectedCount));

        try
        {
            var violations = rule.CheckAsync(options).Result;

            if (violations.Count != expectedCount)
            {
                var formatted = ResultFactory.CreateFromViolations(
                    violations,
                    ruleName: message ?? "Architecture Rule",
                    colored: false,
                    style: FormatStyle.Detailed);

                throw new AssertHelper.AssertException(
                    $"Expected {expectedCount} violation(s) but found {violations.Count}\n\n{formatted.FormattedMessage}");
            }
        }
        catch (AggregateException ex) when (ex.InnerException != null)
        {
            throw new AssertHelper.AssertException(
                $"Architecture rule check failed: {ex.InnerException.Message}",
                ex.InnerException);
        }
    }

    /// <summary>
    /// Assert that an architecture rule produces violations containing specific text.
    /// Useful for validating that a specific violation message appears.
    /// </summary>
    /// <param name="rule">The architecture rule to check</param>
    /// <param name="expectedText">Text that should appear in at least one violation message</param>
    /// <param name="options">Optional check options</param>
    /// <param name="message">Optional custom message for the rule name</param>
    /// <exception cref="AssertHelper.AssertException">Thrown if text not found in violations</exception>
    public static void FailsWithMessageContaining(
        Checkable rule,
        string expectedText,
        CheckOptions? options = null,
        string? message = null)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        if (string.IsNullOrEmpty(expectedText))
            throw new ArgumentException("Expected text cannot be null or empty", nameof(expectedText));

        try
        {
            var violations = rule.CheckAsync(options).Result;

            if (violations.Count == 0)
            {
                throw new AssertHelper.AssertException(
                    "Expected violations containing specific text but found none");
            }

            var allMessages = string.Join("\n", violations.Select(v => v.ToString()));

            if (!allMessages.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
            {
                var formatted = ResultFactory.CreateFromViolations(
                    violations,
                    ruleName: message ?? "Architecture Rule",
                    colored: false,
                    style: FormatStyle.Detailed);

                throw new AssertHelper.AssertException(
                    $"Expected violation message to contain '{expectedText}'\n\n{formatted.FormattedMessage}");
            }
        }
        catch (AggregateException ex) when (ex.InnerException != null)
        {
            throw new AssertHelper.AssertException(
                $"Architecture rule check failed: {ex.InnerException.Message}",
                ex.InnerException);
        }
    }
}
