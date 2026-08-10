namespace ArchUnitNet.Metrics.Common;

/// <summary>
/// Information about a class structure for cohesion metrics analysis.
/// Includes fields, methods, and field access patterns.
/// </summary>
public class ClassInfo
{
    private readonly IReadOnlyList<FieldInfo> _fields;
    private readonly IReadOnlyList<MethodInfo> _methods;

    public ClassInfo(
        string name,
        IEnumerable<FieldInfo>? fields = null,
        IEnumerable<MethodInfo>? methods = null
    )
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _fields = (fields as IReadOnlyList<FieldInfo>) ?? (fields?.ToList() ?? new List<FieldInfo>()).AsReadOnly();
        _methods = (methods as IReadOnlyList<MethodInfo>) ?? (methods?.ToList() ?? new List<MethodInfo>()).AsReadOnly();
    }

    public string Name { get; }

    /// <summary>
    /// All fields in the class.
    /// </summary>
    public IReadOnlyList<FieldInfo> Fields => _fields;

    /// <summary>
    /// All methods in the class.
    /// </summary>
    public IReadOnlyList<MethodInfo> Methods => _methods;

    /// <summary>
    /// Total number of fields (for LCOM calculations).
    /// </summary>
    public int FieldCount => _fields.Count;

    /// <summary>
    /// Total number of methods (for LCOM calculations).
    /// </summary>
    public int MethodCount => _methods.Count;

    /// <summary>
    /// Number of methods that don't access any fields.
    /// These are "isolates" in cohesion analysis.
    /// </summary>
    public int IsolatedMethodCount => _methods.Count(m => m.FieldAccessCount == 0);

    /// <summary>
    /// Get a field by name, or null if not found.
    /// </summary>
    public FieldInfo? GetField(string fieldName) => _fields.FirstOrDefault(f => f.Name == fieldName);

    /// <summary>
    /// Get a method by name, or null if not found.
    /// </summary>
    public MethodInfo? GetMethod(string methodName) => _methods.FirstOrDefault(m => m.Name == methodName);

    /// <summary>
    /// Build a field access matrix: true if method i accesses field j.
    /// Useful for LCOM calculations.
    /// </summary>
    public bool[,] BuildFieldAccessMatrix()
    {
        var matrix = new bool[_methods.Count, _fields.Count];

        for (int i = 0; i < _methods.Count; i++)
        {
            for (int j = 0; j < _fields.Count; j++)
            {
                matrix[i, j] = _methods[i].AccessesField(_fields[j].Name);
            }
        }

        return matrix;
    }

    public override string ToString() => Name;
}
