namespace ArchUnitNet.Slices.Common;

/// <summary>
/// Represents a logical architectural slice extracted from a file path pattern.
/// Example: Pattern "src/{Slice}/**" extracts slice "Feature" from "src/Feature/Component.cs"
/// </summary>
public record Slice(
    string Name,
    IReadOnlyList<string> Files
)
{
    /// <summary>
    /// Get all files in this slice.
    /// </summary>
    public IReadOnlyList<string> GetFiles() => Files;

    /// <summary>
    /// Get the number of files in this slice.
    /// </summary>
    public int FileCount => Files.Count;

    public override string ToString() => $"Slice(Name={Name}, FileCount={FileCount})";
}

/// <summary>
/// Represents a dependency between two slices.
/// </summary>
public record SliceDependency(
    string SourceSlice,
    string TargetSlice,
    string SourceFile,
    string TargetFile
)
{
    public override string ToString() => $"{SourceSlice} → {TargetSlice} ({SourceFile} → {TargetFile})";
}

/// <summary>
/// Represents the complete slice architecture for a project.
/// </summary>
public record SliceArchitecture(
    IReadOnlyDictionary<string, Slice> Slices,
    IReadOnlyList<SliceDependency> Dependencies
)
{
    /// <summary>
    /// Get a slice by name, or null if not found.
    /// </summary>
    public Slice? GetSlice(string sliceName) => Slices.TryGetValue(sliceName, out var slice) ? slice : null;

    /// <summary>
    /// Get all dependencies from a specific slice.
    /// </summary>
    public IReadOnlyList<SliceDependency> GetDependenciesFrom(string sliceName) =>
        Dependencies.Where(d => d.SourceSlice == sliceName).ToList().AsReadOnly();

    /// <summary>
    /// Get all dependencies to a specific slice.
    /// </summary>
    public IReadOnlyList<SliceDependency> GetDependenciesTo(string sliceName) =>
        Dependencies.Where(d => d.TargetSlice == sliceName).ToList().AsReadOnly();

    /// <summary>
    /// Get the number of slices.
    /// </summary>
    public int SliceCount => Slices.Count;
}
