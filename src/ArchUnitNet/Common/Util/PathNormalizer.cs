namespace ArchUnitNet.Common.Util;

/// <summary>
/// Normalizes file paths to a consistent format across platforms.
/// All paths are normalized to use forward slashes (/) and relative format.
/// This ensures pattern matching and comparisons work correctly on Windows and Unix.
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Normalize a path to use forward slashes and remove redundant elements.
    /// Converts: "src\Common\Error" → "src/Common/Error"
    /// Handles: ".\src\..\src\Error" → "src/Error"
    /// </summary>
    public static string Normalize(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        // Convert backslashes to forward slashes
        var normalized = path.Replace("\\", "/");

        // Handle UNC paths (\\server\share → //server/share, then normalize)
        // Preserve the leading // for UNC paths during processing
        var isUncPath = normalized.StartsWith("//");

        // Split by /, process each part, rejoin
        var parts = normalized.Split('/');
        var stack = new Stack<string>();

        for (var i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            // Skip empty parts (from double slashes) except at the very start for UNC
            if (string.IsNullOrEmpty(part))
            {
                if (isUncPath && i == 0)
                    continue; // Will add leading slashes back later
                continue;
            }

            // Current directory — skip
            if (part == ".")
                continue;

            // Parent directory — pop from stack if possible
            if (part == "..")
            {
                if (stack.Count > 0)
                    stack.Pop();
                continue;
            }

            stack.Push(part);
        }

        // Reconstruct path from stack (reversed)
        var result = string.Join("/", stack.Reverse());

        if (isUncPath)
            result = "//" + result;

        return result;
    }

    /// <summary>
    /// Remove trailing slash from a path.
    /// "src/Common/" → "src/Common"
    /// </summary>
    public static string RemoveTrailingSlash(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.EndsWith("/") && path.Length > 1
            ? path[..^1]
            : path;
    }

    /// <summary>
    /// Ensure a path ends with a slash.
    /// "src/Common" → "src/Common/"
    /// </summary>
    public static string EnsureTrailingSlash(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.EndsWith("/") ? path : path + "/";
    }

    /// <summary>
    /// Get the directory part of a path (everything before the last slash).
    /// "src/Common/Error.cs" → "src/Common"
    /// </summary>
    public static string GetDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        var lastSlash = path.LastIndexOf('/');
        if (lastSlash < 0)
            return "";

        return lastSlash == 0 ? "/" : path[..lastSlash];
    }

    /// <summary>
    /// Get the file name part of a path (everything after the last slash).
    /// "src/Common/Error.cs" → "Error.cs"
    /// </summary>
    public static string GetFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "";

        var lastSlash = path.LastIndexOf('/');
        return lastSlash < 0 ? path : path[(lastSlash + 1)..];
    }

    /// <summary>
    /// Get the relative path from a base directory to a target path.
    /// GetRelativePath("src", "src/Common/Error.cs") → "Common/Error.cs"
    /// </summary>
    public static string GetRelativePath(string baseDirectory, string targetPath)
    {
        baseDirectory = Normalize(RemoveTrailingSlash(baseDirectory));
        targetPath = Normalize(RemoveTrailingSlash(targetPath));

        if (targetPath == baseDirectory)
            return "";

        if (targetPath.StartsWith(baseDirectory + "/"))
            return targetPath[(baseDirectory.Length + 1)..];

        throw new ArgumentException(
            $"Target path '{targetPath}' is not relative to base '{baseDirectory}'",
            nameof(targetPath));
    }

    /// <summary>
    /// Check if a path is an absolute path.
    /// "/src/Common" → true
    /// "C:/src/Common" → true
    /// "src/Common" → false
    /// </summary>
    public static bool IsAbsolute(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path.StartsWith("/") || (path.Length > 1 && path[1] == ':');
    }
}
