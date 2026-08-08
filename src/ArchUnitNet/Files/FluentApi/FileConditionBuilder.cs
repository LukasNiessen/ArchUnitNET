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

    public FileConditionBuilder(Graph graph)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public FileConditionBuilder InPath(string pattern)
    {
        _fileMatcher = new PatternMatcher(pattern);
        return this;
    }

    public FileConditionBuilder InFolder(string folder)
    {
        var pattern = $"{folder}/**";
        return InPath(pattern);
    }

    public FilesShouldCondition Should()
    {
        if (_fileMatcher == null)
            throw new InvalidOperationException("Must call InPath() or InFolder() first");

        return new FilesShouldCondition(_graph, _fileMatcher, negated: false);
    }

    public FilesShouldCondition ShouldNot()
    {
        if (_fileMatcher == null)
            throw new InvalidOperationException("Must call InPath() or InFolder() first");

        return new FilesShouldCondition(_graph, _fileMatcher, negated: true);
    }
}
