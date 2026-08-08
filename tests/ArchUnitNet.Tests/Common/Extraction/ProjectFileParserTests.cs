using ArchUnitNet.Common.Error;
using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Tests.Common.Extraction;

public class ProjectFileParserTests : IDisposable
{
    private readonly string _tempDir;

    public ProjectFileParserTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ArchUnitTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void FindSourceFiles_WithValidCsproj_ReturnsSourceFiles()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <Compile Include="src/Common/Error.cs" />
                <Compile Include="src/Common/Util/PathNormalizer.cs" />
              </ItemGroup>
            </Project>
            """;

        var csprojPath = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Create the referenced files so they exist
        Directory.CreateDirectory(Path.Combine(_tempDir, "src", "Common", "Util"));
        File.WriteAllText(Path.Combine(_tempDir, "src", "Common", "Error.cs"), "");
        File.WriteAllText(Path.Combine(_tempDir, "src", "Common", "Util", "PathNormalizer.cs"), "");

        // Act
        var sourceFiles = ProjectFileParser.FindSourceFiles(csprojPath);

        // Assert
        sourceFiles.Should().HaveCount(2);
        sourceFiles.Should().Contain("src/Common/Error.cs");
        sourceFiles.Should().Contain("src/Common/Util/PathNormalizer.cs");
    }

    [Fact]
    public void FindSourceFiles_WithMissingFile_Throws()
    {
        // Arrange
        var csprojPath = Path.Combine(_tempDir, "NonExistent.csproj");

        // Act & Assert
        var action = () => ProjectFileParser.FindSourceFiles(csprojPath);
        action.Should().Throw<UserError>().WithMessage("*not found*");
    }

    [Fact]
    public void FindSourceFiles_WithNonCsprojFile_Throws()
    {
        // Arrange
        var txtPath = Path.Combine(_tempDir, "test.txt");
        File.WriteAllText(txtPath, "not a project file");

        // Act & Assert
        var action = () => ProjectFileParser.FindSourceFiles(txtPath);
        action.Should().Throw<UserError>().WithMessage("*.csproj*");
    }

    [Fact]
    public void FindSourceFiles_WithDirectoryScan_FindsAllCsFiles()
    {
        // Arrange - create project structure without explicit Compile items
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var csprojPath = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Create .cs files directly
        Directory.CreateDirectory(Path.Combine(_tempDir, "src"));
        File.WriteAllText(Path.Combine(_tempDir, "src", "File1.cs"), "");
        File.WriteAllText(Path.Combine(_tempDir, "src", "File2.cs"), "");

        // Act
        var sourceFiles = ProjectFileParser.FindSourceFiles(csprojPath);

        // Assert
        sourceFiles.Should().HaveCount(2);
        sourceFiles.Should().Contain(f => f.Contains("File1.cs"));
        sourceFiles.Should().Contain(f => f.Contains("File2.cs"));
    }

    [Fact]
    public void FindSourceFiles_WithNestedDirectories_FindsAllFiles()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var csprojPath = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Create nested structure
        Directory.CreateDirectory(Path.Combine(_tempDir, "src", "Common"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "src", "Files"));
        Directory.CreateDirectory(Path.Combine(_tempDir, "tests"));

        File.WriteAllText(Path.Combine(_tempDir, "src", "Common", "Error.cs"), "");
        File.WriteAllText(Path.Combine(_tempDir, "src", "Files", "FluentApi.cs"), "");
        File.WriteAllText(Path.Combine(_tempDir, "tests", "Tests.cs"), "");

        // Act
        var sourceFiles = ProjectFileParser.FindSourceFiles(csprojPath);

        // Assert
        sourceFiles.Should().HaveCount(3);
        sourceFiles.Should().Contain(f => f.Contains("Common"));
        sourceFiles.Should().Contain(f => f.Contains("Files"));
        sourceFiles.Should().Contain(f => f.Contains("tests"));
    }

    [Fact]
    public void FindSourceFiles_NormalizesPathsCorrectly()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <Compile Include="src\Common\Error.cs" />
              </ItemGroup>
            </Project>
            """;

        var csprojPath = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        Directory.CreateDirectory(Path.Combine(_tempDir, "src", "Common"));
        File.WriteAllText(Path.Combine(_tempDir, "src", "Common", "Error.cs"), "");

        // Act
        var sourceFiles = ProjectFileParser.FindSourceFiles(csprojPath);

        // Assert
        sourceFiles.Should().HaveCount(1);
        sourceFiles.First().Should().Be("src/Common/Error.cs"); // Forward slashes
    }

    [Fact]
    public void FindSourceFiles_WithInvalidXml_Throws()
    {
        // Arrange
        var csprojPath = Path.Combine(_tempDir, "Bad.csproj");
        File.WriteAllText(csprojPath, "not valid xml");

        // Act & Assert
        var action = () => ProjectFileParser.FindSourceFiles(csprojPath);
        action.Should().Throw<TechnicalError>();
    }
}
