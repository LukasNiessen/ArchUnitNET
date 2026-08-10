using ArchUnitNet.Common.Assertion;
using Xunit;

namespace ArchUnitNet.Tests.Common.Assertion;

public class EmptyTestViolationTests
{
    [Fact]
    public void Constructor_WithPatternAndContext()
    {
        // Arrange
        var pattern = "src/Controllers/**";
        var context = "files in path";

        // Act
        var violation = new EmptyTestViolation(pattern, context);

        // Assert
        Assert.Equal(pattern, violation.Pattern);
        Assert.Equal(context, violation.Context);
    }

    [Fact]
    public void Constructor_WithPatternOnly_UsesDefaultContext()
    {
        // Arrange
        var pattern = "src/Controllers/**";

        // Act
        var violation = new EmptyTestViolation(pattern);

        // Assert
        Assert.Equal(pattern, violation.Pattern);
        Assert.Equal("file selection", violation.Context);
    }

    [Fact]
    public void Implements_Violation()
    {
        // Arrange
        var violation = new EmptyTestViolation("test");

        // Act & Assert
        Assert.NotNull((Violation)violation);
    }

    [Fact]
    public void ToString_ContainsPatternAndContext()
    {
        // Arrange
        var violation = new EmptyTestViolation("src/Controllers/**", "files in folder");

        // Act
        var str = violation.ToString();

        // Assert
        Assert.Contains("EmptyTestViolation", str);
        Assert.Contains("src/Controllers/**", str);
        Assert.Contains("files in folder", str);
    }

    [Fact]
    public void Equality_SamePatternAndContextAreEqual()
    {
        // Arrange
        var v1 = new EmptyTestViolation("src/**", "files");
        var v2 = new EmptyTestViolation("src/**", "files");

        // Act & Assert
        Assert.Equal(v1, v2);
    }

    [Fact]
    public void Equality_DifferentPatternsAreNotEqual()
    {
        // Arrange
        var v1 = new EmptyTestViolation("src/**");
        var v2 = new EmptyTestViolation("tests/**");

        // Act & Assert
        Assert.NotEqual(v1, v2);
    }
}
