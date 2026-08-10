# Contributing to ArchUnitCSharp

Thank you for your interest in contributing to ArchUnitCSharp! This document provides guidelines and instructions for contributing to the project.

## Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Report issues responsibly
- Help others learn and grow

## Getting Started

### Prerequisites
- .NET 8.0 or later
- C# 10+ knowledge
- Git and GitHub account

### Development Setup

```bash
# Clone the repository
git clone https://github.com/LukasNiessen/ArchUnitNET.git
cd ArchUnitNET/ArchUnitNET

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Debug

# Run tests
dotnet test -c Debug
```

## Development Workflow

### 1. Create a Branch

```bash
# Create feature branch
git checkout -b feat/your-feature

# Or fix branch
git checkout -b fix/issue-description
```

**Branch naming conventions:**
- `feat/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `test/` - Test additions/improvements
- `chore/` - Build, CI/CD, dependencies
- `refactor/` - Code refactoring

### 2. Make Changes

Follow these guidelines:

#### Code Style
- Use **latest C# language features** (C# 10+)
- Enable **nullable reference types** (`#nullable enable`)
- Use **records** for immutable data
- Prefer **readonly** properties
- Follow **PascalCase** for public APIs

#### Documentation
- Add XML doc comments to **all public members**
- Include `<summary>`, `<param>`, `<returns>`, `<example>` tags
- Keep comments short and focused on **WHY**, not **WHAT**

#### Testing
- Write tests for **all new features**
- Test **happy path** and **edge cases**
- Use **xUnit** for unit tests
- Test names should describe the scenario: `MethodName_Scenario_Expected`

```csharp
[Fact]
public void Extract_WithValidSyntax_ReturnsCorrectInfo()
{
    // Arrange
    var source = "public class Test { }";
    var extractor = new ClassInfoExtractor(source);

    // Act
    var result = extractor.Extract();

    // Assert
    Assert.NotNull(result);
}
```

### 3. Run Quality Checks

```bash
# Format code
dotnet format

# Build Release
dotnet build -c Release

# Run all tests
dotnet test -c Release

# Check for code quality issues
dotnet build -c Release /p:EnforceCodeStyleInBuild=true /p:EnableNETAnalyzers=true
```

### 4. Commit Changes

```bash
# Stage files
git add src/ArchUnitNet/YourFile.cs

# Commit with descriptive message
git commit -m "feat: add new feature description

Detailed explanation of what was changed and why.

Fixes #123"
```

**Commit message format:**
```
<type>(<scope>): <subject>

<body>

Fixes #<issue-number>
```

**Types:**
- `feat` - New feature
- `fix` - Bug fix
- `docs` - Documentation
- `test` - Tests
- `chore` - Build/CI/dependencies
- `refactor` - Code refactoring

**Scopes:**
- `common` - Common module
- `files` - File-based rules
- `metrics` - Metrics module
- `slices` - Slicing module
- `graph` - Graph reporting

### 5. Push and Create Pull Request

```bash
# Push your branch
git push origin feat/your-feature
```

Then create a Pull Request on GitHub with:
- **Clear title** describing the change
- **Description** explaining why and what changed
- **Tests** for new functionality
- **References** to related issues

## Pull Request Guidelines

### Required Checks
- ✅ All tests pass (CI/CD pipeline)
- ✅ Code coverage maintained or improved
- ✅ No compilation warnings
- ✅ StyleCop compliance
- ✅ Code review approval

### PR Template

```markdown
## Description
Brief description of the changes.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe how this was tested.

- [ ] Unit tests added
- [ ] Integration tests added
- [ ] Manual testing performed

## Checklist
- [ ] Code follows style guidelines
- [ ] Documentation updated
- [ ] Tests added/updated
- [ ] CHANGELOG.md updated
```

## Project Structure

```
ArchUnitNET/
├── src/ArchUnitNet/
│   ├── Common/              # Shared utilities
│   ├── Files/               # File-based rules
│   ├── Metrics/             # Code metrics
│   ├── Slices/              # Architecture slicing
│   └── Graph/               # Graph reporting
├── tests/ArchUnitNet.Tests/
│   ├── Common/
│   ├── Files/
│   ├── Metrics/
│   ├── Slices/
│   └── Graph/
├── .github/workflows/       # CI/CD pipelines
└── docs/                    # Documentation
```

## Architecture Layers

1. **Layer 0**: Core types (Error, Violation)
2. **Layer 1**: Utilities (Path, Logging)
3. **Layer 2**: Extraction (Roslyn-based)
4. **Layer 3**: Projections (Cycles, Slices)
5. **Layer 4**: Rules & Builders (Fluent API)
6. **Layer 5**: Testing Integration

## Testing Strategy

### Unit Tests (Layer 0-2)
- Test individual components in isolation
- Mock external dependencies
- Verify edge cases

### Integration Tests (Layer 3-4)
- Test component interactions
- Use realistic data
- Verify end-to-end workflows

### Test Fixtures
Use sample projects for realistic scenarios:
- `AngularLike` - Public API boundaries
- `SimpleProject` - Cycle detection
- `LayeredArch` - Layered architecture
- `MetricsTestProject` - LCOM cohesion

## Performance Considerations

### Graph Operations
- Tarjan's algorithm: **O(V+E)** for SCC
- Johnson's algorithm: **O(V(V+E))** for all cycles
- Cache results when possible

### Metrics Calculation
- LCOM: **O(M²×F)** where M=methods, F=fields
- Reuse matrices for multiple calculations

## Security

- No hardcoded secrets
- Use GitHub Secrets for sensitive data
- Verify NuGet packages before consuming
- Report security issues privately

## Documentation

### Code Comments
- Explain **WHY**, not **WHAT**
- Keep comments short and focused
- Remove obsolete comments

### API Documentation
```csharp
/// <summary>
/// Extract class information from syntax tree.
/// </summary>
/// <param name="classDeclaration">The class syntax to analyze.</param>
/// <returns>Extracted class information with metrics.</returns>
/// <example>
/// <code>
/// var extractor = new ClassInfoExtractor(classDecl);
/// var info = extractor.Extract();
/// </code>
/// </example>
public ClassInfo Extract(ClassDeclarationSyntax classDeclaration)
{
    // ...
}
```

## Release Process

1. **Version bump** in `Directory.Build.props`
2. **Update** `CHANGELOG.md`
3. **Tag** release: `git tag v2.4.0`
4. **Push** tag: `git push origin v2.4.0`
5. **GitHub Actions** publishes to NuGet automatically

## Questions?

- **Issues**: Use GitHub Issues for bug reports
- **Discussions**: Use GitHub Discussions for questions
- **Email**: Contact maintainers directly

## License

By contributing, you agree your code will be licensed under Apache 2.0.

Thank you for contributing! 🎉
