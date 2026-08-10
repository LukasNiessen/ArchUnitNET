using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.Logging;
using Xunit;

namespace ArchUnitNet.Tests.Common.FluentApi;

public class CheckOptionsTests
{
    [Fact]
    public void DefaultConstructor_HasCorrectDefaults()
    {
        // Act
        var options = new CheckOptions();

        // Assert
        Assert.False(options.AllowEmptyTests);
        Assert.Null(options.Logging);
        Assert.False(options.ClearCache);
        Assert.Null(options.TimeoutMs);
    }

    [Fact]
    public void CanSetAllowEmptyTests()
    {
        // Act
        var options = new CheckOptions(AllowEmptyTests: true);

        // Assert
        Assert.True(options.AllowEmptyTests);
    }

    [Fact]
    public void CanSetLoggingOptions()
    {
        // Arrange
        var logging = new LoggingOptions(Verbose: true);

        // Act
        var options = new CheckOptions(Logging: logging);

        // Assert
        Assert.Equal(logging, options.Logging);
        Assert.True(options.Logging?.Verbose);
    }

    [Fact]
    public void CanSetClearCache()
    {
        // Act
        var options = new CheckOptions(ClearCache: true);

        // Assert
        Assert.True(options.ClearCache);
    }

    [Fact]
    public void CanSetTimeout()
    {
        // Act
        var options = new CheckOptions(TimeoutMs: 5000);

        // Assert
        Assert.Equal(5000, options.TimeoutMs);
    }

    [Fact]
    public void CanCombineMultipleOptions()
    {
        // Arrange
        var logging = new LoggingOptions(Verbose: true, LogToConsole: true);

        // Act
        var options = new CheckOptions(
            AllowEmptyTests: true,
            Logging: logging,
            ClearCache: true,
            TimeoutMs: 10000);

        // Assert
        Assert.True(options.AllowEmptyTests);
        Assert.Equal(logging, options.Logging);
        Assert.True(options.ClearCache);
        Assert.Equal(10000, options.TimeoutMs);
    }

    [Fact]
    public void IsRecord_SupportsEquality()
    {
        // Arrange
        var options1 = new CheckOptions(AllowEmptyTests: true);
        var options2 = new CheckOptions(AllowEmptyTests: true);

        // Act & Assert
        Assert.Equal(options1, options2);
    }
}
