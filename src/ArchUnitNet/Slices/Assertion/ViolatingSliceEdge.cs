using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Slices.Assertion;

/// <summary>
/// Violation when a slice dependency violates defined architectural slices.
/// Example: "Slice 'UI' should not depend on slice 'Model', but found dependency: UI/Component.cs → Model/Entity.cs"
/// </summary>
public record ViolatingSliceEdge(
    string SourceSlice,
    string TargetSlice,
    string SourcePath,
    string TargetPath,
    string Message
) : Violation
{
    public override string ToString() => Message;

    /// <summary>
    /// Create a violation for an unexpected slice dependency.
    /// </summary>
    public static ViolatingSliceEdge CreateUnexpectedDependency(
        string sourceSlice,
        string targetSlice,
        string sourcePath,
        string targetPath)
    {
        var message = $"Slice '{sourceSlice}' should not depend on slice '{targetSlice}', but found: {sourcePath} → {targetPath}";
        return new ViolatingSliceEdge(sourceSlice, targetSlice, sourcePath, targetPath, message);
    }

    /// <summary>
    /// Create a violation when a required dependency is missing.
    /// </summary>
    public static ViolatingSliceEdge CreateMissingDependency(
        string sourceSlice,
        string targetSlice)
    {
        var message = $"Slice '{sourceSlice}' should depend on slice '{targetSlice}', but no dependency found";
        return new ViolatingSliceEdge(sourceSlice, targetSlice, "", "", message);
    }

    /// <summary>
    /// Create a violation for a cycle between slices.
    /// </summary>
    public static ViolatingSliceEdge CreateCyclicSliceDependency(
        string slice1,
        string slice2,
        string path1,
        string path2)
    {
        var message = $"Cyclic slice dependency detected: '{slice1}' ↔ '{slice2}' (via {path1} ↔ {path2})";
        return new ViolatingSliceEdge(slice1, slice2, path1, path2, message);
    }
}
