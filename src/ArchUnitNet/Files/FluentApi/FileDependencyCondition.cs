using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Defines file dependency rule: which files should (not) depend on which.
/// Supports chainable target selectors with multiple conditions.
/// Example: .DependOnFiles().InPath("src/Models/**").And().HaveName("*.cs")
/// </summary>
public class FileDependencyCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly string _sourcePattern;
    private readonly bool _negated;
    private readonly TargetFileSelector _targetSelector;

    public FileDependencyCondition(Graph graph, PatternMatcher sourceMatcher, bool negated, string sourcePattern = "")
    {
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _sourceMatcher = sourceMatcher ?? throw new ArgumentNullException(nameof(sourceMatcher));
        _sourcePattern = sourcePattern;
        _negated = negated;
        _targetSelector = new TargetFileSelector();
    }

    /// <summary>
    /// Select target files by path pattern.
    /// Example: .InPath("src/Models/**")
    /// </summary>
    public FileDependencyCondition InPath(string pattern)
    {
        _targetSelector.InPath(pattern);
        return this;
    }

    /// <summary>
    /// Select target files by folder.
    /// Example: .InFolder("src/Models")
    /// </summary>
    public FileDependencyCondition InFolder(string folder)
    {
        _targetSelector.InFolder(folder);
        return this;
    }

    /// <summary>
    /// Select target files by name pattern.
    /// Example: .HaveName("*.Service.cs")
    /// </summary>
    public FileDependencyCondition HaveName(string namePattern)
    {
        _targetSelector.HaveName(namePattern);
        return this;
    }

    /// <summary>
    /// Exclude target files matching an exception pattern.
    /// Example: .Except("**/Legacy/**")
    /// </summary>
    public FileDependencyCondition Except(string exceptionPattern)
    {
        _targetSelector.Except(exceptionPattern);
        return this;
    }

    /// <summary>
    /// Syntax sugar for chaining selectors.
    /// Example: .InFolder("src/Models").And().HaveName("*.cs")
    /// </summary>
    public FileDependencyCondition And()
    {
        _targetSelector.And();
        return this;
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (!_targetSelector.HasSelectors)
            throw new InvalidOperationException("Must define target selector with InPath(), InFolder(), or HaveName()");

        options ??= new CheckOptions();

        var violations = new List<Violation>();
        var projectedEdges = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Empty-test guard: fail if no files match the source pattern (unless explicitly allowed)
        var matchingEdges = projectedEdges
            .Where(e => _sourceMatcher.Matches(e.Source))
            .ToList();

        if (matchingEdges.Count == 0 && !options.AllowEmptyTests)
        {
            violations.Add(new MatchingFilesViolation(
                _sourcePattern,
                $"No files matched pattern '{_sourcePattern}' - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var edge in matchingEdges)
        {
            bool targetMatches = _targetSelector.Matches(edge.Target);

            bool violates = targetMatches;
            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? $"forbidden dependency on {_targetSelector.GetDescription()}"
                    : $"expected dependency on {_targetSelector.GetDescription()} not found";
                violations.Add(ViolatingFileDependency.Create(edge, reason));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
