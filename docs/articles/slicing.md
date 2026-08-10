# Architecture Slicing

Architecture slicing divides your codebase into logical components based on patterns. Use it to enforce layered or feature-based architecture.

## Overview

Slicing extracts logical "slices" from your file structure and validates relationships between them:

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/{Slice}/**")        // Define slices by pattern
    .Should()
    .AdhereToDefinedSlices();           // Validate no cross-slice violations

await rule.CheckAsync();
```

## Slice Definition

### Basic Pattern

Slices are defined by extracting named groups from file paths:

```csharp
// Pattern: src/{Slice}/**
// Extracts: src/Feature1/** → Slice = "Feature1"
//          src/Feature2/** → Slice = "Feature2"
//          src/Feature3/** → Slice = "Feature3"

var rule = ProjectSlices()
    .DefinedBy("src/{Slice}/**")
    .Should()
    .AdhereToDefinedSlices();
```

**Result**: Creates slices named "Feature1", "Feature2", "Feature3"

### Multiple Capture Groups

Define hierarchical slices:

```csharp
// Pattern: src/{Layer}/{Feature}/**
// Extracts: src/UI/Orders/** → Layer="UI", Feature="Orders"
//          src/UI/Users/**  → Layer="UI", Feature="Users"
//          src/Service/Orders/** → Layer="Service", Feature="Orders"

var rule = ProjectSlices()
    .DefinedBy("src/{Layer}/{Feature}/**")
    .Should()
    .AdhereToDefinedSlices();
```

### Nested Patterns

Create deep slicing hierarchies:

```csharp
// Pattern: src/{Feature}/modules/{Module}/**
// Extracts: src/Orders/modules/Shipping/** → Feature="Orders", Module="Shipping"
//          src/Orders/modules/Invoicing/** → Feature="Orders", Module="Invoicing"

var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/modules/{Module}/**")
    .Should()
    .AdhereToDefinedSlices();
```

## Common Slicing Patterns

### Pattern 1: Feature-Based Architecture

Each feature is a slice with no cross-feature dependencies:

```csharp
// File structure:
// src/
//   Features/
//     Orders/
//       OrderService.cs
//       OrderRepository.cs
//     Users/
//       UserService.cs
//       UserRepository.cs
//     Products/
//       ProductService.cs

// Define slices
var rule = ProjectSlices()
    .DefinedBy("src/Features/{Feature}/**")
    .Should()
    .AdhereToDefinedSlices();

// Validates: Orders doesn't depend on Users or Products
//            Users doesn't depend on Orders or Products
//            Products doesn't depend on Orders or Users
```

### Pattern 2: Layered Architecture

Enforce strict layer separation:

```csharp
// File structure:
// src/
//   Presentation/
//   Business/
//   Data/

// Define slices
var rule = ProjectSlices()
    .DefinedBy("src/{Layer}/**")
    .Should()
    .AdhereToDefinedSlices();

// Standard rule: Data → Business → Presentation (one direction only)
// This rule would detect ANY cross-layer dependency
```

### Pattern 3: Domain-Driven Design (DDD)

Bounded contexts as slices:

```csharp
// File structure:
// src/
//   Catalog/
//     Domain/
//     Application/
//     Infrastructure/
//   Orders/
//     Domain/
//     Application/
//     Infrastructure/

// Define slices
var rule = ProjectSlices()
    .DefinedBy("src/{BoundedContext}/**")
    .Should()
    .AdhereToDefinedSlices();

// Validates: Catalog is independent from Orders
```

### Pattern 4: Microservices

Each service as a slice:

```csharp
// Repository structure:
// services/
//   UserService/
//     src/
//   OrderService/
//     src/
//   ProductService/
//     src/

// Define slices
var rule = ProjectSlices()
    .DefinedBy("services/{Service}/**")
    .Should()
    .AdhereToDefinedSlices();

// Validates: No direct service-to-service dependencies (use APIs instead)
```

## Validation Rules

### Rule 1: Adhere to Defined Slices

Prevent all cross-slice dependencies:

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/{Slice}/**")
    .Should()
    .AdhereToDefinedSlices();

// violations: any file in one slice importing from another slice
```

**Use case**: Strict feature isolation

### Rule 2: Specific Allowed Dependencies (Future)

```csharp
// Not yet available:
var rule = ProjectSlices()
    .DefinedBy("src/{Slice}/**")
    .Should()
    .ContainDependency("Orders", "Products")  // Orders can depend on Products
    .And()
    .ContainDependency("Orders", "Common");   // Orders can also depend on Common

// violations: dependency not in allowed list
```

## Advanced Scenarios

### Excluding Files from Slices

Files that shouldn't be sliced:

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/Features/{Feature}/**")
    .Except("src/Common/**")              // Shared code (all slices can use)
    .Except("src/**/index.cs")            // Barrel exports
    .Except("src/**/Models/**")           // Shared models
    .Should()
    .AdhereToDefinedSlices();

// Now Common, index.cs, and Models are shared across all slices
```

### Hierarchical Slicing

Define slices at different levels:

```csharp
// Method 1: Strict feature isolation
var featureIsolation = ProjectSlices()
    .DefinedBy("src/{Feature}/**")
    .Should()
    .AdhereToDefinedSlices();

// Method 2: Allow layer dependencies within features
var layerViolations = ProjectFiles("./MyProject.csproj")
    .InPath("src/**/Presentation/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/**/Data/**");

