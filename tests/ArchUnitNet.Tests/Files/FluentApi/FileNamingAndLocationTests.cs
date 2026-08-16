using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;
using Xunit;

namespace ArchUnitNet.Tests.Files.FluentApi;

public class FileNamingAndLocationTests
{
    private readonly ArchUnitNet.Common.Extraction.Graph _sampleGraph;

    public FileNamingAndLocationTests()
    {
        _sampleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge("src/Services/UserService.cs", "src/Models/User.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Services/OrderService.cs", "src/Models/Order.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("tests/Services/UserServiceTests.cs", "src/Services/UserService.cs", External: false, ImportKinds: new[] { ImportKind.Using }),
            new Edge("src/Models/User.cs", "System", External: true, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    #region HaveName Tests

    [Fact]
    public async Task HaveName_WithMatchingFiles_ReturnsEmpty()
    {
        // Arrange: All service files in src/Services match *.Service.cs pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .HaveName("*Service.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task HaveName_WithNonMatchingFiles_ReturnsViolations()
    {
        // Arrange: Model files don't match *.Service.cs pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Models")
            .Should()
            .HaveName("*Service.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        Assert.Equal(2, violations.Count);
    }

    [Fact]
    public async Task HaveName_ShouldNot_WithMatchingFiles_ReturnsViolations()
    {
        // Arrange: Test files should NOT match *Service.cs (they have Tests suffix)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("tests")
            .ShouldNot()
            .HaveName("*Service.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task HaveName_ShouldNot_WithNonMatchingFiles_ReturnsEmpty()
    {
        // Arrange: Test files should NOT match *.Service.cs
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("tests")
            .ShouldNot()
            .HaveName("*Service.cs");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region BeInFolder Tests

    [Fact]
    public async Task BeInFolder_WithFilesInFolder_ReturnsEmpty()
    {
        // Arrange: Service files should be in src/Services folder
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/**")
            .Should()
            .BeInFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task BeInFolder_WithFilesNotInFolder_ReturnsViolations()
    {
        // Arrange: Model files should be in src/Services (but they're in src/Models)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Models")
            .Should()
            .BeInFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        Assert.Equal(2, violations.Count);
    }

    [Fact]
    public async Task BeInFolder_ShouldNot_WithFilesInFolder_ReturnsViolations()
    {
        // Arrange: Service files should NOT be in tests folder
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .BeInFolder("tests");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Files are in src/Services, not tests
    }

    [Fact]
    public async Task BeInFolder_ShouldNot_WithFilesNotInFolder_ReturnsEmpty()
    {
        // Arrange: Test files should NOT be in src folder
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("tests")
            .ShouldNot()
            .BeInFolder("src");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region BeInPath Tests

    [Fact]
    public async Task BeInPath_WithMatchingPattern_ReturnsEmpty()
    {
        // Arrange: All files should match src/** pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/**")
            .Should()
            .BeInPath("src/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task BeInPath_WithNonMatchingPattern_ReturnsViolations()
    {
        // Arrange: Service files should match models/** pattern (but they don't)
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .Should()
            .BeInPath("src/Models/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task BeInPath_ShouldNot_WithMatchingPattern_ReturnsViolations()
    {
        // Arrange: Service files should NOT match tests/** pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("src/Services")
            .ShouldNot()
            .BeInPath("tests/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations); // Files are in src, not tests
    }

    [Fact]
    public async Task BeInPath_ShouldNot_WithNonMatchingPattern_ReturnsEmpty()
    {
        // Arrange: Test files should NOT match src/** pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InFolder("tests")
            .ShouldNot()
            .BeInPath("src/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region Combined Tests

    [Fact]
    public async Task HaveName_WithWildcardPattern()
    {
        // Arrange: Files matching *Service* pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/**")
            .Should()
            .HaveName("*Service*");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations); // Models don't match *Service*
    }

    [Fact]
    public async Task BeInFolder_CaseInsensitive()
    {
        // Arrange: Folder matching should normalize paths
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("src/Services/**")
            .Should()
            .BeInFolder("src/Services");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task BeInPath_WithGlobPattern()
    {
        // Arrange: Path pattern with wildcards
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("**/**")
            .Should()
            .BeInPath("**/Services/**");

        // Act
        var violations = await rule.CheckAsync();

        // Assert - Service files should match, others shouldn't
        Assert.NotEmpty(violations);
    }

    #endregion
}
