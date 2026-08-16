namespace ArchUnitNet.Testing.Common;

/// <summary>
/// ANSI color codes and semantic colour helpers for test output formatting.
/// Centralizes all colour handling - adapters and other classes use this exclusively.
/// </summary>
public static class Colours
{
    /// <summary>
    /// ANSI Reset code - clears all formatting.
    /// </summary>
    public const string Reset = "[0m";

    /// <summary>
    /// ANSI Bold code.
    /// </summary>
    public const string Bold = "[1m";

    /// <summary>
    /// ANSI Red foreground.
    /// </summary>
    public const string Red = "[31m";

    /// <summary>
    /// ANSI Green foreground.
    /// </summary>
    public const string Green = "[32m";

    /// <summary>
    /// ANSI Yellow foreground.
    /// </summary>
    public const string Yellow = "[33m";

    /// <summary>
    /// ANSI Blue foreground.
    /// </summary>
    public const string Blue = "[34m";

    /// <summary>
    /// ANSI Cyan foreground.
    /// </summary>
    public const string Cyan = "[36m";

    /// <summary>
    /// ANSI Gray foreground (bright black).
    /// </summary>
    public const string Gray = "[90m";

    /// <summary>
    /// Semantic colour for error messages (maps to Red).
    /// </summary>
    public static string Error => Red;

    /// <summary>
    /// Semantic colour for success messages (maps to Green).
    /// </summary>
    public static string Success => Green;

    /// <summary>
    /// Semantic colour for warning messages (maps to Yellow).
    /// </summary>
    public static string Warning => Yellow;

    /// <summary>
    /// Semantic colour for informational messages (maps to Blue).
    /// </summary>
    public static string Info => Blue;

    /// <summary>
    /// Semantic colour for muted/secondary text (maps to Gray).
    /// </summary>
    public static string Muted => Gray;

    /// <summary>
    /// Wrap text with a colour code and reset sequence.
    /// </summary>
    /// <param name="text">Text to colorize</param>
    /// <param name="colour">Colour code to apply</param>
    /// <param name="enabled">Whether to apply colour (set false to strip colours)</param>
    /// <returns>Wrapped text with colour codes if enabled, otherwise plain text</returns>
    public static string Colorize(string text, string colour, bool enabled = true)
    {
        if (!enabled || string.IsNullOrEmpty(text))
            return text ?? "";

        return $"{colour}{text}{Reset}";
    }

    /// <summary>
    /// Wrap text with a colour code, bold, and reset sequence.
    /// </summary>
    /// <param name="text">Text to colorize</param>
    /// <param name="colour">Colour code to apply</param>
    /// <param name="enabled">Whether to apply colour (set false to strip colours)</param>
    /// <returns>Wrapped text with bold colour codes if enabled, otherwise plain text</returns>
    public static string ColorizeBold(string text, string colour, bool enabled = true)
    {
        if (!enabled || string.IsNullOrEmpty(text))
            return text ?? "";

        return $"{colour}{Bold}{text}{Reset}";
    }
}
