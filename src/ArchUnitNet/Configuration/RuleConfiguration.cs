using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchUnitNet.Configuration;

/// <summary>
/// Configuration for ArchUnit rules in JSON/YAML format.
/// Enables team-wide architecture rule sharing via version control.
/// </summary>
public class RuleConfiguration
{
    /// <summary>
    /// Rule configuration file version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Project name for documentation.
    /// </summary>
    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; } = "";

    /// <summary>
    /// Description of the architecture rules.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// List of file-based rules.
    /// </summary>
    [JsonPropertyName("fileRules")]
    public List<FileRuleConfig> FileRules { get; set; } = new();

    /// <summary>
    /// List of metrics-based rules.
    /// </summary>
    [JsonPropertyName("metricsRules")]
    public List<MetricsRuleConfig> MetricsRules { get; set; } = new();

    /// <summary>
    /// List of slice-based rules.
    /// </summary>
    [JsonPropertyName("sliceRules")]
    public List<SliceRuleConfig> SliceRules { get; set; } = new();

    /// <summary>
    /// Configuration metadata.
    /// </summary>
    [JsonPropertyName("metadata")]
    public MetadataConfig Metadata { get; set; } = new();

    /// <summary>
    /// Load configuration from JSON file.
    /// </summary>
    public static async Task<RuleConfiguration?> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<RuleConfiguration>(json, options);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Save configuration to JSON file.
    /// </summary>
    public async Task SaveToFileAsync(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(this, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Serialize configuration to JSON string.
    /// </summary>
    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Get configuration summary.
    /// </summary>
    public string GetSummary()
    {
        var lines = new List<string>
        {
            $"Project: {ProjectName}",
            $"Description: {Description}",
            $"File Rules: {FileRules.Count}",
            $"Metrics Rules: {MetricsRules.Count}",
            $"Slice Rules: {SliceRules.Count}"
        };

        return string.Join(Environment.NewLine, lines);
    }
}

/// <summary>
/// File-based rule configuration.
/// </summary>
public class FileRuleConfig
{
    /// <summary>
    /// Unique rule identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Human-readable rule name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Rule description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Path pattern to match source files.
    /// </summary>
    [JsonPropertyName("sourcePath")]
    public string SourcePath { get; set; } = "";

    /// <summary>
    /// Type of rule (DependsOn, DoesNotDependOn, HasNoCycles, etc.).
    /// </summary>
    [JsonPropertyName("ruleType")]
    public string RuleType { get; set; } = "";

    /// <summary>
    /// Target path pattern or condition.
    /// </summary>
    [JsonPropertyName("targetPath")]
    public string? TargetPath { get; set; }

    /// <summary>
    /// Whether the rule is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Severity level (Error, Warning, Info).
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Metrics-based rule configuration.
/// </summary>
public class MetricsRuleConfig
{
    /// <summary>
    /// Unique rule identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Human-readable rule name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Rule description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Metric type (LCOM96a, MethodCount, FieldCount, etc.).
    /// </summary>
    [JsonPropertyName("metricType")]
    public string MetricType { get; set; } = "";

    /// <summary>
    /// Target value or threshold.
    /// </summary>
    [JsonPropertyName("threshold")]
    public double Threshold { get; set; }

    /// <summary>
    /// Comparison operator (LessThan, GreaterThan, EqualTo, etc.).
    /// </summary>
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "LessThan";

    /// <summary>
    /// Whether the rule is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Severity level (Error, Warning, Info).
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Slice-based rule configuration.
/// </summary>
public class SliceRuleConfig
{
    /// <summary>
    /// Unique rule identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Human-readable rule name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// Rule description.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// Path pattern for slice extraction (e.g., "src/{Slice}/**").
    /// </summary>
    [JsonPropertyName("slicePattern")]
    public string SlicePattern { get; set; } = "";

    /// <summary>
    /// Type of validation (BeAcyclic, AdhereToDefinedSlices, FollowPattern).
    /// </summary>
    [JsonPropertyName("ruleType")]
    public string RuleType { get; set; } = "";

    /// <summary>
    /// Optional dependency pattern (e.g., "UI -> Service -> Data").
    /// </summary>
    [JsonPropertyName("dependencyPattern")]
    public string? DependencyPattern { get; set; }

    /// <summary>
    /// Whether the rule is enabled.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Severity level (Error, Warning, Info).
    /// </summary>
    [JsonPropertyName("severity")]
    public string Severity { get; set; } = "Error";
}

/// <summary>
/// Configuration metadata.
/// </summary>
public class MetadataConfig
{
    /// <summary>
    /// Author of the configuration.
    /// </summary>
    [JsonPropertyName("author")]
    public string? Author { get; set; }

    /// <summary>
    /// Last modified date.
    /// </summary>
    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tags for categorization.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Documentation URL or reference.
    /// </summary>
    [JsonPropertyName("documentationUrl")]
    public string? DocumentationUrl { get; set; }

    /// <summary>
    /// Custom key-value pairs for extensions.
    /// </summary>
    [JsonPropertyName("custom")]
    public Dictionary<string, object> Custom { get; set; } = new();
}

/// <summary>
/// Extension methods for rule configuration.
/// </summary>
public static class RuleConfigurationExtensions
{
    /// <summary>
    /// Get enabled file rules.
    /// </summary>
    public static IEnumerable<FileRuleConfig> GetEnabledFileRules(this RuleConfiguration config)
    {
        return config.FileRules.Where(r => r.Enabled);
    }

    /// <summary>
    /// Get enabled metrics rules.
    /// </summary>
    public static IEnumerable<MetricsRuleConfig> GetEnabledMetricsRules(this RuleConfiguration config)
    {
        return config.MetricsRules.Where(r => r.Enabled);
    }

    /// <summary>
    /// Get enabled slice rules.
    /// </summary>
    public static IEnumerable<SliceRuleConfig> GetEnabledSliceRules(this RuleConfiguration config)
    {
        return config.SliceRules.Where(r => r.Enabled);
    }

    /// <summary>
    /// Get rules by severity level.
    /// </summary>
    public static IEnumerable<FileRuleConfig> GetFileRulesBySeverity(this RuleConfiguration config, string severity)
    {
        return config.FileRules.Where(r => r.Severity == severity);
    }

    /// <summary>
    /// Get total rule count.
    /// </summary>
    public static int GetTotalRuleCount(this RuleConfiguration config)
    {
        return config.FileRules.Count + config.MetricsRules.Count + config.SliceRules.Count;
    }

    /// <summary>
    /// Merge two configurations (later config overrides earlier).
    /// </summary>
    public static RuleConfiguration Merge(this RuleConfiguration config, RuleConfiguration other)
    {
        var merged = new RuleConfiguration
        {
            Version = other.Version,
            ProjectName = other.ProjectName ?? config.ProjectName,
            Description = other.Description ?? config.Description,
            Metadata = other.Metadata
        };

        merged.FileRules.AddRange(config.FileRules);
        merged.FileRules.AddRange(other.FileRules);

        merged.MetricsRules.AddRange(config.MetricsRules);
        merged.MetricsRules.AddRange(other.MetricsRules);

        merged.SliceRules.AddRange(config.SliceRules);
        merged.SliceRules.AddRange(other.SliceRules);

        return merged;
    }
}
