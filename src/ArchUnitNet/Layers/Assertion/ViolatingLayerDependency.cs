using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Layers.Assertion;

/// <summary>
/// Violation when a layer dependency violates defined layer constraints.
/// Example: "Layer 'Presentation' cannot depend on layer 'Data' (via Presentation/Dashboard.cs → Data/Repository.cs)"
/// </summary>
public record ViolatingLayerDependency(
    string SourceLayer,
    string TargetLayer,
    string SourceFile,
    string TargetFile,
    string Reason,
    string Message
) : Violation
{
    public override string ToString() => Message;

    /// <summary>
    /// Create a violation for a forbidden layer dependency.
    /// </summary>
    public static ViolatingLayerDependency CreateForbiddenDependency(
        string sourceLayer,
        string targetLayer,
        string sourceFile,
        string targetFile)
    {
        var message = $"Layer '{sourceLayer}' cannot depend on layer '{targetLayer}' (via {sourceFile} → {targetFile})";
        return new ViolatingLayerDependency(sourceLayer, targetLayer, sourceFile, targetFile, "Forbidden", message);
    }

    /// <summary>
    /// Create a violation when a sealed layer has external dependencies.
    /// </summary>
    public static ViolatingLayerDependency CreateSealedLayerViolation(
        string sourceLayer,
        string targetLayer,
        string sourceFile,
        string targetFile)
    {
        var message = $"Sealed layer '{sourceLayer}' cannot depend on external layer '{targetLayer}' (via {sourceFile} → {targetFile})";
        return new ViolatingLayerDependency(sourceLayer, targetLayer, sourceFile, targetFile, "Sealed", message);
    }

    /// <summary>
    /// Create a violation when a layer depends on a layer not in its allowlist.
    /// </summary>
    public static ViolatingLayerDependency CreateNotAllowedDependency(
        string sourceLayer,
        string targetLayer,
        string sourceFile,
        string targetFile,
        string[] allowedLayers)
    {
        var allowed = string.Join(", ", allowedLayers.Select(l => $"'{l}'"));
        var message = $"Layer '{sourceLayer}' may only depend on {allowed}, but found dependency on '{targetLayer}' (via {sourceFile} → {targetFile})";
        return new ViolatingLayerDependency(sourceLayer, targetLayer, sourceFile, targetFile, "NotAllowed", message);
    }

    /// <summary>
    /// Create a violation for a cycle between layers.
    /// </summary>
    public static ViolatingLayerDependency CreateCyclicLayerDependency(
        string layer1,
        string layer2,
        string file1,
        string file2)
    {
        var message = $"Cyclic layer dependency detected: '{layer1}' ↔ '{layer2}' (via {file1} ↔ {file2})";
        return new ViolatingLayerDependency(layer1, layer2, file1, file2, "Cyclic", message);
    }

    /// <summary>
    /// Create a violation when no layers match the pattern (empty-test guard).
    /// </summary>
    public static ViolatingLayerDependency CreateEmptyTestViolation(string pattern)
    {
        var message = $"No layers matched the pattern '{pattern}' - this is likely a typo. " +
                      "If intentional, use CheckOptions with AllowEmptyTests = true";
        return new ViolatingLayerDependency("", "", "", "", "EmptyTest", message);
    }
}
