using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Extraction;

public class GraphTests
{
    [Fact]
    public void Constructor_CreatesEmptyGraph()
    {
        // Act
        var graph = new ArchUnitNet.Common.Extraction.Graph();

        // Assert
        Assert.Empty(graph.Edges);
    }

    [Fact]
    public void Constructor_InitializesWithEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
            new Edge("src/B.cs", "src/C.cs", false, new[] { ImportKind.Using }),
        };

        // Act
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);

        // Assert
        Assert.Equal(2, graph.Edges.Count);
    }

    [Fact]
    public void Add_AddsEdgeToGraph()
    {
        // Arrange
        var graph = new ArchUnitNet.Common.Extraction.Graph();
        var edge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act
        graph.Add(edge);

        // Assert
        Assert.Single(graph.Edges);
        Assert.Equal(edge, graph.Edges.First());
    }

    [Fact]
    public void GetNodes_ReturnsAllUniqueNodes()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
            new Edge("src/B.cs", "src/C.cs", false, new[] { ImportKind.Using }),
            new Edge("src/A.cs", "src/C.cs", false, new[] { ImportKind.Using }),
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);

        // Act
        var nodes = graph.GetNodes();

        // Assert
        Assert.Equal(3, nodes.Count);
        Assert.Contains("src/A.cs", nodes);
        Assert.Contains("src/B.cs", nodes);
        Assert.Contains("src/C.cs", nodes);
    }

    [Fact]
    public void Merge_CombinesGraphs()
    {
        // Arrange
        var graph1 = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
        });
        var graph2 = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("src/C.cs", "src/D.cs", false, new[] { ImportKind.Using }),
        });

        // Act
        graph1.Merge(graph2);

        // Assert
        Assert.Equal(2, graph1.Edges.Count);
    }

    [Fact]
    public void Where_FiltersEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
            new Edge("src/C.cs", "external", true, new[] { ImportKind.Using }),
            new Edge("src/D.cs", "src/E.cs", false, new[] { ImportKind.Using }),
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);

        // Act
        var filtered = graph.Where(e => !e.External);

        // Assert
        Assert.Equal(2, filtered.Edges.Count);
        foreach (var e in filtered.Edges)
        {
            Assert.False(e.External);
        }
    }

    [Fact]
    public void Validate_ThrowsOnInvalidEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge(null!, "src/B.cs", false, new[] { ImportKind.Using }),
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.Validate());
    }

    [Fact]
    public void EdgesAreReadOnly()
    {
        // Arrange
        var graph = new ArchUnitNet.Common.Extraction.Graph();

        // Act & Assert
        Assert.IsAssignableFrom<IReadOnlyList<Edge>>(graph.Edges);
        Assert.Throws<InvalidCastException>(() => ((List<Edge>)graph.Edges).Add(
            new Edge("a", "b", false, new[] { ImportKind.Using })));
    }
}
