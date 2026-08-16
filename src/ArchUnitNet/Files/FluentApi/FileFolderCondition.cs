using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// File folder location rule: files should (not) be in a specific folder.
/// Checks if file path contains the folder in its hierarchy.
/// Example: .BeInFolder("src/Services")
/// </summary>
public class FileFolderCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly string _folder;
    private readonly bool _negated;

    public FileFolderCondition(Graph graph, PatternMatcher fileMatcher, string folder, bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _fileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        _folder = folder ?? throw new ArgumentNullException(nameof(folder));
        _negated = negated;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        // Normalize folder path (forward slashes)
        var normalizedFolder = _folder.Replace("\\", "/").TrimEnd('/');

        // Collect all matching source files
        var allNodes = new HashSet<string>();
        foreach (var edge in _graph.Edges)
        {
            if (_fileMatcher.Matches(edge.Source))
                allNodes.Add(edge.Source);
        }

        // Empty-test guard: fail if no files match unless explicitly allowed
        if (allNodes.Count == 0 && !options.AllowEmptyTests)
        {
            violations.Add(new MatchingFilesViolation(
                _folder,
                $"No files matched the selection pattern - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var node in allNodes)
        {
            // Normalize file path
            var normalizedPath = node.Replace("\\", "/");
            bool isInFolder = normalizedPath.Contains(normalizedFolder + "/") ||
                            normalizedPath.StartsWith(normalizedFolder + "/");
            bool violates = !isInFolder;

            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? $"is in forbidden folder {normalizedFolder}"
                    : $"is not in required folder {normalizedFolder}";
                violations.Add(new MatchingFilesViolation(node, $"{node} {reason}"));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
