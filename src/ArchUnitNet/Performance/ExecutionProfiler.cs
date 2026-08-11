using System.Diagnostics;
using ArchUnitNet.Common.FluentApi;

namespace ArchUnitNet.Performance;

/// <summary>
/// Profiles rule execution time and resource usage.
/// Helps identify performance bottlenecks in architecture validation.
/// </summary>
public class ExecutionProfiler : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _ruleName;
    private long _startMemory;
    private ExecutionProfile? _profile;

    public ExecutionProfiler(string ruleName)
    {
        _ruleName = ruleName ?? "Unknown";
        _stopwatch = new Stopwatch();
        _startMemory = GC.GetTotalMemory(false);
    }

    /// <summary>
    /// Start profiling.
    /// </summary>
    public void Start()
    {
        _startMemory = GC.GetTotalMemory(false);
        _stopwatch.Restart();
    }

    /// <summary>
    /// Stop profiling and return profile.
    /// </summary>
    public ExecutionProfile Stop()
    {
        _stopwatch.Stop();
        var endMemory = GC.GetTotalMemory(false);

        _profile = new ExecutionProfile
        {
            RuleName = _ruleName,
            ExecutionTime = _stopwatch.Elapsed,
            MemoryUsed = Math.Max(0, endMemory - _startMemory),
            Timestamp = DateTime.UtcNow,
        };

        return _profile;
    }

    /// <summary>
    /// Get current profile without stopping.
    /// </summary>
    public ExecutionProfile GetCurrentProfile()
    {
        return _profile ?? new ExecutionProfile
        {
            RuleName = _ruleName,
            ExecutionTime = _stopwatch.Elapsed,
            Timestamp = DateTime.UtcNow,
        };
    }

    public void Dispose()
    {
        _stopwatch?.Stop();
    }
}

/// <summary>
/// Profile of a single rule execution.
/// </summary>
public class ExecutionProfile
{
    public string RuleName { get; set; } = "";
    public TimeSpan ExecutionTime { get; set; }
    public long MemoryUsed { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ViolationCount { get; set; }

    /// <summary>
    /// Get human-readable execution time.
    /// </summary>
    public string GetFormattedExecutionTime()
    {
        return ExecutionTime.TotalMilliseconds switch
        {
            < 1 => $"{ExecutionTime.TotalMicroseconds:F2} µs",
            < 1000 => $"{ExecutionTime.TotalMilliseconds:F2} ms",
            _ => $"{ExecutionTime.TotalSeconds:F2} s",
        };
    }

    /// <summary>
    /// Get human-readable memory usage.
    /// </summary>
    public string GetFormattedMemoryUsage()
    {
        return MemoryUsed switch
        {
            < 1024 => $"{MemoryUsed} bytes",
            < 1024 * 1024 => $"{MemoryUsed / 1024.0:F2} KB",
            < 1024 * 1024 * 1024 => $"{MemoryUsed / (1024.0 * 1024):F2} MB",
            _ => $"{MemoryUsed / (1024.0 * 1024 * 1024):F2} GB",
        };
    }

    /// <summary>
    /// Get summary text.
    /// </summary>
    public string GetSummary()
    {
        var violations = ViolationCount > 0 ? $", {ViolationCount} violations" : "";
        return $"{RuleName}: {GetFormattedExecutionTime()}{violations}";
    }
}

/// <summary>
/// Aggregates multiple execution profiles for analysis.
/// </summary>
public class PerformanceReport
{
    private readonly List<ExecutionProfile> _profiles = new();

    public void Add(ExecutionProfile profile)
    {
        _profiles.Add(profile ?? throw new ArgumentNullException(nameof(profile)));
    }

    public void AddRange(IEnumerable<ExecutionProfile> profiles)
    {
        _profiles.AddRange(profiles);
    }

    /// <summary>
    /// Get total execution time.
    /// </summary>
    public TimeSpan GetTotalTime()
    {
        return TimeSpan.FromMilliseconds(_profiles.Sum(p => p.ExecutionTime.TotalMilliseconds));
    }

    /// <summary>
    /// Get total memory used.
    /// </summary>
    public long GetTotalMemory()
    {
        return _profiles.Sum(p => p.MemoryUsed);
    }

