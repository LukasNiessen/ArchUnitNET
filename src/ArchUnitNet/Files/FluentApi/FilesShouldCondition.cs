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
    private string _sourcePattern;

    public FilesShouldCondition(Graph graph, PatternMatcher fileMatcher, bool negated, string sourcePattern = "")
    {
        _graph = graph;
        _fileMatcher = fileMatcher;
        _negated = negated;
        _sourcePattern = sourcePattern;
    }

    public FileDependencyCondition DependOnFiles()
    {
        return new FileDependencyCondition(_graph, _fileMatcher, negated: _negated, sourcePattern: _sourcePattern);
    }

    public FileIndependenceCondition HaveNoCycles()
    {
        return new FileIndependenceCondition(_graph, _fileMatcher, negated: _negated);
    }

    public FilePatternCondition MatchPattern(string pattern)
    {
        return new FilePatternCondition(_graph, _fileMatcher, pattern, negated: _negated);
    }

    /// <summary>
    /// Check dependencies on external modules (NuGet packages).
    /// Example: .DependOnExternalModules().Matching("Newtonsoft.*")
    /// </summary>
    public ExternalDependencyCondition DependOnExternalModules()
    {
        return new ExternalDependencyCondition(_graph, _fileMatcher, negated: _negated);
    }

    /// <summary>
    /// Define custom rules using a predicate function.
    /// Example: .AdhereTo(edge => !edge.Source.Contains("Old") || edge.Target.Contains("New"))
    /// </summary>
    public CustomPredicateCondition AdhereTo(Func<Edge, bool> predicate)
    {
        return new CustomPredicateCondition(_graph, _fileMatcher, negated: _negated).AdhereTo(predicate);
    }

    /// <summary>
    /// Check file names match a pattern (only the filename, not full path).
    /// Example: .Should().HaveName("*.Service.cs") or .ShouldNot().HaveName("*Test*.cs")
    /// </summary>
    public FileNameCondition HaveName(string namePattern)
    {
        return new FileNameCondition(_graph, _fileMatcher, namePattern, negated: _negated);
    }

    /// <summary>
    /// Check if files are in a specific folder.
    /// Example: .Should().BeInFolder("src/Services") or .ShouldNot().BeInFolder("tests")
    /// </summary>
    public FileFolderCondition BeInFolder(string folder)
    {
        return new FileFolderCondition(_graph, _fileMatcher, folder, negated: _negated);
    }

    /// <summary>
    /// Check if files match a path pattern.
    /// Example: .Should().BeInPath("src/**") or .ShouldNot().BeInPath("legacy/**")
    /// </summary>
    public FilePathCondition BeInPath(string pathPattern)
    {
        return new FilePathCondition(_graph, _fileMatcher, pathPattern, negated: _negated);
    }
}
