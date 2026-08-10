using ArchUnitNet.Files;
using ArchUnitNet.Slices;

namespace ArchUnitNet.Presets;

/// <summary>
/// Pre-configured rule templates for common architecture patterns.
/// Simplifies setup for typical scenarios.
/// </summary>
public static class ArchitecturePresets
{
    /// <summary>
    /// Layered architecture template: UI → Service → Data with no circular dependencies.
    /// </summary>
    public static LayeredArchitecturePreset LayeredArchitecture()
        => new();

    /// <summary>
    /// Hexagonal architecture template: Domain independent, adapters depend on ports.
    /// </summary>
    public static HexagonalArchitecturePreset HexagonalArchitecture()
        => new();

    /// <summary>
    /// Feature-based isolation template: Features are independent modules.
    /// </summary>
    public static FeatureIsolationPreset FeatureIsolation()
        => new();

    /// <summary>
    /// Public API boundary template: Barrel exports control public interface.
    /// </summary>
    public static PublicAPIPreset PublicAPI()
        => new();

    /// <summary>
    /// Microservices template: Services are independent with minimal coupling.
    /// </summary>
    public static MicroservicesPreset Microservices()
        => new();

    /// <summary>
    /// Clean architecture template: Entities → UseCases → Controllers.
    /// </summary>
    public static CleanArchitecturePreset CleanArchitecture()
        => new();
}

/// <summary>
/// Layered Architecture: Enforce strict layer separation (UI → Service → Data).
/// </summary>
public class LayeredArchitecturePreset
{
    private string _projectPath = null!;
    private string _uiPath = "src/UI/**";
    private string _servicePath = "src/Service/**";
    private string _dataPath = "src/Data/**";

    public LayeredArchitecturePreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public LayeredArchitecturePreset WithUIPath(string path)
    {
        _uiPath = path;
        return this;
    }

    public LayeredArchitecturePreset WithServicePath(string path)
    {
        _servicePath = path;
        return this;
    }

    public LayeredArchitecturePreset WithDataPath(string path)
    {
        _dataPath = path;
        return this;
    }

    /// <summary>
    /// Build all layered architecture rules.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // UI should not depend on Data
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_uiPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_dataPath),

            // Service should not depend on UI
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_servicePath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_uiPath),

            // No cycles allowed
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**")
                .Should()
                .HaveNoCycles(),

            // All layers should have no internal cycles
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_uiPath)
                .Should()
                .HaveNoCycles()
        };
    }

    /// <summary>
    /// Validate the layered architecture.
    /// </summary>
    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}

/// <summary>
/// Hexagonal Architecture: Domain is independent, adapters depend on ports.
/// </summary>
public class HexagonalArchitecturePreset
{
    private string _projectPath = null!;
    private string _domainPath = "src/Domain/**";
    private string _portsPath = "src/Ports/**";
    private string _adaptersPath = "src/Adapters/**";

    public HexagonalArchitecturePreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public HexagonalArchitecturePreset WithDomainPath(string path)
    {
        _domainPath = path;
        return this;
    }

    public HexagonalArchitecturePreset WithPortsPath(string path)
    {
        _portsPath = path;
        return this;
    }

    public HexagonalArchitecturePreset WithAdaptersPath(string path)
    {
        _adaptersPath = path;
        return this;
    }

    /// <summary>
    /// Build hexagonal architecture rules.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Domain should not depend on anything
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_domainPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_portsPath)
                .And()
                .InFolder(_adaptersPath),

            // Adapters should depend on ports
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_adaptersPath)
                .Should()
                .DependOnFiles()
                .InFolder(_portsPath),

            // No cycles allowed
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**")
                .Should()
                .HaveNoCycles()
        };
    }

    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}

/// <summary>
/// Feature Isolation: Each feature is independent, no cross-feature dependencies.
/// </summary>
public class FeatureIsolationPreset
{
    private string _projectPath = null!;
    private string _featuresPath = "src/Features/**";

    public FeatureIsolationPreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public FeatureIsolationPreset WithFeaturesPath(string path)
    {
        _featuresPath = path;
        return this;
    }

    /// <summary>
    /// Build feature isolation rules using slice-based validation.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Features are isolated
            ArchUnit.ProjectSlices()
                .DefinedBy(_featuresPath.Replace("**", "{Feature}/**"))
                .Should()
                .AdhereToDefinedSlices(),

            // No cycles
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_featuresPath)
                .Should()
                .HaveNoCycles()
        };
    }

    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}

/// <summary>
/// Public API Boundary: Barrel exports (index.cs) control what's public.
/// </summary>
public class PublicAPIPreset
{
    private string _projectPath = null!;
    private string _featuresPath = "src/Features/**";
    private string _internalMarker = "internal";

    public PublicAPIPreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public PublicAPIPreset WithFeaturesPath(string path)
    {
        _featuresPath = path;
        return this;
    }

    public PublicAPIPreset WithInternalMarker(string marker)
    {
        _internalMarker = marker;
        return this;
    }

    /// <summary>
    /// Build public API rules: External code can only use barrel exports.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Cannot import from internal folders
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_featuresPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder($"{_featuresPath.TrimEnd('*')}*/{_internalMarker}/**")
        };
    }

    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}

/// <summary>
/// Microservices: Each service is independent, communication via APIs.
/// </summary>
public class MicroservicesPreset
{
    private string _servicesPath = "services/**";

    public MicroservicesPreset WithServicesPath(string path)
    {
        _servicesPath = path;
        return this;
    }

    /// <summary>
    /// Build microservices rules: No direct service-to-service dependencies.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        return new Checkable[]
        {
            // Services are isolated
            ArchUnit.ProjectSlices()
                .DefinedBy(_servicesPath.Replace("**", "{Service}/**"))
                .Should()
                .AdhereToDefinedSlices()
        };
    }

    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}

/// <summary>
/// Clean Architecture: Entities → UseCases → Controllers → Frameworks.
/// </summary>
public class CleanArchitecturePreset
{
    private string _projectPath = null!;
    private string _entitiesPath = "src/Entities/**";
    private string _useCasesPath = "src/UseCases/**";
    private string _controllersPath = "src/Controllers/**";
    private string _frameworksPath = "src/Frameworks/**";

    public CleanArchitecturePreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    /// <summary>
    /// Build Clean Architecture rules.
    /// </summary>
    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Entities don't depend on anything
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_entitiesPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_useCasesPath),

            // UseCases depend on Entities only
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_useCasesPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_controllersPath),

            // Controllers depend on UseCases
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_controllersPath)
                .Should()
                .DependOnFiles()
                .InFolder(_useCasesPath),

            // No circular dependencies
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**")
                .Should()
                .HaveNoCycles()
        };
    }

    public async Task<List<Violation>> ValidateAsync()
    {
        var violations = new List<Violation>();
        foreach (var rule in BuildRules())
        {
            var result = await rule.CheckAsync();
            violations.AddRange(result);
        }
        return violations;
    }
}
