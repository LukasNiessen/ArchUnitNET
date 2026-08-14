using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Util;
using Xunit;

namespace ArchUnitNet.Tests.Common.Projection;

public class ProjectionExtensionsTests
{
    [Fact]
    public void ProjectEdges_AppliesMapFunction()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectEdges(MapFunctions.Identity);

        // Assert
        Assert.NotEmpty(projected.Edges);
        Assert.Single(projected.Edges);
    }

    [Fact]
    public void ProjectEdges_FiltersOutNullResults()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "A", false, new[] { ImportKind.Using }), // Self-edge
            new Edge("C", "D", false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectEdges(MapFunctions.PerEdge);

        // Assert
        Assert.Equal(2, projected.EdgeCount); // Self-edge filtered out
    }

    [Fact]
    public void ProjectIdentity_PassesAllEdgesThrough()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectIdentity();

        // Assert
        Assert.Equal(2, projected.EdgeCount);
    }

    [Fact]
    public void ProjectPerEdge_FiltersSelfEdges()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "A", false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectPerEdge();

        // Assert
        Assert.Single(projected.Edges);
    }

    [Fact]
    public void ProjectInternalOnly_FiltersExternalDependencies()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "System", true, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectInternalOnly();

        // Assert
        Assert.Single(projected.Edges);
        Assert.False(projected.Edges.First().External);
    }

    [Fact]
    public void ProjectExternalOnly_KeepsOnlyExternalDependencies()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "System", true, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var projected = graph.ProjectExternalOnly();

        // Assert
        Assert.Single(projected.Edges);
        Assert.True(projected.Edges.First().External);
    }

    [Fact]
    public void ProjectEdges_WithNullMapFunctionThrows()
    {
        // Arrange
        var graph = new Graph(new[] { new Edge("A", "B", false, new[] { ImportKind.Using }) });

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.ProjectEdges(null!));
    }

    [Fact]
    public void ProjectEdges_WithNullGraphThrows()
    {
        // Act & Assert
        Graph nullGraph = null!;
        Assert.Throws<ArgumentNullException>(() => nullGraph.ProjectEdges(MapFunctions.Identity));
    }

    [Fact]
    public void ProjectEdges_WithCustomMapFunction()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Custom map function: convert file paths to folder names
        ProjectedEdge? CustomMapper(Edge edge)
        {
            var sourceFolder = edge.Source.Split('/')[0];
            var targetFolder = edge.Target.Split('/')[0];
            return ProjectedEdge.FromRawEdges(sourceFolder, targetFolder, edge);
        }

        // Act
        var projected = graph.ProjectEdges(CustomMapper);

        // Assert
        Assert.Single(projected.Edges);
        Assert.Equal("src", projected.Edges.First().Source);
    }

    [Fact]
    public void ProjectEdges_PreservesRawEdges()
    {
        // Arrange
        var edge = new Edge("A", "B", false, new[] { ImportKind.Using });
        var graph = new Graph(new[] { edge });

        // Act
        var projected = graph.ProjectEdges(MapFunctions.Identity);

        // Assert
        Assert.Single(projected.Edges);
        Assert.Single(projected.Edges.First().RawEdges);
        Assert.Equal(edge, projected.Edges.First().RawEdges[0]);
    }

    [Fact]
    public void ProjectInternalOnly_ChainedWithPerEdge()
    {
        // Arrange
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "A", false, new[] { ImportKind.Using }),
            new Edge("A", "System", true, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);

        // Act
        var combined = MapFunctions.Compose(
            MapFunctions.PerEdge,
            MapFunctions.PerInternalEdge
        );
        var projected = graph.ProjectEdges(combined);

        // Assert
        Assert.Single(projected.Edges); // Only internal non-self edges
    }
}
