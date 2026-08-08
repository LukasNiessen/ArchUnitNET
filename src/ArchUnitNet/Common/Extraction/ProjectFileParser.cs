using System.Xml.Linq;
using ArchUnitNet.Common.Error;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Common.Extraction;

/// <summary>
/// Parses .csproj (project) files to discover source code files.
/// Handles both SDK-style (.NET Core) and legacy projects.
///
/// Extracts:
/// - All C# source files (*.cs)
/// - Project directory
/// - Target framework info (for future use)
/// </summary>
public class ProjectFileParser
{
    /// <summary>
    /// Parse a .csproj file and return all C# source files it contains.
    /// </summary>
    /// <param name="csprojPath">Path to the .csproj file</param>
    /// <returns>List of normalized paths to .cs files</returns>
    public static IReadOnlyList<string> FindSourceFiles(string csprojPath)
    {
        if (!File.Exists(csprojPath))
            throw new UserError($"Project file not found: {csprojPath}");

        if (!csprojPath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            throw new UserError($"Expected .csproj file, got: {csprojPath}");

        try
        {
            var projectDir = Path.GetDirectoryName(csprojPath)
                ?? throw new TechnicalError("Could not determine project directory");

            var xml = XDocument.Load(csprojPath);
            var root = xml.Root
                ?? throw new TechnicalError("Invalid .csproj file: no root element");

            // Find all C# source files
            var sourceFiles = new List<string>();

            // SDK-style projects: Compile items in ItemGroup
            var compileItems = root.Descendants()
                .Where(e => e.Name.LocalName == "Compile")
                .ToList();

            // Add explicitly included .cs files
            foreach (var item in compileItems)
            {
                var include = item.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(include) && include.EndsWith(".cs"))
                {
                    var fullPath = Path.Combine(projectDir, include);
                    var normalized = NormalizeSourceFilePath(fullPath, projectDir);
                    if (!sourceFiles.Contains(normalized))
                        sourceFiles.Add(normalized);
                }
            }

            // If no Compile items found, scan directory for .cs files
            if (sourceFiles.Count == 0)
            {
                sourceFiles.AddRange(ScanDirectoryForCsFiles(projectDir));
            }

            return sourceFiles.AsReadOnly();
        }
        catch (UserError)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TechnicalError($"Failed to parse project file '{csprojPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Scan a directory recursively for all .cs files.
    /// Used when .csproj doesn't explicitly list files.
    /// </summary>
    private static List<string> ScanDirectoryForCsFiles(string projectDir)
    {
        var sourceFiles = new List<string>();

        try
        {
            var csFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles)
            {
                var normalized = NormalizeSourceFilePath(file, projectDir);
                sourceFiles.Add(normalized);
            }
        }
        catch (Exception ex)
        {
            throw new TechnicalError($"Failed to scan directory '{projectDir}' for .cs files: {ex.Message}", ex);
        }

        return sourceFiles;
    }

    /// <summary>
    /// Normalize a source file path to be relative to project directory.
    /// Example: "C:\Projects\MyApp\src\Common\Error.cs" → "src/Common/Error.cs"
    /// </summary>
    private static string NormalizeSourceFilePath(string filePath, string projectDir)
    {
        var fullPath = Path.GetFullPath(filePath);
        var projectFullPath = Path.GetFullPath(projectDir);

        if (fullPath.StartsWith(projectFullPath))
        {
            var relative = fullPath[projectFullPath.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return PathNormalizer.Normalize(relative);
        }

        return PathNormalizer.Normalize(fullPath);
    }
}
