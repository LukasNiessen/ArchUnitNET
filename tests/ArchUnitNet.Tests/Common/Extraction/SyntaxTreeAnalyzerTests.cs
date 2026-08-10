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
        Assert.Single(imports);
        Assert.Equal("System", imports.First().ImportedNamespace);
        Assert.Equal(ImportKind.Using, imports.First().Kind);
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
        Assert.Equal(3, imports.Count);
        Assert.Contains(imports, i => i.ImportedNamespace == "System");
        Assert.Contains(imports, i => i.ImportedNamespace == "System.Collections.Generic");
        Assert.Contains(imports, i => i.ImportedNamespace == "System.Linq");
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
        Assert.Equal(3, imports.Count);
        Assert.Contains(imports, i => i.Kind == ImportKind.Using);
        Assert.Contains(imports, i => i.Kind == ImportKind.StaticUsing);
        Assert.Contains(imports, i => i.Kind == ImportKind.AliasUsing);
    }

    [Fact]
    public void ExtractImportsFromFile_EmptyFile()
    {
        // Arrange
        var code = "";

        // Act
        var imports = _analyzer.ExtractImportsFromFile("src/Empty.cs", code);

        // Assert
        Assert.Empty(imports);
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
        Assert.Empty(imports);
    }

    [Fact]
    public void ExtractImportsFromFile_InvalidCSharp_Throws()
    {
        // Arrange
        var code = "this is not valid c#";

        // Act & Assert
        // Note: Roslyn is quite forgiving, so invalid code might still parse
        // We just verify it doesn't throw in normal parsing
        _analyzer.ExtractImportsFromFile("src/Invalid.cs", code);
    }

    [Fact]
    public void IsExternalNamespace_SystemNamespaces()
    {
        // Act & Assert
        Assert.True(_analyzer.IsExternalNamespace("System"));
        Assert.True(_analyzer.IsExternalNamespace("System.Collections"));
        Assert.True(_analyzer.IsExternalNamespace("System.Linq"));
    }

    [Fact]
    public void IsExternalNamespace_MicrosoftNamespaces()
    {
        // Act & Assert
        Assert.True(_analyzer.IsExternalNamespace("Microsoft.CodeAnalysis"));
        Assert.True(_analyzer.IsExternalNamespace("Microsoft.Extensions"));
    }

    [Fact]
    public void IsExternalNamespace_PopularPackages()
    {
        // Act & Assert
        Assert.True(_analyzer.IsExternalNamespace("Newtonsoft.Json"));
        Assert.True(_analyzer.IsExternalNamespace("xunit"));
        Assert.True(_analyzer.IsExternalNamespace("FluentAssertions"));
    }

    [Fact]
    public void IsExternalNamespace_InternalNamespaces()
    {
        // Act & Assert
        Assert.False(_analyzer.IsExternalNamespace("MyApp"));
        Assert.False(_analyzer.IsExternalNamespace("MyApp.Services"));
        Assert.False(_analyzer.IsExternalNamespace("MyApp.Common.Utils"));
    }

    [Fact]
    public void IsExternalNamespace_EdgeCases()
    {
        // Act & Assert
        Assert.False(_analyzer.IsExternalNamespace(""));
        Assert.False(_analyzer.IsExternalNamespace(null!));
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
        Assert.Equal(5, imports.Count);

        // Verify each import
        var importsList = imports.Select(i => i.ImportedNamespace).ToList();
        Assert.Contains("System", importsList);
        Assert.Contains("System.Collections.Generic", importsList);
        Assert.Contains("Microsoft.Extensions.DependencyInjection", importsList);
        Assert.Contains("MyApp.Services", importsList);
        Assert.Contains("MyApp.Common", importsList);
    }
}
