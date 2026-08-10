# File-Based Architecture Rules

File-based rules are the most common way to enforce architectural boundaries in ArchUnitCSharp. They work on source file dependencies extracted via Roslyn.

## Overview

A file-based rule validates that certain files/folders follow specific dependency patterns:

```csharp
var rule = ProjectFiles(projectPath)
    .InPath(sourcePattern)              // Step 1: Select source files
    .Should() or .ShouldNot()           // Step 2: Specify expectation
    .DependOnFiles()                    // Step 3: Define relationship
    .InFolder(targetPattern);           // Step 4: Specify target

await rule.CheckAsync();
```

## File Selection

### By Folder

Select all files within a folder hierarchy:

```csharp
ProjectFiles("./MyProject.csproj")
    .InFolder("src/UI/**")              // All files in UI folder
    .Should()
    .DependOnFiles()
    .InFolder("src/Common/**");
```

### By Path Pattern

Use glob patterns for precise selection:

```csharp
ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*.cs")              // All .cs files
    .InPath("src/Features/*/Service.cs") // Specific file naming pattern
    .Should()
    .DependOnFiles()
    .InFolder("src/Domain/**");
```

### By Regex

For complex patterns, use regex:

```csharp
ProjectFiles("./MyProject.csproj")
    .InPath(@"src/(UI|Components)/.*\.cs$")  // Regex pattern
    .Should()
    .DependOnFiles()
    .InFolder(@"src/Services/.*\.cs$");
```

### With Exclusions

Exclude patterns from selection:

```csharp
ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*.cs")
    .Except("src/**/Generated.cs")      // Exclude generated files
    .Except("src/**/Models/**")         // Exclude model classes
    .Should()
    .DependOnFiles()
    .InFolder("src/Domain/**");
```

## Dependency Rules

### Rule 1: Should Depend On

Enforce that selected files only depend on specific targets:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .Should()
    .DependOnFiles()
    .InFolder("src/Components/**");

var violations = await rule.CheckAsync();
// violations: any files in UI depending on things OUTSIDE Components
```

Use case: UI layer should only use component library.

### Rule 2: Should NOT Depend On

Enforce that selected files avoid specific dependencies:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/Domain/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Infrastructure/**");

var violations = await rule.CheckAsync();
// violations: any files in Domain importing from Infrastructure
```

Use case: Domain model should not depend on infrastructure.

### Rule 3: Should Have No Cycles

Detect circular dependencies automatically:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**/*.cs")
    .Should()
    .HaveNoCycles();

var violations = await rule.CheckAsync();
// violations: each cyclic dependency group
// e.g., "A → B → C → A"
```

**Algorithm**: Tarjan's Strongly Connected Components (O(V+E))  
**Limitations**: None (finds all elementary cycles)

### Rule 4: Match Pattern

Advanced pattern matching for complex scenarios:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/Features/*/index.cs")  // Barrel exports
    .Should()
    .MatchPattern(@"Features/[^/]+/index\.cs");

var violations = await rule.CheckAsync();
// violations: index.cs files not following barrel export pattern
```

## Advanced Patterns

### Layered Architecture (3-tier)

```csharp
var uiNotUsingData = ProjectFiles("./MyProject.csproj")
    .InPath("src/Presentation/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Data/**");

var businessNotUsingPresentation = ProjectFiles("./MyProject.csproj")
    .InPath("src/Business/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Presentation/**");

var violations = new[]
{
    await uiNotUsingData.CheckAsync(),
    await businessNotUsingPresentation.CheckAsync()
};
```

### Hexagonal (Ports & Adapters)

```csharp
var adaptersShouldUsePort = ProjectFiles("./MyProject.csproj")
    .InPath("src/Adapters/**")
    .Should()
    .DependOnFiles()
    .InFolder("src/Ports/**");

var domainNotUsingAdapters = ProjectFiles("./MyProject.csproj")
    .InPath("src/Domain/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Adapters/**");
```

### Feature-Isolation (SCAM)

```csharp
// Features should not cross-depend
var featureA = ProjectFiles("./MyProject.csproj")
    .InPath("src/Features/A/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Features/B/**");

var featureB = ProjectFiles("./MyProject.csproj")
    .InPath("src/Features/B/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Features/A/**");
