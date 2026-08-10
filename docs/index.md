# ArchUnitCSharp Documentation

Welcome to ArchUnitCSharp - A C#/.NET port of ArchUnitTS for architecture testing.

## Quick Links

- [Getting Started](articles/getting-started.md)
- [API Reference](api/index.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [Changelog](../CHANGELOG.md)

## What is ArchUnitCSharp?

ArchUnitCSharp is a powerful architecture testing framework for C#/.NET applications. It allows you to:

- **Define architecture rules** using a fluent API
- **Validate dependencies** between layers and modules
- **Detect cycles** in your codebase automatically
- **Measure code cohesion** with LCOM metrics
- **Visualize dependencies** in multiple formats (Mermaid, DOT, D2, HTML, JSON, CSV)

## Key Features

### 🏗️ Architecture Rules
Define and enforce architectural boundaries:
- File-based dependencies
- Layer isolation
- Module independence
- Cycle detection

### 📊 Code Metrics
Analyze code quality:
- LCOM (Lack of Cohesion) calculation
- Cyclomatic complexity
- Method and field access patterns
- Cohesion scoring

### 🔄 Dependency Analysis
Understand your codebase:
- Dependency graph extraction (via Roslyn)
- Cycle detection (Tarjan's SCC + Johnson's algorithms)
- Path normalization (Windows/Unix)
- Import classification

### 📈 Visualization
Generate dependency graphs:
- Mermaid diagrams
- Graphviz DOT format
- D2 diagram language
- HTML interactive views
- JSON and CSV exports

## Installation

```bash
dotnet add package ArchUnitCSharp
```

## Quick Example

```csharp
using ArchUnitNet;

// Define a rule
var rule = ArchUnit.Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

// Check the rule
var violations = await rule.CheckAsync();

if (violations.Count > 0)
{
    foreach (var violation in violations)
    {
        Console.WriteLine(violation);
    }
}
```

## Learn More

- [Getting Started Guide](articles/getting-started.md)
- [File-Based Rules](articles/file-rules.md)
- [Metrics Analysis](articles/metrics.md)
- [Slice-Based Architecture](articles/slicing.md)
- [Graph Visualization](articles/graph-reporting.md)

## Project Status

**Current Version**: 2.4.0  
**Status**: Active Development  
**License**: Apache 2.0

### Latest Release
- ✨ Phase 3b: Metrics FluentAPI
- ✨ Phase 3c: Slicing Module
- ✨ Phase 4a: Graph Reporting (6 formats)
- ✨ Phase 4b: Testing Integration

## Contributing

We welcome contributions! See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

## Support

- 📖 [Documentation](https://archunitcsharp.dev)
- 🐛 [Report Issues](https://github.com/LukasNiessen/ArchUnitTS/issues)
- 💬 [Discussions](https://github.com/LukasNiessen/ArchUnitTS/discussions)
- 📧 [Contact](mailto:info@example.com)

---

Made with ❤️ by the ArchUnit community
