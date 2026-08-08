using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Tests.Common.Extraction;

public class DependencyExtractorIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DependencyExtractor _extractor;

    public DependencyExtractorIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ArchUnitIntegration_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        DependencyExtractor.ClearCache();
        _extractor = new DependencyExtractor();
    }

    public void Dispose()
    {
        DependencyExtractor.ClearCache();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public async Task ExtractGraphAsync_WithSimpleProject_CreatesEdges()
    {
        // Arrange - create a simple 2-file project
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var fileAContent = """
            using System;
            using MyApp.Services;

            namespace MyApp.Controllers
            {
                public class UserController { }
            }
            """;

        var fileBContent = """
            using System;

            namespace MyApp.Services
            {
                public class UserService { }
            }
            """;

        CreateProjectStructure(csprojContent, new[] { ("src/Controllers/UserController.cs", fileAContent), ("src/Services/UserService.cs", fileBContent) });

        var csprojPath = Path.Combine(_tempDir, "Test.csproj");

        // Act
        var graph = await _extractor.ExtractGraphAsync(csprojPath);

        // Assert
        graph.Edges.Should().NotBeEmpty();

        // Should have edges from UserController to System and MyApp.Services
        var controllerEdges = graph.Edges
            .Where(e => e.Source.Contains("UserController"))
            .ToList();
        controllerEdges.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExtractGraphAsync_CorrectlyMarkExternalDependencies()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var csharpContent = """
            using System;
            using System.Collections.Generic;
            using MyApp.Services;

            namespace MyApp
            {
                public class App { }
            }
            """;

        CreateProjectStructure(csprojContent, new[] { ("src/App.cs", csharpContent) });
        var csprojPath = Path.Combine(_tempDir, "Test.csproj");

        // Act
        var graph = await _extractor.ExtractGraphAsync(csprojPath);

        // Assert
        var externalEdges = graph.Edges.Where(e => e.External).ToList();
        var internalEdges = graph.Edges.Where(e => !e.External).ToList();

        // System.* should be external
        externalEdges.Should().Contain(e => e.Target.StartsWith("System"));

        // MyApp.* should be internal
        internalEdges.Should().Contain(e => e.Target.StartsWith("MyApp"));
    }

    [Fact]
    public async Task ExtractGraphAsync_CapturesAllImportKinds()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var csharpContent = """
            using System;
            using static System.Console;
            using Collections = System.Collections.Generic;

            namespace Test { }
            """;

        CreateProjectStructure(csprojContent, new[] { ("src/Test.cs", csharpContent) });
        var csprojPath = Path.Combine(_tempDir, "Test.csproj");

        // Act
        var graph = await _extractor.ExtractGraphAsync(csprojPath);

        // Assert
        graph.Edges.Should().NotBeEmpty();
        var kinds = graph.Edges.SelectMany(e => e.ImportKinds).Distinct().ToList();
        kinds.Should().HaveCountGreaterThan(1); // Should have multiple import kinds
    }

    [Fact]
    public async Task ExtractGraphAsync_HandlesMultipleFiles()
    {
        // Arrange - create 5-file project
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        var files = new[]
        {
            ("src/A.cs", "using System; namespace Test { public class A { } }"),
            ("src/B.cs", "using System; namespace Test { public class B { } }"),
            ("src/C.cs", "using System; namespace Test { public class C { } }"),
            ("src/D.cs", "using System; namespace Test { public class D { } }"),
            ("src/E.cs", "using System; namespace Test { public class E { } }"),
        };

        CreateProjectStructure(csprojContent, files);
        var csprojPath = Path.Combine(_tempDir, "Test.csproj");

        // Act
        var graph = await _extractor.ExtractGraphAsync(csprojPath);

        // Assert
        var uniqueSources = graph.Edges.Select(e => e.Source).Distinct().Count();
        uniqueSources.Should().Be(5);
    }

    [Fact]
    public async Task ExtractGraphAsync_CachesResultsCorrectly()
    {
        // Arrange
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
            </Project>
            """;

        CreateProjectStructure(csprojContent, new[] { ("src/Test.cs", "using System; namespace Test { }") });
        var csprojPath = Path.Combine(_tempDir, "Test.csproj");

        // Act
        var graph1 = await _extractor.ExtractGraphAsync(csprojPath);
        var graph2 = await _extractor.ExtractGraphAsync(csprojPath);

        // Assert
        graph1.Should().Be(graph2); // Same object (cached)
    }

    private void CreateProjectStructure(string csprojContent, (string path, string content)[] files)
    {
        // Write .csproj
        var csprojPath = Path.Combine(_tempDir, "Test.csproj");
        File.WriteAllText(csprojPath, csprojContent);

        // Write C# files
        foreach (var (path, content) in files)
        {
            var fullPath = Path.Combine(_tempDir, path);
            var dir = Path.GetDirectoryName(fullPath);
            if (dir != null)
                Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, content);
        }
    }
}
