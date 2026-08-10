using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Metrics.Assertion;
using ArchUnitNet.Metrics.Calculation;
using ArchUnitNet.Metrics.Common;
using ArchUnitNet.Metrics.Extraction;

namespace ArchUnitNet.Metrics.FluentApi;

/// <summary>
/// Builder for LCOM threshold validation.
/// Allows chaining of threshold conditions (ShouldBeLessThan, ShouldBeAbove).
/// </summary>
public class LCOMThresholdBuilder : Checkable
{
    private readonly Type? _targetType;
    private readonly ClassInfoBatchExtractor _extractor;
    private readonly LCOMVariant _variant;
    private double? _maxThreshold;
    private double? _minThreshold;

    internal LCOMThresholdBuilder(Type? targetType, ClassInfoBatchExtractor extractor, LCOMVariant variant)
    {
        _targetType = targetType;
        _extractor = extractor;
        _variant = variant;
    }

    /// <summary>
    /// Set maximum threshold (cohesion must be less than or equal to this value).
    /// Lower LCOM values = higher cohesion, so this sets the "good" threshold.
    /// </summary>
    public LCOMThresholdBuilder ShouldBeLessThan(double threshold)
    {
        if (threshold < 0)
            throw new ArgumentException("Threshold must be non-negative", nameof(threshold));

        _maxThreshold = threshold;
        return this;
    }

    /// <summary>
    /// Set minimum threshold (cohesion must be greater than or equal to this value).
    /// </summary>
    public LCOMThresholdBuilder ShouldBeAbove(double threshold)
    {
        if (threshold < 0)
            throw new ArgumentException("Threshold must be non-negative", nameof(threshold));

        _minThreshold = threshold;
        return this;
    }

    /// <summary>
    /// Execute the rule and return violations for classes exceeding thresholds.
    /// </summary>
    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        var violations = new List<Violation>();

        // Extract classes if not already done
        if (_extractor.GetExtractedClasses().Count == 0 && _targetType != null)
        {
            // For now, we can only analyze if classes were pre-extracted
            // In a full implementation, this would load from assembly reflection
            return violations.AsReadOnly();
        }

        var classes = _extractor.GetExtractedClasses();

        foreach (var classInfo in classes)
        {
            var calculator = new LCOMCalculator(classInfo);
            var value = CalculateLCOMValue(calculator);

            // Check max threshold
            if (_maxThreshold.HasValue && value > _maxThreshold.Value)
            {
                var violation = ThresholdViolation.CreateExceeded(
                    classInfo.Name,
                    _variant.ToString(),
                    value,
                    _maxThreshold.Value
                );
                violations.Add(violation);
            }

            // Check min threshold
            if (_minThreshold.HasValue && value < _minThreshold.Value)
            {
                var violation = ThresholdViolation.CreateBelowThreshold(
                    classInfo.Name,
                    _variant.ToString(),
                    value,
                    _minThreshold.Value
                );
                violations.Add(violation);
            }
        }

        return await Task.FromResult(violations.AsReadOnly());
    }

    private double CalculateLCOMValue(LCOMCalculator calculator)
    {
        return _variant switch
        {
            LCOMVariant.LCOM1 => calculator.CalculateLCOM1(),
            LCOMVariant.LCOM96a => calculator.CalculateLCOM96a(),
            LCOMVariant.LCOM96b => calculator.CalculateLCOM96b(),
            LCOMVariant.LCOM1995 => calculator.CalculateLCOM1995(),
            _ => throw new InvalidOperationException($"Unknown LCOM variant: {_variant}")
        };
    }
}
