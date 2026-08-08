using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Extraction;

public class DependencyExtractorTests
{
    private readonly DependencyExtractor _extractor;

    public DependencyExtractorTests()
    {
        // Clear cache before each test
        DependencyExtractor.ClearCache();
        _extractor = new DependencyExtractor();
    }

    [Fact]
    public async Task ExtractGraphAsync_ReturnsGraph()
    {
        // Arrange - use current project directory (which has .csproj)
        var projectPath = FindTestProjectPath();

        // Act
        var graph = await _extractor.ExtractGraphAsync(projectPath);

        // Assert
        graph.Should().NotBeNull();
        graph.Edges.Should().BeAssignableTo<IReadOnlyList<Edge>>();
    }

    [Fact]
    public async Task ExtractGraphAsync_WithNullPath_AutoDiscoversCsproj()
    {
        // Arrange - change to project directory
        var originalDir = Environment.CurrentDirectory;
        var projectDir = FindProjectDirectory();

        try
        {
            Environment.CurrentDirectory = projectDir;

            // Act
            var graph = await _extractor.ExtractGraphAsync(null);

            // Assert
            graph.Should().NotBeNull();
        }
        finally
        {
            Environment.CurrentDirectory = originalDir;
        }
    }

    [Fact]
    public async Task ExtractGraphAsync_CachesResults()
    {
        // Arrange
        var projectPath = FindTestProjectPath();

        // Act
        var graph1 = await _extractor.ExtractGraphAsync(projectPath);
        var graph2 = await _extractor.ExtractGraphAsync(projectPath);

        // Assert - should be same object (cached)
        graph1.Should().Be(graph2);
    }

    [Fact]
    public void ClearCache_RemovesAllCachedGraphs()
    {
        // Arrange
        DependencyExtractor.ClearCache();

        // Act
        DependencyExtractor.ClearCache();

        // Assert - should not throw
    }

    [Fact]
    public async Task ExtractGraphAsync_WithInvalidPath_Throws()
    {
        // Arrange
        var invalidPath = "/this/path/does/not/exist.csproj";

        // Act & Assert
        var action = async () => await _extractor.ExtractGraphAsync(invalidPath);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ExtractGraphAsync_NormalizesProjectPath()
    {
        // Arrange
        var projectPath = FindTestProjectPath();
        var unnormalizedPath = projectPath.Replace("/", "\\");

        // Act
        var graph = await _extractor.ExtractGraphAsync(unnormalizedPath);

        // Assert
        graph.Should().NotBeNull();
    }

    private static string FindTestProjectPath()
    {
        // Find the ArchUnitNet.csproj
        var current = AppContext.BaseDirectory;
        while (current != null)
        {
            var csproj = Directory.GetFiles(current, "ArchUnitNet.csproj", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (csproj != null)
                return csproj;

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException("Could not find ArchUnitNet.csproj");
    }

    private static string FindProjectDirectory()
    {
        var csproj = FindTestProjectPath();
        return Path.GetDirectoryName(csproj)!;
    }
}

/// <summary>
/// Mock logger for testing logging behavior.
/// </summary>
public class MockExtractorLogger : IDependencyExtractorLogger
{
    public List<string> LoggedMessages { get; } = new();

    public void LogExtractionStart(string projectPath)
    {
        LoggedMessages.Add($"START: {projectPath}");
    }

    public void LogExtractionComplete(string projectPath, int edgeCount)
    {
        LoggedMessages.Add($"COMPLETE: {projectPath} ({edgeCount} edges)");
    }

    public void LogExtractionError(string projectPath, Exception ex)
    {
        LoggedMessages.Add($"ERROR: {projectPath} - {ex.Message}");
    }

    public void LogCacheHit(string projectPath)
    {
        LoggedMessages.Add($"CACHE_HIT: {projectPath}");
    }
}

public class DependencyExtractorLoggerTests
{
    [Fact]
    public async Task ExtractorLogsExtractionEvents()
    {
        // Arrange
        DependencyExtractor.ClearCache();
        var logger = new MockExtractorLogger();
        var extractor = new DependencyExtractor(logger);
        var projectPath = FindTestProjectPath();

        // Act
        await extractor.ExtractGraphAsync(projectPath);

        // Assert
        logger.LoggedMessages.Should().Contain(msg => msg.StartsWith("START:"));
        logger.LoggedMessages.Should().Contain(msg => msg.StartsWith("COMPLETE:"));
    }

    [Fact]
    public async Task ExtractorLogsCacheHits()
    {
        // Arrange
        DependencyExtractor.ClearCache();
        var logger = new MockExtractorLogger();
        var extractor = new DependencyExtractor(logger);
        var projectPath = FindTestProjectPath();

        // Act
        await extractor.ExtractGraphAsync(projectPath);
        logger.LoggedMessages.Clear();

        await extractor.ExtractGraphAsync(projectPath);

        // Assert
        logger.LoggedMessages.Should().Contain(msg => msg.StartsWith("CACHE_HIT:"));
    }

    private static string FindTestProjectPath()
    {
        var current = AppContext.BaseDirectory;
        while (current != null)
        {
            var csproj = Directory.GetFiles(current, "ArchUnitNet.csproj", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (csproj != null)
                return csproj;

            current = Directory.GetParent(current)?.FullName;
        }

        throw new InvalidOperationException("Could not find ArchUnitNet.csproj");
    }
}
