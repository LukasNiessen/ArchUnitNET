using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.FluentApi;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Layers.Assertion;
using ArchUnitNet.Layers.FluentApi;
using ArchUnitNet.Layers.Projection;
using Xunit;

namespace ArchUnitNet.Tests.Layers;

public class LayerConstraintTests
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
            // Data layer (no external dependencies)
            new Edge("src/Data/Repository.cs", "System.Data.SqlClient", External: true, new[] { ImportKind.Using })
        };
        return new Graph(edges);
    }

    [Fact]
    public async Task LayerConstraint_Constructor_WithValidParameters_Succeeds()
    {
        // Arrange
        var graph = CreateSimpleLayeredGraph();
        var projector = new LayerProjector("src/{Layer}/**");

        // Act
        var constraint = new LayerConstraint(graph, projector, "Presentation");

        // Assert
        Assert.NotNull(constraint);
    }

    [Fact]
    public async Task LayerConstraint_Constructor_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        var projector = new LayerProjector("src/{Layer}/**");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LayerConstraint(null!, projector, "Presentation"));
    }

    [Fact]
    public async Task LayerConstraint_Constructor_WithNullProjector_ThrowsArgumentNullException()
    {
        // Arrange
        var graph = CreateSimpleLayeredGraph();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new LayerConstraint(graph, null!, "Presentation"));
    }

    [Fact]
    public async Task LayerConstraint_MayOnlyDependOn_WithAllowedLayer_PassesAsync()
    {
        // Arrange
        var graph = CreateSimpleLayeredGraph();
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        constraint.SetAllowedLayers(new[] { "Business" });

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayerConstraint_MayOnlyDependOn_WithForbiddenLayer_FailsAsync()
    {
        // Arrange: Presentation directly depends on Data (forbidden)
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        constraint.SetAllowedLayers(new[] { "Business" });

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        Assert.Single(violations);
        var violation = violations[0] as ViolatingLayerDependency;
        Assert.NotNull(violation);
        Assert.Equal("Presentation", violation.SourceLayer);
        Assert.Equal("Data", violation.TargetLayer);
    }

    [Fact]
    public async Task LayerConstraint_MayNotDependOn_WithForbiddenLayer_FailsAsync()
    {
        // Arrange: Presentation depends on Data which is forbidden
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        constraint.SetForbiddenLayers(new[] { "Data" });

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        var violation = violations[0] as ViolatingLayerDependency;
        Assert.NotNull(violation);
        Assert.Equal("Forbidden", violation.Reason);
    }

    [Fact]
    public async Task LayerConstraint_SealedLayer_WithExternalDependency_FailsAsync()
    {
        // Arrange: Sealed layer (empty allowlist - only depends on itself)
        var edges = new List<Edge>
        {
            new Edge("src/Business/Service.cs", "src/Data/Repository.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Business");
        constraint.SetAllowedLayers(new[] { "Business" }); // Sealed: only self

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        var violation = violations[0] as ViolatingLayerDependency;
        Assert.NotNull(violation);
        Assert.Equal("Sealed", violation.Reason);
    }

    [Fact]
    public async Task LayerConstraint_IntraLayerDependency_AlwaysAllowedAsync()
    {
        // Arrange: Component within same layer depends on another component
        var edges = new List<Edge>
        {
            new Edge("src/Business/ServiceA.cs", "src/Business/ServiceB.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Business");
        constraint.SetAllowedLayers(new List<string>()); // No external dependencies allowed

        // Act
        var violations = await constraint.CheckAsync();

        // Assert - Intra-layer dependencies are always allowed
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayerConstraint_NonExistentLayer_ReturnsNoViolationsAsync()
    {
        // Arrange: Layer doesn't exist in the graph
        var graph = CreateSimpleLayeredGraph();
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "NonExistent");
        constraint.SetAllowedLayers(new[] { "Business" });

        // Act
        var violations = await constraint.CheckAsync();

        // Assert - No violations if layer doesn't exist
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayerConstraint_BlocklistBeforeAllowlist_BlocklistWinsAsync()
    {
        // Arrange: Layer is in both blocklist and allowlist (blocklist should win)
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/Component.cs", "src/Utility/Helper.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        constraint.SetForbiddenLayers(new[] { "Utility" });
        constraint.SetAllowedLayers(new[] { "Business", "Utility" }); // Utility in both

        // Act
        var violations = await constraint.CheckAsync();

        // Assert - Blocklist should be evaluated first
        Assert.NotEmpty(violations);
        var violation = violations[0] as ViolatingLayerDependency;
        Assert.Equal("Forbidden", violation.Reason);
    }

    [Fact]
    public async Task LayerConstraint_EmptyTestGuard_WithNoLayersAndNoAllowEmptyTests_FailsAsync()
    {
        // Arrange: Empty graph, no layers exist
        var graph = new Graph(new List<Edge>());
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        var violation = violations[0] as ViolatingLayerDependency;
        Assert.NotNull(violation);
        Assert.Equal("EmptyTest", violation.Reason);
    }

    [Fact]
    public async Task LayerConstraint_EmptyTestGuard_WithAllowEmptyTests_PassesAsync()
    {
        // Arrange: Empty graph with AllowEmptyTests option
        var graph = new Graph(new List<Edge>());
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        var options = new CheckOptions { AllowEmptyTests = true };

        // Act
        var violations = await constraint.CheckAsync(options);

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayerConstraint_MultipleViolations_ReturnAllAsync()
    {
        // Arrange: Multiple forbidden dependencies
        var edges = new List<Edge>
        {
            new Edge("src/Presentation/ComponentA.cs", "src/Data/RepositoryA.cs", External: false, new[] { ImportKind.Using }),
            new Edge("src/Presentation/ComponentB.cs", "src/Data/RepositoryB.cs", External: false, new[] { ImportKind.Using })
        };
        var graph = new Graph(edges);
        var projector = new LayerProjector("src/{Layer}/**");
        var constraint = new LayerConstraint(graph, projector, "Presentation");
        constraint.SetForbiddenLayers(new[] { "Data" });

        // Act
        var violations = await constraint.CheckAsync();

        // Assert
        Assert.Equal(2, violations.Count);
    }
}
