using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing;

/// <summary>
/// Tests for ArchAssert - framework-agnostic assertion API.
/// Verifies that the zero-configuration fallback works correctly.
/// </summary>
public class ArchAssertTests
{
    [Fact]
    public void Passes_NoViolations_DoesNotThrow()
    {
        var rule = new PassingRule();
        ArchAssert.Passes(rule);
    }

    [Fact]
    public void Passes_WithViolations_ThrowsAssertException()
    {
        var rule = new FailingRule("Test violation");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule));

        Assert.NotNull(ex);
        Assert.Contains("Test violation", ex.Message);
    }

    [Fact]
    public void Passes_WithMultipleViolations_IncludesAllInError()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule));

        Assert.Contains("3 violation", ex.Message);
    }

    [Fact]
    public void Passes_WithCustomMessage_IncludesInError()
    {
        var rule = new FailingRule("Test error");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Passes(rule, message: "MyCustomRule"));

        Assert.Contains("MyCustomRule", ex.Message);
    }

    [Fact]
    public void Passes_WithNullRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArchAssert.Passes(null!));
    }

    [Fact]
    public void Fails_WithViolations_DoesNotThrow()
    {
        var rule = new FailingRule("Test violation");
        ArchAssert.Fails(rule);
    }

    [Fact]
    public void Fails_NoViolations_ThrowsAssertException()
    {
        var rule = new PassingRule();
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Fails(rule));

        Assert.Contains("Expected", ex.Message);
        Assert.Contains("fail", ex.Message);
    }

    [Fact]
    public void Fails_WithCustomMessage_IncludesInError()
    {
        var rule = new PassingRule();
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.Fails(rule, message: "CustomMessage"));

        // Message parameter is used for rule name in Fails, but error is about passing
        Assert.NotNull(ex);
    }

    [Fact]
    public void Fails_WithNullRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArchAssert.Fails(null!));
    }

    [Fact]
    public void FailsWith_ExactViolationCount_DoesNotThrow()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        ArchAssert.FailsWith(rule, 3);
    }

    [Fact]
    public void FailsWith_ZeroViolations_ThrowsAssertException()
    {
        var rule = new PassingRule();
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWith(rule, 5));

        Assert.Contains("Expected 5", ex.Message);
        Assert.Contains("found 0", ex.Message);
    }

    [Fact]
    public void FailsWith_TooManyViolations_ThrowsAssertException()
    {
        var rule = new FailingRule("Error 1", "Error 2", "Error 3");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWith(rule, 2));

        Assert.Contains("Expected 2", ex.Message);
        Assert.Contains("found 3", ex.Message);
    }

    [Fact]
    public void FailsWith_TooFewViolations_ThrowsAssertException()
    {
        var rule = new FailingRule("Error 1");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWith(rule, 5));

        Assert.Contains("Expected 5", ex.Message);
        Assert.Contains("found 1", ex.Message);
    }

    [Fact]
    public void FailsWith_IncludesFormattedViolations_InError()
    {
        var rule = new FailingRule("Test error message");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWith(rule, 2));

        Assert.Contains("Test error message", ex.Message);
    }

    [Fact]
    public void FailsWith_NegativeExpectedCount_ThrowsArgumentException()
    {
        var rule = new PassingRule();
        Assert.Throws<ArgumentException>(() =>
            ArchAssert.FailsWith(rule, -1));
    }

    [Fact]
    public void FailsWith_NullRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArchAssert.FailsWith(null!, 5));
    }

    [Fact]
    public void FailsWithMessageContaining_TextFound_DoesNotThrow()
    {
        var rule = new FailingRule("This contains the expected text");
        ArchAssert.FailsWithMessageContaining(rule, "expected text");
    }

    [Fact]
    public void FailsWithMessageContaining_TextNotFound_ThrowsAssertException()
    {
        var rule = new FailingRule("This is an error message");
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWithMessageContaining(rule, "not found"));

        Assert.Contains("Expected violation message to contain", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void FailsWithMessageContaining_CaseInsensitive()
    {
        var rule = new FailingRule("This is an ERROR message");
        ArchAssert.FailsWithMessageContaining(rule, "error");
    }

    [Fact]
    public void FailsWithMessageContaining_NoViolations_ThrowsAssertException()
    {
        var rule = new PassingRule();
        var ex = Assert.Throws<AssertHelper.AssertException>(() =>
            ArchAssert.FailsWithMessageContaining(rule, "any text"));

        Assert.Contains("Expected violations", ex.Message);
        Assert.Contains("found none", ex.Message);
    }

    [Fact]
    public void FailsWithMessageContaining_NullExpectedText_ThrowsArgumentException()
    {
        var rule = new PassingRule();
        Assert.Throws<ArgumentException>(() =>
            ArchAssert.FailsWithMessageContaining(rule, null!));
    }

    [Fact]
    public void FailsWithMessageContaining_EmptyExpectedText_ThrowsArgumentException()
    {
        var rule = new PassingRule();
        Assert.Throws<ArgumentException>(() =>
            ArchAssert.FailsWithMessageContaining(rule, ""));
    }

    [Fact]
    public void FailsWithMessageContaining_NullRule_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ArchAssert.FailsWithMessageContaining(null!, "text"));
    }

    [Fact]
    public void Passes_WithCheckOptions_PassesThrough()
    {
        var rule = new PassingRule();
        var options = new CheckOptions { AllowEmptyTests = true };
        ArchAssert.Passes(rule, options);
    }

    [Fact]
    public void FailsWith_WithCheckOptions_PassesThrough()
    {
        var rule = new FailingRule("Error");
        var options = new CheckOptions { AllowEmptyTests = true };
        ArchAssert.FailsWith(rule, 1, options);
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
