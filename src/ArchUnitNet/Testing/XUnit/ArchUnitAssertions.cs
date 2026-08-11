using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Testing.XUnit;

/// <summary>
/// xUnit-native extension methods for architecture assertions.
/// Seamless integration with xUnit's Assert API.
/// </summary>
public static class ArchUnitAssertions
{
    /// <summary>
    /// Assert that an architecture rule passes (no violations).
    /// Example: await Assert.That(rule).PassesAsync();
    /// </summary>
    public static async Task PassesAsync(this Checkable rule, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.PassesAsync(rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            throw new Xunit.Sdk.XunitException(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails (has violations).
    /// Example: await Assert.That(rule).FailsAsync();
    /// </summary>
    public static async Task FailsAsync(this Checkable rule, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsAsync(rule, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            throw new Xunit.Sdk.XunitException(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule fails with exactly N violations.
    /// Example: await Assert.That(rule).FailsWithAsync(3);
    /// </summary>
    public static async Task FailsWithAsync(this Checkable rule, int expectedViolationCount, CheckOptions? options = null)
    {
        try
        {
            await AssertHelper.FailsWithAsync(rule, expectedViolationCount, options);
        }
        catch (AssertHelper.AssertException ex)
        {
            throw new Xunit.Sdk.XunitException(ex.Message);
        }
    }

    /// <summary>
    /// Assert that an architecture rule produces violations containing specific text.
    /// Example: await Assert.That(rule).FailsWithMessageContainingAsync("circular dependency");
    /// </summary>
    public static async Task FailsWithMessageContainingAsync(
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
            throw new Xunit.Sdk.XunitException(ex.Message);
        }
    }
}

/// <summary>
/// Fluent xUnit assertions using method chaining.
/// Alternative to extension methods: Assert.ArchUnit(rule).Should().Pass()
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
            throw new Xunit.Sdk.XunitException(ex.Message);
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
            throw new Xunit.Sdk.XunitException(ex.Message);
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
            throw new Xunit.Sdk.XunitException(ex.Message);
        }
    }
}
