using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArchUnitNet.Metrics.Extraction;
using Xunit;

namespace ArchUnitNet.Tests.Metrics.Extraction;

public class ClassInfoExtractorTests
{
    [Fact]
    public void Extract_SimpleClass_ExtractsFieldsAndMethods()
    {
        // Arrange
        var source = "public class SimpleClass { private int x; public void Method() { } }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var extractor = new ClassInfoExtractor(classDecl);

        // Act
        var classInfo = extractor.Extract();

        // Assert
        Assert.Equal("SimpleClass", classInfo.Name);
        Assert.Equal(1, classInfo.FieldCount);
        Assert.Equal(1, classInfo.MethodCount);
    }

    [Fact]
    public void Extract_WithFieldAccess_DetectsUsage()
    {
        // Arrange
        var source = "public class TestClass { private int value; public void Set() { value = 5; } }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var extractor = new ClassInfoExtractor(classDecl);

        // Act
        var classInfo = extractor.Extract();

        // Assert
        Assert.NotNull(classInfo.GetField("value"));
        var method = classInfo.GetMethod("Set");
        Assert.NotNull(method);
        Assert.True(method!.AccessesField("value"));
    }

    [Fact]
    public void Extract_EmptyClass_HandlesCorrectly()
    {
        // Arrange
        var source = "public class EmptyClass { }";
        var tree = CSharpSyntaxTree.ParseText(source);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var extractor = new ClassInfoExtractor(classDecl);

        // Act
        var classInfo = extractor.Extract();

        // Assert
        Assert.Equal("EmptyClass", classInfo.Name);
        Assert.Empty(classInfo.Fields);
        Assert.Empty(classInfo.Methods);
    }
}

public class ClassInfoBatchExtractorTests
{
    [Fact]
    public void ExtractFromSource_SingleClass_Succeeds()
    {
        // Arrange
        var source = "public class TestClass { private int x; }";
        var extractor = new ClassInfoBatchExtractor();

        // Act
        extractor.ExtractFromSource(source);

        // Assert
        Assert.Single(extractor.GetExtractedClasses());
        Assert.Equal("TestClass", extractor.GetExtractedClasses()[0].Name);
    }

    [Fact]
    public void ExtractFromSource_MultipleClasses_ExtractsAll()
    {
        // Arrange
        var source = "public class Class1 { } public class Class2 { } public class Class3 { }";
        var extractor = new ClassInfoBatchExtractor();

        // Act
        extractor.ExtractFromSource(source);

        // Assert
        Assert.Equal(3, extractor.GetExtractedClasses().Count);
    }

    [Fact]
    public void GetClass_WithValidName_ReturnsClass()
    {
        // Arrange
        var source = "public class MyClass { }";
        var extractor = new ClassInfoBatchExtractor();
        extractor.ExtractFromSource(source);

        // Act
        var classInfo = extractor.GetClass("MyClass");

        // Assert
        Assert.NotNull(classInfo);
        Assert.Equal("MyClass", classInfo!.Name);
    }

    [Fact]
    public void GetClass_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var source = "public class MyClass { }";
        var extractor = new ClassInfoBatchExtractor();
        extractor.ExtractFromSource(source);

        // Act
        var classInfo = extractor.GetClass("NonExistent");

        // Assert
        Assert.Null(classInfo);
    }

    [Fact]
    public void GetSummary_ReturnsCorrectStatistics()
    {
        // Arrange
        var source = "public class C1 { private int x; public void M1() { } } public class C2 { private string s; }";
        var extractor = new ClassInfoBatchExtractor();
        extractor.ExtractFromSource(source);

        // Act
        var summary = extractor.GetSummary();

        // Assert
        Assert.Equal(2, summary.ClassCount);
        Assert.Equal(1, summary.TotalMethods);
        Assert.Equal(2, summary.TotalFields);
    }

    [Fact]
    public void Clear_RemovesAllData()
    {
        // Arrange
        var source = "public class MyClass { }";
        var extractor = new ClassInfoBatchExtractor();
        extractor.ExtractFromSource(source);
        Assert.Single(extractor.GetExtractedClasses());

        // Act
        extractor.Clear();

        // Assert
        Assert.Empty(extractor.GetExtractedClasses());
    }
}
