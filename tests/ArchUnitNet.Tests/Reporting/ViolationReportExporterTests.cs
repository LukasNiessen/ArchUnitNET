using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.Assertion;
using ArchUnitNet.Reporting;
using Xunit;

namespace ArchUnitNet.Tests.Reporting;

public class TestViolation : Violation
{
    private readonly string _message;

    public TestViolation(string message)
    {
        _message = message;
    }

    public override string ToString() => _message;
}

public class ViolationReportExporterTests : IDisposable
{
    private readonly string _testOutputDir;
    private readonly List<Violation> _testViolations;

    public ViolationReportExporterTests()
    {
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"archunit-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDir);

        // Create test violations
        _testViolations = new List<Violation>
        {
            new TestViolation("File A depends on File B unexpectedly"),
            new TestViolation("Circular dependency: A → B → C → A"),
            new ViolatingFileDependency("A.cs", "B.cs", ImportKind.Using, "Failed to analyze project: Missing metadata")
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDir))
        {
            Directory.Delete(_testOutputDir, recursive: true);
        }
    }

    [Fact]
    public async Task ExportToHTMLAsync_CreatesValidHTMLFile()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.html");

        await exporter.ExportToHTMLAsync(outputPath);

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("<!DOCTYPE html>", content);
        Assert.Contains("TestProject", content);
        Assert.Contains("Total Violations", content);
    }

    [Fact]
    public async Task ExportToHTMLAsync_IncludesViolationDetails()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.html");

        await exporter.ExportToHTMLAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("File A depends on File B", content);
        Assert.Contains("Circular dependency", content);
        Assert.Contains("3", content); // Total violations
    }

    [Fact]
    public async Task ExportToSARIFAsync_CreatesValidSARIFFile()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.sarif");

        await exporter.ExportToSARIFAsync(outputPath);

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"version\": \"2.1.0\"", content);
        Assert.Contains("\"tool\"", content);
        Assert.Contains("\"results\"", content);
    }

    [Fact]
    public async Task ExportToSARIFAsync_IncludesProjectName()
    {
        var exporter = new ViolationReportExporter(_testViolations, "MyProject");
        var outputPath = Path.Combine(_testOutputDir, "report.sarif");

        await exporter.ExportToSARIFAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("ArchUnitCSharp", content);
        Assert.Contains("2.4.0", content);
    }

    [Fact]
    public async Task ExportToJSONAsync_CreatesValidJSON()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.json");

        await exporter.ExportToJSONAsync(outputPath);

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"project\"", content);
        Assert.Contains("\"violations\"", content);
        Assert.Contains("TestProject", content);
    }

    [Fact]
    public async Task ExportToJSONAsync_IncludesSummary()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.json");

        await exporter.ExportToJSONAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"summary\"", content);
        Assert.Contains("\"total\": 3", content);
        Assert.Contains("\"errors\": 1", content);
        Assert.Contains("\"warnings\": 2", content);
    }

    [Fact]
    public async Task ExportToTextAsync_CreatesReadableTextReport()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.txt");

        await exporter.ExportToTextAsync(outputPath);

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("Architecture Violations Report", content);
        Assert.Contains("Total Violations: 3", content);
        Assert.Contains("Errors:", content);
    }

    [Fact]
    public async Task ExportToTextAsync_IncludesDetailedViolations()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report.txt");

        await exporter.ExportToTextAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("File A depends on File B", content);
        Assert.Contains("Circular dependency", content);
    }

    [Fact]
    public async Task ExportToHTMLAsync_WithNoViolations_ShowsSuccessMessage()
    {
        var noViolations = new List<Violation>();
        var exporter = new ViolationReportExporter(noViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report_clean.html");

        await exporter.ExportToHTMLAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("No violations found", content);
        Assert.Contains("✓", content);
    }

    [Fact]
    public async Task ExportToJSONAsync_WithNoViolations_ShowsZeroCount()
    {
        var noViolations = new List<Violation>();
        var exporter = new ViolationReportExporter(noViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report_clean.json");

        await exporter.ExportToJSONAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"total\": 0", content);
        Assert.Contains("\"violations\": []", content);
    }

    [Fact]
    public async Task ViolationReportingExtensions_ExportReportAsync_WorksWithHTML()
    {
        var outputPath = Path.Combine(_testOutputDir, "report_ext.html");

        await _testViolations.ExportReportAsync(ReportFormat.HTML, outputPath, "TestProject");

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("<!DOCTYPE html>", content);
    }

    [Fact]
    public async Task ViolationReportingExtensions_ExportReportAsync_WorksWithSARIF()
    {
        var outputPath = Path.Combine(_testOutputDir, "report_ext.sarif");

        await _testViolations.ExportReportAsync(ReportFormat.SARIF, outputPath, "TestProject");

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"version\": \"2.1.0\"", content);
    }

    [Fact]
    public async Task ViolationReportingExtensions_ExportReportAsync_WorksWithJSON()
    {
        var outputPath = Path.Combine(_testOutputDir, "report_ext.json");

        await _testViolations.ExportReportAsync(ReportFormat.JSON, outputPath, "TestProject");

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("\"violations\"", content);
    }

    [Fact]
    public async Task ViolationReportingExtensions_ExportReportAsync_WorksWithText()
    {
        var outputPath = Path.Combine(_testOutputDir, "report_ext.txt");

        await _testViolations.ExportReportAsync(ReportFormat.Text, outputPath, "TestProject");

        Assert.True(File.Exists(outputPath));
        var content = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("Architecture Violations Report", content);
    }

    [Fact]
    public async Task ExportToSARIFAsync_SetsCorrectSeverityLevels()
    {
        var exporter = new ViolationReportExporter(_testViolations, "TestProject");
        var outputPath = Path.Combine(_testOutputDir, "report_severity.sarif");

        await exporter.ExportToSARIFAsync(outputPath);

        var content = await File.ReadAllTextAsync(outputPath);
        // Should have both error and warning levels
        Assert.Contains("\"error\"", content);
        Assert.Contains("\"warning\"", content);
    }

    [Fact]
    public async Task ViolationReportingExtensions_ExportReportAsync_ThrowsOnInvalidFormat()
    {
        var outputPath = Path.Combine(_testOutputDir, "report_invalid");

        // Should throw with invalid format value
        await Assert.ThrowsAsync<ArgumentException>(
            () => _testViolations.ExportReportAsync((ReportFormat)999, outputPath)
        );
    }
}
