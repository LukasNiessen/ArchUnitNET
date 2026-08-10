# CI/CD Setup Checklist

This document outlines the CI/CD pipeline setup for ArchUnitCSharp and what needs to be configured on GitHub.

## ✅ Completed

### Workflow Files Created
- ✅ `.github/workflows/build-and-test.yml` - Build, test, and coverage
- ✅ `.github/workflows/release.yml` - NuGet publishing and GitHub releases
- ✅ `.github/workflows/code-quality.yml` - Code analysis and security checks
- ✅ `.github/workflows/documentation.yml` - DocFX documentation generation

### Configuration Files Created
- ✅ `Directory.Build.props` - Shared project settings
- ✅ `.editorconfig` - Code style enforcement
- ✅ `.gitignore` - Git ignore patterns
- ✅ `docs/docfx.json` - Documentation configuration
- ✅ `CHANGELOG.md` - Release notes template
- ✅ `CONTRIBUTING.md` - Contribution guidelines

## 📋 Next Steps (Manual GitHub Setup Required)

### 1. **Create GitHub Secrets**

You need to add these secrets to your GitHub repository settings:

#### Repository → Settings → Secrets and variables → Actions

**Required Secrets:**

| Secret | Description | Value |
|--------|-------------|-------|
| `NUGET_API_KEY` | NuGet.org API key for publishing packages | Your NuGet API key from https://www.nuget.org/account/apikeys |
| `SONAR_TOKEN` | SonarCloud token for code quality analysis | Optional - get from https://sonarcloud.io |

**Steps to create:**
1. Go to your repository
2. Settings → Secrets and variables → Actions
3. Click "New repository secret"
4. Add `NUGET_API_KEY` with your NuGet API key
5. (Optional) Add `SONAR_TOKEN` for SonarCloud integration

### 2. **Enable GitHub Pages**

For automatic documentation deployment:

1. Settings → Pages
2. Source: `Deploy from a branch`
3. Branch: `gh-pages` (created automatically by workflow)
4. Directory: `/ (root)`
5. Save

### 3. **Enable Branch Protection Rules**

To enforce CI/CD checks:

1. Settings → Branches
2. Add rule for `main` branch:
   - Require pull request reviews
   - Require status checks to pass:
     - `build (ubuntu-latest, 8.0.x)` ✅
     - `build (windows-latest, 8.0.x)` ✅
     - `build (macos-latest, 8.0.x)` ✅
     - `analyze` ✅

### 4. **Configure Code Quality Tools** (Optional)

#### SonarCloud Integration
1. Sign up at https://sonarcloud.io
2. Authorize with GitHub
3. Add your project
4. Create a token and add as `SONAR_TOKEN` secret
5. Workflow will automatically scan on PR/push

#### Codecov Integration
- Currently configured in `build-and-test.yml`
- Works automatically without additional setup
- Generates coverage badges

### 5. **Create Release Tags**

To trigger the release workflow:

```bash
# Make sure you're on main and everything is pushed
git checkout main
git pull origin main

# Create and push a tag
git tag v2.4.0
git push origin v2.4.0
```

This will:
- ✅ Build the release
- ✅ Create NuGet package
- ✅ Publish to NuGet.org
- ✅ Create GitHub Release
- ✅ Deploy documentation to GitHub Pages

## 🔄 Workflow Triggers

| Workflow | Trigger | Actions |
|----------|---------|---------|
| **build-and-test.yml** | Push to main/develop/feat/fix | Build, Test, Coverage, Artifacts |
| **code-quality.yml** | Push/PR to main/develop | StyleCop, FxCop, Security Scan |
| **release.yml** | Push tag `v*.*.*` | Package, Publish NuGet, Release |
| **documentation.yml** | Push to main (docs changes) | Generate & Deploy Docs |

## 📊 Status Badges

Add these to your README.md:

```markdown
[![Build](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions)
[![Code Quality](https://github.com/LukasNiessen/ArchUnitNET/actions/workflows/code-quality.yml/badge.svg)](https://github.com/LukasNiessen/ArchUnitNET/actions)
[![NuGet](https://img.shields.io/nuget/v/ArchUnitCSharp.svg)](https://www.nuget.org/packages/ArchUnitCSharp/)
[![codecov](https://codecov.io/gh/LukasNiessen/ArchUnitNET/branch/main/graph/badge.svg)](https://codecov.io/gh/LukasNiessen/ArchUnitNET)
```

## 🔧 Environment Requirements

| Tool | Version | Purpose |
|------|---------|---------|
| .NET | 8.0+ | Build and test |
| DocFX | Latest | Documentation generation |
| Roslyn | 4.12.0+ | Code analysis |
| xUnit | 2.7.0+ | Testing framework |

## 📝 Maintenance Tasks

### Regular
- [ ] Review and merge dependabot updates
- [ ] Check code coverage trends
- [ ] Monitor security alerts
- [ ] Update CHANGELOG.md for new features

### Per Release
- [ ] Update version in `Directory.Build.props`
- [ ] Update `CHANGELOG.md`
- [ ] Create and push git tag
- [ ] Verify NuGet package published
- [ ] Verify documentation deployed

## 🚀 Deployment Strategy

### Versioning (Semantic)
- `MAJOR.MINOR.PATCH`
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes

### Release Frequency
- Weekly/Bi-weekly minor releases
- ASAP for critical bugs (patch)
- Monthly for MAJOR versions

### Rollback Strategy
- NuGet packages are immutable (can't delete)
- GitHub releases can be unpublished
- Use next patch version to fix critical issues

## ✨ Benefits of This Setup

✅ **Automated Testing** - Run on every push/PR  
✅ **Code Quality** - StyleCop, FxCop, security checks  
✅ **Code Coverage** - Track with Codecov  
✅ **Automated Publishing** - Tag triggers NuGet push  
✅ **Documentation** - Auto-generated and deployed  
✅ **Multiple Platforms** - Test on Linux/Windows/macOS  
✅ **Dependency Scanning** - Security vulnerabilities  
✅ **PR Checks** - Enforce quality standards  

## 🆘 Troubleshooting

### NuGet Publish Fails
- Check `NUGET_API_KEY` secret is set correctly
- Verify API key has push permissions
- Check version number doesn't already exist on NuGet

### Documentation Deploy Fails
- Ensure `gh-pages` branch exists
- Check GitHub Pages settings (Settings → Pages)
- Verify DocFX configuration is correct

### Tests Fail in CI but Pass Locally
- Check .NET version compatibility
- Verify all dependencies are restored
- Check for platform-specific issues (Windows vs. Linux)

### Coverage Not Uploading
- Ensure Codecov action has proper permissions
- Check coverage file format (.cobertura.xml)
- Verify public repository (private needs token)

## 📚 References

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [NuGet Documentation](https://docs.microsoft.com/nuget/)
- [DocFX Documentation](https://dotnet.github.io/docfx/)
- [Codecov Setup](https://docs.codecov.io/)
- [SonarCloud Setup](https://docs.sonarcloud.io/getting-started/github/)

## Summary

This CI/CD pipeline provides:
- 🟢 Continuous Integration (build/test on every push)
- 🟡 Code Quality (analysis and security checks)
- 🔵 Continuous Deployment (automatic NuGet publishing)
- 🟣 Documentation (auto-generated and deployed)

**Next Action**: Set up GitHub Secrets and enable GitHub Pages to complete the setup.
