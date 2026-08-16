using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Testing.Common;

/// <summary>
/// Factory for formatting individual violations into human-readable messages.
/// Centralizes all violation message formatting - adapters use this exclusively.
/// Single responsibility: convert a Violation to a formatted string with optional colours.
/// </summary>
public static class ViolationFactory
{
    /// <summary>
    /// Format a single violation for console output.
    /// </summary>
    /// <param name="violation">The violation to format</param>
    /// <param name="index">Optional index for numbering (1-based)</param>
    /// <param name="colored">Whether to include ANSI colour codes</param>
    /// <param name="showTypeName">Whether to include the violation type name</param>
    /// <returns>Formatted violation message</returns>
    public static string FormatViolation(
        Violation violation,
        int index = 0,
        bool colored = true,
        bool showTypeName = true)
    {
        if (violation == null)
            return "No violation information";

        var message = violation.ToString() ?? "No message";

        // Format prefix with optional colour
        var prefix = Colours.Colorize("[✗]", Colours.Error, colored);

        // Format index if provided (1-based numbering)
        var number = index > 0 ? $"{index}. " : "";

        // Format type name with muted colour if shown
        var typeName = showTypeName
            ? Colours.Colorize(violation.GetType().Name, Colours.Muted, colored)
            : "";

        // Format message - use error colour for error-like violations
        var formattedMessage = colored && IsErrorLike(violation)
            ? Colours.Colorize(message, Colours.Error, colored)
            : message;

        // Assemble final format
        if (typeName != "")
        {
            return $"{prefix} {number}{typeName}\n  {formattedMessage}";
        }

        return $"{prefix} {number}{formattedMessage}";
    }

    /// <summary>
    /// Determine if a violation should be displayed with error colour.
    /// Detects violation types that represent errors/failures.
    /// </summary>
    private static bool IsErrorLike(Violation violation)
    {
        var typeName = violation.GetType().Name;
        return typeName.Contains("Violation")
            || typeName.Contains("Dependency")
            || typeName.Contains("Cyclic")
            || typeName.Contains("Matching");
    }
}
