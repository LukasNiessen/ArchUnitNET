using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing.Common;

/// <summary>
/// Tests for AssertHelper integration with ResultFactory (Issue #26).
/// Verifies that async adapters use unified violation formatting.
/// </summary>
public class AssertHelperResultFactoryTests
{
    [Fact]
    public async Task PassesAsync_WithViolations_UsesResultFactory()
    {
        var rule = new FailingRule("Test violation message");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Verify ResultFactory formatting is used (contains violation count indicator)
        Assert.Contains("violation(s) found", ex.Message);
    }

    [Fact]
    public async Task PassesAsync_WithMultipleViolations_IncludesAllInError()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Verify all violations are included in formatted output
        Assert.Contains("Error 1", ex.Message);
        Assert.Contains("Error 2", ex.Message);
        Assert.Contains("Error 3", ex.Message);
        Assert.Contains("3 violation", ex.Message);
    }

    [Fact]
    public async Task PassesAsync_DetailedFormat_ShowsFirst10()
    {
        var messages = Enumerable.Range(1, 15).Select(i => $"Error {i}").ToArray();
        var rule = new FailingRule(messages);
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Verify detailed format with truncation
        Assert.Contains("Error 1", ex.Message);
        Assert.Contains("Error 10", ex.Message);
        Assert.Contains("... and 5 more", ex.Message);
        Assert.DoesNotContain("Error 11", ex.Message);
    }

    [Fact]
    public async Task PassesAsync_WithCustomMessage_IncludesRuleName()
    {
        var rule = new FailingRule("Test error");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule, message: "MyCustomRule"));

        // Verify rule name is used in formatting
        Assert.Contains("MyCustomRule", ex.Message);
    }

    [Fact]
    public async Task PassesAsync_Colored_False_NoANSICodes()
    {
        var rule = new FailingRule("Test error");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.PassesAsync(rule));

        // Verify no ANSI colour codes (colored: false)
        Assert.DoesNotContain("[31m", ex.Message); // Red colour code
        Assert.DoesNotContain("[0m", ex.Message);  // Reset code
    }

    [Fact]
    public async Task FailsWithAsync_ExactCount_DoesNotThrow()
    {
        var rule = new FailingRule("Error 1", "Error 2");
        await AssertHelper.FailsWithAsync(rule, 2);
    }

    [Fact]
    public async Task FailsWithAsync_WrongCount_ThrowsWithDetailedFormat()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsWithAsync(rule, 2));

        // Verify uses detailed formatting from ResultFactory
        Assert.Contains("Expected 2 violation(s) but found 3", ex.Message);
        Assert.Contains("Error 1", ex.Message);
    }

    [Fact]
    public async Task FailsWithAsync_IncludesFormattedViolations()
    {
        var rule = new FailingRule("Important error message");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsWithAsync(rule, 5));

        // Verify violation details are included
        Assert.Contains("Important error message", ex.Message);
    }

    [Fact]
    public async Task FailsWithMessageContainingAsync_TextFound_DoesNotThrow()
    {
        var rule = new FailingRule("This contains the expected text");
        await AssertHelper.FailsWithMessageContainingAsync(rule, "expected text");
    }

    [Fact]
    public async Task FailsWithMessageContainingAsync_TextNotFound_ThrowsWithDetailedFormat()
    {
        var rule = new FailingRule("Error message without target");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsWithMessageContainingAsync(rule, "target"));

        // Verify uses detailed formatting from ResultFactory
        Assert.Contains("Expected violation message to contain", ex.Message);
        Assert.Contains("target", ex.Message);
        Assert.Contains("Error message without target", ex.Message);
    }

    [Fact]
    public async Task FailsWithMessageContainingAsync_IncludesFormattedViolations()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        var ex = await Assert.ThrowsAsync<AssertHelper.AssertException>(() =>
            AssertHelper.FailsWithMessageContainingAsync(rule, "nonexistent"));

        // Verify all violations shown in detailed format
        Assert.Contains("Error 1", ex.Message);
        Assert.Contains("Error 2", ex.Message);
        Assert.Contains("Error 3", ex.Message);
    }

    [Fact]
    public async Task PassesAsync_WithCheckOptions_PassesThrough()
    {
        var rule = new PassingRule();
        var options = new CheckOptions { AllowEmptyTests = true };
        await AssertHelper.PassesAsync(rule, options);
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
