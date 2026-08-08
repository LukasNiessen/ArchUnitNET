using ArchUnitNet.Common.Logging;

namespace ArchUnitNet.Common.FluentApi;

/// <summary>
/// Options passed to CheckAsync() to control rule execution behavior.
/// All properties are optional with sensible defaults.
///
/// Design principle: Options bags, always.
/// No terminal method takes more than one parameter beyond its required argument.
/// This allows options to be added later without breaking existing callers.
///
/// In ArchUnitTS: interface CheckOptions { allowEmptyTests?: boolean; ... }
/// </summary>
public record CheckOptions(
    /// <summary>
    /// Allow patterns that match zero files/classes to pass.
    /// Default: false (zero matches is almost always a typo — better to fail loudly).
    /// This is the highest-value defensive check in the library.
    /// </summary>
    bool AllowEmptyTests = false,

    /// <summary>
    /// Logging configuration for this check.
    /// Default: null (no logging).
    /// </summary>
    LoggingOptions? Logging = null,

    /// <summary>
    /// Clear the internal extraction cache before this check.
    /// Default: false (reuse cached results for performance).
    /// Set to true if the codebase has changed during testing.
    /// </summary>
    bool ClearCache = false,

    /// <summary>
    /// Optional custom timeout in milliseconds for the check.
    /// Default: null (no timeout).
    /// </summary>
    int? TimeoutMs = null);
