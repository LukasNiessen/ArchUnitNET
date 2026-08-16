using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.PatternMatching;
using ArchUnitNet.Files.Assertion;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Custom predicate-based rule for fine-grained control over dependencies.
/// Allows users to define arbitrary logic for what constitutes a violation.
/// </summary>
public class CustomPredicateCondition : Checkable
{
    private readonly Graph _graph;
    private readonly PatternMatcher _sourceMatcher;
    private readonly bool _negated;
    private Func<Edge, bool>? _predicate;

    public CustomPredicateCondition(Graph graph, PatternMatcher sourceMatcher, bool negated)
    {
        _graph = graph;
        _sourceMatcher = sourceMatcher;
        _negated = negated;
    }

    /// <summary>
    /// Define custom rule as a predicate function.
    /// Predicate should return true if dependency is valid (should not be a violation).
    /// </summary>
    public CustomPredicateCondition AdhereTo(Func<Edge, bool> predicate)
    {
        _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        return this;
    }

    /// <summary>
    /// Alternative method name: WhereEdgesSatisfy
    /// </summary>
    public CustomPredicateCondition Where(Func<Edge, bool> predicate)
    {
        return AdhereTo(predicate);
    }

    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (_predicate == null)
            throw new InvalidOperationException("Must call AdhereTo() or Where() first");

        options ??= new CheckOptions();
        var violations = new List<Violation>();

        // Collect matching edges
        var matchingEdges = _graph.Edges
            .Where(e => _sourceMatcher.Matches(e.Source))
            .ToList();

        // Empty-test guard: fail if no edges match unless explicitly allowed
        if (matchingEdges.Count == 0 && !options.AllowEmptyTests)
        {
            violations.Add(new MatchingFilesViolation(
                "file selection",
                $"No files matched the selection pattern - this is likely a typo. " +
                "If intentional, use CheckOptions with AllowEmptyTests = true"));
            return await Task.FromResult(violations.AsReadOnly());
        }

        foreach (var edge in matchingEdges)
        {

            bool satisfiesPredicate = _predicate(edge);
            bool violates = !satisfiesPredicate;

            if (_negated)
                violates = !violates;

            if (violates)
            {
                var reason = _negated
                    ? "violates custom predicate (should not)"
                    : "violates custom predicate (should)";
                violations.Add(ViolatingFileDependency.Create(edge, reason));
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }
}
