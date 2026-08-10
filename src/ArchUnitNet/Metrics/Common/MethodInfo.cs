namespace ArchUnitNet.Metrics.Common;

/// <summary>
/// Information about a class method for cohesion analysis.
/// Tracks which fields this method accesses.
/// </summary>
public record MethodInfo(
    string Name,
    IReadOnlySet<string> AccessedFields,
    int Complexity = 1
)
{
    public override string ToString() => Name;

    /// <summary>
    /// Check if this method accesses a specific field.
    /// </summary>
    public bool AccessesField(string fieldName) => AccessedFields.Contains(fieldName);

    /// <summary>
    /// Get the number of fields accessed by this method.
    /// </summary>
    public int FieldAccessCount => AccessedFields.Count;
}
