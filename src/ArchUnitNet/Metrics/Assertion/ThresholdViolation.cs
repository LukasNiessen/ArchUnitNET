using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Metrics.Assertion;

/// <summary>
/// Violation when a metric exceeds or falls below a threshold.
/// Example: "LCOM96a should be less than 0.5 but was 0.8"
/// </summary>
public record ThresholdViolation(
    string ClassName,
    string MetricName,
    double ActualValue,
    double ThresholdValue,
    string Operator, // "<", ">", "<=", ">=", "=="
    string Message
) : Violation
{
    public override string ToString() => Message;

    public static ThresholdViolation CreateExceeded(string className, string metricName, double actual, double threshold)
    {
        var message = $"{className}.{metricName} should be <= {threshold:F2} but was {actual:F2}";
        return new ThresholdViolation(className, metricName, actual, threshold, "<=", message);
    }

    public static ThresholdViolation CreateBelowThreshold(string className, string metricName, double actual, double threshold)
    {
        var message = $"{className}.{metricName} should be >= {threshold:F2} but was {actual:F2}";
        return new ThresholdViolation(className, metricName, actual, threshold, ">=", message);
    }
}
