using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Metrics.Assertion;

/// <summary>
/// Base violation for metric-based architecture rules.
/// </summary>
public record MetricViolation(
    string ClassName,
    string MetricName,
    double Value,
    string Message
) : Violation
{
    public override string ToString() => Message;

    public static MetricViolation Create(string className, string metricName, double value, string reason)
    {
        var message = $"{className}.{metricName} = {value:F2} ({reason})";
        return new MetricViolation(className, metricName, value, message);
    }
}
