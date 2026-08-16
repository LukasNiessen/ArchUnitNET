using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// File path location rule: files should (not) match a specific path pattern.
/// Checks full file path against glob or regex pattern.
/// Example: .BeInPath("src/**") or .BeInPath("legacy/.*")
/// </summary>
public class FilePathCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly PatternMatcher _pathMatcher;
    private readonly bool _negated;

    public FilePathCondition(Graph graph, PatternMatcher fileMatcher, string pathPattern, bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _fileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        _pathMatcher = new PatternMatcher(pathPattern);
        _negated = negated;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

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
                "file selection",
                $"No files matched the selection pattern - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var node in allNodes)
        {
            bool matchesPathPattern = _pathMatcher.Matches(node);
            bool violates = !matchesPathPattern;

            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? $"matches forbidden path pattern"
                    : $"does not match required path pattern";
                violations.Add(new MatchingFilesViolation(node, $"{node} {reason}"));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
