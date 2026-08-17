# ArchUnit - Architecture Testing for .NET

<div align="center" name="top">

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://github.com/LukasNiessen/ArchUnitNET/blob/main/LICENSE) [![Build & tests](https://img.shields.io/github/actions/workflow/status/LukasNiessen/ArchUnitNET/build-and-test.yml?branch=main&label=build%20%26%20tests)](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml) [![NuGet version](https://img.shields.io/nuget/vpre/ArchUnit.svg)](https://www.nuget.org/packages/ArchUnit/)<br>
[![NuGet downloads](https://img.shields.io/nuget/dt/ArchUnit.svg?color=007ec6)](https://www.nuget.org/packages/ArchUnit/) [![GitHub stars](https://img.shields.io/github/stars/LukasNiessen/ArchUnitNET.svg)](https://github.com/LukasNiessen/ArchUnitNET)

</div>

> **Alpha prerelease:** ArchUnit for .NET is under active development. APIs and behavior may change, and the full test suite is not yet green. Use this release for evaluation only.

Enforce architecture rules in C# and .NET projects. Check dependency directions, detect circular dependencies, validate layers, measure cohesion, and generate reports. Integrates with xUnit, NUnit, MSTest, and any testing framework that can assert on returned violations.

A .NET implementation inspired by [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS) and the original [ArchUnit](https://github.com/TNG/ArchUnit). This project is not affiliated with TNG.

[Setup](#-setup) • [Use Cases](#-use-cases) • [Features](#-features) • [Pattern Matching](#-pattern-matching-system) • [Contributing](https://github.com/LukasNiessen/ArchUnitNET/blob/main/CONTRIBUTING.md) • [Documentation](https://lukasniessen.github.io/ArchUnitNET/)

## ⚡ 5 min Quickstart

### Installation

```bash
dotnet add package ArchUnit --version 2.4.0-alpha.2
```

### Add tests

Simply add architecture rules to your existing test suites. The following example uses xUnit. First, ensure that the project has no circular dependencies.

```csharp
using ArchUnitNet;
using ArchUnitNet.Testing.XUnit;

[Fact]
public async Task ProjectShouldNotHaveCircularDependencies()
{
    var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
        .InPath("src/**")
        .Should()
        .HaveNoCycles();

    await rule.PassesAsync();
}
```

Next, ensure that the layered architecture is respected.

```csharp
[Fact]
public async Task PresentationShouldNotDependOnData()
{
    var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
        .InPath("src/Presentation/**")
        .ShouldNot()
        .DependOnFiles()
        .InPath("src/Data/**");

    await rule.PassesAsync();
}
```

Lastly, add a code-metric rule for a specific class.

```csharp
[Fact]
public async Task OrderServiceShouldHaveHighCohesion()
{
    var rule = ArchUnit.Metrics<OrderService>()
        .Methods()
        .LCOM96a()
        .ShouldBeLessThan(0.5);

    await rule.PassesAsync();
}
```

### CI Integration

These rules are regular tests, so they run automatically in the existing test setup and CI pipeline:

```bash
dotnet test
```

This setup ensures that the architectural rules you define remain enforced as the codebase changes. 🌻🐣

Reports can also be saved as CI artifacts. For example, violations can be exported as SARIF, JSON, text, or HTML:

```csharp
using ArchUnitNet.Reporting;

var violations = (await rule.CheckAsync()).ToList();
var report = new ViolationReportExporter(violations, "MyProject");

await report.ExportToSARIFAsync("reports/architecture.sarif");
await report.ExportToHTMLAsync("reports/architecture.html");
```

## 🚐 Setup

Installation:

```bash
dotnet add package ArchUnit --version 2.4.0-alpha.2
```

The package exposes framework-specific assertion adapters and a framework-agnostic fallback. Add the normal runner packages for the testing framework used by your project.

### xUnit

Import the xUnit adapter and use `PassesAsync()`:

```csharp
using ArchUnitNet.Testing.XUnit;

await rule.PassesAsync();
```

### NUnit

Import the NUnit adapter and use `PassAsync()`:

```csharp
using ArchUnitNet.Testing.NUnit;

await rule.PassAsync();
```

### MSTest

Import the MSTest adapter and use `PassAsync()`:

```csharp
using ArchUnitNet.Testing.MSTest;

await rule.PassAsync();
```

### Other Framework

For any other framework, inspect the returned violations or use the synchronous framework-agnostic assertion helper:

```csharp
using ArchUnitNet.Testing;

ArchAssert.Passes(rule);
```

## 🐹 Use Cases

The fluent API covers common architectural fitness functions.

**Layered Architecture:** prevent presentation code from reaching directly into data-access code.

```csharp
var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/Presentation/**")
    .ShouldNot()
    .DependOnFiles()
    .InPath("src/Data/**");
```

**Public API Protection:** stop external-facing code from depending on internal implementation details.

```csharp
var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/Public/**")
    .ShouldNot()
    .DependOnFiles()
    .InPath("src/**/Internal/**");
```

**Feature Isolation:** keep feature folders independent with the built-in preset or explicit dependency rules.

```csharp
var preset = ArchitecturePresets.FeatureIsolation()
    .WithProjectPath("./MyProject.csproj")
    .WithFeaturesPath("src/Features/*");
```

**Gradual Adoption:** record known violations in a baseline and fail only on new ones.

```csharp
var baseline = await ViolationBaseline.LoadFromFileAsync("architecture-baseline.json");
var newViolations = violations.WithoutBaseline(baseline);
```

## 🐣 Features

This is an overview of what you can do with ArchUnit for .NET.

### Circular Dependencies

```csharp
var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InFolder("src/Services")
    .Should()
    .HaveNoCycles();

await rule.PassesAsync();
```

### Layer Dependencies

Use file rules for direct boundaries or define named layers with `{Layer}` path extraction.

```csharp
using ArchUnitNet.Layers.Common;

var rule = ArchUnit.ProjectLayers("./MyProject.csproj")
    .DefinedBy("src/{Layer}/**")
    .Where(Layer.Defined("Presentation"))
    .MayOnlyDependOn(
        Layer.Defined("Business"),
        Layer.Defined("Common"));

await rule.PassesAsync();
```

### Naming Conventions

```csharp
var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InFolder("src/Services")
    .Should()
    .HaveName("*Service.cs");

await rule.PassesAsync();
```

### Code Metrics

LCOM cohesion and count thresholds can target concrete .NET types.

```csharp
var cohesionRule = ArchUnit.Metrics<OrderService>()
    .Methods()
    .LCOM96b()
    .ShouldBeLessThan(0.5);

var methodCountRule = ArchUnit.Metrics<OrderService>()
    .Classes()
    .MethodCount()
    .ShouldHaveAtMost(20);
```

Supported cohesion variants include LCOM1, LCOM96a, LCOM96b, and LCOM1995. Count rules cover methods, fields, and field access.

### Custom Rules

Define custom file or dependency predicates when a built-in rule is not enough.

```csharp
var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/**")
    .Should()
    .AdhereTo(
        file => file.NonBlankLineCount < 500,
        "Source files should stay below 500 non-blank lines");

await rule.PassesAsync();
```

### Architecture Slices

Group files into logical slices and check that slice dependencies remain acyclic.

```csharp
var rule = ArchUnit.ProjectSlices("./MyProject.csproj")
    .DefinedBy("src/{Slice}/**")
    .Should()
    .BeAcyclic();

await rule.PassesAsync();
```

### Architecture Presets

Built-in presets cover common structures:

- Layered architecture
- Hexagonal architecture
- Feature isolation
- Microservices
- Clean architecture
- Modular monoliths
- Domain-driven design
- Event-driven architecture

Presets produce regular rules that can be composed and checked like hand-written rules.

### Dependency Graph Reports

Generate dependency graphs in several formats and narrow them to the code you want to inspect.

```csharp
using ArchUnitNet.Common.Extraction;
using ArchUnitNet.GraphReporting;

var graph = await new DependencyExtractor()
    .ExtractGraphAsync("./MyProject.csproj");

await ArchUnit.ProjectGraph()
    .AddEdges(graph.Edges)
    .CollapseToFolderDepth(2)
    .ExportToFileAsync(
        GraphExportFormat.Mermaid,
        "reports/dependencies.mmd");
```

Supported formats:

- DOT
- Mermaid
- D2
- CSV
- JSON
- HTML

Graph exploration supports external dependencies, folder-depth collapsing, and path-focused views.

### Reports

Architecture violations can be exported for people and CI systems:

- HTML for a readable report
- SARIF for GitHub and other code-scanning tools
- JSON for programmatic processing
- Text for build artifacts and logs

Metrics reports are also available in HTML and JSON. Reporting remains experimental in the alpha releases.

### Configuration and Baselines

Load and save JSON rule configuration:

```csharp
using ArchUnitNet.Configuration;

var configuration = await RuleConfiguration.LoadFromFileAsync("architecture-rules.json");
```

Use violation baselines and rule snapshots to introduce architecture checks gradually without hiding new regressions.

### Performance Profiling

Profile a rule when architecture checks become part of a larger test suite:

```csharp
using ArchUnitNet.Performance;

var (violations, profile) = await rule.ProfileCheckAsync("Layer boundaries");
Console.WriteLine(profile.GetFormattedExecutionTime());
```

## 🔎 Pattern Matching System

The file-rule API offers three targeting options:

- **`InPath(pattern)`** checks a glob against the full normalized path.
- **`InFolder(folder)`** targets everything below a folder.
- **`ByName(pattern)`** checks a glob against file names.

### Glob Patterns Guide

| Pattern | Meaning |
| --- | --- |
| `*` | Any characters except a path separator |
| `**` | Any number of folders and files |
| `?` | One character |
| `src/**/*.cs` | Every C# file below `src` |
| `src/Services/**` | Everything below `src/Services` |
| `**/*Service.cs` | Every file ending in `Service.cs` |

### Pattern Matching Examples

```csharp
var services = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*Service.cs");

var repositories = ArchUnit.ProjectFiles("./MyProject.csproj")
    .InFolder("src/Repositories");

var controllers = ArchUnit.ProjectFiles("./MyProject.csproj")
    .ByName("*Controller.cs");
```

Use forward slashes in patterns. Extracted paths are normalized before matching.

## 📢 Informative Error Messages

Framework adapters convert violations into native test failures. Messages contain the violated rule and the relevant source and target paths so failures can be investigated directly from test output.

You can also check rules directly when custom reporting is needed:

```csharp
var violations = await rule.CheckAsync();

foreach (var violation in violations)
{
    Console.WriteLine(violation);
}
```

Empty matches fail by default for rules that support empty-test protection. This prevents a typo in a path from silently turning an architecture test green. `CheckOptions` can explicitly allow empty checks where appropriate.

## 🏈 Architecture Fitness Functions

ArchUnit rules are executable architecture fitness functions. Because they live in normal .NET test projects, they can be reviewed with the code, run locally, and enforced in CI together with unit and integration tests.

## 🔲 Core Modules

| Module | Description | Alpha status |
| --- | --- | --- |
| **Files** | File, folder, naming, and dependency rules | Available |
| **Layers** | Named layer extraction and dependency constraints | Available |
| **Metrics** | Cohesion and count metrics | Available |
| **Slices** | Logical architecture slicing | Available |
| **Graph** | Dependency graph reports | Experimental |
| **Testing** | xUnit, NUnit, MSTest, and generic assertions | Available |
| **Reporting** | HTML, SARIF, JSON, text, and metrics reports | Experimental |
| **Common** | Extraction, projections, matching, and shared types | Internal foundation |

## 🕵️ Technical Deep Dive

ArchUnit uses Roslyn to parse C# syntax trees from the project referenced by each rule. It turns using directives and other dependency information into a graph, projects that graph into files, layers, or slices, and evaluates fluent constraints against the result.

```text
Project files → Roslyn extraction → Dependency graph
              → Files / Layers / Slices / Metrics
              → Violations → Test failure or report
```

The implementation is organized so extraction, projection, assertion, and reporting remain separate. See the [architecture documentation](https://github.com/LukasNiessen/ArchUnitNET/tree/main/docs/articles) for more detail.

## 🦊 Contributing

Contributions are very welcome. Please open a focused issue or pull request and keep changes covered by the relevant architecture or unit tests. See the [contributing guide](https://github.com/LukasNiessen/ArchUnitNET/blob/main/CONTRIBUTING.md) for repository conventions.

## ℹ️ FAQ

**Q: Which .NET testing frameworks are supported?**

xUnit, NUnit, and MSTest have dedicated assertion adapters. Any other framework can inspect `CheckAsync()` results or use `ArchAssert`.

**Q: Does the NuGet package name match the namespace?**

The package is installed as `ArchUnit`. Existing C# namespaces remain under `ArchUnitNet`, for example `using ArchUnitNet;`.

**Q: Can I use ArchUnit in CI?**

Yes. Architecture checks are normal tests and run through `dotnet test`. Reports can be retained as pipeline artifacts.

**Q: How do I adopt rules in a codebase with existing violations?**

Create a `ViolationBaseline`, filter known violations, and fail only when new violations appear. The baseline can be reduced as the architecture improves.

**Q: Is this release stable?**

No. The current package is an alpha prerelease. APIs may change, and some repository tests are still failing.

## 📅 Plans

The immediate focus is to stabilize the public API, bring the complete test suite back to green, harden project extraction across platforms, and validate the package with realistic .NET example repositories.

## 🐣 Origin Story

ArchUnit for .NET brings the architecture-testing approach used by ArchUnitTS to C# projects: architecture rules should be executable, readable, and enforced continuously alongside the rest of the test suite.

## 💟 Community

### Maintainers

- **[LukasNiessen](https://github.com/LukasNiessen)** - Maintainer

### Contributors

See everyone who has contributed on the [GitHub contributors page](https://github.com/LukasNiessen/ArchUnitNET/graphs/contributors).

### Questions

Found a bug or want to discuss a feature?

- Submit an [issue on GitHub](https://github.com/LukasNiessen/ArchUnitNET/issues/new/choose)
- Join the [GitHub Discussions](https://github.com/LukasNiessen/ArchUnitNET/discussions)
- Read the [documentation](https://lukasniessen.github.io/ArchUnitNET/)

If ArchUnit helps your project, please consider:

- Starring the repository 💚
- Suggesting new features 💭
- Contributing code or documentation ⌨️

### Star History

[![Star History Chart](https://star-history.dera.page/svg?repos=LukasNiessen/ArchUnitNET&type=Date)](https://star-history.dera.page/#LukasNiessen/ArchUnitNET&Date)

## 📄 License

This project is available under the **Apache License 2.0**. See the [license](https://github.com/LukasNiessen/ArchUnitNET/blob/main/LICENSE) for details.

---

<p align="center">
  <a href="#top"><strong>Go Back to Top</strong></a>
</p>

---

## Post Scriptum

### Special Note on Cycle-Free Checks

Cycle-free checks are intentionally scoped to the selected file set. A selected folder may contain files while having no internal dependency edges. When this happens, the cycle check avoids treating the absence of internal edges as proof that the folder itself did not match.
