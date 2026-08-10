using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Metrics.Assertion;
using ArchUnitNet.Metrics.Common;
using ArchUnitNet.Metrics.Extraction;

namespace ArchUnitNet.Metrics.FluentApi;

/// <summary>
/// Builder for count-based metrics (method count, field count, field access count).
/// </summary>
public class CountMetricsBuilder : Checkable
{
    private readonly Type? _targetType;
    private readonly ClassInfoBatchExtractor _extractor;
    private readonly CountMetricType _metricType;
    private int? _maxCount;
    private int? _minCount;

    internal CountMetricsBuilder(Type? targetType, ClassInfoBatchExtractor extractor, CountMetricType metricType)
    {
        _targetType = targetType;
        _extractor = extractor;
        _metricType = metricType;
    }

    /// <summary>
    /// Set maximum count threshold.
    /// </summary>
    public CountMetricsBuilder ShouldHaveAtMost(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative", nameof(count));

        _maxCount = count;
        return this;
    }

    /// <summary>
    /// Set minimum count threshold.
    /// </summary>
    public CountMetricsBuilder ShouldHaveAtLeast(int count)
    {
        if (count < 0)
            throw new ArgumentException("Count must be non-negative", nameof(count));

        _minCount = count;
        return this;
    }

    /// <summary>
    /// Execute the rule and return violations for classes exceeding count thresholds.
    /// </summary>
    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        var violations = new List<Violation>();

        if (_extractor.GetExtractedClasses().Count == 0)
            return violations.AsReadOnly();

        var classes = _extractor.GetExtractedClasses();

        foreach (var classInfo in classes)
        {
            var value = GetCountValue(classInfo);
            var metricName = _metricType.ToString();

            // Check max threshold
            if (_maxCount.HasValue && value > _maxCount.Value)
            {
                var violation = ThresholdViolation.CreateExceeded(
                    classInfo.Name,
                    metricName,
                    value,
                    _maxCount.Value
                );
                violations.Add(violation);
            }

            // Check min threshold
            if (_minCount.HasValue && value < _minCount.Value)
            {
                var violation = ThresholdViolation.CreateBelowThreshold(
                    classInfo.Name,
                    metricName,
                    value,
                    _minCount.Value
                );
                violations.Add(violation);
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    private double GetCountValue(ClassInfo classInfo)
    {
        return _metricType switch
        {
            CountMetricType.MethodCount => classInfo.MethodCount,
            CountMetricType.FieldCount => classInfo.FieldCount,
            CountMetricType.FieldAccessCount => classInfo.Methods.Sum(m => m.FieldAccessCount),
            _ => throw new InvalidOperationException($"Unknown metric type: {_metricType}")
        };
    }
}
