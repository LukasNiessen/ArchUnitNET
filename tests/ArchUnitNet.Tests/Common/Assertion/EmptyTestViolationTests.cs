using ArchUnitNet.Common.Assertion;

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
        violation.Pattern.Should().Be(pattern);
        violation.Context.Should().Be(context);
    }

    [Fact]
    public void Constructor_WithPatternOnly_UsesDefaultContext()
    {
        // Arrange
        var pattern = "src/Controllers/**";

        // Act
        var violation = new EmptyTestViolation(pattern);

        // Assert
        violation.Pattern.Should().Be(pattern);
        violation.Context.Should().Be("file selection");
    }

    [Fact]
    public void Implements_Violation()
    {
        // Arrange
        var violation = new EmptyTestViolation("test");

        // Act & Assert
        ((Violation)violation).Should().NotBeNull();
    }

    [Fact]
    public void ToString_ContainsPatternAndContext()
    {
        // Arrange
        var violation = new EmptyTestViolation("src/Controllers/**", "files in folder");

        // Act
        var str = violation.ToString();

        // Assert
        str.Should().Contain("EmptyTestViolation");
        str.Should().Contain("src/Controllers/**");
        str.Should().Contain("files in folder");
    }

    [Fact]
    public void Equality_SamePatternAndContextAreEqual()
    {
        // Arrange
        var v1 = new EmptyTestViolation("src/**", "files");
        var v2 = new EmptyTestViolation("src/**", "files");

        // Act & Assert
        v1.Should().Be(v2);
    }

    [Fact]
    public void Equality_DifferentPatternsAreNotEqual()
    {
        // Arrange
        var v1 = new EmptyTestViolation("src/**");
        var v2 = new EmptyTestViolation("tests/**");

        // Act & Assert
        v1.Should().NotBe(v2);
    }
}
