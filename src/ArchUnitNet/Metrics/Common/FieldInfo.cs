namespace ArchUnitNet.Metrics.Common;

/// <summary>
/// Information about a class field for cohesion analysis.
/// </summary>
public record FieldInfo(
    string Name,
    string Type,
    bool IsPublic = false
)
{
    public override string ToString() => Name;
}
