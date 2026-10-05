---
title: ArchUnit.NET
---

# ArchUnit.NET

The implementation developed by Robey Beswick as ArchUnitSharp is now maintained in
[LukasNiessen/ArchUnitNET](https://github.com/LukasNiessen/ArchUnitNET). Project names and public
C# namespaces currently remain `ArchUnitSharp`.

Architecture testing for C# and .NET — a fluent, chainable rule DSL over a graph of your project's
files and their dependencies, in the spirit of ArchUnit. A rule reads as a sentence an architect
could write:

```
project files in folder 'src/Web' should not depend on files in folder 'src/Persistence'
```

The library analyses a project's C# source tree into a file-to-file dependency graph from its
`using` directives (resolved with Roslyn's semantic model), and lets a test project assert rules over
that graph — that files sit where they should, that layers only depend on the layers they may, that
slices stay free of forbidden cross-feature edges, that metrics stay under a threshold.

## How this site is organised

Like the sibling libraries (ArchUnitTS, ArchUnitRuby, ArchUnitPython), this site pairs a **guide**
with a **searchable, source-generated API reference** for every public module, class and method.
The guide — [Getting started](guide/getting-started.md), [The fluent grammar](guide/fluent-grammar.md)
and one page per module — is the place to start; the [API reference](api/ArchUnitSharp.yml) is the
generated reference for the exact surface. The site is rebuilt and deployed to GitHub Pages from
`main`, so the published reference follows the repository.

## The modules

The library is one fluent surface over five domain modules, plus the testing adapters that make a
rule terminal assert natively in your suite.

| Module | What it is | Guide |
|---|---|---|
| Files | File and folder based rules: cycles, naming, location, dependencies, custom predicates. | [Files](guide/files.md) |
| Layers | A named-layer policy: allowlists and blocklists over groups of files. | [Layers](guide/layers.md) |
| Slices | Architecture by feature: slicing patterns, forbidden dependencies, PlantUML adherence. | [Slices](guide/slices.md) |
| Graph | Dependency-graph reports in six output formats, narrowed and collapsed to what you want to see. | [Graph](guide/graph.md) |
| Metrics | Count, cohesion (LCOM), distance and custom metrics, each with threshold rules and HTML reports. | [Metrics](guide/metrics.md) |
| Testing | The framework-agnostic assert helper and the native xUnit adapter. | [Testing and configuration](guide/testing.md) |

## Status

This implementation was built with the [ArchUnitDev](https://github.com/RobeyBeswick/ArchUnitDev)
loop and is not yet published to NuGet. Existing `ArchUnit` packages contain the previous
implementation; use this version from source. See [Getting started](guide/getting-started.md) for the
install steps and [Limitations](guide/limitations.md) for what is not implemented yet.
