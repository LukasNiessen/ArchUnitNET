using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Util;
using Xunit;

namespace ArchUnitNet.Tests.Common.Projection;

public class ProjectedEdgeTests
{
    [Fact]
    public void FromRawEdge_CreatesProjectedEdgeWithSameLabels()
    {
        // Arrange
        var rawEdge = new Edge(
            Source: "src/Dashboard.cs",
            Target: "src/Orders/OrderRepository.cs",
            External: false,
            ImportKinds: new[] { ImportKind.Using }
        );

        // Act
        var projected = ProjectedEdge.FromRawEdge(rawEdge);

        // Assert
        Assert.Equal("src/Dashboard.cs", projected.Source);
        Assert.Equal("src/Orders/OrderRepository.cs", projected.Target);
        Assert.False(projected.External);
        Assert.Single(projected.ImportKinds);
        Assert.Contains(ImportKind.Using, projected.ImportKinds);
    }

    [Fact]
    public void FromRawEdge_PreservesRawEdge()
    {
        // Arrange
        var rawEdge = new Edge(
            Source: "src/A.cs",
            Target: "src/B.cs",
            External: false,
            ImportKinds: new[] { ImportKind.Using }
        );

        // Act
        var projected = ProjectedEdge.FromRawEdge(rawEdge);

        // Assert
        Assert.Single(projected.RawEdges);
        Assert.Equal(rawEdge, projected.RawEdges.First());
    }

    [Fact]
    public void FromRawEdge_WithExternalDependency()
    {
        // Arrange
        var rawEdge = new Edge(
            Source: "src/Dashboard.cs",
            Target: "System.Collections",
            External: true,
            ImportKinds: new[] { ImportKind.Using }
        );

        // Act
        var projected = ProjectedEdge.FromRawEdge(rawEdge);

        // Assert
        Assert.True(projected.External);
    }

    [Fact]
    public void FromRawEdges_WithMultipleRawEdges_MergesImportKinds()
    {
        // Arrange
        var rawEdge1 = new Edge(
            Source: "src/A.cs",
            Target: "src/B.cs",
            External: false,
            ImportKinds: new[] { ImportKind.Using }
        );
        var rawEdge2 = new Edge(
            Source: "src/A2.cs",
            Target: "src/B.cs",
            External: false,
            ImportKinds: new[] { ImportKind.StaticUsing }
        );

        // Act
        var projected = ProjectedEdge.FromRawEdges("src", "models", rawEdge1, rawEdge2);

        // Assert
        Assert.Equal("src", projected.Source);
        Assert.Equal("models", projected.Target);
        Assert.Equal(2, projected.ImportKinds.Count);
        Assert.Contains(ImportKind.Using, projected.ImportKinds);
        Assert.Contains(ImportKind.StaticUsing, projected.ImportKinds);
    }

    [Fact]
    public void FromRawEdges_PreservesAllRawEdges()
    {
        // Arrange
        var rawEdge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var rawEdge2 = new Edge("src/A2.cs", "src/B.cs", false, new[] { ImportKind.StaticUsing });

        // Act
        var projected = ProjectedEdge.FromRawEdges("src", "models", rawEdge1, rawEdge2);

        // Assert
        Assert.Equal(2, projected.RawEdges.Length);
        Assert.Contains(rawEdge1, projected.RawEdges);
        Assert.Contains(rawEdge2, projected.RawEdges);
    }

    [Fact]
    public void FromRawEdges_MergesDuplicateImportKinds()
    {
        // Arrange
        var rawEdge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var rawEdge2 = new Edge("src/A2.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act
        var projected = ProjectedEdge.FromRawEdges("src", "models", rawEdge1, rawEdge2);

        // Assert
        Assert.Single(projected.ImportKinds);
        Assert.Contains(ImportKind.Using, projected.ImportKinds);
    }

    [Fact]
    public void FromRawEdges_WithEmptyRawEdgesThrows()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ProjectedEdge.FromRawEdges("src", "models")
        );
    }

    [Fact]
    public void FromRawEdges_WithNullSourceThrows()
    {
        // Arrange
        var rawEdge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ProjectedEdge.FromRawEdges(null!, "models", rawEdge)
        );
    }

    [Fact]
    public void FromRawEdges_WithNullTargetThrows()
    {
        // Arrange
        var rawEdge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ProjectedEdge.FromRawEdges("src", null!, rawEdge)
        );
    }

    [Fact]
    public void IsSelfEdge_ReturnsTrueWhenSourceEqualsTarget()
    {
        // Arrange
        var projected = ProjectedEdge.FromRawEdges(
            "src",
            "src",
            new Edge("src/A.cs", "src/A.cs", false, new[] { ImportKind.Using })
        );

        // Act & Assert
        Assert.True(projected.IsSelfEdge);
    }

    [Fact]
    public void IsSelfEdge_ReturnsFalseWhenSourceDiffersFromTarget()
    {
        // Arrange
        var projected = ProjectedEdge.FromRawEdges(
            "src",
            "models",
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using })
        );

        // Act & Assert
        Assert.False(projected.IsSelfEdge);
    }

    [Fact]
    public void ToString_IncludesSourceAndTarget()
    {
        // Arrange
        var projected = ProjectedEdge.FromRawEdges(
            "src",
            "models",
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using })
        );

        // Act
        var result = projected.ToString();

        // Assert
        Assert.Contains("src", result);
        Assert.Contains("models", result);
        Assert.Contains("→", result);
    }

    [Fact]
    public void ToString_ShowsExternalMarkerWhenExternal()
    {
        // Arrange
        var projected = ProjectedEdge.FromRawEdges(
            "src",
            "System",
            new Edge("src/A.cs", "System.Collections", true, new[] { ImportKind.Using })
        );

        // Act
        var result = projected.ToString();

        // Assert
        Assert.Contains("external", result);
    }

    [Fact]
    public void ToString_ShowsRawEdgeCountWhenMultiple()
    {
        // Arrange
        var projected = ProjectedEdge.FromRawEdges(
            "src",
            "models",
            new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using }),
            new Edge("src/A2.cs", "src/B.cs", false, new[] { ImportKind.StaticUsing })
        );

        // Act
        var result = projected.ToString();

        // Assert
        Assert.Contains("2 raw edges", result);
    }

    [Fact]
    public void FromRawEdge_FailsValidationWithInvalidRawEdge()
    {
        // Arrange
        var invalidEdge = new Edge("", "", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProjectedEdge.FromRawEdge(invalidEdge));
    }

    [Fact]
    public void FromRawEdges_TakesExternalFlagFromFirstRawEdge()
    {
        // Arrange
        var externalEdge = new Edge("src/A.cs", "System", true, new[] { ImportKind.Using });

        // Act
        var projected = ProjectedEdge.FromRawEdges("src", "System", externalEdge);

        // Assert
        Assert.True(projected.External);
    }
}