    /// <summary>
    /// Get slowest rules.
    /// </summary>
    public List<ExecutionProfile> GetSlowestRules(int count = 10)
    {
        return _profiles
            .OrderByDescending(p => p.ExecutionTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Get most memory-intensive rules.
    /// </summary>
    public List<ExecutionProfile> GetMemoryIntensiveRules(int count = 10)
    {
        return _profiles
            .OrderByDescending(p => p.MemoryUsed)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Get average execution time.
    /// </summary>
    public TimeSpan GetAverageTime()
    {
        if (_profiles.Count == 0)
            return TimeSpan.Zero;

        return TimeSpan.FromMilliseconds(
            _profiles.Average(p => p.ExecutionTime.TotalMilliseconds)
        );
    }

    /// <summary>
    /// Get rules by performance tier.
    /// </summary>
    public IDictionary<string, List<ExecutionProfile>> GroupByPerformanceTier()
    {
        var average = GetAverageTime();
        var fast = average.TotalMilliseconds / 3;
        var slow = average.TotalMilliseconds * 2;

        return _profiles.GroupBy(p =>
        {
            var ms = p.ExecutionTime.TotalMilliseconds;
            return ms < fast ? "Fast" :
                   ms > slow ? "Slow" : "Average";
        }).ToDictionary(g => g.Key, g => g.ToList());
    }

    /// <summary>
    /// Get performance summary.
    /// </summary>
    public PerformanceSummary GetSummary()
    {
        return new PerformanceSummary
        {
            RuleCount = _profiles.Count,
            TotalTime = GetTotalTime(),
            AverageTime = GetAverageTime(),
            TotalMemory = GetTotalMemory(),
            SlowestRule = _profiles.OrderByDescending(p => p.ExecutionTime).FirstOrDefault(),
            MostMemoryIntensive = _profiles.OrderByDescending(p => p.MemoryUsed).FirstOrDefault(),
            TotalViolations = _profiles.Sum(p => p.ViolationCount),
        };
    }

    /// <summary>
    /// Export report as text.
    /// </summary>
    public string ExportAsText()
    {
        var lines = new List<string>
        {
            "=== Performance Report ===",
            "",
            $"Total Rules: {_profiles.Count}",
            $"Total Time: {GetTotalTime().TotalSeconds:F2}s",
            $"Average Time: {GetAverageTime().TotalMilliseconds:F2}ms",
            $"Total Memory: {GetMemoryFormatted()}",
            "",
            "=== Slowest Rules ===",
        };

        foreach (var profile in GetSlowestRules(5))
        {
            lines.Add($"  {profile.GetSummary()}");
        }

        lines.Add("");
        lines.Add("=== Most Memory-Intensive ===");

        foreach (var profile in GetMemoryIntensiveRules(5))
        {
            lines.Add($"  {profile.RuleName}: {profile.GetFormattedMemoryUsage()}");
        }

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Get formatted total memory.
    /// </summary>
    public string GetMemoryFormatted()
    {
        var bytes = GetTotalMemory();
        return bytes switch
        {
            < 1024 => $"{bytes} bytes",
            < 1024 * 1024 => $"{bytes / 1024.0:F2} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F2} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
        };
    }
}

/// <summary>
/// Summary statistics for a performance report.
/// </summary>
public class PerformanceSummary
{
    public int RuleCount { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan AverageTime { get; set; }
    public long TotalMemory { get; set; }
    public ExecutionProfile? SlowestRule { get; set; }
    public ExecutionProfile? MostMemoryIntensive { get; set; }
    public int TotalViolations { get; set; }

    /// <summary>
    /// Get summary text.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string>
        {
            $"{RuleCount} rules",
            $"{TotalTime.TotalSeconds:F2}s total",
            $"{AverageTime.TotalMilliseconds:F0}ms average",
            $"{TotalViolations} violations",
        };

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Extension methods for performance profiling.
/// </summary>
public static class ProfileExtensions
{
    private static readonly PerformanceReport _globalReport = new();

    /// <summary>
    /// Profile a rule check operation.
    /// </summary>
    public static async Task<(IReadOnlyList<Common.Assertion.Violation> Violations, ExecutionProfile Profile)> ProfileCheckAsync(
        this Checkable rule,
        string ruleName)
    {
        using (var profiler = new ExecutionProfiler(ruleName))
        {
            profiler.Start();
            var violations = await rule.CheckAsync();
            var profile = profiler.Stop();
            profile.ViolationCount = violations.Count;
            _globalReport.Add(profile);
            return (violations, profile);
        }
    }

    /// <summary>
    /// Get global performance report.
    /// </summary>
    public static PerformanceReport GetGlobalReport()
    {
        return _globalReport;
    }

    /// <summary>
    /// Clear global performance report.
    /// </summary>
    public static void ClearGlobalReport()
    {
        _globalReport.GetType()
            .GetField("_profiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(_globalReport, new List<ExecutionProfile>());
    }
}
