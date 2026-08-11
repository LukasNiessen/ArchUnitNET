using Newtonsoft.Json.Linq;

namespace ArchUnitNet.Configuration;

/// <summary>
/// Loads and parses architecture rules from JSON configuration files.
/// Enables version-controlled, team-consistent rule definitions.
/// </summary>
public class RuleConfigurationLoader
{
    public async Task<RuleConfiguration> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Configuration file not found: {filePath}");

        var content = await File.ReadAllTextAsync(filePath);
        return LoadFromJson(content);
    }

    public RuleConfiguration LoadFromJson(string jsonContent)
    {
        try
        {
            var jObject = JObject.Parse(jsonContent);
            return ParseConfiguration(jObject);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to parse rule configuration JSON", ex);
        }
    }

    private RuleConfiguration ParseConfiguration(JObject jObject)
    {
        var config = new RuleConfiguration();

        if (jObject["projectPath"] != null)
            config.ProjectPath = jObject["projectPath"]!.Value<string>();

        if (jObject["rules"] is JArray rulesArray)
        {
            foreach (var ruleToken in rulesArray)
            {
                if (ruleToken is JObject ruleObj)
                {
                    config.Rules.Add(ParseRule(ruleObj));
                }
            }
        }

        if (jObject["presets"] is JArray presetsArray)
        {
            foreach (var presetToken in presetsArray)
            {
                config.Presets.Add(presetToken.Value<string>() ?? "");
            }
        }

        if (jObject["excludePatterns"] is JArray excludeArray)
        {
            foreach (var excludeToken in excludeArray)
            {
                config.ExcludePatterns.Add(excludeToken.Value<string>() ?? "");
            }
        }

        if (jObject["severity"] != null)
            config.Severity = jObject["severity"]!.Value<string>() ?? "error";

        return config;
    }

    private RuleDefinition ParseRule(JObject ruleObj)
    {
        var rule = new RuleDefinition
        {
            Id = ruleObj["id"]?.Value<string>() ?? $"rule_{Guid.NewGuid()}",
            Type = ruleObj["type"]?.Value<string>() ?? "FileDependency",
            Description = ruleObj["description"]?.Value<string>() ?? "",
            Enabled = ruleObj["enabled"]?.Value<bool>() ?? true,
        };

        if (ruleObj["source"] is JObject sourceObj)
        {
            rule.Source = ParsePattern(sourceObj);
        }

        if (ruleObj["target"] is JObject targetObj)
        {
            rule.Target = ParsePattern(targetObj);
        }

        if (ruleObj["action"] != null)
            rule.Action = ruleObj["action"]!.Value<string>() ?? "forbid";

        if (ruleObj["severity"] != null)
            rule.Severity = ruleObj["severity"]!.Value<string>() ?? "error";

        if (ruleObj["tags"] is JArray tagsArray)
        {
            rule.Tags = tagsArray.Select(t => t.Value<string>() ?? "").ToList();
        }

        return rule;
    }

    private PatternDefinition ParsePattern(JObject patternObj)
    {
        return new PatternDefinition
        {
            Path = patternObj["path"]?.Value<string>() ?? "",
            Exclude = patternObj["exclude"]?.Value<string>() ?? "",
            Type = patternObj["type"]?.Value<string>() ?? "glob",
        };
    }
}

/// <summary>
/// Architecture rules configuration.
/// </summary>
public class RuleConfiguration
{
    public string? ProjectPath { get; set; }
    public List<RuleDefinition> Rules { get; set; } = new();
    public List<string> Presets { get; set; } = new();
    public List<string> ExcludePatterns { get; set; } = new();
    public string Severity { get; set; } = "error";
}

/// <summary>
/// Single rule definition from configuration.
/// </summary>
public class RuleDefinition
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "FileDependency";
    public string Description { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public PatternDefinition? Source { get; set; }
    public PatternDefinition? Target { get; set; }
    public string Action { get; set; } = "forbid"; // forbid, require, cycleFree, acyclic
    public string Severity { get; set; } = "error"; // error, warning, info
    public List<string> Tags { get; set; } = new();
    public string? OnViolation { get; set; } // fail, warn, log

    public override string ToString()
    {
        return $"{Type}({Id}): {Description}";
    }
}

/// <summary>
/// Pattern definition for source/target paths.
/// </summary>
public class PatternDefinition
{
    public string Path { get; set; } = "";
    public string Exclude { get; set; } = "";
    public string Type { get; set; } = "glob"; // glob, regex, exact
}

/// <summary>
/// Extension methods for rule configuration.
/// </summary>
public static class RuleConfigurationExtensions
{
    /// <summary>
    /// Load rules from JSON configuration file.
    /// </summary>
    public static async Task<RuleConfiguration> LoadArchitectureRulesAsync(string configPath)
    {
        var loader = new RuleConfigurationLoader();
        return await loader.LoadFromFileAsync(configPath);
    }

    /// <summary>
    /// Get rules by tag.
    /// </summary>
    public static List<RuleDefinition> GetRulesByTag(this RuleConfiguration config, string tag)
    {
        return config.Rules
            .Where(r => r.Enabled && r.Tags.Contains(tag))
            .ToList();
    }

    /// <summary>
    /// Get rules by type.
    /// </summary>
    public static List<RuleDefinition> GetRulesByType(this RuleConfiguration config, string type)
    {
        return config.Rules
            .Where(r => r.Enabled && r.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Get all enabled rules.
    /// </summary>
    public static List<RuleDefinition> GetEnabledRules(this RuleConfiguration config)
    {
        return config.Rules.Where(r => r.Enabled).ToList();
    }

    /// <summary>
    /// Validate configuration structure.
    /// </summary>
    public static List<string> Validate(this RuleConfiguration config)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(config.ProjectPath))
            errors.Add("ProjectPath is required");

        if (!config.Rules.Any() && !config.Presets.Any())
            errors.Add("At least one rule or preset must be defined");

        foreach (var rule in config.Rules)
        {
            if (string.IsNullOrEmpty(rule.Id))
                errors.Add("Rule must have an ID");

            if (rule.Source == null || string.IsNullOrEmpty(rule.Source.Path))
                errors.Add($"Rule {rule.Id} must define source path");

            if (rule.Action == "forbid" || rule.Action == "require")
            {
                if (rule.Target == null || string.IsNullOrEmpty(rule.Target.Path))
                    errors.Add($"Rule {rule.Id} requires target path for action '{rule.Action}'");
            }
        }

        return errors;
    }
}
