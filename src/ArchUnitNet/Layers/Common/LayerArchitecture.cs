namespace ArchUnitNet.Layers.Common;

/// <summary>
/// Represents the complete layer architecture for a project.
/// Contains layer definitions, inter-layer dependencies, and file-to-layer mappings.
/// </summary>
public record LayerArchitecture(
    IReadOnlyDictionary<string, Layer> Layers,
    IReadOnlyList<LayerDependency> Dependencies,
    IReadOnlyDictionary<string, IReadOnlyList<string>> LayerFiles
)
{
    /// <summary>
    /// Get a layer by name, or null if not found.
    /// </summary>
    public Layer? GetLayer(string layerName) =>
        Layers.TryGetValue(layerName, out var layer) ? layer : null;

    /// <summary>
    /// Get all files in a specific layer.
    /// </summary>
    public IReadOnlyList<string> GetFilesInLayer(string layerName) =>
        LayerFiles.TryGetValue(layerName, out var files) ? files : new List<string>().AsReadOnly();

    /// <summary>
    /// Determine which layer a file belongs to, or null if not in any layer.
    /// </summary>
    public string? GetLayerForFile(string filePath) =>
        LayerFiles.FirstOrDefault(kvp => kvp.Value.Contains(filePath)).Key;

    /// <summary>
    /// Get all dependencies originating from a specific layer.
    /// </summary>
    public IReadOnlyList<LayerDependency> GetDependenciesFrom(string layerName) =>
        Dependencies.Where(d => d.SourceLayer == layerName).ToList().AsReadOnly();

    /// <summary>
    /// Get all dependencies targeting a specific layer.
    /// </summary>
    public IReadOnlyList<LayerDependency> GetDependenciesTo(string layerName) =>
        Dependencies.Where(d => d.TargetLayer == layerName).ToList().AsReadOnly();

    /// <summary>
    /// Get the number of layers.
    /// </summary>
    public int LayerCount => Layers.Count;

    /// <summary>
    /// Get the number of inter-layer dependencies.
    /// </summary>
    public int DependencyCount => Dependencies.Count;

    public override string ToString() => $"LayerArchitecture(Layers={LayerCount}, Dependencies={DependencyCount})";
}
