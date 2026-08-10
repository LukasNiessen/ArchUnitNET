using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.GraphReporting;
using Xunit;

namespace ArchUnitNet.Tests.Graph;

public class GraphReporterTests
{
    private readonly List<Edge> _sampleEdges = new()
    {
        new Edge("src/Feature1/Component.cs", "src/Feature1/Service.cs", External: false, new[] { ImportKind.Using }),
        new Edge("src/Feature1/Component.cs", "System.String", External: true, new[] { ImportKind.Using }),
        new Edge("src/Feature2/Service.cs", "src/Model/Entity.cs", External: false, new[] { ImportKind.Using })
    };

    [Fact]
    public void Constructor_WithValidEdges_Succeeds()
    {
        // Act
        var reporter = new GraphReporter(_sampleEdges);

        // Assert
        Assert.NotNull(reporter);
    }

    [Fact]
    public void Constructor_WithNullEdges_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GraphReporter(null!));
    }

    [Fact]
    public async Task ExportToMermaidAsync_WithValidEdges_ReturnsValidMermaidDiagram()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToMermaidAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("graph TD", result);
        Assert.Contains("-->", result);
    }

    [Fact]
    public async Task ExportToDOTAsync_WithValidEdges_ReturnsValidDOT()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToDOTAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("digraph Dependencies", result);
        Assert.Contains("->", result);
        Assert.Contains("}", result);
    }

    [Fact]
    public async Task ExportToD2Async_WithValidEdges_ReturnsValidD2()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToD2Async();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("->", result);
    }

    [Fact]
    public async Task ExportToCSVAsync_WithValidEdges_ReturnsValidCSV()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToCSVAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("Source,Target,External,ImportKinds", result);
        Assert.Contains("src/Feature1/Component.cs", result);
    }

    [Fact]
    public async Task ExportToJSONAsync_WithValidEdges_ReturnsValidJSON()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToJSONAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("\"nodes\"", result);
        Assert.Contains("\"edges\"", result);
        Assert.Contains("{", result);
        Assert.Contains("}", result);
    }

    [Fact]
    public async Task ExportToHTMLAsync_WithValidEdges_ReturnsValidHTML()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportToHTMLAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("<!DOCTYPE html>", result);
        Assert.Contains("<html>", result);
        Assert.Contains("</html>", result);
        Assert.Contains("Dependency Graph", result);
    }

    [Fact]
    public void IncludeExternalDependencies_ReturnsSelf()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = reporter.IncludeExternalDependencies();

        // Assert
        Assert.Same(reporter, result);
    }

    [Fact]
    public void CollapseToFolderDepth_WithValidDepth_ReturnsSelf()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = reporter.CollapseToFolderDepth(2);

        // Assert
        Assert.Same(reporter, result);
    }

    [Fact]
    public void CollapseToFolderDepth_WithInvalidDepth_ThrowsArgumentException()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => reporter.CollapseToFolderDepth(0));
    }

    [Fact]
    public void FocusOn_WithValidPath_ReturnsSelf()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = reporter.FocusOn("Feature1");

        // Assert
        Assert.Same(reporter, result);
    }

    [Fact]
    public void FocusOn_WithNullPath_ThrowsArgumentException()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => reporter.FocusOn(null!));
    }

    [Fact]
    public async Task ExportAsync_WithMermaidFormat_CallsMermaidExport()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportAsync(GraphExportFormat.Mermaid);

        // Assert
        Assert.Contains("graph TD", result);
    }

    [Fact]
    public async Task ExportAsync_WithDOTFormat_CallsDOTExport()
    {
        // Arrange
        var reporter = new GraphReporter(_sampleEdges);

        // Act
        var result = await reporter.ExportAsync(GraphExportFormat.DOT);

        // Assert
        Assert.Contains("digraph", result);
    }

    [Fact]
    public async Task IncludeExternalDependencies_ExcludesExternalEdges()
    {
        // Arrange
        var edges = new List<Edge>
        {
            new Edge("src/A.cs", "src/B.cs", External: false, new[] { ImportKind.Using }),
            new Edge("src/A.cs", "System.String", External: true, new[] { ImportKind.Using })
        };
        var reporter = new GraphReporter(edges);

        // Act
        var resultWithoutExternal = await reporter.ExportToCSVAsync();
        var resultWithExternal = await reporter.IncludeExternalDependencies().ExportToCSVAsync();

        // Assert
        Assert.Single(resultWithoutExternal.Split(Environment.NewLine).Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("Source")));
        Assert.Equal(2, resultWithExternal.Split(Environment.NewLine).Count(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith("Source")));
    }
}

public class ProjectGraphBuilderTests
{
    [Fact]
    public void Constructor_CreatesEmptyGraph()
    {
        // Act
        var builder = ArchUnit.ProjectGraph();

        // Assert
        Assert.NotNull(builder);
        Assert.Equal(0, builder.GetEdgeCount());
    }

    [Fact]
    public void AddEdge_WithValidEdge_Succeeds()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        var edge = new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using });

        // Act
        builder.AddEdge(edge);

        // Assert
        Assert.Equal(1, builder.GetEdgeCount());
    }

    [Fact]
    public void AddEdge_WithNullEdge_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddEdge(null!));
    }

    [Fact]
    public void AddEdges_WithValidEdges_Succeeds()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        var edges = new[]
        {
            new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using }),
            new Edge("B.cs", "C.cs", External: false, new[] { ImportKind.Using })
        };

        // Act
        builder.AddEdges(edges);

        // Assert
        Assert.Equal(2, builder.GetEdgeCount());
    }

    [Fact]
    public void AddEdges_WithNullEdges_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddEdges(null!));
    }

    [Fact]
    public void IncludeExternalDependencies_ReturnsSelf()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();

        // Act
        var result = builder.IncludeExternalDependencies();

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void CollapseToFolderDepth_WithValidDepth_ReturnsSelf()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();

        // Act
        var result = builder.CollapseToFolderDepth(2);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void FocusOn_WithValidPath_ReturnsSelf()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();

        // Act
        var result = builder.FocusOn("Feature1");

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public async Task ExportToMermaidAsync_WithValidEdges_ReturnsValidDiagram()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        builder.AddEdge(new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using }));

        // Act
        var result = await builder.ExportToMermaidAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("graph TD", result);
    }

    [Fact]
    public async Task ExportToFileAsync_WithValidPath_CreatesFile()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        builder.AddEdge(new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using }));
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            await builder.ExportToFileAsync(GraphExportFormat.CSV, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            var content = await File.ReadAllTextAsync(tempFile);
            Assert.NotEmpty(content);
            Assert.Contains("Source,Target", content);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetNodeCount_WithSingleEdge_ReturnsTwo()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        builder.AddEdge(new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using }));

        // Act
        var count = builder.GetNodeCount();

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void GetNodeCount_WithDuplicateNodes_CountsUnique()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        builder.AddEdge(new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using }));
        builder.AddEdge(new Edge("A.cs", "C.cs", External: false, new[] { ImportKind.Using }));

        // Act
        var count = builder.GetNodeCount();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void GetEdges_ReturnsAllEdges()
    {
        // Arrange
        var builder = ArchUnit.ProjectGraph();
        var edge1 = new Edge("A.cs", "B.cs", External: false, new[] { ImportKind.Using });
        var edge2 = new Edge("B.cs", "C.cs", External: false, new[] { ImportKind.Using });
        builder.AddEdge(edge1);
        builder.AddEdge(edge2);

        // Act
        var edges = builder.GetEdges();

        // Assert
        Assert.Equal(2, edges.Count);
    }
}
