namespace ArchUnitNet.Common.Assertion;

/// <summary>
/// Base marker interface for all violations.
/// Every rule violation (broken architecture rule) is a subclass of this.
///
/// Important distinction:
/// - Violation: architecture rule failed (returned as data in IReadOnlyList, not thrown)
/// - TechnicalError: library bug or environment problem (thrown)
/// - UserError: API misuse (thrown)
///
/// In ArchUnitTS: interface Violation { }
/// </summary>
public interface Violation
{
    // Marker interface — subclasses add specific violation data
}
