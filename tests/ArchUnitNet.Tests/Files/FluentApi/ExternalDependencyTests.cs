using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;
using Xunit;

namespace ArchUnitNet.Tests.Files.FluentApi;

public class ExternalDependencyTests
{
    private readonly ArchUnitNet.Common.Extraction.Graph _sampleGraph;

    public ExternalDependencyTests()
    {
        _sampleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            // Services depend on external NuGet packages
            new Edge("src/Services/UserService.cs", "Newtonsoft.Json", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Services/OrderService.cs", "Microsoft.Extensions.Configuration", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Services/PaymentService.cs", "Stripe.Net", External: true, ImportKinds: new[] { ImportKind.Using }),

            // Models also depend on external packages
            new Edge("src/Models/Order.cs", "System.Linq", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Models/User.cs", "System.ComponentModel.DataAnnotations", External: true, ImportKinds: new[] { ImportKind.Using }),

            // Internal dependencies
            new Edge("src/Services/UserService.cs", "src/Models/User.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Services/OrderService.cs", "src/Models/Order.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    #region Single Pattern Matching

    [Fact]
    public async Task DependOnExternalModules_WithMatchingDependency_ReturnsEmpty()
    {
        // Arrange: UserService should depend on Newtonsoft.Json
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/UserService.cs")
            .Should()
            .DependOnExternalModules()
            .Matching("Newtonsoft.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task DependOnExternalModules_WithNonMatchingDependency_ReturnsViolations()
    {
        // Arrange: UserService should depend on Microsoft.* (but depends on Newtonsoft)
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/UserService.cs")
            .Should()
            .DependOnExternalModules()
            .Matching("Microsoft.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task DependOnExternalModules_Named_WithExactMatch()
    {
        // Arrange: OrderService should depend on exactly "Microsoft.Extensions.Configuration"
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/OrderService.cs")
            .Should()
            .DependOnExternalModules()
            .Named("Microsoft.Extensions.Configuration");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task DependOnExternalModules_Named_WithWrongName_ReturnsViolations()
    {
        // Arrange: OrderService should depend on "Microsoft.Extensions.Logging" (but has Configuration)
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/OrderService.cs")
            .Should()
            .DependOnExternalModules()
            .Named("Microsoft.Extensions.Logging");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    #endregion

    #region Repeatable Matching (OR Logic)

    [Fact]
    public async Task DependOnExternalModules_WithMultipleMatching_OrLogic()
    {
        // Arrange: Services should depend on either Newtonsoft or Microsoft
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnExternalModules()
            .Matching("Newtonsoft.*")
            .Or()
            .Matching("Microsoft.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        // UserService has Newtonsoft ✓
        // OrderService has Microsoft ✓
        // PaymentService has Stripe ✗
        Assert.NotEmpty(violations);
        Assert.Single(violations); // Only PaymentService violates
    }

    [Fact]
    public async Task DependOnExternalModules_WithThreePatterns_OrLogic()
    {
        // Arrange: Services should depend on Newtonsoft OR Microsoft OR Stripe
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnExternalModules()
            .Matching("Newtonsoft.*")
            .Or()
            .Matching("Microsoft.*")
            .Or()
            .Matching("Stripe.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // All three services have matching dependencies
    }

    [Fact]
    public async Task DependOnExternalModules_WithMultipleNamed_OrLogic()
    {
        // Arrange: Files should depend on either Stripe.Net or Newtonsoft.Json
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/**")
            .Should()
            .DependOnExternalModules()
            .Named("Stripe.Net")
            .Or()
            .Named("Newtonsoft.Json");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        // UserService has Newtonsoft.Json ✓
        // PaymentService has Stripe.Net ✓
        // Others have System.* ✗
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task DependOnExternalModules_MixedMatchingAndNamed_OrLogic()
    {
        // Arrange: Services should depend on Microsoft.* OR exactly Newtonsoft.Json
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnExternalModules()
            .Matching("Microsoft.*")
            .Or()
            .Named("Newtonsoft.Json");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        // UserService has Newtonsoft.Json ✓
        // OrderService has Microsoft.Extensions.Configuration ✓
        // PaymentService has Stripe.Net ✗
        Assert.NotEmpty(violations);
        Assert.Single(violations);
    }

    #endregion

    #region Negated (ShouldNot) Tests

    [Fact]
    public async Task DependOnExternalModules_ShouldNot_WithForbiddenDependency_ReturnsViolations()
    {
        // Arrange: Services should NOT depend on System.* (legacy)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .DependOnExternalModules()
            .Matching("System.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Services don't depend on System
    }

    [Fact]
    public async Task DependOnExternalModules_ShouldNot_WithAllowedDependency_ReturnsEmpty()
    {
        // Arrange: Services should NOT depend on legacy packages
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .DependOnExternalModules()
            .Matching("System.*")
            .Or()
            .Matching("Legacy.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // No violations - services use modern packages
    }

    [Fact]
    public async Task DependOnExternalModules_ShouldNot_WithMultiplePatterns()
    {
        // Arrange: Models should NOT depend on Newtonsoft OR Microsoft
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Models")
            .ShouldNot()
            .DependOnExternalModules()
            .Matching("Newtonsoft.*")
            .Or()
            .Matching("Microsoft.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Models use System.* packages
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task ThirdPartyDependencyPolicy_ApprovedNuGetPackages()
    {
        // Arrange: Only approved packages are allowed
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src")
            .Should()
            .DependOnExternalModules()
            .Matching("Newtonsoft.*")
            .Or()
            .Matching("Microsoft.*")
            .Or()
            .Matching("Stripe.*")
            .Or()
            .Matching("System.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // All dependencies are approved
    }

    [Fact]
    public async Task ThirdPartyDependencyPolicy_ForbiddenLegacyPackages()
    {
        // Arrange: Legacy packages forbidden
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src")
            .ShouldNot()
            .DependOnExternalModules()
            .Matching("OldLib.*")
            .Or()
            .Matching("Deprecated.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // No legacy packages used
    }

    [Fact]
    public async Task DependOnExternalModules_WithPatternCombinations()
    {
        // Arrange: Microsoft or System or Stripe packages allowed
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/**")
            .Should()
            .DependOnExternalModules()
            .Matching("Microsoft.*")
            .Or()
            .Matching("System.*")
            .Or()
            .Matching("Stripe.*")
            .Or()
            .Matching("Newtonsoft.*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // All used packages are allowed
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task DependOnExternalModules_WithoutMatching_Throws()
    {
        // Arrange: No Matching() call
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnExternalModules(); // Missing Matching()!

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => rule.CheckAsync());
    }

    [Fact]
    public async Task DependOnExternalModules_WithEmptyPattern_Throws()
    {
        // Arrange: Empty pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .DependOnExternalModules()
            .Matching(""); // Empty pattern!

        // Act & Assert - constructor should throw
        Assert.Throws<ArgumentException>(() => rule.Matching(""));
    }

    #endregion
}
