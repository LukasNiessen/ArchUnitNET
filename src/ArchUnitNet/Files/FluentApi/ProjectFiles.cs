using ArchUnitNet.Common.Extraction;

namespace ArchUnitNet.Files.FluentApi;

/// <summary>
/// Entry point for file-based architecture rules.
/// Example: ProjectFiles.From(graph).InPath("src/**").Should().DependOnFiles()...
/// </summary>
public static class ProjectFiles
{
    public static FileConditionBuilder From(Graph graph)
    {
        return new FileConditionBuilder(graph);
    }
}
