using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;

#pragma warning disable xUnit2012 // Use Assert.Collection() to check multiple items in a collection

namespace ArchUnitNet.Tests.Files.FluentApi;

public class ProjectFilesTests
{
    private readonly ArchUnitNet.Common.Extraction.Graph _sampleGraph;

    public ProjectFilesTests()
    {
        _sampleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("src/Dashboard/Dashboard.cs", "src/Orders/OrderRepository.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Dashboard/Dashboard.cs", "System", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Orders/OrderService.cs", "src/Orders/OrderRepository.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Orders/OrderService.cs", "System.Linq", External: true, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    [Fact]
    public void ProjectFiles_CreatesBuilder()
    {
        // Act
        var builder = ProjectFiles.From(_sampleGraph);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void InPath_FiltersFilesByPattern()
    {
        // Arrange
        var builder = ProjectFiles.From(_sampleGraph).InPath("src/Dashboard/**");

        // Act
        var condition = builder.Should();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void Should_CreatesPositiveCondition()
    {
        // Arrange & Act
        var condition = ProjectFiles.From(_sampleGraph)
            .InPath("src/Dashboard/**")
            .Should();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void ShouldNot_CreatesNegatedCondition()
    {
        // Arrange & Act
        var condition = ProjectFiles.From(_sampleGraph)
            .InPath("src/Dashboard/**")
            .ShouldNot();

        // Assert
        Assert.NotNull(condition);
    }

    [Fact]
    public void DependOnFiles_CreatesFileDependencyRule()
    {
        // Arrange & Act
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Dashboard/**")
            .Should()
            .DependOnFiles()
            .InPath("src/Models/**");

        // Assert
        Assert.NotNull(rule);
    }

    [Fact]
    public async Task CheckAsync_WithViolations_ReturnsViolations()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Dashboard/**")
            .ShouldNot()
            .DependOnFiles()
            .InPath("src/Orders/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotNull(violations);
        Assert.NotEmpty(violations);
        Assert.Contains(violations, v => v.ToString()?.Contains("Dashboard.cs") ?? false);
    }

    [Fact]
    public async Task CheckAsync_WithoutViolations_ReturnsEmpty()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Dashboard/**")
            .Should()
            .DependOnFiles()
            .InPath("src/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task InFolder_WorksAsAlternativeToInPath()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Dashboard")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Orders");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task HaveNoCycles_WithSimpleCycle_ReturnsViolations()
    {
        // Arrange: Create a cyclic graph
        var cycleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("A.cs", "B.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("B.cs", "C.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("C.cs", "A.cs", External: false, ImportKinds: new[] { ImportKind.Using })
        });

        var rule = ProjectFiles.From(cycleGraph)
            .InPath("**/*.cs")
            .ShouldNot()
            .HaveNoCycles();

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotNull(violations);
        Assert.NotEmpty(violations);
        Assert.True(violations.Any(v => v.ToString()?.Contains("Cyclic dependency") ?? false));
    }

    [Fact]
    public async Task HaveNoCycles_WithNoCycles_ReturnsEmpty()
    {
        // Arrange: Linear graph with no cycles
        var linearGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("A.cs", "B.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("B.cs", "C.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("C.cs", "Model.cs", External: false, ImportKinds: new[] { ImportKind.Using })
        });

        var rule = ProjectFiles.From(linearGraph)
            .InPath("**/*.cs")
            .ShouldNot()
            .HaveNoCycles();

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task HaveNoCycles_WithFilteredPath_OnlyChecksMatchingFiles()
    {
        // Arrange: Cycle exists but outside filtered path
        var cycleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("src/A.cs", "src/B.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/B.cs", "src/A.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("models/Model.cs", "models/Helper.cs", External: false, ImportKinds: new[] { ImportKind.Using })
        });

        var rule = ProjectFiles.From(cycleGraph)
            .InPath("models/**")
            .ShouldNot()
            .HaveNoCycles();

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // No cycles in models folder
    }

    [Fact]
    public async Task HaveNoCycles_WithSelfLoop_ReturnsViolation()
    {
        // Arrange: Self-loop cycle
        var selfLoopGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("A.cs", "A.cs", External: false, ImportKinds: new[] { ImportKind.Using })
        });

        var rule = ProjectFiles.From(selfLoopGraph)
            .InPath("**/*.cs")
            .ShouldNot()
            .HaveNoCycles();

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task HaveNoCycles_WithMultipleSeparateCycles_ReturnsAllViolations()
    {
        // Arrange: Two separate cycles
        var multiCycleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("A.cs", "B.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("B.cs", "A.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("C.cs", "D.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("D.cs", "C.cs", External: false, ImportKinds: new[] { ImportKind.Using })
        });

        var rule = ProjectFiles.From(multiCycleGraph)
            .InPath("**/*.cs")
            .ShouldNot()
            .HaveNoCycles();

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        Assert.True(violations.Count >= 2, "Should detect multiple cycles");
    }
}
