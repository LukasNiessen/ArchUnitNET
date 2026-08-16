using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// File name matching rule: files should (not) match a specific name pattern.
/// Checks only the filename, not the full path.
/// Example: .HaveName("*.Service.cs")
/// </summary>
public class FileNameCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly PatternMatcher _nameMatcher;
    private readonly bool _negated;

    public FileNameCondition(Graph graph, PatternMatcher fileMatcher, string namePattern, bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _fileMatcher = fileMatcher ?? throw new ArgumentNullException(nameof(fileMatcher));
        _nameMatcher = new PatternMatcher(namePattern);
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
            var fileName = Path.GetFileName(node);
            bool matchesNamePattern = _nameMatcher.Matches(fileName);
            bool violates = matchesNamePattern;

            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? $"has forbidden name matching {fileName}"
                    : $"does not match required name pattern {fileName}";
                violations.Add(new MatchingFilesViolation(node, $"{node} {reason}"));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
