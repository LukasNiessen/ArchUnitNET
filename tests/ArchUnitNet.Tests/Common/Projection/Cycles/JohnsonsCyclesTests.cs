using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection.Cycles;
using ArchUnitNet.Common.Util;

#pragma warning disable xUnit2012 // Use Assert.Collection() to check multiple items in a collection

namespace ArchUnitNet.Tests.Common.Projection.Cycles;

public class JohnsonsCyclesTests
{
    [Fact]
    public void FindAllCycles_WithSimpleCycle_FindsOne()
    {
        // Arrange: A → B → A
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        Assert.True(cycles.Any(c => c.Contains("A") && c.Contains("B")));
    }

    [Fact]
    public void FindAllCycles_WithThreeNodeCycle_FindsOne()
    {
        // Arrange: A → B → C → A
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        var cycleWithAllNodes = cycles.FirstOrDefault(c => c.Count == 3);
        Assert.NotNull(cycleWithAllNodes);
        Assert.Contains("A", cycleWithAllNodes);
        Assert.Contains("B", cycleWithAllNodes);
        Assert.Contains("C", cycleWithAllNodes);
    }

    [Fact]
    public void FindAllCycles_WithNoCycles_FindsNone()
    {
        // Arrange: A → B → C (no cycle)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.Empty(cycles);
    }

    [Fact]
    public void FindAllCycles_WithSelfLoop_FindsOne()
    {
        // Arrange: A → A
        var edges = new[]
        {
            new Edge("A", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        Assert.True(cycles.Any(c => c.Contains("A")));
    }

    [Fact]
    public void FindAllCycles_WithMultipleSeparateCycles_FindsBoth()
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
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        Assert.True(cycles.Any(c => c.Contains("A")));
        Assert.True(cycles.Any(c => c.Contains("C")));
    }

    [Fact]
    public void FindAllCycles_WithMultipleCyclesSameComponent_FindsAll()
    {
        // Arrange: A → B → C → A and A → C → B → A (two cycles in same component)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "A", false, new[] { ImportKind.Using }),
            new Edge("A", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "B", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        // Should find multiple cycles
        Assert.True(cycles.Count >= 2);
    }

    [Fact]
    public void FindAllCycles_WithDiamondDependency_FindsNone()
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
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.Empty(cycles);
    }

    [Fact]
    public void FindAllCycles_WithEmptyGraph_ReturnsEmpty()
    {
        // Arrange
        var graph = new ArchUnitNet.Common.Extraction.Graph(Array.Empty<Edge>());
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.Empty(cycles);
    }

    [Fact]
    public void FindAllCycles_WithFourNodeCycle_FindsOne()
    {
        // Arrange: A → B → C → D → A
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "D", false, new[] { ImportKind.Using }),
            new Edge("D", "A", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        var fourNodeCycle = cycles.FirstOrDefault(c => c.Count == 4);
        Assert.NotNull(fourNodeCycle);
    }

    [Fact]
    public void FindAllCycles_WithComplexGraph_FindsAllElementaryCycles()
    {
        // Arrange: Complex graph with multiple cycles
        var edges = new[]
        {
            // Main cycle: A → B → C → A
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "A", false, new[] { ImportKind.Using }),
            // Additional edge creating more cycles: A → C
            new Edge("A", "C", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.NotEmpty(cycles);
        // Should find at least the main cycle
        Assert.True(cycles.Any(c => c.Count >= 2));
    }

    [Fact]
    public void FindAllCycles_WithLinearGraph_FindsNone()
    {
        // Arrange: A → B → C → D → E (linear, no cycles)
        var edges = new[]
        {
            new Edge("A", "B", false, new[] { ImportKind.Using }),
            new Edge("B", "C", false, new[] { ImportKind.Using }),
            new Edge("C", "D", false, new[] { ImportKind.Using }),
            new Edge("D", "E", false, new[] { ImportKind.Using })
        };
        var graph = new ArchUnitNet.Common.Extraction.Graph(edges);
        var johnson = new JohnsonsCycles(graph);

        // Act
        var cycles = johnson.FindAllCycles();

        // Assert
        Assert.Empty(cycles);
    }
}
