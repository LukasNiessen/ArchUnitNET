---
title: Getting started
---

# Getting started

## Requirements

The .NET 10 SDK. Every project targets `net10.0`.

## Install

This implementation is not yet published to NuGet; existing `ArchUnit` packages contain the
previous implementation. Clone [the canonical repository](https://github.com/LukasNiessen/ArchUnitNET), then add
the composition root and the assertion adapter to your test project:

```
dotnet add tests/MyApp.Tests/MyApp.Tests.csproj reference src/ArchUnitSharp/ArchUnitSharp.csproj src/ArchUnitSharp.Testing.Xunit/ArchUnitSharp.Testing.Xunit.csproj
```

`ArchUnitSharp` is the entry-point surface — `Project.ProjectFiles()` and the other `Project.*`
nouns. `ArchUnitSharp.Testing.Xunit` makes a rule terminal assert natively under xUnit through
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

Next: [The fluent grammar](fluent-grammar.md).
