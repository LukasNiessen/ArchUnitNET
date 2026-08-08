using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.Logging;

namespace ArchUnitNet.Tests.Common.FluentApi;

public class CheckOptionsTests
{
    [Fact]
    public void DefaultConstructor_HasCorrectDefaults()
    {
        // Act
        var options = new CheckOptions();

        // Assert
        options.AllowEmptyTests.Should().BeFalse();
        options.Logging.Should().BeNull();
        options.ClearCache.Should().BeFalse();
        options.TimeoutMs.Should().BeNull();
    }

    [Fact]
    public void CanSetAllowEmptyTests()
    {
        // Act
        var options = new CheckOptions(AllowEmptyTests: true);

        // Assert
        options.AllowEmptyTests.Should().BeTrue();
    }

    [Fact]
    public void CanSetLoggingOptions()
    {
        // Arrange
        var logging = new LoggingOptions(Verbose: true);

        // Act
        var options = new CheckOptions(Logging: logging);

        // Assert
        options.Logging.Should().Be(logging);
        options.Logging?.Verbose.Should().BeTrue();
    }

    [Fact]
    public void CanSetClearCache()
    {
        // Act
        var options = new CheckOptions(ClearCache: true);

        // Assert
        options.ClearCache.Should().BeTrue();
    }

    [Fact]
    public void CanSetTimeout()
    {
        // Act
        var options = new CheckOptions(TimeoutMs: 5000);

        // Assert
        options.TimeoutMs.Should().Be(5000);
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
        options.AllowEmptyTests.Should().BeTrue();
        options.Logging.Should().Be(logging);
        options.ClearCache.Should().BeTrue();
        options.TimeoutMs.Should().Be(10000);
    }

    [Fact]
    public void IsRecord_SupportsEquality()
    {
        // Arrange
        var options1 = new CheckOptions(AllowEmptyTests: true);
        var options2 = new CheckOptions(AllowEmptyTests: true);

        // Act & Assert
        options1.Should().Be(options2);
    }
}
