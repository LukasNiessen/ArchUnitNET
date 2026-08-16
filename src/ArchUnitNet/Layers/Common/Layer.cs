namespace ArchUnitNet.Layers.Common;

/// <summary>
/// Represents a named architectural layer in a layered architecture.
/// Example: "Presentation", "Business", "Data" layer in a 3-layer architecture.
/// </summary>
public record Layer(string Name)
{
    /// <summary>
    /// Create a layer with the given name.
    /// Factory method for use in fluent API.
    /// </summary>
    public static Layer Defined(string name) => new(name);

    public override string ToString() => Name;
}

/// <summary>
/// Represents a dependency between two layers.
/// Includes source/target files to provide violation context.
/// </summary>
public record LayerDependency(
    string SourceLayer,
    string TargetLayer,
    string SourceFile,
    string TargetFile
)
{
    public override string ToString() => $"{SourceLayer} → {TargetLayer} ({SourceFile} → {TargetFile})";
}
