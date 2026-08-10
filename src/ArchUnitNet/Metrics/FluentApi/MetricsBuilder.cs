using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Metrics.Calculation;
using ArchUnitNet.Metrics.Extraction;

namespace ArchUnitNet.Metrics.FluentApi;

/// <summary>
/// Entry point for metrics-based architecture rules.
/// Provides fluent API to select classes and metrics to analyze.
/// </summary>
public class MetricsBuilder
{
    private readonly Type? _targetType;
    private readonly ClassInfoBatchExtractor _extractor;

    public MetricsBuilder(Type? targetType, ClassInfoBatchExtractor extractor)
    {
        _targetType = targetType;
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
    }

    /// <summary>
    /// Create a metrics rule for a specific type.
    /// </summary>
    public static MetricsBuilder Of(Type targetType)
    {
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        var extractor = new ClassInfoBatchExtractor();
        return new MetricsBuilder(targetType, extractor);
    }

    /// <summary>
    /// Create a metrics rule that will analyze all classes in the specified directory.
    /// </summary>
    public static MetricsBuilder OfProject(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
            throw new ArgumentException("Project path cannot be null or empty", nameof(projectPath));

        var extractor = new ClassInfoBatchExtractor();
        extractor.ExtractFromDirectory(projectPath, recursive: true);
        return new MetricsBuilder(null, extractor);
    }

    /// <summary>
    /// Transition to method-level metrics analysis.
    /// </summary>
    public MethodMetricsBuilder Methods()
    {
        return new MethodMetricsBuilder(_targetType, _extractor);
    }

    /// <summary>
    /// Transition to class-level metrics analysis (e.g., line count, cyclomatic complexity).
    /// </summary>
    public ClassMetricsBuilder Classes()
    {
        return new ClassMetricsBuilder(_targetType, _extractor);
    }
}

/// <summary>
/// Builder for method-level metrics (LCOM variants, field access count).
/// </summary>
public class MethodMetricsBuilder
{
    private readonly Type? _targetType;
    private readonly ClassInfoBatchExtractor _extractor;

    internal MethodMetricsBuilder(Type? targetType, ClassInfoBatchExtractor extractor)
    {
        _targetType = targetType;
        _extractor = extractor;
    }

    /// <summary>
    /// Select LCOM96a metric (0-1 range, lower = higher cohesion).
    /// </summary>
    public LCOMThresholdBuilder LCOM96a()
    {
        return new LCOMThresholdBuilder(_targetType, _extractor, LCOMVariant.LCOM96a);
    }

    /// <summary>
    /// Select LCOM96b metric (0-1 range with isolation penalty).
    /// </summary>
    public LCOMThresholdBuilder LCOM96b()
    {
        return new LCOMThresholdBuilder(_targetType, _extractor, LCOMVariant.LCOM96b);
    }

    /// <summary>
    /// Select LCOM1 metric (Henderson-Sellers, 0-2 range).
    /// </summary>
    public LCOMThresholdBuilder LCOM1()
    {
        return new LCOMThresholdBuilder(_targetType, _extractor, LCOMVariant.LCOM1);
    }

    /// <summary>
    /// Select LCOM1995 metric (original Chidamber & Kemerer).
    /// </summary>
    public LCOMThresholdBuilder LCOM1995()
    {
        return new LCOMThresholdBuilder(_targetType, _extractor, LCOMVariant.LCOM1995);
    }

    /// <summary>
    /// Select method count metric.
    /// </summary>
    public CountMetricsBuilder Count()
    {
        return new CountMetricsBuilder(_targetType, _extractor, CountMetricType.MethodCount);
    }

    /// <summary>
    /// Select field access count metric.
    /// </summary>
    public CountMetricsBuilder FieldAccessCount()
    {
        return new CountMetricsBuilder(_targetType, _extractor, CountMetricType.FieldAccessCount);
    }
}

/// <summary>
/// Builder for class-level metrics.
/// </summary>
public class ClassMetricsBuilder
{
    private readonly Type? _targetType;
    private readonly ClassInfoBatchExtractor _extractor;

    internal ClassMetricsBuilder(Type? targetType, ClassInfoBatchExtractor extractor)
    {
        _targetType = targetType;
        _extractor = extractor;
    }

    /// <summary>
    /// Select field count metric.
    /// </summary>
    public CountMetricsBuilder FieldCount()
    {
        return new CountMetricsBuilder(_targetType, _extractor, CountMetricType.FieldCount);
    }

    /// <summary>
    /// Select method count metric at class level.
    /// </summary>
    public CountMetricsBuilder MethodCount()
    {
        return new CountMetricsBuilder(_targetType, _extractor, CountMetricType.MethodCount);
    }
}

/// <summary>
/// Supported LCOM variants.
/// </summary>
public enum LCOMVariant
{
    LCOM1,
    LCOM96a,
    LCOM96b,
    LCOM1995
}

/// <summary>
/// Supported count metrics.
/// </summary>
public enum CountMetricType
{
    MethodCount,
    FieldCount,
    FieldAccessCount
}
