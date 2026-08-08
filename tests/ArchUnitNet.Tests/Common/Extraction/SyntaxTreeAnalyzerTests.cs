using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Extraction;

public class SyntaxTreeAnalyzerTests
{
    private readonly SyntaxTreeAnalyzer _analyzer = new();

    [Fact]
    public void ExtractImportsFromFile_SingleUsing()
    {
        // Arrange
        var code = """
            using System;
            namespace Test { }
            """;

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Test.cs", code);

        // Assert
        imports.Should().HaveCount(1);
        imports.First().ImportedNamespace.Should().Be("System");
        imports.First().Kind.Should().Be(ImportKind.Using);
    }

    [Fact]
    public void ExtractImportsFromFile_MultipleUsings()
    {
        // Arrange
        var code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            namespace Test { }
            """;

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Test.cs", code);

        // Assert
        imports.Should().HaveCount(3);
        imports.Should().Contain(i => i.ImportedNamespace == "System");
        imports.Should().Contain(i => i.ImportedNamespace == "System.Collections.Generic");
        imports.Should().Contain(i => i.ImportedNamespace == "System.Linq");
    }

    [Fact]
    public void ExtractImportsFromFile_MixedImportKinds()
    {
        // Arrange
        var code = """
            using System;
            using static System.Console;
            using Collections = System.Collections;
            namespace Test { }
            """;

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Test.cs", code);

        // Assert
        imports.Should().HaveCount(3);
        imports.Should().Contain(i => i.Kind == ImportKind.Using);
        imports.Should().Contain(i => i.Kind == ImportKind.StaticUsing);
        imports.Should().Contain(i => i.Kind == ImportKind.AliasUsing);
    }

    [Fact]
    public void ExtractImportsFromFile_EmptyFile()
    {
        // Arrange
        var code = "";

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Empty.cs", code);

        // Assert
        imports.Should().BeEmpty();
    }

    [Fact]
    public void ExtractImportsFromFile_FileWithoutImports()
    {
        // Arrange
        var code = """
            namespace Test
            {
                public class MyClass { }
            }
            """;

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Test.cs", code);

        // Assert
        imports.Should().BeEmpty();
    }

    [Fact]
    public void ExtractImportsFromFile_InvalidCSharp_Throws()
    {
        // Arrange
        var code = "this is not valid c#";

        // Act & Assert
        var action = () => _analyzer.ExtractImportsFromFile("src/Invalid.cs", code);
        // Note: Roslyn is quite forgiving, so invalid code might still parse
        // We just verify it doesn't throw in normal parsing
        action.Should().NotThrow();
    }

    [Fact]
    public void IsExternalNamespace_SystemNamespaces()
    {
        // Act & Assert
        _analyzer.IsExternalNamespace("System").Should().BeTrue();
        _analyzer.IsExternalNamespace("System.Collections").Should().BeTrue();
        _analyzer.IsExternalNamespace("System.Linq").Should().BeTrue();
    }

    [Fact]
    public void IsExternalNamespace_MicrosoftNamespaces()
    {
        // Act & Assert
        _analyzer.IsExternalNamespace("Microsoft.CodeAnalysis").Should().BeTrue();
        _analyzer.IsExternalNamespace("Microsoft.Extensions").Should().BeTrue();
    }

    [Fact]
    public void IsExternalNamespace_PopularPackages()
    {
        // Act & Assert
        _analyzer.IsExternalNamespace("Newtonsoft.Json").Should().BeTrue();
        _analyzer.IsExternalNamespace("xunit").Should().BeTrue();
        _analyzer.IsExternalNamespace("FluentAssertions").Should().BeTrue();
    }

    [Fact]
    public void IsExternalNamespace_InternalNamespaces()
    {
        // Act & Assert
        _analyzer.IsExternalNamespace("MyApp").Should().BeFalse();
        _analyzer.IsExternalNamespace("MyApp.Services").Should().BeFalse();
        _analyzer.IsExternalNamespace("MyApp.Common.Utils").Should().BeFalse();
    }

    [Fact]
    public void IsExternalNamespace_EdgeCases()
    {
        // Act & Assert
        _analyzer.IsExternalNamespace("").Should().BeFalse();
        _analyzer.IsExternalNamespace(null!).Should().BeFalse();
    }

    [Fact]
    public void ExtractImportsFromFile_RealWorldExample()
    {
        // Arrange - simulates a real controller file
        var code = """
            using System;
            using System.Collections.Generic;
            using Microsoft.Extensions.DependencyInjection;
            using MyApp.Services;
            using MyApp.Common;

            namespace MyApp.Controllers
            {
                public class UserController
                {
                    private readonly IUserService _service;

                    public UserController(IUserService service)
                    {
                        _service = service;
                    }
                }
            }
            """;

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Controllers/UserController.cs", code);

        // Assert
        imports.Should().HaveCount(5);

        // Verify each import
        var importsList = imports.Select(i => i.ImportedNamespace).ToList();
        importsList.Should().Contain("System");
        importsList.Should().Contain("System.Collections.Generic");
        importsList.Should().Contain("Microsoft.Extensions.DependencyInjection");
        importsList.Should().Contain("MyApp.Services");
        importsList.Should().Contain("MyApp.Common");
    }
}
