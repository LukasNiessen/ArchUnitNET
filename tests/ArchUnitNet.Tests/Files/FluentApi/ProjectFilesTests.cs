using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;

namespace ArchUnitNet.Tests.Files.FluentApi;

public class ProjectFilesTests
{
    private readonly Graph _sampleGraph;

    public ProjectFilesTests()
    {
        _sampleGraph = new Graph(new[]
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
}