```

## Import Types

ArchUnitCSharp categorizes imports by type:

```csharp
public enum ImportKind
{
    Using,              // using statement
    ProjectReference,   // <ProjectReference> in .csproj
    PackageReference,   // NuGet package
    FrameworkReference  // .NET framework/runtime
}
```

Filter by import type (future enhancement):

```csharp
// Currently all import types are considered
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/Domain/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Infrastructure/**");
    // .OfImportKind(ImportKind.ProjectReference)  // Future API
```

## Debugging Violations

### View Violation Details

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Internal/**");

var violations = await rule.CheckAsync();

foreach (var violation in violations)
{
    if (violation is ViolatingFileDependency fileDep)
    {
        Console.WriteLine($"File: {fileDep.Source}");
        Console.WriteLine($"Imports: {fileDep.Target}");
        Console.WriteLine($"Type: {fileDep.ViolationType}");
    }
    else if (violation is CyclicDependency cycle)
    {
        Console.WriteLine($"Cycle: {string.Join(" → ", cycle.FilesInCycle)}");
    }
}
```

### Export Architecture for Analysis

```csharp
// Export full graph for manual inspection
var graph = ProjectGraph("./MyProject.csproj");
await graph.ExportToFileAsync(GraphFormat.DOT, "architecture.dot");
await graph.ExportToFileAsync(GraphFormat.Mermaid, "architecture.md");

// Use Graphviz or Mermaid to visualize:
// dot -Tsvg architecture.dot -o architecture.svg
// https://mermaid.live (paste architecture.md)
```

## Performance Considerations

### Large Projects (1000+ files)

For better performance:

```csharp
// Option 1: Use folder aggregation
var graph = ProjectGraph("./MyProject.csproj")
    .CollapseToFolderDepth(2);  // Reduces nodes significantly

// Option 2: Exclude external dependencies
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**")
    .ExcludeExternalDependencies()  // Skip NuGet packages
    .Should()
    .HaveNoCycles();

// Option 3: Run less frequently
// Cache results, run only on main branch or nightly
```

### Caching

Results are cached within a single rule execution:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**");

var violations1 = await rule.CheckAsync();  // Full analysis
var violations2 = await rule.CheckAsync();  // Returns cached result
```

For different rules, analysis is repeated. To reuse analysis:

```csharp
// Extract graph once, use for multiple rules
var graph = await ProjectGraph("./MyProject.csproj").BuildAsync();

var cycles = graph.FindCycles();
var dependencies = graph.GetDependencies("src/UI/**");
```

## Common Issues

### Issue: Rule finds violations on legitimate imports

**Solution**: Check your pattern matching:

```csharp
// ❌ Too broad
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/**");  // Blocks ALL internal dependencies!

// ✅ Specific
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/UI/**")
    .ShouldNot()
    .DependOnFiles()
    .InFolder("src/Internal/**");  // Only blocks internal folder
```

### Issue: Circular dependency false positives

**Solution**: Verify the cycle actually exists:

```csharp
var rule = ProjectFiles("./MyProject.csproj")
    .InPath("src/**")
    .Should()
    .HaveNoCycles();

var violations = await rule.CheckAsync();
if (violations.Count > 0)
{
    // Each violation is one cycle
    // Print to verify it's real
    foreach (var v in violations)
    {
        Console.WriteLine(v);
    }
}
```

### Issue: No dependencies found

**Solution**: Ensure project compiles and contains proper `using` statements:

```bash
# First, verify project compiles
dotnet build

# Then, check it has dependencies
grep -r "using " src/
```

## Testing

Integrate file-based rules into your test suite:

```csharp
[TestFixture]
public class ArchitectureTests
{
    private const string ProjectPath = "../../../src/MyProject/MyProject.csproj";

    [Test]
    public async Task DomainShouldNotDependOnInfrastructure()
    {
        var rule = ProjectFiles(ProjectPath)
            .InPath("src/Domain/**")
            .ShouldNot()
            .DependOnFiles()
            .InFolder("src/Infrastructure/**");

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty, 
            $"Domain layer has {violations.Count} illegal dependencies");
    }

    [Test]
    public async Task NoCyclicDependencies()
    {
        var rule = ProjectFiles(ProjectPath)
            .InPath("src/**")
            .Should()
            .HaveNoCycles();

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty, 
            $"Found {violations.Count} circular dependencies");
    }
}
```

---

See also:
- [Getting Started](getting-started.md) — Quick start guide
- [Metrics Analysis](metrics.md) — Code quality rules
- [Architecture Slicing](slicing.md) — Feature-based rules
- [Graph Visualization](graph-reporting.md) — Export dependencies
