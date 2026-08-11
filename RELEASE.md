# Release Process

This document describes how to create and publish a new release of ArchUnitNET.

## Prerequisites

- Write access to the GitHub repository
- NuGet API key configured in GitHub Secrets (`NUGET_API_KEY`)

## Release Checklist

### 1. Prepare the Release

- [ ] Update version number in relevant files
- [ ] Review and finalize CHANGELOG.md
- [ ] Ensure all tests pass: `dotnet test`
- [ ] Verify build succeeds: `dotnet build -c Release`

### 2. Create a Git Tag

```bash
# Create an annotated tag (replace X.Y.Z with actual version)
git tag -a vX.Y.Z -m "Release vX.Y.Z - <description>"

# Verify the tag
git tag -l vX.Y.Z

# Push the tag to GitHub
git push origin vX.Y.Z
```

### 3. Create GitHub Release

1. Go to [GitHub Releases](https://github.com/LukasNiessen/ArchUnitNET/releases)
2. Click "Draft a new release"
3. Select your tag (vX.Y.Z)
4. Title: `ArchUnitNET vX.Y.Z`
5. Description: Copy relevant sections from CHANGELOG.md
6. Click "Publish release"

### 4. Automatic NuGet Publishing

When you publish a GitHub release with a tag matching `v*`, the CI/CD workflow automatically:

1. ✅ Builds the project in Release mode
2. ✅ Runs tests
3. ✅ Packs the NuGet package
4. ✅ Publishes to NuGet.org
5. ✅ Creates release artifacts

**No manual intervention needed!**

## Verifying the Release

After publishing:

1. **Check NuGet Package** (5-10 minutes):
   ```bash
   dotnet add package ArchUnitNET --version X.Y.Z
   ```

2. **Verify GitHub Release**:
   - Navigate to [Releases page](https://github.com/LukasNiessen/ArchUnitNET/releases)
   - Confirm package artifacts are attached

3. **Test Installation**:
   ```bash
   dotnet new console -n TestApp
   cd TestApp
   dotnet add package ArchUnitNET --version X.Y.Z
   ```

## Version Numbering

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR** - Breaking API changes
- **MINOR** - New features (backward compatible)
- **PATCH** - Bug fixes

Example: `vX.Y.Z`

## Rollback

If a release has issues:

1. Delete the GitHub release
2. Delete the Git tag: `git tag -d vX.Y.Z`
3. Delete from NuGet: Contact support or use NuGet.org dashboard
4. Create a new release after fixes

## Environment Setup

### GitHub Secrets Required

Set up this secret in your GitHub repository settings:

- **NUGET_API_KEY** - Your NuGet.org API key ([get one here](https://www.nuget.org/account/apikeys))

### Local Development

For local testing without publishing:

```bash
# Pack without publishing
dotnet pack src/ArchUnitNet/ArchUnitNet.csproj -c Release -o ./packages

# Test locally
dotnet add package ArchUnitNET --version X.Y.Z --source ./packages
```

## CI/CD Workflow

The publish workflow (`publish-nuget.yml`) automatically:

- Triggers on GitHub release publish
- Builds the project
- Runs tests
- Extracts version from Git tag
- Creates NuGet package
- Publishes to NuGet.org
- Uploads package as artifact
- Creates release notes

## FAQ

### How often should we release?

- **Patch releases**: As needed for critical fixes
- **Minor releases**: Monthly or when features accumulate
- **Major releases**: Quarterly or when breaking changes needed

### Can I release from a branch other than main?

Yes, but recommended workflow is:
1. Merge all changes to main
2. Create release from main
3. Tag with version

### What if the NuGet publish fails?

The workflow logs are available in GitHub Actions:
1. Go to Actions tab
2. Find the "Publish to NuGet" workflow run
3. Check logs for error details
4. Fix issue and retry by creating a new tag

---

**Last Updated**: 2026-08-11
