using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.PatternMatching;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// First stage of rule building: defines which files to check.
/// Example: ProjectFiles.From(graph).InPath("src/**")
/// </summary>
public class FileConditionBuilder
{
    private readonly Graph _graph;
    private PatternMatcher? _fileMatcher;
    private string _currentPattern = "";

    public FileConditionBuilder(Graph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public FileConditionBuilder InPath(string pattern)
    {
        _fileMatcher = new PatternMatcher(pattern);
        _currentPattern = pattern;
        return this;
    }

    public FileConditionBuilder InFolder(string folder)
    {
        var pattern = $"{folder}/**";
        return InPath(pattern);
    }

    public FileConditionBuilder ByName(string namePattern)
    {
        _currentPattern = $"**/{namePattern}";
        _fileMatcher = new PatternMatcher(_currentPattern);
        return this;
    }

    public FileConditionBuilder Named(string exactName)
    {
        _currentPattern = $"**/{exactName}";
        _fileMatcher = new PatternMatcher(_currentPattern);
        return this;
    }

    public FilesShouldCondition Should()
    {
        if (_fileMatcher == null)
            throw new InvalidOperationException("Must call InPath() or InFolder() first");

        return new FilesShouldCondition(_graph, _fileMatcher, negated: false, sourcePattern: _currentPattern);
    }

    public FilesShouldCondition ShouldNot()
    {
        if (_fileMatcher == null)
            throw new InvalidOperationException("Must call InPath() or InFolder() first");

        return new FilesShouldCondition(_graph, _fileMatcher, negated: true, sourcePattern: _currentPattern);
    }
}
