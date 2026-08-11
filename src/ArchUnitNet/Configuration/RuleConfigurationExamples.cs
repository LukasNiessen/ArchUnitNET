namespace ArchUnitNet.Configuration;

/// <summary>
/// Example rule configurations for common architecture patterns.
/// </summary>
public static class RuleConfigurationExamples
{
    /// <summary>
    /// Example: Layered architecture configuration (UI → Service → Data).
    /// </summary>
    public static string LayeredArchitectureExample => @"{
  ""projectPath"": ""./MyProject.csproj"",
  ""severity"": ""error"",
  ""rules"": [
    {
      ""id"": ""layer-ui-to-service"",
      ""type"": ""FileDependency"",
      ""description"": ""UI layer can depend on Service layer"",
      ""source"": {
        ""path"": ""src/UI/**"",
        ""type"": ""glob""
      },
      ""target"": {
        ""path"": ""src/Service/**"",
        ""type"": ""glob""
      },
      ""action"": ""require"",
      ""severity"": ""error"",
      ""tags"": [""layering"", ""critical""]
    },
    {
      ""id"": ""layer-ui-not-to-data"",
      ""type"": ""FileDependency"",
      ""description"": ""UI layer should NOT depend on Data layer"",
      ""source"": {
        ""path"": ""src/UI/**"",
        ""type"": ""glob""
      },
      ""target"": {
        ""path"": ""src/Data/**"",
        ""type"": ""glob""
      },
      ""action"": ""forbid"",
      ""severity"": ""error"",
      ""tags"": [""layering"", ""critical""]
    },
    {
      ""id"": ""no-cycles"",
      ""type"": ""NoCycles"",
      ""description"": ""No circular dependencies allowed"",
      ""source"": {
        ""path"": ""src/**"",
        ""type"": ""glob""
      },
      ""action"": ""acyclic"",
      ""severity"": ""error"",
      ""tags"": [""critical""]
    }
  ]
}";

    /// <summary>
    /// Example: Feature isolation configuration (independent features).
    /// </summary>
    public static string FeatureIsolationExample => @"{
  ""projectPath"": ""./MyProject.csproj"",
  ""severity"": ""warning"",
  ""rules"": [
    {
      ""id"": ""feature-isolation"",
      ""type"": ""FileDependency"",
      ""description"": ""Features should be isolated - no cross-feature dependencies"",
      ""source"": {
        ""path"": ""src/Features/{Feature}/**"",
        ""type"": ""glob"",
        ""exclude"": ""src/Features/Shared/**""
      },
      ""target"": {
        ""path"": ""src/Features/**"",
        ""type"": ""glob""
      },
      ""action"": ""forbid"",
      ""severity"": ""warning"",
      ""tags"": [""features""]
    },
    {
      ""id"": ""shared-dependencies-allowed"",
      ""type"": ""FileDependency"",
      ""description"": ""All features can depend on Shared"",
      ""source"": {
        ""path"": ""src/Features/{Feature}/**"",
        ""type"": ""glob""
      },
      ""target"": {
        ""path"": ""src/Features/Shared/**"",
        ""type"": ""glob""
      },
      ""action"": ""require"",
      ""severity"": ""info"",
      ""tags"": [""features""]
    }
  ]
}";

    /// <summary>
    /// Example: Public API boundary configuration (barrel exports).
    /// </summary>
    public static string PublicAPIExample => @"{
  ""projectPath"": ""./MyProject.csproj"",
  ""rules"": [
    {
      ""id"": ""public-api-only"",
      ""type"": ""FileDependency"",
      ""description"": ""External dependencies should only use public API (index.cs)"",
      ""source"": {
        ""path"": ""src/**"",
        ""type"": ""glob"",
        ""exclude"": ""src/Customer/**""
      },
      ""target"": {
        ""path"": ""src/Customer/internal/**"",
        ""type"": ""glob""
      },
      ""action"": ""forbid"",
      ""severity"": ""error"",
      ""tags"": [""api-boundary"", ""critical""]
    }
  ]
}";

    /// <summary>
    /// Example: Microservices configuration (independent services).
    /// </summary>
    public static string MicroservicesExample => @"{
  ""projectPath"": ""./MyProject.csproj"",
  ""rules"": [
    {
      ""id"": ""service-independence"",
      ""type"": ""FileDependency"",
      ""description"": ""Microservices should be independent"",
      ""source"": {
        ""path"": ""src/Services/{Service}/**"",
        ""type"": ""glob""
      },
      ""target"": {
        ""path"": ""src/Services/**"",
        ""type"": ""glob""
      },
      ""action"": ""forbid"",
      ""severity"": ""error"",
      ""tags"": [""microservices""]
    },
    {
      ""id"": ""shared-kernel-allowed"",
      ""type"": ""FileDependency"",
      ""description"": ""Services can depend on SharedKernel"",
      ""source"": {
        ""path"": ""src/Services/{Service}/**"",
        ""type"": ""glob""
      },
      ""target"": {
        ""path"": ""src/SharedKernel/**"",
        ""type"": ""glob""
      },
      ""action"": ""require"",
      ""severity"": ""info"",
      ""tags"": [""microservices""]
    }
  ]
}";

    /// <summary>
    /// Get example configuration by name.
    /// </summary>
    public static string? GetExample(string name)
    {
        return name.ToLowerInvariant() switch
        {
            "layered" or "layered-architecture" => LayeredArchitectureExample,
            "features" or "feature-isolation" => FeatureIsolationExample,
            "api" or "public-api" => PublicAPIExample,
            "microservices" or "services" => MicroservicesExample,
            _ => null,
        };
    }
}
