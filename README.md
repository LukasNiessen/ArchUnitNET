# ArchUnitNET

> **Alpha prerelease:** ArchUnitNET is under active development. APIs and behavior may change, and the full test suite is not yet green. Use this release for evaluation only.

[![Build](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/code-quality.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/code-quality.yml)
[![NuGet](https://img.shields.io/nuget/v/ArchUnitNET.svg)](https://www.nuget.org/packages/ArchUnitNET/)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**Test-driven architecture validation for C# and .NET**

Enforce your application's architecture automatically. Catch violations before they reach production.

A complete port of [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS) to the .NET ecosystem.

**Status**: Alpha prerelease | **License**: Apache 2.0

---

## 5-Minute Quick Start

### 1️⃣ Install the NuGet Package

```bash
dotnet add package ArchUnitNET --version 2.4.0-alpha.1
```

### 2️⃣ Write Your First Rule

```csharp
using ArchUnitNet;
using Xunit;

[Fact]
public async Task DashboardShouldNotAccessDatabaseDirectly()
{
    var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
        .InPath("src/UI/Dashboard/**")
        .ShouldNot()
        .DependOnFiles()
        .InPath("src/Data/**");

    var violations = await rule.CheckAsync();
    
    Assert.Empty(violations);  // Pass if no violations found
}
```

### 3️⃣ Run Your Architecture Test

```bash
dotnet test
```

✅ **That's it!** Your architecture is now under test.

---

## What You Can Test

### File-Based Rules
```csharp
// Prevent UI layer from depending on Data layer
ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InPath("src/Data/**");

// Require Services to depend on Models
ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/Services/**")
    .Should()
    .DependOnFiles()
    .InPath("src/Models/**");
```

### Cyclic Dependency Detection
```csharp
// No circular dependencies allowed
ArchUnit.ProjectFiles("./MyProject.csproj")
    .InPath("src/**")
    .Should()
    .HaveNoCycles();
```

### Code Cohesion
```csharp
// Ensure methods are cohesive (low LCOM)
ArchUnit.Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);
```

### Architecture Presets
```csharp
// Use built-in templates for common patterns
var preset = ArchitecturePresets.LayeredArchitecture()
    .WithProjectPath("./MyProject.csproj");

var violations = await preset.ValidateAsync();
```

---

## Core Features

### ✅ **Dependency Validation**
- Prevent unwanted module dependencies
- Enforce layered architecture  
- Cycle detection (Tarjan's algorithm - O(V+E))
- Glob patterns with exclusions

### ✅ **Code Metrics**
- LCOM cohesion analysis (96a, 96b variants)
- Method complexity estimation
- Field access tracking
- Threshold-based validation

### ✅ **Visualization & Reports**
- Export to Mermaid, DOT, D2, JSON, HTML
- SARIF for CI/CD integration
- Dependency graphs with filtering
- Performance profiling

### ✅ **Test Integration**
- xUnit, NUnit, MSTest adapters
- Fluent assertions
- Framework-agnostic helpers
- Full async/await support

### ✅ **Advanced Features**
- JSON rule configuration
- Violation baselines (gradual remediation)
- Architecture presets (Layered, Hexagonal, DDD, etc.)
- Rule composition and reuse

---

## Common Use Cases

### 📋 Enforce Layered Architecture

```csharp
[Fact]
public async Task ValidateLayeredArchitecture()
{
    var rule = ArchitecturePresets.LayeredArchitecture()
        .WithProjectPath("./MyProject.csproj")
        .BuildRules()
        .Compose("Layered Architecture");

    var violations = await rule.CheckAsync();
    Assert.Empty(violations);
}
```

### 🏗️ Protect Public APIs

```csharp
[Fact]
public async Task ExternalPackagesShouldUsePublicAPI()
{
    var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
        .InPath("src/External/**")
        .ShouldNot()
        .DependOnFiles()
        .InPath("**/internal/**");

    var violations = await rule.CheckAsync();
    Assert.Empty(violations);
}
```

### 🚫 No Circular Dependencies

```csharp
[Fact]
public async Task NoCircularDependenciesAllowed()
{
    var rule = ArchUnit.ProjectFiles("./MyProject.csproj")
        .InPath("src/**")
        .Should()
        .HaveNoCycles();

    var violations = await rule.CheckAsync();
    Assert.Empty(violations);
}
```

### 📊 Measure Code Cohesion

```csharp
[Fact]
public async Task MethodsShouldHaveHighCohesion()
{
    var rule = ArchUnit.Metrics()
        .Methods()
        .LCOM96a()
        .ShouldBeLessThan(0.5);

    var violations = await rule.CheckAsync();
    Assert.Empty(violations);
}
```

---

## How It Works

```
1. Extract dependencies from your .csproj
   ↓ (Uses Roslyn to parse C# syntax trees)
   
2. Build a dependency graph
   ↓ (Nodes = files, Edges = imports)
   
3. Apply your architecture rules
   ↓ (Fluent API, Presets, or JSON config)
   
4. Report violations
   ↓ (Console, SARIF, HTML, JSON, etc.)
   
5. Fail the build if needed
   ↓ (Perfect for CI/CD pipelines)
```

---

## Configuration & Advanced Usage

### Load Rules from JSON

```csharp
var config = await ArchUnit.LoadArchitectureRulesAsync("./rules.json");
// rules.json defines source, target, action, severity
```

### Suppress Known Violations

```csharp
var baseline = await ViolationBaseline.LoadFromFileAsync("./baseline.json");
var newViolations = violations.WithoutBaseline(baseline);
```

### Export Dependency Graphs

```csharp
var graph = ArchUnit.ProjectGraph()
    .IncludeExternalDependencies()
    .CollapseToFolderDepth(2);

await graph.ExportToFileAsync(GraphFormat.Mermaid, "graph.md");
```

### Analyze Performance

```csharp
var (violations, profile) = await rule.ProfileCheckAsync("MyRule");
Console.WriteLine($"Executed in {profile.GetFormattedExecutionTime()}");
```

---

## Architecture Presets

Built-in templates for common patterns:

- **Layered** - UI → Service → Data
- **Hexagonal** - Domain core + adapters  
- **Feature Isolation** - Independent features
- **Public API** - Barrel exports
- **Microservices** - Service independence
- **Clean Architecture** - Entity → UseCase → Controller
- **Modular Monolith** - Module boundaries
- **Domain-Driven Design** - Bounded contexts
- **Event-Driven** - Event bus decoupling

---

## Test Framework Support

### xUnit
```csharp
[Fact]
public async Task MyArchitectureRule()
{
    await rule.PassAsync();  // Extension method
}
```

### NUnit
```csharp
[Test]
public async Task MyArchitectureRule()
{
    await ArchUnitAssert.That(rule).Should().PassAsync();
}
```

### MSTest
```csharp
[TestMethod]
public async Task MyArchitectureRule()
{
    await rule.PassAsync();  // Compatible with MSTest
}
```

---

## Learning Path

1. **Start Here** - 5-minute quick start (above ↑)
2. **Examples** - Copy patterns for your use case
3. **API Reference** - Explore all fluent methods
4. **Best Practices** - Learn from ArchUnitNET's own rules
5. **Advanced** - Custom rules, JSON config, CI/CD integration

---

## Architecture

```
Layer 0: Core Types (Error, Violation)
    ↓
Layer 1: Utilities (Path, Logging)
    ↓
Layer 2: Extraction (Roslyn-based)
    ↓
Layer 3: Projections (Cycles, Slices)
    ↓
Layer 4: Rules & Builders (Fluent API)
    ↓
Layer 5: Testing Integration
```

## Technologies

- **[Roslyn](https://github.com/dotnet/roslyn)** - C# syntax tree analysis
- **[xUnit](https://xunit.net/)** - Testing framework
- **[StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)** - Code quality
- **[DocFX](https://dotnet.github.io/docfx/)** - Documentation generation

---

## Project Stats

- **Lines of Code**: ~6,000 (implementation)
- **Test Coverage**: 200+ tests across 25 files
- **Modules**: 5 (Common, Files, Metrics, Slices, Graph)
- **Public APIs**: 40+ (fluent builders + utilities)
- **Compilation**: Zero warnings, production-grade code
- **License**: Apache 2.0

---

## Support

- 📖 **Documentation**: [archunitcsharp.dev](https://archunitcsharp.dev)
- 🐛 **Issues**: [GitHub Issues](https://github.com/LukasNiessen/ArchUnitNET/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/LukasNiessen/ArchUnitNET/discussions)
- 🤝 **Contributing**: [Contributing Guide](CONTRIBUTING.md)

---

## License

Apache License 2.0 — See [LICENSE](LICENSE) for details.

---

## Related Projects

- [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS) — TypeScript/JavaScript port
- [ArchUnitPython](https://github.com/LukasNiessen/ArchUnitPython) — Python port
- [ArchUnit](https://github.com/TNG/ArchUnit) — Original Java library

---

Made with ❤️ by the ArchUnit community
