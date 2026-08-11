using System.Diagnostics;

namespace ArchUnitNet.Common.Logging;

/// <summary>
/// Enhanced logging with structured logging, exceptions, and performance tracking.
/// Extends the basic ILogger interface with additional capabilities.
/// </summary>
public interface IEnhancedLogger : ILogger
{
    /// <summary>
    /// Log with a category/context tag.
    /// </summary>
    void Info(string category, string message);

    /// <summary>
    /// Log a warning with category.
    /// </summary>
    void Warn(string category, string message);

    /// <summary>
    /// Log an error with category.
    /// </summary>
    void Error(string category, string message);

    /// <summary>
    /// Log an exception.
    /// </summary>
    void Error(string message, Exception exception);

    /// <summary>
    /// Log an exception with category.
    /// </summary>
    void Error(string category, string message, Exception exception);

    /// <summary>
    /// Log a debug message with category.
    /// </summary>
    void Debug(string category, string message);

    /// <summary>
    /// Log timing information (performance metrics).
    /// </summary>
    void Timing(string operation, TimeSpan duration);

    /// <summary>
    /// Start a performance timer and get disposable context.
    /// </summary>
    IDisposable StartTimer(string operation);
}

/// <summary>
/// No-op implementation of enhanced logger.
/// </summary>
public class NullEnhancedLogger : NullLogger, IEnhancedLogger
{
    public static new readonly NullEnhancedLogger Instance = new();

    public void Info(string category, string message) { }
    public void Warn(string category, string message) { }
    public void Error(string category, string message) { }
    public void Error(string message, Exception exception) { }
    public void Error(string category, string message, Exception exception) { }
    public void Debug(string category, string message) { }
    public void Timing(string operation, TimeSpan duration) { }
    public IDisposable StartTimer(string operation) => new TimerContext();

    private class TimerContext : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// Enhanced console logger with structured logging and performance tracking.
/// </summary>
public class EnhancedConsoleLogger : ConsoleLogger, IEnhancedLogger
{
    private readonly LogLevel _minLevel;

    public EnhancedConsoleLogger(bool verbose = false, LogLevel minLevel = LogLevel.Info)
        : base(verbose)
    {
        _minLevel = minLevel;
    }

    public void Info(string category, string message) => Info($"[{category}] {message}");
    public void Warn(string category, string message) => Warn($"[{category}] {message}");
    public void Error(string category, string message) => Error($"[{category}] {message}");

    public void Error(string message, Exception exception)
    {
        Error($"{message}: {exception.Message}");
        if (exception.InnerException != null)
        {
            Debug($"  Inner: {exception.InnerException.Message}");
        }
    }

    public void Error(string category, string message, Exception exception)
    {
        Error(category, $"{message}: {exception.Message}");
        if (exception.InnerException != null)
        {
            Debug(category, $"Inner: {exception.InnerException.Message}");
        }
    }

    public void Debug(string category, string message) => Debug($"[{category}] {message}");

    public void Timing(string operation, TimeSpan duration)
    {
        var formatted = duration.TotalMilliseconds switch
        {
            < 1 => $"{duration.TotalMicroseconds:F2}µs",
            < 1000 => $"{duration.TotalMilliseconds:F2}ms",
            _ => $"{duration.TotalSeconds:F2}s",
        };

        Info("TIMING", $"{operation}: {formatted}");
    }

    public IDisposable StartTimer(string operation) => new TimerContext(this, operation);

    private class TimerContext : IDisposable
    {
        private readonly EnhancedConsoleLogger _logger;
        private readonly string _operation;
        private readonly Stopwatch _stopwatch;

        public TimerContext(EnhancedConsoleLogger logger, string operation)
        {
            _logger = logger;
            _operation = operation;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Timing(_operation, _stopwatch.Elapsed);
        }
    }
}

/// <summary>
/// Log levels for filtering messages.
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Info = 2,
    Warn = 3,
    Error = 4,
}

/// <summary>
/// Extension methods for enhanced logging.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Create an enhanced console logger.
    /// </summary>
    public static IEnhancedLogger CreateEnhancedLogger(bool verbose = false, LogLevel minLevel = LogLevel.Info)
    {
        return new EnhancedConsoleLogger(verbose, minLevel);
    }

    /// <summary>
    /// Log operation timing with automatic timer.
    /// </summary>
    public static void LogTiming(this IEnhancedLogger logger, string operation, Action action)
    {
        using (logger.StartTimer(operation))
        {
            action();
        }
    }

    /// <summary>
    /// Log async operation timing.
    /// </summary>
    public static async Task LogTimingAsync(this IEnhancedLogger logger, string operation, Func<Task> action)
    {
        using (logger.StartTimer(operation))
        {
            await action();
        }
    }

    /// <summary>
    /// Log async operation timing with result.
    /// </summary>
    public static async Task<T> LogTimingAsync<T>(this IEnhancedLogger logger, string operation, Func<Task<T>> action)
    {
        using (logger.StartTimer(operation))
        {
            return await action();
        }
    }
}

/// <summary>
/// File logger that writes to a log file.
/// </summary>
public class FileLogger : IEnhancedLogger
{
    private readonly string _filePath;
    private readonly object _lockObject = new();

    public FileLogger(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public void Info(string message) => WriteLog("INFO", message);
    public void Warn(string message) => WriteLog("WARN", message);
    public void Error(string message) => WriteLog("ERROR", message);
    public void Debug(string message) => WriteLog("DEBUG", message);

    public void Info(string category, string message) => WriteLog("INFO", $"[{category}] {message}");
    public void Warn(string category, string message) => WriteLog("WARN", $"[{category}] {message}");
    public void Error(string category, string message) => WriteLog("ERROR", $"[{category}] {message}");
    public void Error(string message, Exception exception) => WriteLog("ERROR", $"{message}: {exception}");
    public void Error(string category, string message, Exception exception) => WriteLog("ERROR", $"[{category}] {message}: {exception}");
    public void Debug(string category, string message) => WriteLog("DEBUG", $"[{category}] {message}");
    public void Timing(string operation, TimeSpan duration) => Info("TIMING", $"{operation}: {duration.TotalMilliseconds:F2}ms");

    public IDisposable StartTimer(string operation) => new TimerContext(this, operation);

    private void WriteLog(string level, string message)
    {
        lock (_lockObject)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logLine = $"[{timestamp}] [{level}] {message}";
                File.AppendAllText(_filePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
    }

    private class TimerContext : IDisposable
    {
        private readonly FileLogger _logger;
        private readonly string _operation;
        private readonly Stopwatch _stopwatch;

        public TimerContext(FileLogger logger, string operation)
        {
            _logger = logger;
            _operation = operation;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _logger.Timing(_operation, _stopwatch.Elapsed);
        }
    }
}
