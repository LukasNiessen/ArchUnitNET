# ArchUnitCSharp

[![Build](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/code-quality.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/code-quality.yml)
[![NuGet](https://img.shields.io/nuget/v/ArchUnitCSharp.svg)](https://www.nuget.org/packages/ArchUnitCSharp/)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

**Architecture testing for C# and .NET** — A complete port of [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS) to the .NET ecosystem.

Part of **ArchUnitEverything** — one architecture-testing library per language.

**Status**: 🟢 Production Ready (v2.4.0)

---

## Quick Start

### Installation
```bash
dotnet add package ArchUnitCSharp
```

### Define & Test Rules
```csharp
using ArchUnitNet;

// Validate low coupling in methods
var rule = ArchUnit.Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

var violations = await rule.CheckAsync();
```

## Features

### 🏗️ Architecture Rules
- **File-based rules** - Control dependencies between files and folders
- **Layer validation** - Enforce layered architecture patterns
- **Cycle detection** - Find circular dependencies automatically (O(V+E))
- **Pattern matching** - Glob patterns with regex and nested exclusions

### 📊 Code Metrics
- **LCOM calculation** - 4 variants (LCOM1, LCOM96a, LCOM96b, LCOM1995)
- **Cohesion analysis** - Measure method-field coupling
- **Complexity metrics** - Cyclomatic complexity estimation
- **Field access tracking** - Detailed dependency analysis

### 🔄 Advanced Analysis
- **Cycle detection** - Tarjan's SCC algorithm (optimal O(V+E))
- **Elementary cycles** - Johnson's algorithm for all cycles
- **Dependency graphs** - Extract via Roslyn syntax trees
- **Path normalization** - Cross-platform support (Windows/Unix)

### 📈 Visualization
- **Mermaid diagrams** - Online graph rendering
- **Graphviz DOT** - Professional visualization tools
- **D2 language** - Modern diagram syntax
- **HTML/JSON/CSV** - Multiple export formats

### 🧪 Test Integration
- **xUnit support** - Custom assertions via extensions
- **NUnit support** - Integration with NUnit framework
- **Fluent API** - Builder pattern for composable rules
- **Async/Await** - Full async support throughout

---

## Documentation

- 📖 [Getting Started](docs/index.md)
- 📚 [API Reference](https://archunitcsharp.dev)
- 🤝 [Contributing Guide](CONTRIBUTING.md)
- 📝 [Changelog](CHANGELOG.md)
- ⚙️ [CI/CD Setup](CI-CD-SETUP.md)

---

## Comparison with ArchUnitTS

| Feature | ArchUnitTS | ArchUnitCSharp |
|---------|------------|---|
| Language | TypeScript | C# |
| Target | JavaScript/Node.js | .NET 8.0+ |
| Package Manager | npm | NuGet |
| Test Framework | Jest | xUnit/NUnit |
| Dependency Analysis | AST walking | **Roslyn syntax trees** |
| Graph Algorithms | Basic DFS | **Tarjan's + Johnson's** |
| Code Quality | ESLint | **StyleCop + FxCop** |
| Documentation | TypeDoc | **DocFX + GitHub Pages** |
| CI/CD | Basic GitHub Actions | **Advanced multi-platform** |
| Status Checks | Manual | **Automated enforcement** |

**ArchUnitCSharp advantages**: Richer code analysis, more sophisticated algorithms, better CI/CD.

---

## Examples

### File-Based Rules
```csharp
// Prevent UI layer from importing from Models
var rule = ArchUnit.ProjectFiles()
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Models/**");

await rule.CheckAsync();
```

### Metrics Validation
```csharp
// Ensure high cohesion (low LCOM)
var rule = ArchUnit.Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

await rule.CheckAsync();
```

### Cycle Detection
```csharp
// No circular dependencies allowed
var rule = ArchUnit.ProjectFiles()
    .InPath("src/**")
    .Should()
    .HaveNoCycles();

await rule.CheckAsync();
```

### Graph Visualization
```csharp
// Export dependency graph
var graph = ArchUnit.ProjectGraph()
    .AddEdges(edges)
    .CollapseToFolderDepth(2);

await graph.ExportToFileAsync(
    GraphExportFormat.Mermaid, 
    "dependency-graph.md"
);
```

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
