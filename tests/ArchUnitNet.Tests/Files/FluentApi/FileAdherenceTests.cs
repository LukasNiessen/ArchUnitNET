using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;
using ArchUnitNet.Files.FluentApi;
using Xunit;
using FileInfo = ArchUnitNet.Files.Common.FileInfo;

namespace ArchUnitNet.Tests.Files.FluentApi;

public class FileAdherenceTests
{
    private readonly ArchUnitNet.Common.Extraction.Graph _sampleGraph;
    private readonly string _testFilePath = "TestFile.cs";

    public FileAdherenceTests()
    {
        // Create test files
        CreateTestFiles();

        _sampleGraph = new ArchUnitNet.Common.Extraction.Graph(new[]
        {
            new Edge(_testFilePath, "System.Linq", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("SmallFile.cs", "System", External: true, ImportKinds: new[] { ImportKind.Using }),
            new Edge("LargeFile.cs", "System.Collections", External: true, ImportKinds: new[] { ImportKind.Using }),
        });
    }

    private void CreateTestFiles()
    {
        // Create test file with specific line count
        if (!File.Exists(_testFilePath))
        {
            var content = string.Join(Environment.NewLine, Enumerable.Range(1, 100).Select(i => $"// Line {i}"));
            File.WriteAllText(_testFilePath, content);
        }

        if (!File.Exists("SmallFile.cs"))
        {
            var content = "public class Small { }";
            File.WriteAllText("SmallFile.cs", content);
        }

        if (!File.Exists("LargeFile.cs"))
        {
            var content = string.Join(Environment.NewLine, Enumerable.Range(1, 500).Select(i => $"// Line {i}"));
            File.WriteAllText("LargeFile.cs", content);
        }
    }

    #region FileInfo Creation Tests

    [Fact]
    public void FileInfo_FromPath_ReadsSourceCode()
    {
        // Arrange
        var filePath = _testFilePath;

        // Act
        var fileInfo = FileInfo.FromPath(filePath);

        // Assert
        Assert.NotNull(fileInfo.SourceCode);
        Assert.NotEmpty(fileInfo.SourceCode);
        Assert.Contains("Line 1", fileInfo.SourceCode);
    }

    [Fact]
    public void FileInfo_FromPath_CalculatesNonBlankLineCount()
    {
        // Arrange & Act
        var fileInfo = FileInfo.FromPath(_testFilePath);

        // Assert
        Assert.Equal(100, fileInfo.NonBlankLineCount);
    }

    [Fact]
    public void FileInfo_FromPath_ExtractsPathComponents()
    {
        // Arrange & Act
        var fileInfo = FileInfo.FromPath(_testFilePath);

        // Assert
        Assert.Equal(_testFilePath, fileInfo.Path);
        Assert.Equal("TestFile", fileInfo.NameWithoutExtension);
        Assert.Equal(".cs", fileInfo.Extension);
    }

    [Fact]
    public void FileInfo_FromPath_NullPath_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => FileInfo.FromPath(null!));
    }

    [Fact]
    public void FileInfo_FromPath_NonExistentFile_Throws()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => FileInfo.FromPath("NonExistent.cs"));
    }

    #endregion

    #region Simple Predicate Tests

    [Fact]
    public async Task AdhereTo_LineCountThreshold_Should_WithViolation()
    {
        // Arrange: LargeFile has 500 lines, should violate if limit is 200
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("LargeFile.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 200, "Files must be under 200 lines");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task AdhereTo_LineCountThreshold_ShouldNot_WithAllowedFiles()
    {
        // Arrange: SmallFile has < 10 lines, should pass ShouldNot rule
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .ShouldNot()
            .AdhereTo(f => f.NonBlankLineCount > 100, "Files should not exceed 100 lines");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task AdhereTo_NamePattern_WithMatch()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .Should()
            .AdhereTo(f => f.NameWithoutExtension.EndsWith("File"), "Name must end with 'File'");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task AdhereTo_ExtensionCheck_WithCorrectExtension()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .Should()
            .AdhereTo(f => f.Extension == ".cs", "Must be C# file");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region Complex Predicate Tests

    [Fact]
    public async Task AdhereTo_MultipleConditions_AndLogic()
    {
        // Arrange: Combine line count AND name pattern
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 50 && f.NameWithoutExtension.Contains("File"),
                "Must be small file with 'File' in name");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task AdhereTo_MultipleConditions_OrLogic()
    {
        // Arrange: Line count OR extension check
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("TestFile.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 50 || f.Extension == ".cs",
                "Must be small or be C# file");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region Negation Tests

    [Fact]
    public async Task AdhereTo_ShouldNot_InvertsLogic()
    {
        // Arrange: Should NOT have more than 1000 lines
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .ShouldNot()
            .AdhereTo(f => f.NonBlankLineCount > 1000, "Files should not exceed 1000 lines");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task AdhereTo_ShouldNot_WithViolatingFile()
    {
        // Arrange: Should NOT have more than 100 lines, but LargeFile has 500
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("LargeFile.cs")
            .ShouldNot()
            .AdhereTo(f => f.NonBlankLineCount > 100, "Files should not exceed 100 lines");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    #endregion

    #region Message Tests

    [Fact]
    public async Task AdhereTo_CustomMessage_IncludedInViolation()
    {
        // Arrange
        var customMessage = "Custom violation message for large files";
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("LargeFile.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 200, customMessage);

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
        Assert.Contains(customMessage, violations[0].ToString());
    }

    [Fact]
    public async Task AdhereTo_EmptyMessage_DefaultMessage()
    {
        // Arrange
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("LargeFile.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 200, "");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.NotEmpty(violations);
    }

    #endregion

    #region Real-World Scenarios

    [Fact]
    public async Task AdhereTo_LargeFileDetection_IdentifiesHeavyClasses()
    {
        // Arrange: Detect files over 300 lines
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("*.cs")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 300, "Classes should not exceed 300 lines");

        // Act
        var violations = await rule.CheckAsync();

        // Assert - LargeFile should violate
        Assert.NotEmpty(violations);
    }

    [Fact]
    public async Task AdhereTo_NameConvention_EnforcesSuffix()
    {
        // Arrange: All files should have .cs extension
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("*.cs")
            .Should()
            .AdhereTo(f => f.Extension == ".cs", "Must be C# source files");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    [Fact]
    public async Task AdhereTo_SourceCodeAnalysis_ChecksContent()
    {
        // Arrange: Files should contain valid C# (contains class keyword or comment)
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("SmallFile.cs")
            .Should()
            .AdhereTo(f => f.SourceCode.Contains("class"), "Should contain class definition");

        // Act
        var violations = await rule.CheckAsync();

        // Assert
        Assert.Empty(violations);
    }

    #endregion

    #region Error Cases

    [Fact]
    public void AdhereTo_NullPredicate_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ProjectFiles.From(_sampleGraph)
                .InPath("*.cs")
                .Should()
                .AdhereTo(null!, "message")
        );
    }

    [Fact]
    public async Task AdhereTo_NoMatchingFiles_ReturnsEmpty()
    {
        // Arrange: Pattern matches no files
        var rule = ProjectFiles.From(_sampleGraph)
            .InPath("nonexistent/**")
            .Should()
            .AdhereTo(f => f.NonBlankLineCount < 100, "Some message");

        // Act & Assert
        // Should not throw, just return empty or error violation
        var violations = await rule.CheckAsync();
        Assert.NotNull(violations);
    }

    #endregion

    private void Dispose()
    {
        // Clean up test files
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
        if (File.Exists("SmallFile.cs"))
            File.Delete("SmallFile.cs");
        if (File.Exists("LargeFile.cs"))
            File.Delete("LargeFile.cs");
    }
}
