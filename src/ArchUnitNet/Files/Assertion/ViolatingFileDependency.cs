using ArchUnitNet.Common.Assertion;
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Files.Assertion;

/// <summary>
/// Violation when a file depends on something it shouldn't.
/// Example: "Dashboard.cs depends on OrderRepository.cs (forbidden)"
/// </summary>
public record ViolatingFileDependency(
    string Source,
    string Target,
    ImportKind ImportKind,
    string Message
) : Violation
{
    public override string ToString() => Message;

    public static ViolatingFileDependency Create(Edge edge, string reason)
    {
        var message = $"{edge.Source} depends on {edge.Target} ({reason})";
        var kind = edge.ImportKinds.First(); // For simplicity, use first kind
        return new ViolatingFileDependency(edge.Source, edge.Target, kind, message);
    }
}
