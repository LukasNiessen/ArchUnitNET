using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing.Common;

/// <summary>
/// Tests for ViolationFactory - single violation formatting.
/// </summary>
public class ViolationFactoryTests
{
    [Fact]
    public void FormatViolation_WithValidViolation_ReturnsFormattedString()
    {
        var violation = new TestViolation("Test error message");
        var result = ViolationFactory.FormatViolation(violation);

        Assert.NotNull(result);
        Assert.Contains("[✗]", result);
        Assert.Contains("Test error message", result);
    }

    [Fact]
    public void FormatViolation_WithIndex_IncludesNumbering()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, index: 1);

        Assert.Contains("1. ", result);
    }

    [Fact]
    public void FormatViolation_WithoutIndex_NoNumbering()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, index: 0);

        Assert.DoesNotContain("1. ", result);
    }

    [Fact]
    public void FormatViolation_WithTypeName_ShowsType()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, showTypeName: true);

        Assert.Contains("TestViolation", result);
    }

    [Fact]
    public void FormatViolation_WithoutTypeName_OmitsType()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, showTypeName: false);

        Assert.DoesNotContain("TestViolation", result);
    }

    [Fact]
    public void FormatViolation_Colored_IncludesANSICodes()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, colored: true);

        Assert.Contains(Colours.Reset, result);
        Assert.Contains(Colours.Error, result);
    }

    [Fact]
    public void FormatViolation_Uncolored_NoANSICodes()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, colored: false);

        Assert.DoesNotContain(Colours.Reset, result);
        Assert.DoesNotContain(Colours.Error, result);
    }

    [Fact]
    public void FormatViolation_NullViolation_ReturnsPlaceholder()
    {
        var result = ViolationFactory.FormatViolation(null!);

        Assert.Equal("No violation information", result);
    }

    [Fact]
    public void FormatViolation_MultipleIndices_CorrectNumbering()
    {
        var v1 = new TestViolation("Error 1");
        var v2 = new TestViolation("Error 2");

        var result1 = ViolationFactory.FormatViolation(v1, index: 1);
        var result2 = ViolationFactory.FormatViolation(v2, index: 2);

        Assert.Contains("1. ", result1);
        Assert.Contains("2. ", result2);
    }

    [Fact]
    public void FormatViolation_WithNewlines_PreservesStructure()
    {
        var violation = new TestViolation("Test error");
        var result = ViolationFactory.FormatViolation(violation, index: 1, showTypeName: true);

        Assert.Contains("\n", result);
    }

    [Fact]
    public void FormatViolation_EmptyMessage_HandlesGracefully()
    {
        var violation = new TestViolation("");
        var result = ViolationFactory.FormatViolation(violation);

        Assert.NotNull(result);
        Assert.Contains("[✗]", result);
    }

    /// <summary>
    /// Test violation type for testing purposes.
    /// </summary>
    private record TestViolation(string Message) : Violation
    {
        public override string ToString() => Message;
    }
}
