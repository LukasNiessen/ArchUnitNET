using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Second stage: defines what condition (should/should not) and transitions to rule types.
/// Example: .Should().DependOnFiles()
/// </summary>
public class FilesShouldCondition
{
    private readonly Graph _graph;
    private readonly PatternMatcher _fileMatcher;
    private readonly bool _negated;

    public FilesShouldCondition(Graph graph, PatternMatcher fileMatcher, bool negated)
    {
        _graph = graph;
        _fileMatcher = fileMatcher;
        _negated = negated;
    }

    public FileDependencyCondition DependOnFiles()
    {
        return new FileDependencyCondition(_graph, _fileMatcher, negated: _negated);
    }

    public FileIndependenceCondition HaveNoCycles()
    {
        return new FileIndependenceCondition(_graph, _fileMatcher, negated: _negated);
    }

    public FilePatternCondition MatchPattern(string pattern)
    {
        return new FilePatternCondition(_graph, _fileMatcher, pattern, negated: _negated);
    }
}
