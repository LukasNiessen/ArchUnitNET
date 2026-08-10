# Getting Started with ArchUnitCSharp

Welcome to ArchUnitCSharp! This guide will help you set up your first architecture test in less than 5 minutes.

## Installation

### Step 1: Install the NuGet Package

```bash
dotnet add package ArchUnitCSharp
```

Or via the Package Manager in Visual Studio:
```
Install-Package ArchUnitCSharp
```

### Step 2: Create Your First Test

Create a new test file in your test project:

```csharp
using ArchUnitNet;
using Xunit;

public class ArchitectureTests
{
    [Fact]
    public async Task FilesShouldFollowLayering()
    {
        var rule = ProjectFiles("./src/MyProject.csproj")
            .InPath("src/**/*.cs")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Internal/**");

        var violations = await rule.CheckAsync();
        Assert.Empty(violations);
    }
}
```

## Core Concepts

### 1. Rule Builder Pattern

ArchUnitCSharp uses a fluent API for building rules:

```csharp
var rule = ProjectFiles(projectPath)
    .InPath(pattern)        // Select files
    .Should()               // Define expectation
    .NotDependOnFiles()     // Specify rule
    .InFolder(otherPattern) // Select dependency target
    // .CheckAsync()        // Execute (shown in examples)
```

**Components**:
- **Selection**: Which files to validate (`.InPath()`, `.InFolder()`)
- **Precondition**: `.Should()` (positive) or `.ShouldNot()` (negative)
- **Rule**: What relationship to enforce (`.DependOnFiles()`, `.HaveNoCycles()`)
- **Target**: What files/patterns the rule applies to
- **Execution**: `.CheckAsync()` returns `Violation[]`

### 2. Violations

Each rule returns a list of violations if checks fail:

```csharp
var violations = await rule.CheckAsync();

foreach (var violation in violations)
{
    Console.WriteLine($"Source: {violation.Source}");
    Console.WriteLine($"Target: {violation.Target}");
    Console.WriteLine($"Type: {violation.ViolationType}");
}
```

### 3. Async-First Design

All operations are async to support non-blocking I/O:

```csharp
// All execution is async
var violations = await rule.CheckAsync();

// Errors are wrapped in TechnicalError or UserError
if (violations.OfType<TechnicalError>().Any())
{
    // Handle errors
}
```

## Common Patterns

### Pattern 1: Layered Architecture

Enforce that UI layer doesn't depend on internal services:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Services/Internal/**");

var violations = await rule.CheckAsync();
if (violations.Count > 0)
{
    throw new AssertionFailedException(
        $"UI layer violates internal service boundary ({violations.Count} violations)");
}
```

### Pattern 2: Cycle Detection

Prevent circular dependencies automatically:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*.cs")
    .Should()
    .HaveNoCycles();

var violations = await rule.CheckAsync();
// violations contains any cyclic dependencies found
```

### Pattern 3: Public API Boundaries

Enforce public/internal separation using index files:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/Features/**/index.cs")
    .Should()
    .DependOnFiles()
    .MatchPattern(@"Features/[^/]+/index\.cs");

var violations = await rule.CheckAsync();
```

### Pattern 4: Code Metrics (LCOM)

Ensure high cohesion in your classes:

```csharp
var rule = Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

var violations = await rule.CheckAsync();
// Finds classes with low cohesion (too many independent method groups)
```

### Pattern 5: Architecture Slicing

Validate layered or feature-based architecture:

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/{Slice}/**")
    .Should()
    .AdhereToDefinedSlices();

var violations = await rule.CheckAsync();
```

## Testing Integration

### With xUnit

```csharp
public class ArchitectureTests
{
    [Fact]
    public async Task CoreLayerShouldNotDependOnUI()
    {
        var rule = ProjectFiles("./src/MyApp.csproj")
            .InPath("src/Core/**")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/UI/**");

        var violations = await rule.CheckAsync();
        Assert.Empty(violations); // Fails if violations found
    }

    [Theory]
    [InlineData("src/Features/*/index.cs")]
    public async Task FeatureIndexFilesShouldExist(string pattern)
    {
        var rule = ProjectFiles("./src/MyApp.csproj")
            .InPath(pattern);
        // Custom validation logic
    }
}
```

