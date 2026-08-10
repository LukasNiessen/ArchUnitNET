using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Slices.Projection;

namespace ArchUnitNet.Slices.FluentApi;

/// <summary>
/// Entry point for slice-based architecture rules.
/// Defines how files are mapped to logical slices based on patterns.
/// </summary>
public class SliceConditionBuilder
{
    private string? _slicePattern;
    private IEnumerable<Edge>? _edges;

    /// <summary>
    /// Define slices based on a file path pattern.
    /// Example: "src/{Slice}/**/*.cs" extracts slices like "Feature1", "Feature2"
    /// </summary>
    public SliceConditionBuilder DefinedBy(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        _slicePattern = pattern;
        return this;
    }

    /// <summary>
    /// Provide the edges to analyze (dependency graph).
    /// </summary>
    internal SliceConditionBuilder WithEdges(IEnumerable<Edge> edges)
    {
        _edges = edges ?? throw new ArgumentNullException(nameof(edges));
        return this;
    }

    /// <summary>
    /// Transition to positive condition builder (should adhere, should contain, etc.).
    /// </summary>
    public PositiveSliceCondition Should()
    {
        if (string.IsNullOrEmpty(_slicePattern))
            throw new InvalidOperationException("Slice pattern must be defined first via DefinedBy()");

        return new PositiveSliceCondition(_slicePattern, _edges);
    }

    /// <summary>
    /// Transition to negative condition builder (should not depend on, etc.).
    /// </summary>
    public NegativeSliceCondition ShouldNot()
    {
        if (string.IsNullOrEmpty(_slicePattern))
            throw new InvalidOperationException("Slice pattern must be defined first via DefinedBy()");

        return new NegativeSliceCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Builder for positive slice conditions (should adhere, should contain).
/// </summary>
public class PositiveSliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal PositiveSliceCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    /// <summary>
    /// Slices should adhere to defined architecture (no cycles, no unexpected dependencies).
    /// </summary>
    public AdhereToSlicesCondition AdhereToDefinedSlices()
    {
        return new AdhereToSlicesCondition(_slicePattern, _edges);
    }

    /// <summary>
    /// Slices should follow a specific dependency structure.
    /// </summary>
    public DependencyPatternCondition FollowPattern(string dependencyPattern)
    {
        if (string.IsNullOrEmpty(dependencyPattern))
            throw new ArgumentException("Dependency pattern cannot be null or empty", nameof(dependencyPattern));

        return new DependencyPatternCondition(_slicePattern, _edges, dependencyPattern);
    }

    /// <summary>
    /// Slices should not have any cyclic dependencies.
    /// </summary>
    public NoCyclicSlicesCondition BeAcyclic()
    {
        return new NoCyclicSlicesCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Builder for negative slice conditions (should not).
/// </summary>
public class NegativeSliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal NegativeSliceCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    /// <summary>
    /// Slices should not have any cyclic dependencies.
    /// </summary>
    public NoCyclicSlicesCondition HaveCycles()
    {
        return new NoCyclicSlicesCondition(_slicePattern, _edges);
    }
}

/// <summary>
/// Marker interface for conditions that can be checked.
/// </summary>
public interface ISliceCondition
{
}

/// <summary>
/// Represents a condition where slices should adhere to defined architecture.
/// </summary>
public class AdhereToSlicesCondition : ISliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal AdhereToSlicesCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    // TODO: Implement CheckAsync() when integrated with dependency graph
}

/// <summary>
/// Represents a dependency pattern condition between slices.
/// </summary>
public class DependencyPatternCondition : ISliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;
    private readonly string _dependencyPattern;

    internal DependencyPatternCondition(string slicePattern, IEnumerable<Edge>? edges, string dependencyPattern)
    {
        _slicePattern = slicePattern;
        _edges = edges;
        _dependencyPattern = dependencyPattern;
    }

    // TODO: Implement CheckAsync() when integrated with dependency graph
}

/// <summary>
/// Represents a no-cycles condition between slices.
/// </summary>
public class NoCyclicSlicesCondition : ISliceCondition
{
    private readonly string _slicePattern;
    private readonly IEnumerable<Edge>? _edges;

    internal NoCyclicSlicesCondition(string slicePattern, IEnumerable<Edge>? edges)
    {
        _slicePattern = slicePattern;
        _edges = edges;
    }

    // TODO: Implement CheckAsync() when integrated with dependency graph
}
