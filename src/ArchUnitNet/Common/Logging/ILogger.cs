namespace ArchUnitNet.Common.Logging;

/// <summary>
/// Core logging interface for ArchUnit.
/// Implementations handle where and how log messages are output.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Log an informational message.
    /// </summary>
    void Info(string message);

    /// <summary>
    /// Log a warning message.
    /// </summary>
    void Warn(string message);

    /// <summary>
    /// Log an error message.
    /// </summary>
    void Error(string message);

    /// <summary>
    /// Log a debug/verbose message.
    /// Only logged if verbose mode is enabled.
    /// </summary>
    void Debug(string message);
}

/// <summary>
/// No-op logger that discards all log messages.
/// Used when logging is disabled.
/// </summary>
public class NullLogger : ILogger
{
    public static readonly NullLogger Instance = new();

    public void Info(string message) { }
    public void Warn(string message) { }
    public void Error(string message) { }
    public void Debug(string message) { }
}

/// <summary>
/// Logger that writes to the console (stdout).
/// Respects verbose mode for debug messages.
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly bool _verbose;

    public ConsoleLogger(bool verbose = false)
    {
        _verbose = verbose;
    }

    public void Info(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void Warn(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARN] {message}");
        Console.ForegroundColor = originalColor;
    }

    public void Error(string message)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {message}");
        Console.ForegroundColor = originalColor;
    }

    public void Debug(string message)
    {
        if (_verbose)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[DEBUG] {message}");
            Console.ForegroundColor = originalColor;
        }
    }
}