### With NUnit

```csharp
[TestFixture]
public class ArchitectureTests
{
    [Test]
    public async Task CoreLayerShouldNotDependOnUI()
    {
        var rule = ProjectFiles("./src/MyApp.csproj")
            .InPath("src/Core/**")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/UI/**");

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty);
    }
}
```

## Advanced Topics

### Working with Patterns

ArchUnitCSharp supports glob patterns with exclusions:

```csharp
// Glob pattern with negation
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*.cs")              // All .cs files
    .Except("src/**/Generated.cs")      // Except generated files
    .Should()
    .NotDependOnFiles()
    .InFolder("src/Internal/**");

// Regex patterns also supported
var regexRule = ProjectFiles("./MyProject.csproj")
    .InPath(@"src/.*Service\.cs$")     // Regex pattern
    .Should()
    .DependOnFiles()
    .InFolder("src/Domain/**");
```

### Visualizing Dependencies

Export your architecture as a diagram:

```csharp
var graph = ProjectGraph("./MyProject.csproj")
    .CollapseToFolderDepth(2)           // Aggregate to folder level
    .ExcludeExternalDependencies();     // Only internal edges

// Export to multiple formats
await graph.ExportToFileAsync(GraphFormat.Mermaid, "architecture.md");
await graph.ExportToFileAsync(GraphFormat.DOT, "architecture.dot");
await graph.ExportToFileAsync(GraphFormat.D2, "architecture.d2");
```

### Custom Analysis

Combine multiple rules:

```csharp
var rules = new[]
{
    ProjectFiles("./MyProject.csproj")
        .InPath("src/UI/**")
        .ShouldNot()
        .DependOnFiles()
        .InFolder("src/Services/Internal/**"),

    ProjectFiles("./MyProject.csproj")
        .InPath("src/**")
        .Should()
        .HaveNoCycles(),

    Metrics()
        .Methods()
        .LCOM96a()
        .ShouldBeLessThan(0.5)
};

var allViolations = new List<Violation>();
foreach (var rule in rules)
{
    var violations = await rule.CheckAsync();
    allViolations.AddRange(violations);
}

if (allViolations.Count > 0)
{
    Console.WriteLine($"Found {allViolations.Count} architecture violations");
}
```

## Troubleshooting

### Issue: "Project file not found"

**Solution**: Ensure the path to your .csproj file is correct and relative to the test execution directory.

```csharp
// Use full path if needed
var rule = ProjectFiles("C:/Full/Path/To/MyProject.csproj");
// Or relative from test project
var rule = ProjectFiles("../../src/MyProject.csproj");
```

### Issue: "No dependencies found"

**Solution**: Verify your project builds successfully and contains `using` statements. ArchUnitCSharp uses Roslyn to extract dependencies from compiled metadata.

### Issue: "Slow analysis on large projects"

**Solution**: ArchUnitCSharp caches results. For large projects:
1. Run analysis once per CI/CD pipeline (not per test)
2. Use `.CollapseToFolderDepth()` to reduce graph complexity
3. Consider splitting into multiple smaller rules

## Next Steps

- Read the [File-Based Rules Guide](file-rules.md) for detailed pattern matching
- Learn about [Metrics Analysis](metrics.md) for code quality
- Explore [Architecture Slicing](slicing.md) for layered designs
- Check out [Graph Visualization](graph-reporting.md) for dependency diagrams

## Examples Repository

For complete working examples, see the test fixtures:
- `tests/Files/Integration/Samples/AngularLike` — Public API boundaries
- `tests/Files/Integration/Samples/SimpleProject` — Cycle detection
- `tests/Files/Integration/Samples/LayeredArch` — Layered architecture
- `tests/Metrics/Samples/MetricsTestProject` — LCOM cohesion analysis

## Questions?

- 📖 [Full API Reference](../api/index.md)
- 🐛 [Report Issues](https://github.com/LukasNiessen/ArchUnitNET/issues)
- 💬 [GitHub Discussions](https://github.com/LukasNiessen/ArchUnitNET/discussions)
- 📧 Contributing? See [CONTRIBUTING.md](../../CONTRIBUTING.md)

---

Happy architecture testing! 🎉
