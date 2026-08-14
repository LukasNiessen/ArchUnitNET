using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Projection;
using ArchUnitNet.Common.Util;
using Xunit;

namespace ArchUnitNet.Tests.Common.Projection;

public class MapFunctionTests
{
    private readonly Edge _internalEdge = new(
        Source: "src/Dashboard.cs",
        Target: "src/Orders/OrderRepository.cs",
        External: false,
        ImportKinds: new[] { ImportKind.Using }
    );

    private readonly Edge _externalEdge = new(
        Source: "src/Dashboard.cs",
        Target: "System.Collections",
        External: true,
        ImportKinds: new[] { ImportKind.Using }
    );

    private readonly Edge _selfEdge = new(
        Source: "src/Dashboard.cs",
        Target: "src/Dashboard.cs",
        External: false,
        ImportKinds: new[] { ImportKind.Using }
    );

    [Fact]
    public void Identity_PassesAllEdgesThrough()
    {
        // Act
        var result = MapFunctions.Identity(_internalEdge);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_internalEdge.Source, result.Source);
        Assert.Equal(_internalEdge.Target, result.Target);
    }

    [Fact]
    public void Identity_PreservesRawEdge()
    {
        // Act
        var result = MapFunctions.Identity(_internalEdge);

        // Assert
        Assert.Single(result!.RawEdges);
        Assert.Equal(_internalEdge, result.RawEdges[0]);
    }

    [Fact]
    public void Identity_WithExternalEdge()
    {
        // Act
        var result = MapFunctions.Identity(_externalEdge);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.External);
    }

    [Fact]
    public void PerEdge_FiltersSelfEdges()
    {
        // Act
        var result = MapFunctions.PerEdge(_selfEdge);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void PerEdge_PassesNormalEdges()
    {
        // Act
        var result = MapFunctions.PerEdge(_internalEdge);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_internalEdge.Source, result.Source);
        Assert.Equal(_internalEdge.Target, result.Target);
    }

    [Fact]
    public void PerEdge_PassesExternalEdges()
    {
        // Act
        var result = MapFunctions.PerEdge(_externalEdge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void PerInternalEdge_PassesInternalEdges()
    {
        // Act
        var result = MapFunctions.PerInternalEdge(_internalEdge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void PerInternalEdge_FiltersExternalEdges()
    {
        // Act
        var result = MapFunctions.PerInternalEdge(_externalEdge);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void PerInternalEdge_PassesSelfEdges()
    {
        // Act
        var result = MapFunctions.PerInternalEdge(_selfEdge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void PerExternalEdge_PassesExternalEdges()
    {
        // Act
        var result = MapFunctions.PerExternalEdge(_externalEdge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void PerExternalEdge_FiltersInternalEdges()
    {
        // Act
        var result = MapFunctions.PerExternalEdge(_internalEdge);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Compose_AppliesFirstThenSecond()
    {
        // Arrange
        var composed = MapFunctions.Compose(
            MapFunctions.PerEdge,      // First: filter self-edges
            MapFunctions.PerInternalEdge  // Second: filter external
        );

        // Act
        var result = composed(_internalEdge);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Compose_ReturnsNullIfFirstFiltersOut()
    {
        // Arrange
        var composed = MapFunctions.Compose(
            MapFunctions.PerEdge,
            MapFunctions.PerInternalEdge
        );

        // Act
        var result = composed(_selfEdge);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Compose_ReturnsNullIfSecondFiltersOut()
    {
        // Arrange
        var composed = MapFunctions.Compose(
            MapFunctions.PerEdge,      // Passes external edge
            MapFunctions.PerInternalEdge  // Filters external
        );

        // Act
        var result = composed(_externalEdge);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Compose_WithMultipleFilters()
    {
        // Arrange
        var composed = MapFunctions.Compose(
            edge => edge.External ? null : MapFunctions.Identity(edge),
            MapFunctions.PerEdge
        );

        // Act
        var internalResult = composed(_internalEdge);
        var externalResult = composed(_externalEdge);
        var selfResult = composed(_selfEdge);

        // Assert
        Assert.NotNull(internalResult); // Passes both filters
        Assert.Null(externalResult);     // Fails first filter
        Assert.Null(selfResult);         // Passes first filter, fails second
    }

    [Fact]
    public void PerEdge_MultipleEdgeTypes()
    {
        // Arrange
        var edges = new[] { _internalEdge, _externalEdge, _selfEdge };

        // Act
        var results = edges.Select(MapFunctions.PerEdge).ToList();

        // Assert
        Assert.Equal(2, results.Count(r => r != null)); // internal + external, but not self
        Assert.Single(results.Where(r => r == null));   // self-edge filtered
    }
}
