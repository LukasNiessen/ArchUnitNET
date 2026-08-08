using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Tests.Common.PatternMatching;

public class GlobPatternTests
{
    [Theory]
    [InlineData("src/**/*.cs", "src/Common/Error.cs", true)]
    [InlineData("src/**/*.cs", "src/Files/FluentApi.cs", true)]
    [InlineData("src/**/*.cs", "tests/Common/Error.cs", false)]
    [InlineData("src/Common/**", "src/Common/Error.cs", true)]
    [InlineData("src/Common/**", "src/Files/Error.cs", false)]
    public void Matches_WithGlobPattern_ReturnsExpected(string pattern, string path, bool expected)
    {
        // Arrange
        var matcher = new GlobPattern(pattern);

        // Act
        var result = matcher.Matches(path);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Matches_WithRecursiveWildcard_MatchesNestedPaths()
    {
        // Arrange
        var matcher = new GlobPattern("src/**/internal/**");

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/internal/Helper.cs"));
        Assert.True(matcher.Matches("src/Files/internal/Parser.cs"));
        Assert.False(matcher.Matches("src/Common/Helper.cs"));
    }

    [Fact]
    public void Matches_WithSingleWildcard_MatchesSingleSegment()
    {
        // Arrange
        var matcher = new GlobPattern("src/*/Error.cs");

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/Error.cs"));
        Assert.False(matcher.Matches("src/Common/Sub/Error.cs"));
    }

    [Fact]
    public void Matches_WithCharacterClass_Works()
    {
        // Arrange
        var matcher = new GlobPattern("src/**/*[Tt]est.cs");

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/ErrorTest.cs"));
        Assert.True(matcher.Matches("src/Common/Errortest.cs"));
        Assert.False(matcher.Matches("src/Common/Error.cs"));
    }

    [Fact]
    public void Matches_WithExtension_Works()
    {
        // Arrange
        var matcher = new GlobPattern("**/*.cs");

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/Error.cs"));
        Assert.False(matcher.Matches("src/Common/Error.txt"));
    }
}

public class GlobFilterTests
{
    [Fact]
    public void Matches_WithIncludePattern_ReturnsTrue()
    {
        // Arrange
        var filter = new GlobFilter(include: "src/**");

        // Act & Assert
        Assert.True(filter.Matches("src/Common/Error.cs"));
        Assert.False(filter.Matches("tests/Common/Error.cs"));
    }

    [Fact]
    public void Matches_WithExcludePattern_ReturnsFalse()
    {
        // Arrange
        var filter = new GlobFilter(include: "src/**", exclude: new[] { "src/internal/**" });

        // Act & Assert
        Assert.True(filter.Matches("src/Common/Error.cs"));
        Assert.False(filter.Matches("src/internal/Helper.cs"));
    }

    [Fact]
    public void Matches_WithMultipleExcludes_WorksCorrectly()
    {
        // Arrange
        var filter = new GlobFilter(
            include: "src/**",
            exclude: new[] { "src/internal/**", "src/**/temp/**" }
        );

        // Act & Assert
        Assert.True(filter.Matches("src/Common/Error.cs"));
        Assert.False(filter.Matches("src/internal/Helper.cs"));
        Assert.False(filter.Matches("src/Common/temp/Cache.cs"));
    }

    [Fact]
    public void Matches_WithoutInclude_DefaultsToAll()
    {
        // Arrange
        var filter = new GlobFilter(include: null, exclude: new[] { "**/*.tmp" });

        // Act & Assert
        Assert.True(filter.Matches("src/Common/Error.cs"));
        Assert.False(filter.Matches("src/Common/Cache.tmp"));
    }
}

public class RegexPatternTests
{
    [Theory]
    [InlineData(@"src/.*\.cs$", "src/Common/Error.cs", true)]
    [InlineData(@"src/.*\.cs$", "tests/Common/Error.cs", false)]
    [InlineData(@"(src|tests)/.*/(internal|temp)/.*", "src/Common/internal/Helper.cs", true)]
    public void Matches_WithRegexPattern_ReturnsExpected(string pattern, string path, bool expected)
    {
        // Arrange
        var matcher = new RegexPattern(pattern);

        // Act
        var result = matcher.Matches(path);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Constructor_WithInvalidRegex_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new RegexPattern("[invalid(regex"));
        Assert.Contains("regex", ex.Message.ToLower());
    }
}

public class PatternMatcherTests
{
    [Fact]
    public void Matches_WithGlobPattern_Works()
    {
        // Arrange
        var matcher = new PatternMatcher("src/**/*.cs");

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/Error.cs"));
        Assert.False(matcher.Matches("tests/Common/Error.cs"));
    }

    [Fact]
    public void Matches_WithRegexPattern_Works()
    {
        // Arrange
        var matcher = new PatternMatcher(@"^src/.*\.cs$", isRegex: true, exclude: null);

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/Error.cs"));
        Assert.False(matcher.Matches("tests/Common/Error.cs"));
    }

    [Fact]
    public void Matches_WithExclusions_Works()
    {
        // Arrange
        var matcher = new PatternMatcher(
            "src/**",
            isRegex: false,
            exclude: new[] { "src/internal/**" }
        );

        // Act & Assert
        Assert.True(matcher.Matches("src/Common/Error.cs"));
        Assert.False(matcher.Matches("src/internal/Helper.cs"));
    }
}
