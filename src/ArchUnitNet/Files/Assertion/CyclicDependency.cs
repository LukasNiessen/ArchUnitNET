using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Files.Assertion;

/// <summary>
/// Violation when files have circular/cyclic dependencies.
/// Example: "A.cs → B.cs → C.cs → A.cs (cycle detected)"
/// </summary>
public record CyclicDependency(
    IReadOnlyList<string> Cycle,
    string Message
) : Violation
{
    public override string ToString() => Message;

    public static CyclicDependency Create(IReadOnlyList<string> cycle)
    {
        var cycleStr = string.Join(" → ", cycle) + " → " + cycle[0];
        var message = $"Cyclic dependency detected: {cycleStr}";
        return new CyclicDependency(cycle, message);
    }
}
