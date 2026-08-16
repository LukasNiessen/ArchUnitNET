using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Layers.Common;
using ArchUnitNet.Layers.FluentApi;
using Xunit;
using Graph = ArchUnitNet.Common.Extraction.Graph;

namespace ArchUnitNet.Tests.Layers;

public class LayersConditionBuilderTests
{
    private static Graph CreateSimpleLayeredGraph()
    {
        // Create a 3-layer architecture: Presentation -> Business -> Data
        var edges = new List<Edge>
        {
            // Presentation layer
            new Edge("src/Presentation/Component.cs", "src/Business/Service.cs", External: false, new[] { ImportKind.Using }),
            // Business layer
            new Edge("src/Business/Service.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using }),
            // Data layer
            new Edge("src/Data/Repository.cs", "System.Data.SqlClient", External: true, new[] { ImportKind.Using })
        };
        return new Graph(edges);
    }

    [Fact]
    public void ProjectLayers_From_CreatesBuilder()
    {
        // Act
        var builder = ProjectLayers.From(CreateSimpleLayeredGraph());

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void ProjectLayers_From_WithNullGraph_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ProjectLayers.From(null!));
    }

    [Fact]
    public void DefinedBy_WithValidPattern_Succeeds()
    {
        // Act
        var builder = ProjectLayers.From(CreateSimpleLayeredGraph()).DefinedBy("src/{Layer}/**");

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void DefinedBy_WithNullPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProjectLayers.From(CreateSimpleLayeredGraph()).DefinedBy(null!));
    }

    [Fact]
    public void DefinedBy_WithEmptyPattern_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => ProjectLayers.From(CreateSimpleLayeredGraph()).DefinedBy(""));
    }

    [Fact]
    public void Where_WithoutPattern_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ProjectLayers.From(CreateSimpleLayeredGraph()).Where(new Layer("Presentation")));
    }

    [Fact]
    public void Where_WithNullLayer_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ProjectLayers.From(CreateSimpleLayeredGraph())
                .DefinedBy("src/{Layer}/**")
                .Where(null!));
    }

    [Fact]
    public void Where_AfterDefinedBy_ReturnsWhereClause()
    {
        // Act
        var whereClause = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"));

        // Assert
        Assert.NotNull(whereClause);
    }

    [Fact]
    public void MayOnlyDependOn_WithAllowedLayers_ReturnsConstraint()
    {
        // Act
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"));

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public void MayOnlyDependOn_WithMultipleAllowedLayers_ReturnsConstraint()
    {
        // Act
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"), new Layer("Common"));

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public void MayOnlyDependOn_WithNoArguments_SealedLayer()
    {
        // Act - Calling with no arguments creates a sealed layer
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Data"))
            .MayOnlyDependOn(); // No arguments = sealed

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public void MayNotDependOn_WithForbiddenLayers_ReturnsConstraint()
    {
        // Act
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayNotDependOn(new Layer("Data"));

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public void MayNotDependOn_WithMultipleForbiddenLayers_ReturnsConstraint()
    {
        // Act
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayNotDependOn(new Layer("Data"), new Layer("External"));

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public async Task FluentApi_MayOnlyDependOn_ValidArchitectureAsync()
    {
        // Arrange: Presentation -> Business -> Data (valid 3-layer)
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"));

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task FluentApi_MayOnlyDependOn_InvalidArchitectureAsync()
    {
        // Arrange: Presentation directly depends on Data (invalid)
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using })
        };
        var constraint = ProjectLayers.From(new Graph(edges))
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"));

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task FluentApi_MayNotDependOn_ValidAsync()
    {
        // Arrange: Presentation depends only on Business (Data is forbidden but not used)
        var constraint = ProjectLayers.From(CreateSimpleLayeredGraph())
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayNotDependOn(new Layer("Data"));

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task FluentApi_MayNotDependOn_InvalidAsync()
    {
        // Arrange: Presentation violates by depending on forbidden Data layer
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using })
        };
        var constraint = ProjectLayers.From(new Graph(edges))
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayNotDependOn(new Layer("Data"));

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task FluentApi_SealedLayer_AllowsIntraLayerOnlyAsync()
    {
        // Arrange: Data layer is sealed (no external dependencies)
        // This should pass because Data has no external dependencies
        var edges = new List<Edge>
        {
            new Edge("src/Data/RepositoryA.cs", "src/Data/RepositoryB.cs", External: false, new[] { ImportKind.Using })
        };
        var constraint = ProjectLayers.From(new Graph(edges))
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Data"))
            .MayOnlyDependOn(); // Sealed

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task FluentApi_EmptyPattern_WithAllowEmptyTestsAsync()
    {
        // Arrange: No matching layers, but AllowEmptyTests is true
        var edges = new List<Edge>();
        var options = new CheckOptions { AllowEmptyTests = true };
        var constraint = ProjectLayers.From(new Graph(edges))
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"));

        // Act
        var violations = await constraint.CheckAsync(options);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task FluentApi_ComplexScenario_ThreeLayerArchitectureAsync()
    {
        // Arrange: Full 3-layer architecture test
        var graph = CreateSimpleLayeredGraph();

        // Test each layer's constraints
        var presentationConstraint = ProjectLayers.From(graph)
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Presentation"))
            .MayOnlyDependOn(new Layer("Business"));

        var businessConstraint = ProjectLayers.From(graph)
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Business"))
            .MayOnlyDependOn(new Layer("Data"));

        var dataConstraint = ProjectLayers.From(graph)
            .DefinedBy("src/{Layer}/**")
            .Where(new Layer("Data"))
            .MayOnlyDependOn(); // Sealed - no external dependencies on other layers

        // Act
        var presentationViolations = await presentationConstraint.CheckAsync();
        var businessViolations = await businessConstraint.CheckAsync();
        var dataViolations = await dataConstraint.CheckAsync();

        // Assert - All should pass
        Assert.Empty(presentationViolations);
        Assert.Empty(businessViolations);
        Assert.Empty(dataViolations);
    }

    [Fact]
    public void Layer_Factory_CreatesLayer()
    {
        // Act
        var layer = Layer.Defined("Presentation");

        // Assert
        Assert.NotNull(layer);
        Assert.Equal("Presentation", layer.Name);
    }

    [Fact]
    public void Layer_ToString_ReturnsName()
    {
        // Act
        var layer = new Layer("Business");

        // Assert
        Assert.Equal("Business", layer.ToString());
    }
}
