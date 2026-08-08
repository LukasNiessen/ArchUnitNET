using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Files.Assertion;

/// <summary>
/// Violation when no files match the given pattern.
/// This can be an error if you expect files to exist.
/// </summary>
public record MatchingFilesViolation(
    string Pattern,
    string Message
) : Violation
{
    public override string ToString() => Message;

    public static MatchingFilesViolation NoFilesFound(string pattern)
    {
        return new MatchingFilesViolation(pattern, $"No files found matching pattern: {pattern}");
    }
}
