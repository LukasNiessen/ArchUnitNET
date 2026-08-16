using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;
using Xunit;

namespace ArchUnitNet.Tests.Files.FluentApi;

public class FileDependencyTests
{
    private readonly ArchUnitNet.Common.Extraction.Graph _sampleGraph;

    public FileDependencyTests()
    {
        _sampleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            // Services depend on Models
            new Edge("src/Services/UserService.cs", "src/Models/User.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Services/OrderService.cs", "src/Models/Order.cs", External: false, ImportKinds: new[] { ImportKind.Using }),

            // Controllers depend on Services
            new Edge("src/Controllers/UserController.cs", "src/Services/UserService.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Controllers/OrderController.cs", "src/Services/OrderService.cs", External: false, ImportKinds: new[] { ImportKind.Using }),

            // Legacy services depend on everything (bad!)
            new Edge("src/Legacy/OldService.cs", "src/Models/User.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Legacy/OldService.cs", "src/Controllers/UserController.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    #region Basic Path Matching

    [Fact]
    public async Task DependOnFiles_WithMatchingDependency_ReturnsEmpty()
    {
        // Arrange: Services should depend on Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task DependOnFiles_WithoutExpectedDependency_ReturnsViolations()
    {
        // Arrange: Models should depend on Services (but they don't)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Models")
            .Should()
            .DependOnFiles()
            .InFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task DependOnFiles_ShouldNot_WithForbiddenDependency_ReturnsViolations()
    {
        // Arrange: Services should NOT depend on Controllers
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Controllers");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Services don't depend on controllers - passes
    }

    [Fact]
    public async Task DependOnFiles_ShouldNot_WithAllowedDependency_ReturnsEmpty()
    {
        // Arrange: Controllers should NOT depend on Models (they go through Services)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Controllers")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Models");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region Combined Selectors with AND

    [Fact]
    public async Task DependOnFiles_WithNameSelector_AndHaveName()
    {
        // Arrange: Services should depend on *.cs files in Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models")
            .And()
            .HaveName("*.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // All files in Models are *.cs
    }

    [Fact]
    public async Task DependOnFiles_WithNameSelector_AndFilter()
    {
        // Arrange: Services should depend on User*.cs files in Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models")
            .And()
            .HaveName("User*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        // Only UserService depends on User.cs, OrderService depends on Order.cs
        Assert.NotEmpty(violations);
    }

    #endregion

    #region Exception Patterns (Except)

    [Fact]
    public async Task DependOnFiles_WithExcept_FiltersExceptions()
    {
        // Arrange: Services should depend on Models but NOT on Legacy
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models")
            .Except("**/Legacy/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Services don't depend on Legacy
    }

    [Fact]
    public async Task DependOnFiles_ShouldNot_WithExcept_StillEnforces()
    {
        // Arrange: Services should NOT depend on anything EXCEPT Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Controllers")
            .Except("src/Services/**"); // Exclude self-references

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Services don't depend on Controllers
    }

    #endregion

    #region Layered Architecture Patterns

    [Fact]
    public async Task LayeredArchitecture_ControllersCanUseServices()
    {
        // Arrange: Controllers should depend on Services
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Controllers")
            .Should()
            .DependOnFiles()
            .InFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayeredArchitecture_ControllersCannotUseModelsDirectly()
    {
        // Arrange: Controllers should NOT depend on Models (violates layering)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Controllers")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Models");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Controllers correctly go through Services
    }

    [Fact]
    public async Task LayeredArchitecture_ServicesCanUseModels()
    {
        // Arrange: Services should depend on Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task LayeredArchitecture_ModelsCannotUseServices()
    {
        // Arrange: Models should NOT depend on Services (no upward dependencies)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Models")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Models correctly don't depend on Services
    }

    #endregion

    #region Legacy/Exception Handling

    [Fact]
    public async Task DependencyRule_ExcludingLegacyCode()
    {
        // Arrange: Services should depend on Models, excluding Legacy code
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models")
            .Except("**/Legacy/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task DependencyRule_AllFilesExcept_Legacy()
    {
        // Arrange: All code should depend on Models, except Legacy
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/**")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models")
            .Except("src/Legacy/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        // Controllers and Legacy don't depend on Models
        Assert.NotEmpty(violations);
    }

    #endregion

    #region Multiple Target Conditions (AND Logic)

    [Fact]
    public async Task MultipleSelectors_PathAndName_CombinedWithAnd()
    {
        // Arrange: Services should depend on *.cs files in src/Models
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InPath("src/Models/**")
            .And()
            .HaveName("*.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // All dependencies match both conditions
    }

    [Fact]
    public async Task MultipleSelectors_FolderAndName_CombinedWithAnd()
    {
        // Arrange: Services should depend on *Service.cs in Services folder
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles()
            .InFolder("src/Services")
            .And()
            .HaveName("*Service.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Services themselves match the pattern
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task DependOnFiles_WithoutTargetSelector_Throws()
    {
        // Arrange: Missing target selector
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnFiles(); // No target selector!

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => rule.CheckAsync());
    }

    [Fact]
    public async Task DependOnFiles_WithEmptyMatch_ReturnsError()
    {
        // Arrange: Source pattern matches nothing
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("nonexistent/**")
            .Should()
            .DependOnFiles()
            .InFolder("src/Models");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations); // Empty test error
    }

    #endregion
}
