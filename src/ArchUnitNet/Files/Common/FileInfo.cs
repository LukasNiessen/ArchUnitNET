using System.IO;
using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Files.Common;

/// <summary>
/// Lightweight projection of a file with metadata for custom rule evaluation.
/// Example: file.NonBlankLineCount &lt; 200
/// </summary>
public record FileInfo(
    string Path,
    string NameWithoutExtension,
    string Extension,
    string Directory,
    string SourceCode,
    int NonBlankLineCount
)
{
    /// <summary>
    /// Create FileInfo from file path by reading the file.
    /// </summary>
    public static FileInfo FromPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var sourceCode = File.ReadAllText(filePath);
        var lines = File.ReadAllLines(filePath);
        var nonBlankLineCount = lines.Count(line => !string.IsNullOrWhiteSpace(line));

        var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
        var extension = System.IO.Path.GetExtension(filePath);
        var directory = System.IO.Path.GetDirectoryName(filePath) ?? "";

        return new FileInfo(
            Path: filePath,
            NameWithoutExtension: nameWithoutExtension,
            Extension: extension,
            Directory: directory,
            SourceCode: sourceCode,
            NonBlankLineCount: nonBlankLineCount
        );
    }

    /// <summary>
    /// Check if file path matches pattern using glob/regex.
    /// </summary>
    public bool PathMatches(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        var matcher = new PatternMatcher(pattern);
        return matcher.Matches(Path);
    }

    /// <summary>
    /// Check if filename (without extension) matches pattern.
    /// </summary>
    public bool NameMatches(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        var matcher = new PatternMatcher(pattern);
        return matcher.Matches(NameWithoutExtension);
    }
}
