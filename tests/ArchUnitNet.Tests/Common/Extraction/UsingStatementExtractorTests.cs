using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace ArchUnitNet.Tests.Common.Extraction;

public class UsingStatementExtractorTests
{
    [Fact]
    public void Extract_SimpleUsing()
    {
        // Arrange
        var code = """
            using System;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Single(imports);
        Assert.Equal("System", imports.First().Name);
        Assert.Equal(ImportKind.Using, imports.First().Kind);
        Assert.False(imports.First().IsGlobal);
    }

    [Fact]
    public void Extract_MultipleUsings()
    {
        // Arrange
        var code = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Equal(3, imports.Count);
        Assert.Contains(imports, i => i.Name == "System");
        Assert.Contains(imports, i => i.Name == "System.Collections.Generic");
        Assert.Contains(imports, i => i.Name == "System.Linq");
    }

    [Fact]
    public void Extract_StaticUsing()
    {
        // Arrange
        var code = """
            using static System.Console;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Single(imports);
        Assert.Equal("System.Console", imports.First().Name);
        Assert.Equal(ImportKind.StaticUsing, imports.First().Kind);
    }

    [Fact]
    public void Extract_GlobalUsing()
    {
        // Arrange
        var code = """
            global using System;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Single(imports);
        Assert.Equal("System", imports.First().Name);
        Assert.Equal(ImportKind.GlobalUsing, imports.First().Kind);
        Assert.True(imports.First().IsGlobal);
    }

    [Fact]
    public void Extract_AliasUsing()
    {
        // Arrange
        var code = """
            using str = System.String;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Single(imports);
        Assert.Equal("System.String", imports.First().Name);
        Assert.Equal(ImportKind.AliasUsing, imports.First().Kind);
    }

    [Fact]
    public void Extract_MixedUsingTypes()
    {
        // Arrange
        var code = """
            using System;
            using static System.Console;
            using Collections = System.Collections.Generic;
            global using System.Linq;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Equal(4, imports.Count);
        Assert.Contains(imports, i => i.Kind == ImportKind.Using);
        Assert.Contains(imports, i => i.Kind == ImportKind.StaticUsing);
        Assert.Contains(imports, i => i.Kind == ImportKind.AliasUsing);
        Assert.Contains(imports, i => i.Kind == ImportKind.GlobalUsing);
    }

    [Fact]
    public void Extract_IgnoresEmptyUsings()
    {
        // Arrange
        var code = """
            using;
            namespace Test { }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Empty(imports);
    }

    [Fact]
    public void Extract_NoUsings()
    {
        // Arrange
        var code = """
            namespace Test
            {
                class MyClass { }
            }
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Empty(imports);
    }

    [Fact]
    public void Extract_UsingsInNamespace()
    {
        // Arrange - file-scoped namespace with using
        var code = """
            namespace Test;
            using System;
            using System.Collections;
            """;
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();

        // Act
        extractor.Visit(root);
        var imports = extractor.GetImports();

        // Assert
        Assert.Equal(2, imports.Count);
    }

    [Fact]
    public void GetImports_ReturnsReadOnlyList()
    {
        // Arrange
        var code = "using System; namespace Test { }";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = (Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax)tree.GetRoot();
        var extractor = new UsingStatementExtractor();
        extractor.Visit(root);

        // Act
        var imports = extractor.GetImports();

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<ImportedNamespace>>(imports);
    }
}
