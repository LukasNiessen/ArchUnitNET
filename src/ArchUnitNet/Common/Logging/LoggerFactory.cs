namespace ArchUnitNet.Common.Logging;

/// <summary>
/// Factory for creating logger instances.
/// Provides convenient methods for the most common logging scenarios.
/// </summary>
public static class LoggerFactory
{
    /// <summary>
    /// Create a logger based on LoggingOptions.
    /// Returns NullLogger if LogToConsole is false.
    /// </summary>
    public static ILogger Create(LoggingOptions? options = null)
    {
        options ??= new LoggingOptions();

        if (!options.LogToConsole)
            return NullLogger.Instance;

        return new ConsoleLogger(options.Verbose);
    }

    /// <summary>
    /// Create a console logger with explicit verbose setting.
    /// </summary>
    public static ILogger CreateConsole(bool verbose = false)
    {
        return new ConsoleLogger(verbose);
    }

    /// <summary>
    /// Get the null logger (no-op).
    /// </summary>
    public static ILogger Null => NullLogger.Instance;
}
