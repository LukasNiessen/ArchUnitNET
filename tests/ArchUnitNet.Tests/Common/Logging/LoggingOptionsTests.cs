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
        options.Verbose.Should().BeFalse();
        options.LogToConsole.Should().BeFalse();
        options.Context.Should().BeNull();
    }

    [Fact]
    public void CanSetVerbose()
    {
        // Act
        var options = new LoggingOptions(Verbose: true);

        // Assert
        options.Verbose.Should().BeTrue();
    }

    [Fact]
    public void CanSetLogToConsole()
    {
        // Act
        var options = new LoggingOptions(LogToConsole: true);

        // Assert
        options.LogToConsole.Should().BeTrue();
    }

    [Fact]
    public void CanSetContext()
    {
        // Act
        var options = new LoggingOptions(Context: "FileRules");

        // Assert
        options.Context.Should().Be("FileRules");
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
        options.Verbose.Should().BeTrue();
        options.LogToConsole.Should().BeTrue();
        options.Context.Should().Be("MetricsCheck");
    }

    [Fact]
    public void IsRecord_SupportsEquality()
    {
        // Arrange
        var options1 = new LoggingOptions(Verbose: true);
        var options2 = new LoggingOptions(Verbose: true);

        // Act & Assert
        options1.Should().Be(options2);
    }
}
