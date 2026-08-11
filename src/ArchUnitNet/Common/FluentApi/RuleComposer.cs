using ArchUnitNet.Common.Assertion;

namespace ArchUnitNet.Common.FluentApi;

/// <summary>
/// Composes multiple rules into a single checkable unit.
/// Enables rule reuse and complex architectural validation.
/// </summary>
public class RuleComposer : Checkable
{
    private readonly List<Checkable> _rules = new();
    private string _name = "ComposedRule";
    private CompositionMode _mode = CompositionMode.All;

    public RuleComposer()
    {
    }

    public RuleComposer(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Add a rule to the composer.
    /// </summary>
    public RuleComposer Add(Checkable rule)
    {
        _rules.Add(rule ?? throw new ArgumentNullException(nameof(rule)));
        return this;
    }

    /// <summary>
    /// Add multiple rules to the composer.
    /// </summary>
    public RuleComposer AddRange(IEnumerable<Checkable> rules)
    {
        _rules.AddRange(rules);
        return this;
    }

    /// <summary>
    /// Set composition mode (All rules must pass, or any rule can pass).
    /// </summary>
    public RuleComposer WithMode(CompositionMode mode)
    {
        _mode = mode;
        return this;
    }

    /// <summary>
    /// Require all rules to pass (default).
    /// </summary>
    public RuleComposer RequireAll()
    {
        _mode = CompositionMode.All;
        return this;
    }

    /// <summary>
    /// Require any rule to pass (at least one).
    /// </summary>
    public RuleComposer RequireAny()
    {
        _mode = CompositionMode.Any;
        return this;
    }

    /// <summary>
    /// Require none of the rules to pass (all must fail).
    /// </summary>
    public RuleComposer RequireNone()
    {
        _mode = CompositionMode.None;
        return this;
    }

    /// <summary>
    /// Set a friendly name for this composition.
    /// </summary>
    public RuleComposer Named(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Check all composed rules.
    /// </summary>
    public async Task<IReadOnlyList<Violation>> CheckAsync(CheckOptions? options = null)
    {
        var allViolations = new List<Violation>();

        foreach (var rule in _rules)
        {
            var violations = await rule.CheckAsync(options);
            allViolations.AddRange(violations);
        }

        return _mode switch
        {
            CompositionMode.All => allViolations,
            CompositionMode.Any => allViolations.Count > 0 ? new List<Violation>() : allViolations,
            CompositionMode.None => allViolations.Count == 0 ? new List<Violation>() : allViolations,
            _ => allViolations,
        };
    }

    /// <summary>
    /// Get rule count in composition.
    /// </summary>
    public int GetRuleCount() => _rules.Count;

    /// <summary>
    /// Get composition name.
    /// </summary>
    public string GetName() => _name;
}

/// <summary>
/// Composition mode for combining rules.
/// </summary>
public enum CompositionMode
{
    /// <summary>
    /// All rules must pass (violations from any rule are failures).
    /// </summary>
    All,

    /// <summary>
    /// Any rule can pass (at least one rule must pass).
    /// </summary>
    Any,

    /// <summary>
    /// None of the rules should pass (all must fail).
    /// </summary>
    None,
}

/// <summary>
/// Builder for common rule compositions.
/// </summary>
public static class RuleCompositions
{
    /// <summary>
    /// Create a composition requiring all rules to pass.
    /// </summary>
    public static RuleComposer All(string name, params Checkable[] rules)
    {
        return new RuleComposer(name)
            .WithMode(CompositionMode.All)
            .AddRange(rules);
    }

    /// <summary>
    /// Create a composition requiring any rule to pass.
    /// </summary>
    public static RuleComposer Any(string name, params Checkable[] rules)
    {
        return new RuleComposer(name)
            .WithMode(CompositionMode.Any)
            .AddRange(rules);
    }

    /// <summary>
    /// Create a composition requiring none of the rules to pass.
    /// </summary>
    public static RuleComposer None(string name, params Checkable[] rules)
    {
        return new RuleComposer(name)
            .WithMode(CompositionMode.None)
            .AddRange(rules);
    }

    /// <summary>
    /// Create a layered architecture composition.
    /// </summary>
    public static RuleComposer LayeredArchitecture(
        string uiLayer,
        string businessLayer,
        string dataLayer)
    {
        var composer = new RuleComposer("LayeredArchitecture").RequireAll();

        // UI can depend on business
        composer.Add(ArchUnit.ProjectFiles(".")
            .InPath(uiLayer)
            .Should()
            .DependOnFiles()
            .InPath(businessLayer));

        // Business cannot depend on UI
        composer.Add(ArchUnit.ProjectFiles(".")
            .InPath(businessLayer)
            .ShouldNot()
            .DependOnFiles()
            .InPath(uiLayer));

        // Business can depend on data
        composer.Add(ArchUnit.ProjectFiles(".")
            .InPath(businessLayer)
            .Should()
            .DependOnFiles()
            .InPath(dataLayer));

        // Data cannot depend on business or UI
        composer.Add(ArchUnit.ProjectFiles(".")
            .InPath(dataLayer)
            .ShouldNot()
            .DependOnFiles()
            .InPath(businessLayer));

        return composer;
    }

    /// <summary>
    /// Create a no-cycles composition for multiple paths.
    /// </summary>
    public static RuleComposer NoCycles(params string[] paths)
    {
        var composer = new RuleComposer("NoCycles").RequireAll();

        foreach (var path in paths)
        {
            composer.Add(ArchUnit.ProjectFiles(".")
                .InPath(path)
                .Should()
                .HaveNoCycles());
        }

        return composer;
    }

    /// <summary>
    /// Create a strict independence composition for multiple modules.
    /// </summary>
    public static RuleComposer StrictModuleIndependence(params string[] modulePaths)
    {
        var composer = new RuleComposer("StrictModuleIndependence").RequireAll();

        for (int i = 0; i < modulePaths.Length; i++)
        {
            for (int j = i + 1; j < modulePaths.Length; j++)
            {
                var module1 = modulePaths[i];
                var module2 = modulePaths[j];

                // Module 1 should not depend on Module 2
                composer.Add(ArchUnit.ProjectFiles(".")
                    .InPath(module1)
                    .ShouldNot()
                    .DependOnFiles()
                    .InPath(module2));

                // Module 2 should not depend on Module 1
                composer.Add(ArchUnit.ProjectFiles(".")
                    .InPath(module2)
                    .ShouldNot()
                    .DependOnFiles()
                    .InPath(module1));
            }
        }

        return composer;
    }

    /// <summary>
    /// Create a hierarchical dependency composition.
    /// </summary>
    public static RuleComposer HierarchicalDependencies(params string[] layers)
    {
        var composer = new RuleComposer("HierarchicalDependencies").RequireAll();

        for (int i = 0; i < layers.Length - 1; i++)
        {
            var upperLayer = layers[i];
            var lowerLayer = layers[i + 1];

            // Upper can depend on lower
            composer.Add(ArchUnit.ProjectFiles(".")
                .InPath(upperLayer)
                .Should()
                .DependOnFiles()
                .InPath(lowerLayer));

            // Lower cannot depend on upper
            composer.Add(ArchUnit.ProjectFiles(".")
                .InPath(lowerLayer)
                .ShouldNot()
                .DependOnFiles()
                .InPath(upperLayer));
        }

        return composer;
    }
}

/// <summary>
/// Extension methods for rule composition.
/// </summary>
public static class RuleCompositionExtensions
{
    /// <summary>
    /// Combine multiple rules into a single checkable.
    /// </summary>
    public static RuleComposer Compose(this IEnumerable<Checkable> rules, string name)
    {
        var composer = new RuleComposer(name);
        composer.AddRange(rules);
        return composer;
    }

    /// <summary>
    /// Combine rules with "all must pass" semantics.
    /// </summary>
    public static RuleComposer ComposeAll(this IEnumerable<Checkable> rules, string name)
    {
        return rules.Compose(name).RequireAll();
    }

    /// <summary>
    /// Combine rules with "any can pass" semantics.
    /// </summary>
    public static RuleComposer ComposeAny(this IEnumerable<Checkable> rules, string name)
    {
        return rules.Compose(name).RequireAny();
    }

    /// <summary>
    /// Combine rules with "none should pass" semantics.
    /// </summary>
    public static RuleComposer ComposeNone(this IEnumerable<Checkable> rules, string name)
    {
        return rules.Compose(name).RequireNone();
    }

    /// <summary>
    /// Create a composition from a list of rules.
    /// </summary>
    public static RuleComposer CreateComposition(this List<Checkable> rules, string name)
    {
        return new RuleComposer(name).AddRange(rules);
    }
}
