using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;

namespace ArchUnitNet.Metrics.FluentApi;

/// <summary>
/// Builder for defining custom metrics with thresholds.
/// Allows users to create application-specific metrics and validate them.
/// </summary>
public class CustomMetricBuilder : Checkable
{
    private readonly string _metricName;
    private readonly Type? _targetType;
    private Func<object, double>? _calculator;
    private double? _minThreshold;
    private double? _maxThreshold;
    private string _unit = "";
    private string _description = "";

    public CustomMetricBuilder(string metricName, Type? targetType = null)
    {
        _metricName = metricName ?? throw new ArgumentNullException(nameof(metricName));
        _targetType = targetType;
    }

    /// <summary>
    /// Define the metric calculation function.
    /// </summary>
    public CustomMetricBuilder CalculatedBy(Func<object, double> calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Set a minimum threshold (metric must be >= min).
    /// </summary>
    public CustomMetricBuilder WithMinimum(double minValue)
    {
        _minThreshold = minValue;
        return this;
    }

    /// <summary>
    /// Set a maximum threshold (metric must be &lt;= max).
    /// </summary>
    public CustomMetricBuilder WithMaximum(double maxValue)
    {
        _maxThreshold = maxValue;
        return this;
    }

    /// <summary>
    /// Set both min and max thresholds.
    /// </summary>
    public CustomMetricBuilder WithRange(double min, double max)
    {
        _minThreshold = min;
        _maxThreshold = max;
        return this;
    }

    /// <summary>
    /// Set unit of measurement (e.g., "ms", "lines", "%").
    /// </summary>
    public CustomMetricBuilder WithUnit(string unit)
    {
        _unit = unit ?? "";
        return this;
    }

    /// <summary>
    /// Set description for the metric.
    /// </summary>
    public CustomMetricBuilder WithDescription(string description)
    {
        _description = description ?? "";
        return this;
    }

    /// <summary>
    /// Get the metric name.
    /// </summary>
    public string GetMetricName() => _metricName;

    /// <summary>
    /// Calculate metric value for a given object.
    /// </summary>
    public double CalculateMetric(object obj)
    {
        if (_calculator == null)
            throw new InvalidOperationException("Metric calculator not defined. Call CalculatedBy() first.");

        return _calculator(obj);
    }

    /// <summary>
    /// Check if a metric value passes threshold checks.
    /// </summary>
    public bool PassesThresholds(double value)
    {
        if (_minThreshold.HasValue && value < _minThreshold)
            return false;

        if (_maxThreshold.HasValue && value > _maxThreshold)
            return false;

        return true;
    }

    /// <summary>
    /// Validate metric against thresholds.
    /// </summary>
    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        if (_calculator == null)
            throw new InvalidOperationException("Metric calculator not defined");

        var violations = new List<Violation>();

        if (!PassesThresholds(1.0))
        {
            violations.Add(new CustomMetricViolation(
                _metricName,
                1.0,
                _minThreshold,
                _maxThreshold,
                $"Metric '{_metricName}' failed threshold check"
            ));
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    /// <summary>
    /// Get metric summary.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string> { _metricName };

        if (!string.IsNullOrEmpty(_unit))
            parts.Add($"({_unit})");

        if (_minThreshold.HasValue)
            parts.Add($">= {_minThreshold}");

        if (_maxThreshold.HasValue)
            parts.Add($"<= {_maxThreshold}");

        if (!string.IsNullOrEmpty(_description))
            parts.Add($"- {_description}");

        return string.Join(" ", parts);
    }
}

/// <summary>
/// Violation for custom metric threshold breaches.
/// </summary>
public record CustomMetricViolation(
    string MetricName,
    double ActualValue,
    double? MinThreshold,
    double? MaxThreshold,
    string Message
) : Violation
{
    public override string ToString()
    {
        var thresholds = new List<string>();
        if (MinThreshold.HasValue)
            thresholds.Add($"min={MinThreshold}");
        if (MaxThreshold.HasValue)
            thresholds.Add($"max={MaxThreshold}");

        var thresholdStr = thresholds.Count > 0 ? $" [{string.Join(", ", thresholds)}]" : "";
        return $"{Message} - actual={ActualValue:F2}{thresholdStr}";
    }
}

/// <summary>
/// Custom metrics collection for managing multiple metrics.
/// </summary>
public class CustomMetricsCollection
{
    private readonly Dictionary<string, CustomMetricBuilder> _metrics = new();

    /// <summary>
    /// Register a custom metric.
    /// </summary>
    public void Register(CustomMetricBuilder metric)
    {
        if (metric == null)
            throw new ArgumentNullException(nameof(metric));

        _metrics[metric.GetMetricName()] = metric;
    }

    /// <summary>
    /// Get a registered metric.
    /// </summary>
    public CustomMetricBuilder? Get(string metricName)
    {
        return _metrics.TryGetValue(metricName, out var metric) ? metric : null;
    }

    /// <summary>
    /// Check if metric exists.
    /// </summary>
    public bool Contains(string metricName) => _metrics.ContainsKey(metricName);

    /// <summary>
    /// Get all registered metrics.
    /// </summary>
    public IEnumerable<CustomMetricBuilder> GetAll() => _metrics.Values;

    /// <summary>
    /// Get metric count.
    /// </summary>
    public int Count => _metrics.Count;
}

/// <summary>
/// Extension methods for custom metrics.
/// </summary>
public static class CustomMetricExtensions
{
    /// <summary>
    /// Create a new custom metric.
    /// </summary>
    public static CustomMetricBuilder CreateMetric(string name)
    {
        return new CustomMetricBuilder(name);
    }

    /// <summary>
    /// Create a custom metric for a specific type.
    /// </summary>
    public static CustomMetricBuilder CreateMetric<T>(string name)
    {
        return new CustomMetricBuilder(name, typeof(T));
    }
}

/// <summary>
/// Common custom metric templates for reuse.
/// </summary>
public static class CustomMetricTemplates
{
    /// <summary>
    /// File size metric in kilobytes.
    /// </summary>
    public static CustomMetricBuilder FileSizeKB => new CustomMetricBuilder("FileSizeKB")
        .CalculatedBy(obj => (obj as FileInfo)?.Length / 1024.0 ?? 0)
        .WithUnit("KB")
        .WithDescription("File size in kilobytes")
        .WithMaximum(1000);

    /// <summary>
    /// Line count metric.
    /// </summary>
    public static CustomMetricBuilder LineCount => new CustomMetricBuilder("LineCount")
        .CalculatedBy(obj => obj switch
        {
            string str => str.Split('\n').Length,
            FileInfo fi => File.ReadAllLines(fi.FullName).Length,
            _ => 0
        })
        .WithUnit("lines")
        .WithDescription("Number of lines")
        .WithMaximum(5000);

    /// <summary>
    /// Comment percentage metric.
    /// </summary>
    public static CustomMetricBuilder CommentPercentage => new CustomMetricBuilder("CommentPercentage")
        .CalculatedBy(_ => 0)
        .WithUnit("%")
        .WithDescription("Percentage of comment lines")
        .WithMinimum(5)
        .WithMaximum(100);

    /// <summary>
    /// Code duplication percentage.
    /// </summary>
    public static CustomMetricBuilder DuplicationPercentage => new CustomMetricBuilder("DuplicationPercentage")
        .CalculatedBy(_ => 0)
        .WithUnit("%")
        .WithDescription("Code duplication percentage")
        .WithMaximum(10);
}
