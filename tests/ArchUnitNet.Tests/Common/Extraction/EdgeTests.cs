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
        edge.Source.Should().Be(source);
        edge.Target.Should().Be(target);
        edge.External.Should().BeFalse();
        edge.ImportKinds.Should().Equal(kinds);
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
        edge.External.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SupportsCombinedImportKinds()
    {
        // Arrange
        var kinds = new[] { ImportKind.Using, ImportKind.StaticUsing };

        // Act
        var edge = new Edge("src/A.cs", "src/B.cs", false, kinds);

        // Assert
        edge.ImportKinds.Should().HaveCount(2);
    }

    [Fact]
    public void IsSelfEdge_ReturnsTrueWhenSourceEqualsTarget()
    {
        // Arrange
        var edge = new Edge("src/Common.cs", "src/Common.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        edge.IsSelfEdge.Should().BeTrue();
    }

    [Fact]
    public void IsSelfEdge_ReturnsFalseWhenSourceDiffersFromTarget()
    {
        // Arrange
        var edge = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        edge.IsSelfEdge.Should().BeFalse();
    }

    [Fact]
    public void Validate_ThrowsWhenSourceIsNull()
    {
        // Arrange
        var edge = new Edge(null!, "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        var action = () => edge.Validate();
        action.Should().Throw<ArgumentException>().WithMessage("*source*");
    }

    [Fact]
    public void Validate_ThrowsWhenTargetIsNull()
    {
        // Arrange
        var edge = new Edge("src/A.cs", null!, false, new[] { ImportKind.Using });

        // Act & Assert
        var action = () => edge.Validate();
        action.Should().Throw<ArgumentException>().WithMessage("*target*");
    }

    [Fact]
    public void Validate_ThrowsWhenImportKindsEmpty()
    {
        // Arrange
        var edge = new Edge("src/A.cs", "src/B.cs", false, new List<ImportKind>());

        // Act & Assert
        var action = () => edge.Validate();
        action.Should().Throw<ArgumentException>().WithMessage("*ImportKind*");
    }

    [Fact]
    public void EdgeEquality_TwoEdgesWithSameDataAreEqual()
    {
        // Arrange
        var edge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var edge2 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        edge1.Should().Be(edge2);
    }

    [Fact]
    public void EdgeEquality_TwoEdgesWithDifferentDataAreNotEqual()
    {
        // Arrange
        var edge1 = new Edge("src/A.cs", "src/B.cs", false, new[] { ImportKind.Using });
        var edge2 = new Edge("src/A.cs", "src/C.cs", false, new[] { ImportKind.Using });

        // Act & Assert
        edge1.Should().NotBe(edge2);
    }
}
