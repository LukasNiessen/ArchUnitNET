using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Pattern matching rule: files should (not) match specific patterns.
/// Example: .MatchPattern("**/*Test.cs")
/// </summary>
public class FilePatternCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly string _pattern;
    private readonly bool _negated;

    public FilePatternCondition(Graph graph, PatternMatcher fileMatcher, string pattern, bool negated)
    {
        _graph = graph;
        _fileMatcher = fileMatcher;
        _pattern = pattern;
        _negated = negated;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        var violations = new List<Violation>();
        var patternMatcher = new PatternMatcher(_pattern);

        var allNodes = new HashSet<string>();
        foreach (var edge in _graph.Edges)
        {
            if (_fileMatcher.Matches(edge.Source))
                allNodes.Add(edge.Source);
        }

        foreach (var node in allNodes)
        {
            bool matchesPattern = patternMatcher.Matches(node);
            bool violates = matchesPattern;

            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? $"matches forbidden pattern {_pattern}"
                    : $"does not match required pattern {_pattern}";
                violations.Add(new MatchingFilesViolation(node, $"{node} {reason}"));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
