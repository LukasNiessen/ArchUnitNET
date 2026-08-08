using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using Microsoft.CodeAnalysis.CSharp;

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
        imports.Should().HaveCount(1);
        imports.First().Name.Should().Be("System");
        imports.First().Kind.Should().Be(ImportKind.Using);
        imports.First().IsGlobal.Should().BeFalse();
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
        imports.Should().HaveCount(3);
        imports.Should().Contain(i => i.Name == "System");
        imports.Should().Contain(i => i.Name == "System.Collections.Generic");
        imports.Should().Contain(i => i.Name == "System.Linq");
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
        imports.Should().HaveCount(1);
        imports.First().Name.Should().Be("System.Console");
        imports.First().Kind.Should().Be(ImportKind.StaticUsing);
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
        imports.Should().HaveCount(1);
        imports.First().Name.Should().Be("System");
        imports.First().Kind.Should().Be(ImportKind.GlobalUsing);
        imports.First().IsGlobal.Should().BeTrue();
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
        imports.Should().HaveCount(1);
        imports.First().Name.Should().Be("System.String");
        imports.First().Kind.Should().Be(ImportKind.AliasUsing);
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
        imports.Should().HaveCount(4);
        imports.Should().Contain(i => i.Kind == ImportKind.Using);
        imports.Should().Contain(i => i.Kind == ImportKind.StaticUsing);
        imports.Should().Contain(i => i.Kind == ImportKind.AliasUsing);
        imports.Should().Contain(i => i.Kind == ImportKind.GlobalUsing);
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
        imports.Should().BeEmpty();
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
        imports.Should().BeEmpty();
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
        imports.Should().HaveCount(2);
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
        imports.Should().BeAssignableTo<IReadOnlyList<ImportedNamespace>>();
    }
}
