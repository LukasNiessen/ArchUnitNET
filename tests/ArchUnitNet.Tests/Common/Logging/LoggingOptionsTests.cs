using ArchUnitNet.Common.Logging;

namespace ArchUnitNet.Tests.Common.Logging;

public class LoggingOptionsTests
{
    [Fact]
    public void DefaultConstructor_HasCorrectDefaults()
    {
        // Act
        var options = new LoggingOptions();

        // Assert
        Assert.False(options.Verbose);
        Assert.False(options.LogToConsole);
        Assert.Null(options.Context);
    }

    [Fact]
    public void CanSetVerbose()
    {
        // Act
        var options = new LoggingOptions(Verbose: true);

        // Assert
        Assert.True(options.Verbose);
    }

    [Fact]
    public void CanSetLogToConsole()
    {
        // Act
        var options = new LoggingOptions(LogToConsole: true);

        // Assert
        Assert.True(options.LogToConsole);
    }

    [Fact]
    public void CanSetContext()
    {
        // Act
        var options = new LoggingOptions(Context: "FileRules");

        // Assert
        Assert.Equal("FileRules", options.Context);
    }

    [Fact]
    public void CanCombineAllOptions()
    {
        // Act
        var options = new LoggingOptions(
            Verbose: true,
            LogToConsole: true,
            Context: "MetricsCheck");

        // Assert
        Assert.True(options.Verbose);
        Assert.True(options.LogToConsole);
        Assert.Equal("MetricsCheck", options.Context);
    }

    [Fact]
    public void IsRecord_SupportsEquality()
    {
        // Arrange
        var options1 = new LoggingOptions(Verbose: true);
        var options2 = new LoggingOptions(Verbose: true);

        // Act & Assert
        Assert.Equal(options1, options2);
    }
}
