using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Util;
using Xunit;

namespace ArchUnitNet.Tests.Common.Projection;

public class ProjectedGraphTests
{
    [Fact]
    public void Constructor_WithEdges_CreatesValidGraph()
    {
        // Arrange
        var edge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var projectedEdge = ProjectedEdge.FromRawEdge(edge);

        // Act
        var graph = new ProjectedGraph(new[] { projectedEdge });

        // Assert
        Assert.NotNull(graph);
        Assert.Single(graph.Edges);
    }

    [Fact]
    public void Constructor_WithEmptyEdges_CreatesEmptyGraph()
    {
        // Act
        var graph = new ProjectedGraph(Enumerable.Empty<ProjectedEdge>());

        // Assert
        Assert.Empty(graph.Edges);
    }

    [Fact]
    public void ExtractAllNodes_ReturnsUniqueNodes()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("src", "models", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("models", "services", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var nodes = graph.ExtractAllNodes();

        // Assert
        Assert.Equal(3, nodes.Count);
        Assert.Contains("src", nodes);
        Assert.Contains("models", nodes);
        Assert.Contains("services", nodes);
    }

    [Fact]
    public void ExtractDependencies_ReturnsTargetNodes()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("src", "models", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("src", "services", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var deps = graph.ExtractDependencies("src");

        // Assert
        Assert.Equal(2, deps.Count);
        Assert.Contains("models", deps);
        Assert.Contains("services", deps);
    }

    [Fact]
    public void ExtractDependents_ReturnsSourceNodes()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("ui", "models", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("services", "models", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var dependents = graph.ExtractDependents("models");

        // Assert
        Assert.Equal(2, dependents.Count);
        Assert.Contains("ui", dependents);
        Assert.Contains("services", dependents);
    }

    [Fact]
    public void ExtractOutDegree_CountsOutgoingEdges()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("a", "b", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("a", "c", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var outDegree = graph.ExtractOutDegree("a");

        // Assert
        Assert.Equal(2, outDegree);
    }

    [Fact]
    public void ExtractInDegree_CountsIncomingEdges()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("a", "c", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("b", "c", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var inDegree = graph.ExtractInDegree("c");

        // Assert
        Assert.Equal(2, inDegree);
    }

    [Fact]
    public void IsIsolated_ReturnsTrueForUnconnectedNode()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("a", "b", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var isolated = graph.IsIsolated("c");

        // Assert
        Assert.True(isolated);
    }

    [Fact]
    public void IsIsolated_ReturnsFalseForConnectedNode()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("a", "b", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var isolated = graph.IsIsolated("a");

        // Assert
        Assert.False(isolated);
    }

    [Fact]
    public void FindAllCycles_WithSimpleCycle_ReturnsCycle()
    {
        // Arrange: A → B → C → A
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("C", "A", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var cycles = graph.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
    }

    [Fact]
    public void FindAllCycles_WithNoCycles_ReturnsEmpty()
    {
        // Arrange: Linear: A → B → C
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var cycles = graph.FindAllCycles();

        // Assert
        Assert.Empty(cycles);
    }

    [Fact]
    public void FindAllCycles_WithSelfLoop_ReturnsCycle()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "A", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var cycles = graph.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
    }

    [Fact]
    public void FindStronglyConnectedComponents_WithNoCycles()
    {
        // Arrange: Linear: A → B → C
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var sccs = graph.FindStronglyConnectedComponents();

        // Assert
        Assert.Equal(3, sccs.Count); // Each node is its own SCC
    }

    [Fact]
    public void FindStronglyConnectedComponents_WithCycle()
    {
        // Arrange: A → B → C → A
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("C", "A", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var sccs = graph.FindStronglyConnectedComponents();

        // Assert
        Assert.Single(sccs.Where(s => s.Count > 1)); // One SCC with size > 1
    }

    [Fact]
    public void FindCyclicSCCs_OnlyReturnsCyclicComponents()
    {
        // Arrange: A → B → C → A (cycle), D → E (no cycle)
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("C", "A", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("D", "E", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var cyclicSccs = graph.FindCyclicSCCs();

        // Assert
        Assert.Single(cyclicSccs);
    }

    [Fact]
    public void HasCycles_ReturnsTrueWhenCyclesExist()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "A", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var hasCycles = graph.HasCycles();

        // Assert
        Assert.True(hasCycles);
    }

    [Fact]
    public void HasCycles_ReturnsFalseWhenNoCycles()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var hasCycles = graph.HasCycles();

        // Assert
        Assert.False(hasCycles);
    }

    [Fact]
    public void EdgeCount_ReturnsCorrectCount()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var count = graph.EdgeCount;

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void NodeCount_ReturnsCorrectCount()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "C", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var count = graph.NodeCount;

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void PreservesRawEdgesForTracing()
    {
        // Arrange: Create projected edges from multiple raw edges
        var rawEdge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var rawEdge2 = new Edge("src/A2.cs", "src/B.cs", false, new[] { ImportKind.StaticUsing });
        var projectedEdge = ProjectedEdge.FromRawEdges("src", "models", rawEdge1, rawEdge2);

        var graph = new ProjectedGraph(new[] { projectedEdge });

        // Act
        var edges = graph.Edges;

        // Assert
        Assert.Single(edges);
        Assert.Equal(2, edges.First().RawEdges.Length);
        Assert.Contains(rawEdge1, edges.First().RawEdges);
        Assert.Contains(rawEdge2, edges.First().RawEdges);
    }

    [Fact]
    public void FindAllCycles_WithMultipleSeparateCycles()
    {
        // Arrange: Two separate cycles
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("B", "A", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("C", "D", false, new[] { ImportKind.Using })),
            ProjectedEdge.FromRawEdge(new Edge("D", "C", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var cycles = graph.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
    }

    [Fact]
    public void ExtractDependencies_WithNoDependencies_ReturnsEmpty()
    {
        // Arrange
        var edges = new[]
        {
            ProjectedEdge.FromRawEdge(new Edge("A", "B", false, new[] { ImportKind.Using }))
        };
        var graph = new ProjectedGraph(edges);

        // Act
        var deps = graph.ExtractDependencies("B");

        // Assert
        Assert.Empty(deps);
    }
}
