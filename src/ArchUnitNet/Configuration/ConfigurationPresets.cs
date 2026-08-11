using ArchUnitNet.Configuration;

namespace ArchUnitNet.Presets;

/// <summary>
/// Pre-built rule configurations for common architecture patterns.
/// Enables quick setup for typical scenarios without manual configuration.
/// </summary>
public static class ConfigurationPresets
{
    /// <summary>
    /// Get a layered architecture configuration (UI -> Service -> Data).
    /// </summary>
    public static RuleConfiguration LayeredArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Layered Architecture",
            Description = "Enforces strict layering: UI can depend on Service, Service on Data, but no reverse dependencies",
            FileRules = new List<FileRuleConfig>
            {
                new FileRuleConfig
                {
                    Id = "layer-1",
                    Name = "UI must not depend on Data",
                    Description = "User interface layer should not directly depend on data layer",
                    SourcePath = "src/UI/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/Data/**",
                    Enabled = true,
                    Severity = "Error"
                },
                new FileRuleConfig
                {
                    Id = "layer-2",
                    Name = "Service must not depend on UI",
                    Description = "Business logic layer should not depend on UI layer",
                    SourcePath = "src/Service/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/UI/**",
                    Enabled = true,
                    Severity = "Error"
                },
                new FileRuleConfig
                {
                    Id = "layer-3",
                    Name = "No circular dependencies in layers",
                    Description = "Layers should form an acyclic dependency structure",
                    SourcePath = "src/**",
                    RuleType = "HaveNoCycles",
                    Enabled = true,
                    Severity = "Error"
                }
            }
        };
    }

    /// <summary>
    /// Get a hexagonal (ports and adapters) architecture configuration.
    /// </summary>
    public static RuleConfiguration HexagonalArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Hexagonal Architecture",
            Description = "Enforces hexagonal pattern: Domain is independent, adapters depend on ports",
            FileRules = new List<FileRuleConfig>
            {
                new FileRuleConfig
                {
                    Id = "hex-1",
                    Name = "Domain must not depend on Adapters",
                    Description = "Core domain logic should not depend on infrastructure",
                    SourcePath = "src/Domain/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/Adapters/**",
                    Enabled = true,
                    Severity = "Error"
                },
                new FileRuleConfig
                {
                    Id = "hex-2",
                    Name = "Domain must not depend on Ports",
                    Description = "Core domain should not depend on its own ports",
                    SourcePath = "src/Domain/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/Ports/**",
                    Enabled = true,
                    Severity = "Error"
                }
            }
        };
    }

    /// <summary>
    /// Get a feature-based architecture configuration (feature isolation).
    /// </summary>
    public static RuleConfiguration FeatureBasedArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Feature-Based Architecture",
            Description = "Ensures features are independent modules with minimal cross-feature dependencies",
            SliceRules = new List<SliceRuleConfig>
            {
                new SliceRuleConfig
                {
                    Id = "feat-1",
                    Name = "Features must be acyclic",
                    Description = "No circular dependencies between features",
                    SlicePattern = "src/Features/{Slice}/**",
                    RuleType = "BeAcyclic",
                    Enabled = true,
                    Severity = "Error"
                }
            }
        };
    }

    /// <summary>
    /// Get a microservices architecture configuration.
    /// </summary>
    public static RuleConfiguration MicroservicesArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Microservices Architecture",
            Description = "Services are independent with minimal coupling and no circular dependencies",
            SliceRules = new List<SliceRuleConfig>
            {
                new SliceRuleConfig
                {
                    Id = "ms-1",
                    Name = "No service cycles",
                    Description = "Microservices should not have circular dependencies",
                    SlicePattern = "src/Services/{Slice}/**",
                    RuleType = "BeAcyclic",
                    Enabled = true,
                    Severity = "Error"
                }
            }
        };
    }

    /// <summary>
    /// Get a clean architecture configuration.
    /// </summary>
    public static RuleConfiguration CleanArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Clean Architecture",
            Description = "Enforces clean architecture layering: Entities -> UseCases -> Interfaces -> Frameworks",
            FileRules = new List<FileRuleConfig>
            {
                new FileRuleConfig
                {
                    Id = "clean-1",
                    Name = "Entities must be independent",
                    Description = "Enterprise business rules should not depend on any other layer",
                    SourcePath = "src/Core/Entities/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/**",
                    Enabled = true,
                    Severity = "Error"
                },
                new FileRuleConfig
                {
                    Id = "clean-2",
                    Name = "Frameworks must not depend on Core",
                    Description = "External frameworks should depend on core, not vice versa",
                    SourcePath = "src/Frameworks/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/Core/**",
                    Enabled = true,
                    Severity = "Warning"
                }
            }
        };
    }

    /// <summary>
    /// Get a domain-driven design (bounded contexts) configuration.
    /// </summary>
    public static RuleConfiguration DomainDrivenDesign()
    {
        return new RuleConfiguration
        {
            ProjectName = "Domain-Driven Design",
            Description = "Bounded contexts are isolated with clear boundaries and minimal coupling",
            SliceRules = new List<SliceRuleConfig>
            {
                new SliceRuleConfig
                {
                    Id = "ddd-1",
                    Name = "Bounded contexts must be acyclic",
                    Description = "Bounded contexts should form an acyclic dependency graph",
                    SlicePattern = "src/BoundedContexts/{Slice}/**",
                    RuleType = "BeAcyclic",
                    Enabled = true,
                    Severity = "Error"
                }
            }
        };
    }

    /// <summary>
    /// Get an event-driven architecture configuration.
    /// </summary>
    public static RuleConfiguration EventDrivenArchitecture()
    {
        return new RuleConfiguration
        {
            ProjectName = "Event-Driven Architecture",
            Description = "Services communicate through events with minimal direct coupling",
            FileRules = new List<FileRuleConfig>
            {
                new FileRuleConfig
                {
                    Id = "event-1",
                    Name = "Services must not have direct dependencies",
                    Description = "Services should communicate only through event bus",
                    SourcePath = "src/Services/**",
                    RuleType = "ShouldNot",
                    TargetPath = "src/Services/**",
                    Enabled = true,
                    Severity = "Warning"
                }
            }
        };
    }

    /// <summary>
    /// Get a minimal metrics configuration for code quality.
    /// </summary>
    public static RuleConfiguration CodeQualityMetrics()
    {
        return new RuleConfiguration
        {
            ProjectName = "Code Quality Metrics",
            Description = "Ensures basic code quality standards using metrics",
            MetricsRules = new List<MetricsRuleConfig>
            {
                new MetricsRuleConfig
                {
                    Id = "metrics-1",
                    Name = "Classes must maintain high cohesion",
                    Description = "LCOM96a should be less than 0.5 for good cohesion",
                    MetricType = "LCOM96a",
                    Threshold = 0.5,
                    Operator = "LessThan",
                    Enabled = true,
                    Severity = "Warning"
                },
                new MetricsRuleConfig
                {
                    Id = "metrics-2",
                    Name = "Classes should not have too many methods",
                    Description = "Classes with more than 20 methods may have too many responsibilities",
                    MetricType = "MethodCount",
                    Threshold = 20,
                    Operator = "LessThan",
                    Enabled = true,
                    Severity = "Warning"
                }
            }
        };
    }
}
