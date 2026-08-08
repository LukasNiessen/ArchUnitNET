using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Extraction;

public class GraphTests
{
    [Fact]
    public void Constructor_CreatesEmptyGraph()
    {
        // Act
        var graph = new Graph();

        // Assert
        graph.Edges.Should().BeEmpty();
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
        var graph = new Graph(edges);

        // Assert
        graph.Edges.Should().HaveCount(2);
    }

    [Fact]
    public void Add_AddsEdgeToGraph()
    {
        // Arrange
        var graph = new Graph();
        var edge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act
        graph.Add(edge);

        // Assert
        graph.Edges.Should().HaveCount(1);
        graph.Edges.First().Should().Be(edge);
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
        var graph = new Graph(edges);

        // Act
        var nodes = graph.GetNodes();

        // Assert
        nodes.Should().HaveCount(3);
        nodes.Should().Contain(new[] { "src/A.cs", "src/B.cs", "src/C.cs" });
    }

    [Fact]
    public void Merge_CombinesGraphs()
    {
        // Arrange
        var graph1 = new Graph(new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
        });
        var graph2 = new Graph(new[]
        {
            new Edge("src/C.cs", "src/D.cs", false, new[] { ImportKind.Using }),
        });

        // Act
        graph1.Merge(graph2);

        // Assert
        graph1.Edges.Should().HaveCount(2);
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
        var graph = new Graph(edges);

        // Act
        var filtered = graph.Where(e => !e.External);

        // Assert
        filtered.Edges.Should().HaveCount(2);
        filtered.Edges.Should().AllSatisfy(e => e.External.Should().BeFalse());
    }

    [Fact]
    public void Validate_ThrowsOnInvalidEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge(null!, "src/B.cs", false, new[] { ImportKind.Using }),
        };
        var graph = new Graph(edges);

        // Act & Assert
        var action = () => graph.Validate();
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EdgesAreReadOnly()
    {
        // Arrange
        var graph = new Graph();

        // Act & Assert
        graph.Edges.Should().BeAssignableTo<IReadOnlyList<Edge>>();
        ((Action)(() => ((List<Edge>)graph.Edges).Add(
            new Edge("a", "b", false, new[] { ImportKind.Using }))))
            .Should().Throw<InvalidCastException>();
    }
}
