using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Extraction;

public class EdgeTests
{
    [Fact]
    public void Constructor_CreatesEdgeWithValidData()
    {
        // Arrange
        var source = "src/Common/Error.cs";
        var target = "src/Files/FluentApi.cs";
        var kinds = new[] { ImportKind.Using };

        // Act
        var edge = new Edge(source, target, false, kinds);

        // Assert
        Assert.Equal(source, edge.Source);
        Assert.Equal(target, edge.Target);
        Assert.False(edge.External);
        Assert.Equal(kinds, edge.ImportKinds);
    }

    [Fact]
    public void Constructor_SupportsExternalDependencies()
    {
        // Arrange
        var source = "src/Common/Error.cs";
        var target = "Microsoft.CodeAnalysis";
        var kinds = new[] { ImportKind.Using };

        // Act
        var edge = new Edge(source, target, true, kinds);

        // Assert
        Assert.True(edge.External);
    }

    [Fact]
    public void Constructor_SupportsCombinedImportKinds()
    {
        // Arrange
        var kinds = new[] { ImportKind.Using, ImportKind.StaticUsing };

        // Act
        var edge = new Edge("src/A.cs", "src/B.cs", false, kinds);

        // Assert
        Assert.Equal(2, edge.ImportKinds.Count);
    }

    [Fact]
    public void IsSelfEdge_ReturnsTrueWhenSourceEqualsTarget()
    {
        // Arrange
        var edge = new Edge("src/Common.cs", "src/Common.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.True(edge.IsSelfEdge);
    }

    [Fact]
    public void IsSelfEdge_ReturnsFalseWhenSourceDiffersFromTarget()
    {
        // Arrange
        var edge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.False(edge.IsSelfEdge);
    }

    [Fact]
    public void Validate_ThrowsWhenSourceIsNull()
    {
        // Arrange
        var edge = new Edge(null!, "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => edge.Validate());
        Assert.Contains("source", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsWhenTargetIsNull()
    {
        // Arrange
        var edge = new Edge("src/A.cs", null!, false, new[] { ImportKind.Using });

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => edge.Validate());
        Assert.Contains("target", ex.Message);
    }

    [Fact]
    public void Validate_ThrowsWhenImportKindsEmpty()
    {
        // Arrange
        var edge = new Edge("src/A.cs", "src/B.cs", false, new List<ImportKind>());

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => edge.Validate());
        Assert.Contains("ImportKind", ex.Message);
    }

    [Fact]
    public void EdgeEquality_TwoEdgesWithSameDataAreEqual()
    {
        // Arrange
        var edge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var edge2 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.Equal(edge1, edge2);
    }

    [Fact]
    public void EdgeEquality_TwoEdgesWithDifferentDataAreNotEqual()
    {
        // Arrange
        var edge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var edge2 = new Edge("src/A.cs", "src/C.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        Assert.NotEqual(edge1, edge2);
    }
}
