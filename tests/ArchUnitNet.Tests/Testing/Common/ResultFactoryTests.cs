using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing.Common;

/// <summary>
/// Tests for ResultFactory - violation list to test result conversion.
/// </summary>
public class ResultFactoryTests
{
    [Fact]
    public void CreateFromViolations_NoViolations_ReturnsPassed()
    {
        var violations = new List<Violation>();
        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.True(result.Passed);
        Assert.False(result.Failed);
        Assert.Equal(0, result.ViolationCount);
    }

    [Fact]
    public void CreateFromViolations_WithViolations_ReturnsFailed()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1")
        };
        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.False(result.Passed);
        Assert.True(result.Failed);
        Assert.Equal(1, result.ViolationCount);
    }

    [Fact]
    public void CreateFromViolations_PassedResult_IncludesMessage()
    {
        var violations = new List<Violation>();
        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.Contains("TestRule", result.Message);
    }

    [Fact]
    public void CreateFromViolations_FailedResult_IncludesViolationCount()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1"),
            new TestViolation("Error 2")
        };
        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.Contains("2", result.Message);
    }

    [Fact]
    public void CreateFromViolations_DetailedStyle_ShowsFirst10()
    {
        var violations = new List<Violation>();
        for (int i = 0; i < 15; i++)
        {
            violations.Add(new TestViolation($"Error {i + 1}"));
        }

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            style: FormatStyle.Detailed);

        Assert.Contains("Error 1", result.FormattedMessage);
        Assert.Contains("Error 10", result.FormattedMessage);
        Assert.Contains("... and 5 more", result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_DetailedStyle_TruncatesOver10()
    {
        var violations = new List<Violation>();
        for (int i = 0; i < 15; i++)
        {
            violations.Add(new TestViolation($"Error {i + 1}"));
        }

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            style: FormatStyle.Detailed);

        Assert.DoesNotContain("Error 11", result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_GroupedStyle_CategorizesByType()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1"),
            new TestViolation("Error 2"),
            new OtherViolation("Other 1")
        };

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            style: FormatStyle.Grouped);

        Assert.Contains("TestViolation", result.FormattedMessage);
        Assert.Contains("OtherViolation", result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_CompactStyle_ShowsSummary()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1"),
            new TestViolation("Error 2")
        };

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            style: FormatStyle.Compact);

        Assert.Contains("✗", result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_ColoredTrue_IncludesANSICodes()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1")
        };

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            colored: true);

        Assert.Contains(Colours.Reset, result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_ColoredFalse_NoANSICodes()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1")
        };

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            colored: false);

        Assert.DoesNotContain(Colours.Reset, result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_NullViolations_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ResultFactory.CreateFromViolations(null!, "TestRule"));
    }

    [Fact]
    public void CreateFromViolations_PassedResult_IncludesFormattedMessage()
    {
        var violations = new List<Violation>();
        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.NotEmpty(result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_FailedResult_IncludesFormattedMessage()
    {
        var violations = new List<Violation>
        {
            new TestViolation("Error 1")
        };

        var result = ResultFactory.CreateFromViolations(violations, "TestRule");

        Assert.NotEmpty(result.FormattedMessage);
    }

    [Fact]
    public void CreateFromViolations_ExactlyTenViolations_NoTruncation()
    {
        var violations = new List<Violation>();
        for (int i = 0; i < 10; i++)
        {
            violations.Add(new TestViolation($"Error {i + 1}"));
        }

        var result = ResultFactory.CreateFromViolations(
            violations,
            "TestRule",
            style: FormatStyle.Detailed);

        Assert.DoesNotContain("... and", result.FormattedMessage);
    }

    /// <summary>
    /// Test violation type.
    /// </summary>
    private record TestViolation(string Message) : Violation
    {
        public override string ToString() => Message;
    }

    /// <summary>
    /// Other test violation type for grouping tests.
    /// </summary>
    private record OtherViolation(string Message) : Violation
    {
        public override string ToString() => Message;
    }
}
