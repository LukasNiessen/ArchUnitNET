using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Slices.Projection;
using Xunit;

namespace ArchUnitNet.Tests.Slices;

public class SliceProjectorTests
{
    [Fact]
    public void Constructor_WithValidPattern_Succeeds()
    {
        // Act
        var projector = new SliceProjector("src/{Slice}/**");

        // Assert
        Assert.NotNull(projector);
    }

    [Fact]
    public void Constructor_WithNullPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SliceProjector(null!));
    }

    [Fact]
    public void Constructor_WithEmptyPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SliceProjector(""));
    }

    [Fact]
    public void ExtractSliceName_WithMatchingPath_ReturnsSliceName()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");

        // Act
        var sliceName = projector.ExtractSliceName("src/Feature1/Component.cs");

        // Assert
        Assert.Equal("Feature1", sliceName);
    }

    [Fact]
    public void ExtractSliceName_WithNestedPath_ReturnsSliceName()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");

        // Act
        var sliceName = projector.ExtractSliceName("src/Feature1/nested/Component.cs");

        // Assert
        Assert.Equal("Feature1", sliceName);
    }

    [Fact]
    public void ExtractSliceName_WithNonMatchingPath_ReturnsNull()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");

        // Act
        var sliceName = projector.ExtractSliceName("other/path/Component.cs");

        // Assert
        Assert.Null(sliceName);
    }

    [Fact]
    public void ExtractSliceName_WithDifferentPattern_ReturnsCorrectSlice()
    {
        // Arrange
        var projector = new SliceProjector("packages/{Package}/**/index.cs");

        // Act
        var sliceName = projector.ExtractSliceName("packages/utils/helpers/index.cs");

        // Assert
        Assert.Equal("utils", sliceName);
    }

    [Fact]
    public void Project_WithEmptyEdges_ReturnsEmptyArchitecture()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>();

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(0, architecture.SliceCount);
        Assert.Empty(architecture.Dependencies);
    }

    [Fact]
    public void Project_WithSingleSlice_CreatesSingleSlice()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Feature1/Component.cs", "System.String", External: true, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Single(architecture.Slices);
        Assert.NotNull(architecture.GetSlice("Feature1"));
        Assert.Single(architecture.GetSlice("Feature1")!.Files);
    }

    [Fact]
    public void Project_WithMultipleSlices_CreatesMultipleSlices()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Feature1/Component.cs", "System.String", External: true, new[] { ImportKind.Using }),
            new Edge("src/Feature2/Service.cs", "System.String", External: true, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(2, architecture.SliceCount);
        Assert.NotNull(architecture.GetSlice("Feature1"));
        Assert.NotNull(architecture.GetSlice("Feature2"));
    }

    [Fact]
    public void Project_WithInterSliceDependency_RecordsDependency()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Feature1/Component.cs", "src/Feature2/Service.cs", External: false, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(2, architecture.SliceCount);
        Assert.Single(architecture.Dependencies);
        Assert.Equal("Feature1", architecture.Dependencies[0].SourceSlice);
        Assert.Equal("Feature2", architecture.Dependencies[0].TargetSlice);
    }

    [Fact]
    public void GetDependenciesFrom_WithValidSlice_ReturnsDependencies()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Feature1/Component.cs", "src/Feature2/Service.cs", External: false, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var deps = architecture.GetDependenciesFrom("Feature1");

        // Assert
        Assert.Single(deps);
        Assert.Equal("Feature2", deps[0].TargetSlice);
    }

    [Fact]
    public void GetDependenciesTo_WithValidSlice_ReturnsDependencies()
    {
        // Arrange
        var projector = new SliceProjector("src/{Slice}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Feature1/Component.cs", "src/Feature2/Service.cs", External: false, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var deps = architecture.GetDependenciesTo("Feature2");

        // Assert
        Assert.Single(deps);
        Assert.Equal("Feature1", deps[0].SourceSlice);
    }
}

public class SliceConditionBuilderTests
{
    [Fact]
    public void DefinedBy_WithValidPattern_Succeeds()
    {
        // Act
        var builder = ArchUnit.ProjectSlices().DefinedBy("src/{Slice}/**");

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void DefinedBy_WithNullPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArchUnit.ProjectSlices().DefinedBy(null!));
    }

    [Fact]
    public void DefinedBy_WithEmptyPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ArchUnit.ProjectSlices().DefinedBy(""));
    }

    [Fact]
    public void Should_WithoutPattern_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => ArchUnit.ProjectSlices().Should());
    }

    [Fact]
    public void Should_AfterDefinedBy_ReturnsPositiveCondition()
    {
        // Act
        var condition = ArchUnit.ProjectSlices().DefinedBy("src/{Slice}/**").Should();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void ShouldNot_AfterDefinedBy_ReturnsNegativeCondition()
    {
        // Act
        var condition = ArchUnit.ProjectSlices().DefinedBy("src/{Slice}/**").ShouldNot();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void AdhereToDefinedSlices_ReturnsAdhereCondition()
    {
        // Act
        var condition = ArchUnit.ProjectSlices()
            .DefinedBy("src/{Slice}/**")
            .Should()
            .AdhereToDefinedSlices();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void BeAcyclic_ReturnsNoCyclicCondition()
    {
        // Act
        var condition = ArchUnit.ProjectSlices()
            .DefinedBy("src/{Slice}/**")
            .Should()
            .BeAcyclic();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void FollowPattern_WithValidPattern_ReturnsDependencyPatternCondition()
    {
        // Act
        var condition = ArchUnit.ProjectSlices()
            .DefinedBy("src/{Slice}/**")
            .Should()
            .FollowPattern("UI -> Service -> Model");

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void FollowPattern_WithNullPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            ArchUnit.ProjectSlices()
                .DefinedBy("src/{Slice}/**")
                .Should()
                .FollowPattern(null!)
        );
    }
}
