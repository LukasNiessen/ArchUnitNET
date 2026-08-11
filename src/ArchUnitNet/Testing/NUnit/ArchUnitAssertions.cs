using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing.Common;
using NUnit.Framework;

namespace ArchUnitNet.Testing.NUnit;

/// <summary>
/// NUnit-native extension methods for architecture assertions.
/// Integrates with NUnit's Assert API.
/// </summary>
public static class ArchUnitAssertions
{
    /// <summary>
    /// Assert that an architecture rule passes (no violations).
    /// Example: await rule.Should().PassAsync();
    /// </summary>
    public static async Task PassAsync(this Checkable rule, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.PassesAsync(rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails (has violations).
    /// Example: await rule.Should().FailAsync();
    /// </summary>
    public static async Task FailAsync(this Checkable rule, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsAsync(rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails with exactly N violations.
    /// Example: await rule.Should().FailWithAsync(3);
    /// </summary>
    public static async Task FailWithAsync(this Checkable rule, int expectedViolationCount, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsWithAsync(rule, expectedViolationCount, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule produces violations containing specific text.
    /// Example: await rule.Should().FailWithMessageContainingAsync("circular");
    /// </summary>
    public static async Task FailWithMessageContainingAsync(
        this Checkable rule,
        string expectedText,
        CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsWithMessageContainingAsync(rule, expectedText, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}

/// <summary>
/// Fluent NUnit assertions using method chaining.
/// Alternative API: That(rule).Should().Pass()
/// </summary>
public class ArchUnitAssert
{
    private readonly Checkable _rule;

    private ArchUnitAssert(Checkable rule)
    {
        _rule = rule;
    }

    public static ArchUnitAssert That(Checkable rule) => new(rule);

    public ArchUnitAssertRuleAssertion Should() => new(_rule);
}

/// <summary>
/// Fluent assertion methods for rules.
/// </summary>
public class ArchUnitAssertRuleAssertion
{
    private readonly Checkable _rule;

    internal ArchUnitAssertRuleAssertion(Checkable rule)
    {
        _rule = rule;
    }

    /// <summary>
    /// Assert rule passes (no violations).
    /// </summary>
    public async Task PassAsync(CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.PassesAsync(_rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Assert rule fails (has violations).
    /// </summary>
    public async Task FailAsync(CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsAsync(_rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    /// <summary>
    /// Assert rule fails with exactly N violations.
    /// </summary>
    public async Task FailWithAsync(int expectedViolationCount, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsWithAsync(_rule, expectedViolationCount, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}
