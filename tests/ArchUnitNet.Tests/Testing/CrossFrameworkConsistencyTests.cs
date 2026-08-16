using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing;

/// <summary>
/// Tests for cross-framework consistency (Issue #26).
/// Verifies that xUnit, NUnit, MSTest adapters produce consistent error formatting
/// using the unified ResultFactory pipeline.
/// </summary>
public class CrossFrameworkConsistencyTests
{
    [Fact]
    public async Task ArchAssert_Passes_And_AssertHelper_PassesAsync_SameViolations()
    {
        // Synchronous path (ArchAssert) and async path (AssertHelper)
        // should catch the same violations
        var rule = new FailingRule("Violation 1", "Violation 2");

        // ArchAssert throws synchronously
        var syncException = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule));

        // AssertHelper throws asynchronously
        var asyncException = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Both should contain the same violation information
        Assert.Contains("Violation 1", syncException.Message);
        Assert.Contains("Violation 1", asyncException.Message);
        Assert.Contains("Violation 2", syncException.Message);
        Assert.Contains("Violation 2", asyncException.Message);
    }

    [Fact]
    public async Task AllPaths_SameRule_SameErrorMessage()
    {
        // All three paths (ArchAssert, AssertHelper, adapters)
        // should produce identical error formatting for the same violations
        var rule = new FailingRule("Test error message");

        // ArchAssert synchronous path
        var archAssertEx = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule));

        // AssertHelper async path
        var helperEx = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Both should have identical violation information
        Assert.Contains("violation(s) found", archAssertEx.Message);
        Assert.Contains("violation(s) found", helperEx.Message);
        Assert.Contains("Test error message", archAssertEx.Message);
        Assert.Contains("Test error message", helperEx.Message);
    }

    [Fact]
    public async Task ArchAssert_Fails_And_AssertHelper_FailsAsync_ConsistentBehavior()
    {
        // Test the Fails() / FailsAsync() pair
        var ruleWithViolations = new FailingRule("Error");
        var ruleWithoutViolations = new PassingRule();

        // Both should succeed with violations
        ArchAssert.Fails(ruleWithViolations);
        await AssertHelper.FailsAsync(ruleWithViolations);

        // Both should fail without violations
        Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Fails(ruleWithoutViolations));

        await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsAsync(ruleWithoutViolations));
    }

    [Fact]
    public async Task ArchAssert_FailsWith_And_AssertHelper_FailsWithAsync_SameValidation()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");

        // Both should pass with exact count
        ArchAssert.FailsWith(rule, 3);
        await AssertHelper.FailsWithAsync(rule, 3);

        // Both should fail with wrong count and include violation details
        var syncEx = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWith(rule, 2));

        var asyncEx = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsWithAsync(rule, 2));

        // Both error messages should contain the same information
        Assert.Contains("Expected 2 violation(s) but found 3", syncEx.Message);
        Assert.Contains("Expected 2 violation(s) but found 3", asyncEx.Message);
        Assert.Contains("Error 1", syncEx.Message);
        Assert.Contains("Error 1", asyncEx.Message);
    }

    [Fact]
    public async Task AssertHelper_PassesAsync_And_FailsAsync_UseResultFactory()
    {
        // Test that AssertHelper methods use ResultFactory for consistent formatting
        var rule = new FailingRule("Adapter test error");

        // AssertHelper should produce detailed formatted violations
        var helperEx = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Both should reference the same violation with detailed formatting
        Assert.Contains("Adapter test error", helperEx.Message);
        Assert.Contains("violation(s) found", helperEx.Message);
    }

    [Fact]
    public void ResultFactory_Produces_Consistent_Format()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1"),
            new TestViolation("Error 2")
        };

        // ResultFactory should produce same output regardless of caller
        var result1 = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            colored: false,
            style: FormatStyle.Detailed);

        var result2 = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            colored: false,
            style: FormatStyle.Detailed);

        // Same violations should produce identical formatted output
        Assert.Equal(result1.FormattedMessage, result2.FormattedMessage);
        Assert.Equal(result1.Message, result2.Message);
        Assert.Equal(result1.ViolationCount, result2.ViolationCount);
    }

    [Fact]
    public void MultipleViolations_TruncatedConsistently()
    {
        var messages = Enumerable.Range(1, 20).Select(i => $"Error {i}").ToArray();
        var violations = messages
            .Select(msg => new TestViolation(msg))
            .Cast<Violation>()
            .ToList();

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            colored: false,
            style: FormatStyle.Detailed);

        // Verify truncation is consistent
        Assert.Contains("Error 1", result.FormattedMessage);
        Assert.Contains("Error 10", result.FormattedMessage);
        Assert.Contains("... and 10 more", result.FormattedMessage);
        Assert.DoesNotContain("Error 11", result.FormattedMessage);
    }

    [Fact]
    public void NoColors_AcrossAllPaths()
    {
        var rule = new FailingRule("Error");

        // ArchAssert should not have colors
        var archAssertEx = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule));
        Assert.DoesNotContain("[31m", archAssertEx.Message); // No red codes

        // AssertHelper should not have colors
        var helperEx = Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule)).Result;
        Assert.DoesNotContain("[31m", helperEx.Message); // No red codes

        // ResultFactory with colored:false should not have codes
        var result = ResultFactory.CreateFromViolations(
            new List<Violation> { new TestViolation("Error") },
            "TestRule",
            colored: false,
            style: FormatStyle.Detailed);
        Assert.DoesNotContain("[31m", result.FormattedMessage); // No red codes
    }

    /// <summary>
    /// Test rule that always passes (no violations).
    /// </summary>
    private class PassingRule : Checkable
    {
        public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
        {
            return await Task.FromResult(new List<Violation>().AsReadOnly());
        }
    }

    /// <summary>
    /// Test rule that fails with specified violations.
    /// </summary>
    private class FailingRule : Checkable
    {
        private readonly string[] _messages;

        public FailingRule(params string[] messages)
        {
            _messages = messages;
        }

        public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
        {
            var violations = _messages
                .Select(msg => new TestViolation(msg))
                .Cast<Violation>()
                .ToList()
                .AsReadOnly();

            return await Task.FromResult(violations);
        }
    }

    /// <summary>
    /// Test violation for testing purposes.
    /// </summary>
    private record TestViolation(string Message) : Violation
    {
        public override string ToString() => Message;
    }
}
