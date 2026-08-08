namespace ArchUnitNet.Common.Assertion;

/// <summary>
/// Violation raised when a pattern selector matches zero files/classes.
/// This is almost always a typo or misconfiguration, so it's a violation by default.
/// Can be suppressed with CheckOptions.AllowEmptyTests = true.
///
/// Example: ProjectFiles().InPath("src/Controllers/**") matches nothing
/// → EmptyTestViolation: no files matched the pattern
///
/// In ArchUnitTS: class EmptyTestViolation implements Violation
/// </summary>
public record EmptyTestViolation(
    /// <summary>
    /// The pattern that matched nothing (e.g., "src/Controllers/**").
    /// This helps the user understand which selector was the problem.
    /// </summary>
    string Pattern,

    /// <summary>
    /// Optional context about what was being selected.
    /// Example: "files in path", "classes matching", etc.
    /// </summary>
    string Context = "file selection") : Violation
{
    public override string ToString() =>
        $"EmptyTestViolation: Pattern '{Pattern}' matched no files ({Context})";
}
