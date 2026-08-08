using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Projection;

public class ProjectEdgesTests
{
    private readonly Graph _graph;

    public ProjectEdgesTests()
    {
        _graph = new Graph(new[]
        {
            new Edge("src/Common/Error.cs", "System", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Common/Error.cs", "System", External: true, ImportKinds: new[] { ImportKind.StaticUsing }),
            new Edge("src/Files/Parser.cs", "src/Common/Error.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Files/Parser.cs", "System.Linq", External: true, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    [Fact]
    public void GroupBySourceAndTarget_AggregatesImportKinds()
    {
        // Act
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Assert
        Assert.Equal(3, projected.Count);

        var systemEdges = projected.Where(e => e.Source == "src/Common/Error.cs" && e.Target == "System").ToList();
        Assert.Single(systemEdges);
        Assert.Equal(2, systemEdges[0].ImportKinds.Count);
        Assert.Contains(ImportKind.Using, systemEdges[0].ImportKinds);
        Assert.Contains(ImportKind.StaticUsing, systemEdges[0].ImportKinds);
    }

    [Fact]
    public void GroupBySourceAndTarget_PreservesExternalFlag()
    {
        // Act
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Assert
        var externalEdges = projected.Where(e => e.External).ToList();
        var internalEdges = projected.Where(e => !e.External).ToList();

        Assert.Equal(2, externalEdges.Count);
        Assert.Single(internalEdges);
    }

    [Fact]
    public void FilterByTarget_ReturnsMatchingEdges()
    {
        // Arrange
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var filtered = ProjectEdges.FilterByTarget(projected, "System");

        // Assert
        Assert.Single(filtered);
        Assert.Equal("src/Common/Error.cs", filtered[0].Source);
    }

    [Fact]
    public void FilterBySource_ReturnsMatchingEdges()
    {
        // Arrange
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var filtered = ProjectEdges.FilterBySource(projected, "src/Files/Parser.cs");

        // Assert
        Assert.Equal(2, filtered.Count);
    }

    [Fact]
    public void RemoveExternalDependencies_FiltersOutExternalEdges()
    {
        // Arrange
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var internal_ = ProjectEdges.RemoveExternalDependencies(projected);

        // Assert
        Assert.Single(internal_);
        Assert.False(internal_[0].External);
    }

    [Fact]
    public void RemoveInternalDependencies_FiltersOutInternalEdges()
    {
        // Arrange
        var projected = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var external = ProjectEdges.RemoveInternalDependencies(projected);

        // Assert
        Assert.Equal(2, external.Count);
        Assert.All(external, e => Assert.True(e.External));
    }
}

public class ProjectNodesTests
{
    private readonly Graph _graph;

    public ProjectNodesTests()
    {
        _graph = new Graph(new[]
        {
            new Edge("src/Common/Error.cs", "src/Files/Parser.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Files/Parser.cs", "src/Util/Helper.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Util/Helper.cs", "System", External: true, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    [Fact]
    public void ExtractAllNodes_ReturnsUniqueNodes()
    {
        // Act
        var nodes = ProjectNodes.ExtractAllNodes(_graph);

        // Assert
        Assert.Equal(4, nodes.Count);
        Assert.Contains("src/Common/Error.cs", nodes);
        Assert.Contains("src/Files/Parser.cs", nodes);
        Assert.Contains("src/Util/Helper.cs", nodes);
        Assert.Contains("System", nodes);
    }

    [Fact]
    public void ExtractInternalNodes_ReturnsOnlyInternalDependencies()
    {
        // Act
        var internal_ = ProjectNodes.ExtractInternalNodes(_graph);

        // Assert
        Assert.Equal(3, internal_.Count);
        Assert.DoesNotContain("System", internal_);
    }

    [Fact]
    public void ExtractExternalNodes_ReturnsOnlyExternalDependencies()
    {
        // Act
        var external = ProjectNodes.ExtractExternalNodes(_graph);

        // Assert
        Assert.Single(external);
        Assert.Contains("System", external);
    }

    [Fact]
    public void ExtractNodeDegree_CountsInAndOutConnections()
    {
        // Arrange
        var edges = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var inDegree = ProjectNodes.ExtractInDegree(edges, "src/Files/Parser.cs");
        var outDegree = ProjectNodes.ExtractOutDegree(edges, "src/Files/Parser.cs");

        // Assert
        Assert.Equal(1, inDegree);  // incoming from Error.cs
        Assert.Equal(1, outDegree); // outgoing to Helper.cs
    }

    [Fact]
    public void ExtractNodeDegree_ZeroForIsolatedNodes()
    {
        // Arrange
        var edges = ProjectEdges.GroupBySourceAndTarget(_graph);

        // Act
        var inDegree = ProjectNodes.ExtractInDegree(edges, "src/Isolated/File.cs");
        var outDegree = ProjectNodes.ExtractOutDegree(edges, "src/Isolated/File.cs");

        // Assert
        Assert.Equal(0, inDegree);
        Assert.Equal(0, outDegree);
    }
}
