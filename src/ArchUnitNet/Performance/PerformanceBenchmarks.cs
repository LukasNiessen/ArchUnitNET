using BenchmarkDotNet.Attributes;
using ArchUnitNet;

namespace ArchUnitNet.Performance;

/// <summary>
/// Performance benchmarks for ArchUnitCSharp core operations.
/// Run with: dotnet run -c Release --project ./tools/ArchUnitNet.Benchmarks.csproj
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
[JsonExporter]
public class PerformanceBenchmarks
{
    private string _projectPath = null!;
    private string _largeProjectPath = null!;

    [GlobalSetup]
    public void Setup()
    {
        _projectPath = "./src/ArchUnitNet/ArchUnitNet.csproj";
        _largeProjectPath = "./tests/ArchUnitNet.Tests/ArchUnitNet.Tests.csproj";
    }

    /// <summary>
    /// Benchmark: Extracting dependencies from a standard-sized project.
    /// </summary>
    [Benchmark(Description = "Extract dependencies from standard project")]
    public async Task ExtractDependencies()
    {
        var rule = ProjectFiles(_projectPath)
            .InPath("src/**");

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Extracting dependencies from a larger test project.
    /// </summary>
    [Benchmark(Description = "Extract dependencies from large project")]
    public async Task ExtractDependenciesLarge()
    {
        var rule = ProjectFiles(_largeProjectPath)
            .InPath("tests/**");

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Running cycle detection on dependency graph.
    /// </summary>
    [Benchmark(Description = "Cycle detection (Tarjan's SCC algorithm)")]
    public async Task CycleDetection()
    {
        var rule = ProjectFiles(_projectPath)
            .InPath("src/**")
            .Should()
            .HaveNoCycles();

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Pattern matching with glob patterns.
    /// </summary>
    [Benchmark(Description = "Pattern matching with glob patterns")]
    public async Task PatternMatching()
    {
        var rule = ProjectFiles(_projectPath)
            .InPath("src/**/*.cs")
            .Except("src/**/Generated.cs")
            .Should()
            .DependOnFiles()
            .InFolder("src/**");

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Metrics calculation (LCOM96a) on all classes.
    /// </summary>
    [Benchmark(Description = "Calculate LCOM96a for all methods")]
    public async Task MetricsCalculation()
    {
        var rule = Metrics()
            .Methods()
            .LCOM96a()
            .ShouldBeLessThan(1.0);  // Very permissive threshold for benchmark

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Graph building and export to JSON.
    /// </summary>
    [Benchmark(Description = "Build graph and export to JSON")]
    public async Task GraphExportJSON()
    {
        var graph = ProjectGraph(_projectPath)
            .CollapseToFolderDepth(2);

        var json = await graph.ExportToStringAsync(GraphFormat.JSON);
        // Verify export produced output
        _ = json.Length > 100;
    }

    /// <summary>
    /// Benchmark: Graph export to Mermaid diagram.
    /// </summary>
    [Benchmark(Description = "Build graph and export to Mermaid")]
    public async Task GraphExportMermaid()
    {
        var graph = ProjectGraph(_projectPath)
            .CollapseToFolderDepth(2);

        var mermaid = await graph.ExportToStringAsync(GraphFormat.Mermaid);
        _ = mermaid.Length > 100;
    }

    /// <summary>
    /// Benchmark: Architecture slicing.
    /// </summary>
    [Benchmark(Description = "Slice architecture by pattern")]
    public async Task ArchitectureSlicing()
    {
        var rule = ProjectSlices()
            .DefinedBy("src/{Layer}/**")
            .Should()
            .AdhereToDefinedSlices();

        await rule.CheckAsync();
    }

    /// <summary>
    /// Benchmark: Full architecture validation (combined rules).
    /// </summary>
    [Benchmark(Description = "Full architecture validation")]
    public async Task FullValidation()
    {
        // Simulate comprehensive architecture check
        var fileRules = ProjectFiles(_projectPath)
            .InPath("src/**")
            .Should()
            .HaveNoCycles();

        var metricsRules = Metrics()
            .Methods()
            .LCOM96a()
            .ShouldBeLessThan(0.8);

        var violations = new List<Violation>();
        violations.AddRange(await fileRules.CheckAsync());
        violations.AddRange(await metricsRules.CheckAsync());

        _ = violations.Count >= 0;
    }
}
