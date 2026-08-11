using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.FluentApi;
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

    /// <summary>
    /// Modular monolith template: Independent modules with clear boundaries.
    /// </summary>
    public static ModularMonolithPreset ModularMonolith()
        => new();

    /// <summary>
    /// Domain-driven design template: Bounded contexts with domain-focused isolation.
    /// </summary>
    public static DomainDrivenDesignPreset DomainDrivenDesign()
        => new();

    /// <summary>
    /// Event-driven architecture template: Event bus decoupling with minimal direct dependencies.
    /// </summary>
    public static EventDrivenArchitecturePreset EventDrivenArchitecture()
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
            // Domain should not depend on ports
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_domainPath)
                .ShouldNot()
                .DependOnFiles()
                .InFolder(_portsPath),

            // Domain should not depend on adapters
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_domainPath)
                .ShouldNot()
                .DependOnFiles()
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

/// <summary>
/// Modular Monolith: Independent modules with clear boundaries and minimal cross-module dependencies.
/// </summary>
public class ModularMonolithPreset
{
    private string _projectPath = null!;
    private string _modulesPattern = "src/Modules/{Module}/**";

    public ModularMonolithPreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public ModularMonolithPreset WithModulesPattern(string modulesPattern)
    {
        _modulesPattern = modulesPattern;
        return this;
    }

    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Modules should not have circular dependencies
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_modulesPattern)
                .Should()
                .HaveNoCycles(),

            // Cross-module dependencies should only go through public APIs
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/Modules/**/public/**")
                .Should()
                .DependOnFiles()
                .InPath("src/Modules/**/public/**"),
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
/// Domain-Driven Design: Bounded contexts with domain logic isolation and minimal coupling.
/// </summary>
public class DomainDrivenDesignPreset
{
    private string _projectPath = null!;
    private string _boundedContextsPattern = "src/{BoundedContext}/**";

    public DomainDrivenDesignPreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public DomainDrivenDesignPreset WithBoundedContextsPattern(string boundedContextsPattern)
    {
        _boundedContextsPattern = boundedContextsPattern;
        return this;
    }

    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Domain models should only depend on other domain models
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**/Domain/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/**/Application/**"),

            // Application services can depend on domain but not on presentation
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**/Application/**")
                .ShouldNot()
                .DependOnFiles()
                .InPath("src/**/Presentation/**"),

            // Bounded contexts should not have circular dependencies
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_boundedContextsPattern)
                .Should()
                .HaveNoCycles(),
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
/// Event-Driven Architecture: Components communicate via events with minimal direct coupling.
/// </summary>
public class EventDrivenArchitecturePreset
{
    private string _projectPath = null!;
    private string _eventBusPath = "src/EventBus/**";
    private string _handlersPath = "src/Handlers/**";
    private string _publishersPath = "src/Publishers/**";

    public EventDrivenArchitecturePreset WithProjectPath(string projectPath)
    {
        _projectPath = projectPath;
        return this;
    }

    public EventDrivenArchitecturePreset WithEventBusPath(string eventBusPath)
    {
        _eventBusPath = eventBusPath;
        return this;
    }

    public EventDrivenArchitecturePreset WithHandlersPath(string handlersPath)
    {
        _handlersPath = handlersPath;
        return this;
    }

    public EventDrivenArchitecturePreset WithPublishersPath(string publishersPath)
    {
        _publishersPath = publishersPath;
        return this;
    }

    public IEnumerable<Checkable> BuildRules()
    {
        if (string.IsNullOrEmpty(_projectPath))
            throw new InvalidOperationException("Project path must be set via WithProjectPath()");

        return new Checkable[]
        {
            // Publishers should depend on EventBus but not on Handlers
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_publishersPath)
                .Should()
                .DependOnFiles()
                .InPath(_eventBusPath),

            // Handlers should depend on EventBus but not directly on Publishers
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_handlersPath)
                .Should()
                .DependOnFiles()
                .InPath(_eventBusPath),

            // Publishers and Handlers should not depend on each other
            ArchUnit.ProjectFiles(_projectPath)
                .InPath(_publishersPath)
                .ShouldNot()
                .DependOnFiles()
                .InPath(_handlersPath),

            // No circular dependencies in event flow
            ArchUnit.ProjectFiles(_projectPath)
                .InPath("src/**")
                .Should()
                .HaveNoCycles(),
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
