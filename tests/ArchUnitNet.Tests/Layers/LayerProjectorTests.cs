using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Layers.Projection;
using Xunit;

namespace ArchUnitNet.Tests.Layers;

public class LayerProjectorTests
{
    [Fact]
    public void Constructor_WithValidPattern_Succeeds()
    {
        // Act
        var projector = new LayerProjector("src/{Layer}/**");

        // Assert
        Assert.NotNull(projector);
    }

    [Fact]
    public void Constructor_WithNullPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LayerProjector(null!));
    }

    [Fact]
    public void Constructor_WithEmptyPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new LayerProjector(""));
    }

    [Fact]
    public void ExtractLayerName_WithMatchingPath_ReturnsLayerName()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");

        // Act
        var layerName = projector.ExtractLayerName("src/Presentation/Component.cs");

        // Assert
        Assert.Equal("Presentation", layerName);
    }

    [Fact]
    public void ExtractLayerName_WithNestedPath_ReturnsLayerName()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");

        // Act
        var layerName = projector.ExtractLayerName("src/Business/Services/Service.cs");

        // Assert
        Assert.Equal("Business", layerName);
    }

    [Fact]
    public void ExtractLayerName_WithNonMatchingPath_ReturnsNull()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");

        // Act
        var layerName = projector.ExtractLayerName("other/path/Component.cs");

        // Assert
        Assert.Null(layerName);
    }

    [Fact]
    public void ExtractLayerName_WithDifferentPattern_ReturnsCorrectLayer()
    {
        // Arrange
        var projector = new LayerProjector("app/{Layer}/src/**");

        // Act
        var layerName = projector.ExtractLayerName("app/API/src/Controller.cs");

        // Assert
        Assert.Equal("API", layerName);
    }

    [Fact]
    public void Project_WithEmptyEdges_ReturnsEmptyArchitecture()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>();

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(0, architecture.LayerCount);
        Assert.Empty(architecture.Dependencies);
    }

    [Fact]
    public void Project_WithSingleLayer_CreatesSingleLayer()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "System.String", External: true, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Single(architecture.Layers);
        Assert.NotNull(architecture.GetLayer("Presentation"));
        var files = architecture.GetFilesInLayer("Presentation");
        Assert.Single(files);
    }

    [Fact]
    public void Project_WithMultipleLayers_CreatesMultipleLayers()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "System.String", External: true, new[] { ImportKind.Using }),
            new Edge("src/Business/Service.cs", "System.String", External: true, new[] { ImportKind.Using }),
            new Edge("src/Data/Repository.cs", "System.String", External: true, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(3, architecture.LayerCount);
        Assert.NotNull(architecture.GetLayer("Presentation"));
        Assert.NotNull(architecture.GetLayer("Business"));
        Assert.NotNull(architecture.GetLayer("Data"));
    }

    [Fact]
    public void Project_WithInterLayerDependency_RecordsDependency()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Business/Service.cs", External: false, new[] { ImportKind.Using })
        };

        // Act
        var architecture = projector.Project(edges);

        // Assert
        Assert.Equal(2, architecture.LayerCount);
        Assert.Single(architecture.Dependencies);
        Assert.Equal("Presentation", architecture.Dependencies[0].SourceLayer);
        Assert.Equal("Business", architecture.Dependencies[0].TargetLayer);
    }

    [Fact]
    public void GetDependenciesFrom_WithValidLayer_ReturnsDependencies()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Business/Service.cs", External: false, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var deps = architecture.GetDependenciesFrom("Presentation");

        // Assert
        Assert.Single(deps);
        Assert.Equal("Business", deps[0].TargetLayer);
    }

    [Fact]
    public void GetDependenciesTo_WithValidLayer_ReturnsDependencies()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Business/Service.cs", External: false, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var deps = architecture.GetDependenciesTo("Business");

        // Assert
        Assert.Single(deps);
        Assert.Equal("Presentation", deps[0].SourceLayer);
    }

    [Fact]
    public void GetLayerForFile_WithValidFile_ReturnsLayerName()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "System.String", External: true, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var layer = architecture.GetLayerForFile("src/Presentation/Component.cs");

        // Assert
        Assert.Equal("Presentation", layer);
    }

    [Fact]
    public void GetLayerForFile_WithUnknownFile_ReturnsNull()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "System.String", External: true, new[] { ImportKind.Using })
        };
        var architecture = projector.Project(edges);

        // Act
        var layer = architecture.GetLayerForFile("unknown/file.cs");

        // Assert
        Assert.Null(layer);
    }
}
