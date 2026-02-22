# NuGet Feed Configuration

## Overview

This document explains the NuGet feed configuration for the Mystira Admin API project.

## Problem Statement

The project references internal Mystira packages (e.g., `Mystira.Domain`, `Mystira.Application`, etc.) that are hosted in GitHub Packages. The repository has migrated from Azure DevOps to GitHub for package hosting, following [ADR-0007: NuGet Feed Strategy for Shared Libraries](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md).

## Solution

The project now uses **GitHub Packages** as the primary source for internal Mystira packages. The CI/CD workflow automatically authenticates using the `GITHUB_TOKEN` that is available in GitHub Actions.

## Implementation Details

### NuGet.config

The `NuGet.config` file at the repository root is configured with two package sources and package source mapping:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/phoenixvc/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="github">
      <package pattern="Mystira.*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
```

**Note:** The `NuGet.config` file does not include `packageSourceCredentials` to avoid committing sensitive tokens. Authentication is handled differently for CI/CD and local development (see sections below).

### CI Workflow Changes

The `.github/workflows/ci.yml` file configures GitHub Packages authentication in all jobs (lint, test, build):

```yaml
- name: Configure GitHub Packages NuGet Feed
  env:
    GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    echo "Configuring GitHub Packages NuGet feed..."
    dotnet nuget update source github \
      --username phoenixvc \
      --password "$GITHUB_TOKEN" \
      --store-password-in-clear-text \
      --configfile NuGet.config
```

### Workflow Permissions

The workflow includes the necessary permissions to read packages:

```yaml
permissions:
  contents: read
  packages: read
```

## Configuration for Different Environments

### CI/CD (GitHub Actions)

GitHub Actions automatically provides the `GITHUB_TOKEN` with appropriate permissions:

- **No additional secrets required** - `GITHUB_TOKEN` is automatically available
- **Automatic authentication** - The workflow configures the feed using the token
- **Read access** - The token has read access to packages in the phoenixvc organization

### Local Development

For local development, you need to create a Personal Access Token (PAT) with `read:packages` scope:

#### Step 1: Create a GitHub PAT

1. Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Give it a name (e.g., "NuGet Package Access")
4. Select the `read:packages` scope
5. Click "Generate token" and copy the token

#### Step 2: Configure Local NuGet Authentication

Since `NuGet.config` uses package source mapping without embedded credentials, you need to configure authentication separately. You have two options:

**Option A: Environment Variable (Recommended)**

Set the token as an environment variable. NuGet will automatically use it for authentication:

**Windows (PowerShell):**
```powershell
$env:NUGET_AUTH_TOKEN = "your-github-pat-here"
# Or use GITHUB_TOKEN (both work)
$env:GITHUB_TOKEN = "your-github-pat-here"
```

**macOS/Linux (Bash):**
```bash
export NUGET_AUTH_TOKEN="your-github-pat-here"
# Or use GITHUB_TOKEN (both work)
export GITHUB_TOKEN="your-github-pat-here"
```

**Option B: User-Level NuGet.config**

Add credentials to your user-level `NuGet.config` (see "Alternative: User-Level Configuration" section below).

#### Step 3: Restore Packages

```bash
dotnet restore
```

NuGet will use the environment variable or user-level config to authenticate with GitHub Packages.

#### Alternative: User-Level Configuration

You can also configure credentials in your user-level `NuGet.config`:

**Windows:** `%APPDATA%\NuGet\NuGet.config`  
**macOS/Linux:** `~/.nuget/NuGet/NuGet.config`

```xml
<configuration>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="your-github-username" />
      <add key="ClearTextPassword" value="your-github-pat" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## Package Sources

### Primary Sources

1. **nuget.org** - Public packages (e.g., Microsoft.AspNetCore, Entity Framework)
2. **GitHub Packages (phoenixvc)** - Internal Mystira packages

### Internal Packages

The following packages are hosted on GitHub Packages:

- `Mystira.Domain`
- `Mystira.Application`
- `Mystira.Contracts`
- `Mystira.Infrastructure.Azure`
- `Mystira.Infrastructure.Data`
- `Mystira.Infrastructure.Discord`
- `Mystira.Infrastructure.StoryProtocol`
- `Mystira.Shared`

## Error Messages

### Authentication Errors

**401 Unauthorized:**
```
error NU1301: Unable to load the service index for source https://nuget.pkg.github.com/phoenixvc/index.json
error NU1301: Response status code does not indicate success: 401 (Unauthorized).
```

**Solution:** 
- For CI: Ensure the workflow has `packages: read` permission
- For local: Verify your `GITHUB_TOKEN` environment variable is set correctly with a valid PAT

### Package Not Found

**NU1101:**
```
error NU1101: Unable to find package Mystira.Domain. No packages exist with this id in source(s): nuget.org, github
```

**Solution:** 
- Verify the package has been published to GitHub Packages
- Check the package version in your `.csproj` matches an available version
- Ensure you have access to the phoenixvc organization packages

## Migration from Azure DevOps

This repository has been migrated from Azure DevOps to GitHub Packages. The following changes were made:

### Removed
- ❌ Azure DevOps feed configuration
- ❌ Azure DevOps secrets (`MYSTIRA_DEVOPS_AZURE_ORG`, `MYSTIRA_DEVOPS_AZURE_PAT`, etc.)
- ❌ Conditional Azure DevOps feed setup in CI workflow

### Added
- ✅ GitHub Packages feed configuration in `NuGet.config`
- ✅ Automatic `GITHUB_TOKEN` authentication in CI workflow
- ✅ Workflow permissions for package access
- ✅ Documentation for local development setup

## Benefits

✅ **Unified Platform** - Source code and packages on GitHub  
✅ **Simplified CI/CD** - No manual secret configuration required  
✅ **Automatic Authentication** - `GITHUB_TOKEN` works out of the box  
✅ **Better Access Control** - Leverages GitHub organization permissions  
✅ **Standard Practice** - Aligns with .NET ecosystem conventions

## Related Documentation

- [ADR-0007: NuGet Feed Strategy for Shared Libraries](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0006: Admin API Repository Extraction](./architecture/adr/0006-admin-api-repository-extraction.md)
- [GitHub Packages Documentation](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)

## Related Files

- `.github/workflows/ci.yml` - CI workflow with GitHub Packages configuration
- `NuGet.config` - Package source configuration
- `README.md` - Setup instructions
- `docs/cicd/README.md` - CI/CD documentation
