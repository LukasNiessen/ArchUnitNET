using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Presets;
using Xunit;

namespace ArchUnitNet.Tests.Presets;

public class ArchitecturePresetsTests
{
    private const string TestProjectPath = "./tests/ArchUnitNet.Tests/ArchUnitNet.Tests.csproj";

    [Fact]
    public void LayeredArchitecture_WithProjectPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.LayeredArchitecture()
            .WithProjectPath(TestProjectPath);

        Assert.NotNull(preset);
    }

    [Fact]
    public void LayeredArchitecture_BuildRules_ReturnsMultipleRules()
    {
        var preset = ArchitecturePresets.LayeredArchitecture()
            .WithProjectPath(TestProjectPath);

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
        Assert.Equal(4, rules.Count);
    }

    [Fact]
    public async Task LayeredArchitecture_ValidateAsync_ReturnsViolations()
    {
        var preset = ArchitecturePresets.LayeredArchitecture()
            .WithProjectPath(TestProjectPath);

        var violations = await preset.ValidateAsync();

        Assert.IsType<List<Violation>>(violations);
    }

    [Fact]
    public void HexagonalArchitecture_WithProjectPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.HexagonalArchitecture()
            .WithProjectPath(TestProjectPath);

        Assert.NotNull(preset);
    }

    [Fact]
    public void HexagonalArchitecture_BuildRules_ReturnsMultipleRules()
    {
        var preset = ArchitecturePresets.HexagonalArchitecture()
            .WithProjectPath(TestProjectPath);

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
        Assert.Equal(3, rules.Count);
    }

    [Fact]
    public async Task HexagonalArchitecture_ValidateAsync_ReturnsViolations()
    {
        var preset = ArchitecturePresets.HexagonalArchitecture()
            .WithProjectPath(TestProjectPath);

        var violations = await preset.ValidateAsync();

        Assert.IsType<List<Violation>>(violations);
    }

    [Fact]
    public void FeatureIsolation_WithProjectPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.FeatureIsolation()
            .WithProjectPath(TestProjectPath);

        Assert.NotNull(preset);
    }

    [Fact]
    public void FeatureIsolation_BuildRules_ReturnsMultipleRules()
    {
        var preset = ArchitecturePresets.FeatureIsolation()
            .WithProjectPath(TestProjectPath);

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
    }

    [Fact]
    public async Task FeatureIsolation_ValidateAsync_ReturnsViolations()
    {
        var preset = ArchitecturePresets.FeatureIsolation()
            .WithProjectPath(TestProjectPath);

        var violations = await preset.ValidateAsync();

        Assert.IsType<List<Violation>>(violations);
    }

    [Fact]
    public void PublicAPI_WithProjectPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.PublicAPI()
            .WithProjectPath(TestProjectPath);

        Assert.NotNull(preset);
    }

    [Fact]
    public void PublicAPI_BuildRules_ReturnsRules()
    {
        var preset = ArchitecturePresets.PublicAPI()
            .WithProjectPath(TestProjectPath);

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
    }

    [Fact]
    public void Microservices_WithServicesPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.Microservices()
            .WithServicesPath("services/{Service}/**");

        Assert.NotNull(preset);
    }

    [Fact]
    public void Microservices_BuildRules_ReturnsRules()
    {
        var preset = ArchitecturePresets.Microservices()
            .WithServicesPath("services/{Service}/**");

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
    }

    [Fact]
    public void CleanArchitecture_WithProjectPath_ReturnsPreset()
    {
        var preset = ArchitecturePresets.CleanArchitecture()
            .WithProjectPath(TestProjectPath);

        Assert.NotNull(preset);
    }

    [Fact]
    public void CleanArchitecture_BuildRules_ReturnsMultipleRules()
    {
        var preset = ArchitecturePresets.CleanArchitecture()
            .WithProjectPath(TestProjectPath);

        var rules = preset.BuildRules().ToList();

        Assert.NotEmpty(rules);
        Assert.Equal(4, rules.Count);
    }

    [Fact]
    public async Task CleanArchitecture_ValidateAsync_ReturnsViolations()
    {
        var preset = ArchitecturePresets.CleanArchitecture()
            .WithProjectPath(TestProjectPath);

        var violations = await preset.ValidateAsync();

        Assert.IsType<List<Violation>>(violations);
    }

    [Fact]
    public void PresetFactory_Methods_ReturnCorrectTypes()
    {
        var layered = ArchitecturePresets.LayeredArchitecture();
        var hexagonal = ArchitecturePresets.HexagonalArchitecture();
        var features = ArchitecturePresets.FeatureIsolation();
        var publicApi = ArchitecturePresets.PublicAPI();
        var microservices = ArchitecturePresets.Microservices();
        var clean = ArchitecturePresets.CleanArchitecture();

        Assert.IsType<LayeredArchitecturePreset>(layered);
        Assert.IsType<HexagonalArchitecturePreset>(hexagonal);
        Assert.IsType<FeatureIsolationPreset>(features);
        Assert.IsType<PublicAPIPreset>(publicApi);
        Assert.IsType<MicroservicesPreset>(microservices);
        Assert.IsType<CleanArchitecturePreset>(clean);
    }
}
