# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.4.0] - 2026-08-09

### Added
- ✨ **Phase 3b: Metrics FluentAPI** - MetricsBuilder, LCOMThresholdBuilder, CountMetricsBuilder
  - LCOM calculation (4 variants: LCOM1, LCOM96a, LCOM96b, LCOM1995)
  - Fluent API for metrics validation
  - Threshold-based assertions

- ✨ **Phase 3c: Slicing Module** - Architecture slicing support
  - SliceProjector for pattern-based slice extraction
  - SliceConditionBuilder for slice rule definitions
  - Slice dependency tracking and validation

- ✨ **Phase 4a: Graph Reporting** - Multi-format dependency visualization
  - Export to: Mermaid, DOT, D2, CSV, JSON, HTML
  - Graph filtering (external deps, folder collapse, focus)
  - ProjectGraphBuilder fluent API

- ✨ **Phase 4b: Testing Integration** - Framework adapters
  - ResultFactory for test result creation
  - XUnit extension methods
  - NUnit custom assertions

### Fixed
- 🐛 Fixed Edge constructor type mismatches (16 occurrences)
- 🐛 Fixed missing ImportKind using statements
- 🐛 Fixed Graph namespace conflicts with GraphReporting
- 🐛 Fixed ThresholdViolation import issues
- 🐛 Fixed TestResult record property casing

### Changed
- 📝 Renamed Graph namespace to GraphReporting to avoid conflicts
- 🔧 Updated Roslyn version to 4.12.0 for better compatibility
- 📦 Added comprehensive Directory.Build.props settings

### Testing
- ✅ 200+ unit and integration tests
- ✅ 25 test files with comprehensive coverage
- ✅ All modules tested (Common, Files, Metrics, Slices, Graph)
- ✅ Zero compilation errors after fixes

## [2.3.0] - 2026-07-01

### Added
- ✨ Cycle detection (Tarjan's SCC + Johnson's elementary cycles)
- ✨ File-based architecture rules
- ✨ Pattern matching with glob + regex support
- ✨ Graph-based dependency projection

### Changed
- 📝 Improved README with dependency graph visualization
- 🔧 Added CODEOWNERS for required reviews

## [2.2.0] - 2026-06-15

### Added
- ✨ Metrics foundation (FieldInfo, MethodInfo, ClassInfo)
- ✨ LCOM calculator implementation
- ✨ Metrics extraction from Roslyn syntax trees

## [2.1.0] - 2026-06-01

### Added
- ✨ Basic project structure and architecture
- ✨ Core modules (Common, Files, Metrics, Slices, Graph)
- ✨ Roslyn-based code analysis

---

## Unreleased

### Planned for v3.0.0
- [ ] Assembly-level rule validation
- [ ] Dependency graph optimization
- [ ] Advanced caching strategies
- [ ] Plugin system for custom rules
- [ ] IDE integration (Visual Studio extensions)
- [ ] Real-time architecture monitoring

### Known Issues
- Roslyn 4.12.0 requires .NET 8.0+ runtime (net7.0 not available in test environment)
- Some test fixtures require actual .NET runtime to execute

## Version Format

```
MAJOR.MINOR.PATCH
- MAJOR: Breaking changes or significant new features
- MINOR: New functionality, non-breaking
- PATCH: Bug fixes and improvements
```
