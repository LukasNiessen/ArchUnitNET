using ArchUnitNet.GraphReporting;
using ArchUnitNet.Metrics.Extraction;
using ArchUnitNet.Metrics.FluentApi;
using ArchUnitNet.Slices.FluentApi;

namespace ArchUnitNet;

/// <summary>
/// ArchUnit: Architecture testing for C# and .NET.
///
/// This is the public API surface. All entry points are re-exported here.
/// The actual implementation lives in submodules (Common, Files, Metrics, Slices, Graph, Testing).
/// </summary>
public static class ArchUnit
{
    /// <summary>
    /// Create a metrics-based architecture rule.
    /// Entry point for method cohesion, field usage, and complexity analysis.
    /// </summary>
    public static MetricsBuilder Metrics() => new MetricsBuilder(null, new ClassInfoBatchExtractor());

    /// <summary>
    /// Create a metrics rule for a specific type.
    /// </summary>
    public static MetricsBuilder Metrics<T>() => MetricsBuilder.Of(typeof(T));

    /// <summary>
    /// Create a slice-based architecture rule.
    /// Entry point for defining and validating logical slices.
    /// </summary>
    public static SliceConditionBuilder ProjectSlices() => new SliceConditionBuilder();

    /// <summary>
    /// Create a graph builder for visualizing dependencies.
    /// Entry point for graph reporting in multiple formats (Mermaid, DOT, D2, CSV, JSON, HTML).
    /// </summary>
    public static ProjectGraphBuilder ProjectGraph() => new ProjectGraphBuilder();

    // Entry points will be added here as modules are implemented
    // e.g., public static IFileConditionBuilder ProjectFiles(string? projectPath = null) => ...
}
