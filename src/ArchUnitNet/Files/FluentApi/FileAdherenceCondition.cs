using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;
using ArchUnitNet.Files.Common;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Condition that evaluates custom predicates against FileInfo objects.
/// Example: .AdhereTo(file =&gt; file.NonBlankLineCount &lt; 200, "Files must be under 200 lines")
/// </summary>
public class FileAdherenceCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly bool _negated;
    private readonly Func<FileInfo, bool> _predicate;
    private readonly string _message;

    public FileAdherenceCondition(
        Graph graph,
        PatternMatcher sourceMatcher,
        Func<FileInfo, bool> predicate,
        string message,
        bool negated)
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _sourceMatcher = sourceMatcher ?? throw new ArgumentNullException(nameof(sourceMatcher));
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        _message = message;
        _negated = negated;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        options ??= new CheckOptions();
        var violations = new List<Violation>();

        // Get unique source files from graph edges
        var sourceFiles = _graph.Edges
            .Select(e => e.Source)
            .Distinct()
            .Where(source => _sourceMatcher.Matches(source))
            .ToList();

        foreach (var sourceFile in sourceFiles)
        {
            try
            {
                var fileInfo = FileInfo.FromPath(sourceFile);
                var predicateResult = _predicate(fileInfo);

                bool violates = predicateResult;
                if (_negated)
                    violates = !violates;

                if (violates)
                {
                    // Create a violation using the first edge as reference
                    var referenceEdge = _graph.Edges.First(e => e.Source == sourceFile);
                    var reason = string.IsNullOrEmpty(_message)
                        ? "violates custom file adherence rule"
                        : _message;
                    violations.Add(ViolatingFileDependency.Create(referenceEdge, reason));
                }
            }
            catch (FileNotFoundException)
            {
                // File no longer exists, skip it
                continue;
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
