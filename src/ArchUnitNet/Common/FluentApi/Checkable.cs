using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Common.FluentApi;

/// <summary>
/// Core interface that every architectural rule implements.
/// A Checkable represents a built rule that can be executed against a codebase.
///
/// Key principle: Building a rule does no work. Only CheckAsync() touches the filesystem.
///
/// In ArchUnitTS: interface Checkable { check(options?): Promise&lt;Violation[]&gt; }
/// </summary>
public interface Checkable
{
    /// <summary>
    /// Execute the rule against the codebase and return violations.
    /// This is the only method that performs filesystem access and analysis.
    ///
    /// Returns an empty list if the rule passed (no violations).
    /// Returns a list of violations if the rule failed.
    ///
    /// A failing rule is NOT an exception — it's a list of violations that the test framework handles.
    /// Only TechnicalError or UserError are thrown; violations are returned as data.
    /// </summary>
    /// <param name="options">Optional configuration for this check run</param>
    /// <returns>List of violations (empty if rule passes)</returns>
    Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null);
}