var violations = new[]
{
    await featureIsolation.CheckAsync(),
    await layerViolations.CheckAsync()
};
```

### Custom Slice Names

Map patterns to meaningful names:

```csharp
// Pattern: src/Features/{Feature}/{SubModule}/**
// Files in: src/Features/Orders/Shipping/
// Becomes: Slice "Orders::Shipping"

var rule = ProjectSlices()
    .DefinedBy("src/Features/{Feature}/{SubModule}/**")
    .Should()
    .AdhereToDefinedSlices();

// In violation output: "Orders::Shipping violated by importing from Users"
```

## Debugging Slices

### Viewing Extracted Slices

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/**")
    .Should()
    .AdhereToDefinedSlices();

var violations = await rule.CheckAsync();

foreach (var violation in violations)
{
    if (violation is ViolatingSliceEdge sliceViolation)
    {
        Console.WriteLine($"Source Slice: {sliceViolation.SourceSlice}");
        Console.WriteLine($"Target Slice: {sliceViolation.TargetSlice}");
        Console.WriteLine($"Files: {sliceViolation.Source} → {sliceViolation.Target}");
    }
}
```

### Exporting Slice Graph

```csharp
// Export architecture showing slices
var graph = ProjectGraph("./MyProject.csproj")
    .CollapseToFolderDepth(2)
    .IncludeSlices(new[] { "Orders", "Users", "Products" });

await graph.ExportToFileAsync(GraphFormat.Mermaid, "slices.md");
// Shows slice relationships visually
```

## Performance Considerations

### Large Number of Slices

For codebases with 100+ slices:

```csharp
// Option 1: Validate specific slices only
var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/**")
    .For(new[] { "Orders", "Users", "Products" })  // Validate only these
    .Should()
    .AdhereToDefinedSlices();

// Option 2: Use broader pattern to reduce slices
var rule = ProjectSlices()
    .DefinedBy("src/{Area}/{Feature}/**")  // Merge into areas first
    .Should()
    .AdhereToDefinedSlices();
```

### Pattern Compilation

Patterns are compiled once and cached:

```csharp
var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/**");

var violations1 = await rule.CheckAsync();  // Compiles pattern
var violations2 = await rule.CheckAsync();  // Uses cached compiled pattern
```

## Testing

Integrate slicing into your test suite:

```csharp
[TestFixture]
public class ArchitectureTests
{
    [Test]
    public async Task FeaturesAreIsolated()
    {
        var rule = ProjectSlices()
            .DefinedBy("src/Features/{Feature}/**")
            .Should()
            .AdhereToDefinedSlices();

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty,
            $"Found {violations.Count} cross-feature dependencies");
    }

    [Test]
    public async Task SharedCodeIsNotDuplicated()
    {
        var rule = ProjectFiles("./MyProject.csproj")
            .InPath("src/Features/*/Models/**")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Features/*/Models/**");

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty,
            "Model files from different features should use Common namespace");
    }

    [Test]
    public async Task AllFeaturesHaveRequiredStructure()
    {
        var requiredFiles = new[]
        {
            "src/Features/{Feature}/Service.cs",
            "src/Features/{Feature}/Repository.cs",
            "src/Features/{Feature}/Models.cs"
        };

        foreach (var pattern in requiredFiles)
        {
            // Validate all features have required files
            // Implementation depends on file system APIs
        }
    }
}
```

## Comparison with File-Based Rules

| Approach | Use Case | Flexibility | Complexity |
|----------|----------|---|---|
| **File-Based Rules** | Specific relationships | High | Low |
| **Slicing** | Architectural patterns | Medium | Medium |

**When to use slicing**:
- Enforcing feature isolation
- DDD bounded contexts
- Microservice independence

**When to use file-based rules**:
- Layered architecture
- Specific dependency paths
- Complex filtering

**Combine both** for comprehensive architecture validation:

```csharp
// Step 1: Features are isolated
var featureRule = ProjectSlices()
    .DefinedBy("src/Features/{Feature}/**")
    .Should()
    .AdhereToDefinedSlices();

// Step 2: Within features, enforce layers
var layerRule = ProjectFiles("./MyProject.csproj")
    .InPath("src/Features/*/Presentation/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Features/*/Data/**");

// Step 3: No circular dependencies
var cycleRule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**")
    .Should()
    .HaveNoCycles();

var violations = new[]
{
    await featureRule.CheckAsync(),
    await layerRule.CheckAsync(),
    await cycleRule.CheckAsync()
};
```

## Troubleshooting

### Issue: Pattern doesn't match any files

```csharp
// ❌ Wrong pattern
var rule = ProjectSlices()
    .DefinedBy("features/{Feature}/**");  // No src/ prefix

// ✅ Correct
var rule = ProjectSlices()
    .DefinedBy("src/features/{Feature}/**");
```

### Issue: Capture group not extracting correctly

```csharp
// ❌ Wrong
var rule = ProjectSlices()
    .DefinedBy("src/Features/**/{Feature}/**");  // Extra **

// ✅ Correct
var rule = ProjectSlices()
    .DefinedBy("src/Features/{Feature}/**");
```

### Issue: Too many or too few slices

Check your pattern:

```csharp
// If getting too many slices, move ** earlier in pattern
var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/**");  // Groups by immediate subfolder

// If getting too few slices, add more nesting
var rule = ProjectSlices()
    .DefinedBy("src/{Feature}/{Module}/**");  // Groups by feature + module
```

---

See also:
- [Getting Started](getting-started.md) — Quick start guide
- [File-Based Rules](file-rules.md) — Dependency validation
- [Metrics Analysis](metrics.md) — Code quality rules
- [Graph Visualization](graph-reporting.md) — Export dependencies
