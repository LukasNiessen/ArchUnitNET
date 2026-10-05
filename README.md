# ArchUnit.NET

The canonical repository is [LukasNiessen/ArchUnitNET](https://github.com/LukasNiessen/ArchUnitNET).
This implementation was developed by [Robey Beswick](https://github.com/RobeyBeswick) as
ArchUnitSharp and promoted here on 5 October 2026. Its project names and C# namespaces remain
`ArchUnitSharp` while the package and API transition is planned. The previous implementation,
including its branches and release tags, is preserved in
[Deprecated-ArchUnitNET](https://github.com/LukasNiessen/Deprecated-ArchUnitNET).

Architecture testing for C# and .NET — a fluent, chainable rule DSL over a graph of your project's
files and their dependencies, in the spirit of ArchUnit. A rule reads as a sentence an architect
could write:

    project files in folder 'src/Web' should not depend on files in folder 'src/Persistence'

The library analyses a project's C# source tree into a file-to-file dependency graph from its
`using` directives (resolved with Roslyn's semantic model), and lets a test project assert rules over
that graph — that files sit where they should, that layers only depend on the layers they may, that
slices stay free of forbidden cross-feature edges, that metrics stay under a threshold.

The implementation was built with the [ArchUnitDev](https://github.com/RobeyBeswick/ArchUnitDev)
loop. Development conventions are in [AGENTS.md](AGENTS.md). Known correctness limitations and
the migration work are tracked in [this repository's issues](https://github.com/LukasNiessen/ArchUnitNET/issues).

The full documentation — this guide plus a searchable, source-generated API reference for every
public module, class and method — lives in [docs](docs/index.md). Publishing the canonical
documentation site is tracked in [issue #42](https://github.com/LukasNiessen/ArchUnitNET/issues/42).

## Requirements

- The .NET 10 SDK (every project targets `net10.0`).

## Install

This promoted implementation is not yet published to NuGet; existing `ArchUnit` releases contain
the previous implementation. Use this version from source. Clone this repository, then add
the composition root and the assertion adapter to your test project:

    dotnet add tests/MyApp.Tests/MyApp.Tests.csproj reference src/ArchUnitSharp/ArchUnitSharp.csproj src/ArchUnitSharp.Testing.Xunit/ArchUnitSharp.Testing.Xunit.csproj

`ArchUnitSharp` is the entry-point surface — `Project.ProjectFiles()` and the other `Project.*` nouns.
`ArchUnitSharp.Testing.Xunit` makes a rule terminal assert natively under xUnit through
`.AssertPasses()` and `.AssertFails()`. Under any other test framework, or when you want no adapter,
use the framework-agnostic helper `ArchUnitSharp.Testing.RuleAssert.Passes(rule)` /
`RuleAssert.Fails(rule)`; it works with every framework and needs no configuration.

## A first rule in ten lines

A rule is checked from a test. The entry point locates your project by walking up from the current
working directory to the nearest `.sln` or `.csproj`, so a test under the repository root sees the
whole repository:

```csharp
using ArchUnitSharp;
using ArchUnitSharp.Extraction;
using ArchUnitSharp.Testing.Xunit;
using Xunit;

public class ArchitectureTests
{
    [Fact]
    public void Web_must_not_depend_on_Persistence()
    {
        Project.ProjectFiles()
            .InFolder("src/Web")
            .ShouldNot()
            .DependOn()
            .InFolder("src/Persistence")
            .AssertPasses();
    }
}
```

The chain reads as one sentence: *project files in folder 'src/Web' should not depend on files in
folder 'src/Persistence'*. When the rule fails, the assertion throws with a report naming each
offending dependency, and `dotnet test` shows it as a failed test.

To analyse a repository other than the current one, pass an explicit location:

```csharp
var location = ProjectLocator.Locate("/path/to/repo");
Project.ProjectFiles(location).Should().Exist().AssertPasses();
```

## The fluent grammar

Every rule is one chain: `ENTRY → SCOPE → MOOD → PREDICATE → OBJECT → TERMINAL`. Exactly one entry,
one mood, one predicate and one terminal; scopes and objects chainable.

- **Entry.** `project files` / `files`, `project layers` / `layers`, `project slices` / `slices`,
  `project graph` / `graph`, `project metrics` / `metrics`.
- **Scope.** The selectors are prepositional: `with name`, `in folder`, `in path`, `in file`, and the
  metrics module's `for classes matching`. Selectors combine with AND; the `except` companion
  excludes from the most recently applied selector.
- **Mood.** `should` and `should not` on the files and slices surfaces; `may only depend on layers`
  and `may not depend on layers` on the layers surface; the metrics threshold predicates carry the
  mood themselves.
- **Predicate.** `should exist`, `should have name`, `should be in folder`, `should be in path`,
  `should depend on files`, `should depend on external modules`, `should adhere to`,
  `should have no cycles`, `should contain dependency`, `should adhere to diagram`.
- **Object.** A file selection with the scope's selectors, or an external-module selection with
  `matching` (repeats combine with OR).
- **Terminal.** `Check(CheckOptions?)` returns `IReadOnlyList<Violation>` — an empty list means the
  rule passed — or the report terminals `to ...()` / `export as ...(path)` render data.

Two properties every rule inherits:

- **A rule that matches nothing is a violation, not a pass.** The empty-test guard reports an
  `EmptyTestViolation` unless `CheckOptions.AllowEmptyTests` is set, so a typo in a glob is a
  failure, never a silent pass.
- **Builders are immutable.** A half-built chain can be stored in a variable and branched, and two
  branches never see each other's selectors.

The word choice is fixed: a `should equal` or `should be at most` is a defect. The six threshold
predicates are exactly `should be below`, `should be above`, `should be`,
`should be below or equal to`, `should be above or equal to`, and `should satisfy`.

## The modules, one example each

### Files — what your files may and may not do

```csharp
Project.ProjectFiles()
    .InPath("src/**")
    .Except("src/**/generated/**")
    .Should()
    .HaveNoCycles()
    .AssertPasses();
```

The scope is the subject of the rule. The object of `should depend on files` is the file selection
the subject may or must not depend on; `should depend on external modules` matches the targets of
external edges by module name as written (`System.Linq` for `using System.Linq;`), so a third-party
policy is one chain.

### Layers — a named-layer policy

```csharp
Project.Layers()
    .Layer("App").DefinedByFolder("src/App")
    .Layer("Models").DefinedByFolder("src/Models")
    .WhereLayer("App").MayOnlyDependOnLayers("Models")
    .AssertPasses();
```

A policy accumulates declarations and rules and is checked as a whole. The allowlist
`may only depend on layers(...)` permits exactly the named layers — intra-layer dependencies are
always allowed, and with no arguments the layer is sealed. The blocklist `may not depend on
layers(...)` forbids the named layers.

### Slices — architecture by feature, not by layer

```csharp
Project.Slices()
    .DefinedBy("src/features/(**)/*.cs")
    .ShouldNot()
    .ContainDependency("src/features/**", "src/legacy/**")
    .AssertPasses();
```

`defined by` names each file's slice from the text a `(**)` capture matches. Rules assert
dependencies between slices, and `adhere to diagram` checks the actual slicing against a PlantUML
component diagram.

### Graph — a report, not a rule

```csharp
string mermaid = Project.Graph()
    .FocusingOn("src/Web/**", depth: 2)
    .CollapsedToFolderDepth(2)
    .Titled("Web and its neighbourhood")
    .ToMermaid();
```

Build a snapshot with `Build()`, render it with `to dot`, `to mermaid`, `to d2`, `to csv`, `to json`
or `to html`, or write a format to disk with `export as dot(path)` and its siblings. The options
narrow the scope (`focusing on`, `reachable from`, `dependents of`), include external or self
dependencies, and collapse file labels.

### Metrics — thresholds, not moods

```csharp
Project.Metrics()
    .InFolder("src")
    .Count()
    .MethodCount()
    .ShouldBeBelow(20)
    .AssertPasses();
```

A metric rule reads each selected file's source to measure it: the count section (`method count`,
`field count`, `lines of code`, `statements`, `imports`, `classes`, `interfaces`), the cohesion
section (the `lcom96a` … `lcom*` family), the distance section (Robert C. Martin's `abstractness`,
`instability`, `distance from main sequence`, `coupling factor`, `normalised distance`, plus the
`not in zone of pain` / `not in zone of uselessness` guards), and `custom metric(name, description,
calculation)` for a rule over a value of your own. Each section also exports its measurements as an
HTML report: `export as html(path)`.

## What is not implemented yet

- **No NuGet package is published.** Install by referencing the projects from source.
- **Some predicates are positive-mood only.** `should have no cycles` (files) and `should adhere to
  diagram` (slices) have no `should not` form.
- **No `functions` count metric.** C# has no file-level function distinct from a type member, so the
  metric the siblings carry is skipped rather than faked.
- **`Metrics<T>()` is not supported.** The type-scoped entry points (`Project.Metrics<T>()` /
  `Project.ProjectMetrics<T>()`) are not implemented and reject the call with a `UserError` naming the
  type, so a rule written against a type can never silently pass without measuring it. Measure a
  class's source file instead: `Project.Metrics().ForClassesMatching("*.OrderService")`.
- **`except` is limited.** It narrows the files and metrics selectors and the depend-on objects;
  layer definitions, slice definitions and graph report options do not take it.
- **The graph is an import graph.** Dependencies are the project's `using` directives resolved
  through Roslyn, not every type reference inside a method body.
