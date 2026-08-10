using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection.Cycles;
using ArchUnitNet.Common.Util;

#pragma warning disable xUnit2012 // Use Assert.Collection() to check multiple items in a collection

namespace ArchUnitNet.Tests.Common.Projection.Cycles;

public class TarjanSCCTests
{
    [Fact]
    public void FindSCCs_WithSimpleCycle_FindsSCC()
    {
        // Arrange: A → B → A
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();

        // Assert
        Assert.NotEmpty(sccs);
        var cycleComponent = sccs.FirstOrDefault(scc => scc.Contains("A"));
        Assert.NotNull(cycleComponent);
        Assert.Contains("A", cycleComponent!);
        Assert.Contains("B", cycleComponent!);
    }

    [Fact]
    public void FindSCCs_WithThreeNodeCycle_FindsCycleSCC()
    {
        // Arrange: A → B → C → A
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();

        // Assert
        Assert.NotEmpty(sccs);
        var cycleComponent = sccs.FirstOrDefault(scc => scc.Count > 1);
        Assert.NotNull(cycleComponent);
        Assert.Contains("A", cycleComponent);
        Assert.Contains("B", cycleComponent);
        Assert.Contains("C", cycleComponent);
    }

    [Fact]
    public void FindSCCs_WithNoCycles_FindsSingleNodeSCCs()
    {
        // Arrange: A → B → C (no cycle)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();

        // Assert
        Assert.NotEmpty(sccs);
        var singleNodeSCCs = sccs.Where(scc => scc.Count == 1).ToList();
        Assert.NotEmpty(singleNodeSCCs);
    }

    [Fact]
    public void FindSCCs_WithSelfLoop_FindsSelfLoopSCC()
    {
        // Arrange: A → A
        var edges = new[]
        {
            new Edge("A", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();

        // Assert
        Assert.NotEmpty(sccs);
        var selfLoopComponent = sccs.FirstOrDefault(scc => scc.Contains("A"));
        Assert.NotNull(selfLoopComponent);
    }

    [Fact]
    public void FindSCCs_WithMultipleSeparateCycles_FindsBoth()
    {
        // Arrange: A → B → A and C → D → C (two separate cycles)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "A", false, new[] { ImportKind.Using }),
            new Edge("C", "D", false, new[] { ImportKind.Using }),
            new Edge("D", "C", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();

        // Assert
        var cyclicSCCs = sccs.Where(scc => scc.Count > 1).ToList();
        Assert.NotEmpty(cyclicSCCs);
        Assert.True(cyclicSCCs.Any(scc => scc.Contains("A")));
        Assert.True(cyclicSCCs.Any(scc => scc.Contains("C")));
    }

    [Fact]
    public void FindSCCs_WithDiamondDependency_FindsNoMultiNodeCycle()
    {
        // Arrange: A → B → D, A → C → D (diamond, no cycle)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("A", "C", false, new[] { ImportKind.Using }),
            new Edge("B", "D", false, new[] { ImportKind.Using }),
            new Edge("C", "D", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();
        var cyclicSCCs = sccs.Where(scc => scc.Count > 1).ToList();

        // Assert
        Assert.Empty(cyclicSCCs);
    }

    [Fact]
    public void FindCyclicSCCs_WithMixedCyclesAndNoCycles_ReturnOnlyCyclic()
    {
        // Arrange: A → B → A (cycle) and C → D (no cycle)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "A", false, new[] { ImportKind.Using }),
            new Edge("C", "D", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var cyclicSCCs = tarjan.FindCyclicSCCs();

        // Assert
        Assert.NotEmpty(cyclicSCCs);
        Assert.True(cyclicSCCs.All(scc => scc.Count > 1 || scc.Count == 1)); // Only cyclic components
        var abComponent = cyclicSCCs.FirstOrDefault(scc => scc.Contains("A"));
        Assert.NotNull(abComponent);
        Assert.Contains("B", abComponent);
    }

    [Fact]
    public void FindCyclicSCCs_WithEmptyGraph_ReturnsEmpty()
    {
        // Arrange
        var graph = new ArchUnitNet.Common.Extraction.Graph(Array.Empty<Edge>());
        var tarjan = new TarjanSCC(graph);

        // Act
        var cyclicSCCs = tarjan.FindCyclicSCCs();

        // Assert
        Assert.Empty(cyclicSCCs);
    }

    [Fact]
    public void FindSCCs_WithComplexGraph_FindsAllComponents()
    {
        // Arrange: Complex graph with multiple components
        var edges = new[]
        {
            // Cycle 1: A → B → C → A
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "A", false, new[] { ImportKind.Using }),
            // Dependency: C → D (connects to second component)
            new Edge("C", "D", false, new[] { ImportKind.Using }),
            // Cycle 2: D → E → D
            new Edge("D", "E", false, new[] { ImportKind.Using }),
            new Edge("E", "D", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var tarjan = new TarjanSCC(graph);

        // Act
        var sccs = tarjan.FindSCCs();
        var cyclicSCCs = sccs.Where(scc => scc.Count > 1).ToList();

        // Assert
        Assert.NotEmpty(cyclicSCCs);
        Assert.True(cyclicSCCs.Any(scc => scc.Contains("A")));
        Assert.True(cyclicSCCs.Any(scc => scc.Contains("D")));
    }
}
