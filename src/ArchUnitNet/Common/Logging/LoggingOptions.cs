namespace ArchUnitNet.Common.Logging;

/// <summary>
/// Configuration for logging during rule checks.
/// Allows fine-grained control over what gets logged.
///
/// In ArchUnitTS: interface LoggingOptions { verbose?: boolean; ... }
/// </summary>
public record LoggingOptions(
    /// <summary>
    /// Enable verbose logging (shows detailed analysis steps).
    /// Default: false (only errors and summary).
    /// </summary>
    bool Verbose = false,

    /// <summary>
    /// Log to console during the check.
    /// Default: false (only log if Logger is explicitly configured).
    /// </summary>
    bool LogToConsole = false,

    /// <summary>
    /// Additional context to include in log messages.
    /// Useful for debugging specific checks.
    /// </summary>
    string? Context = null);
