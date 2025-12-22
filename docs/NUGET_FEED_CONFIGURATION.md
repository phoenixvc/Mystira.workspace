# NuGet Feed Configuration

## Overview

This document explains the NuGet feed configuration for the Mystira Admin API project.

## Problem Statement

The project references internal Mystira packages (e.g., `Mystira.App.Domain`, `Mystira.App.Application`, etc.) that may be hosted in a private Azure DevOps NuGet feed. Previously, the CI workflow required Azure DevOps credentials to be configured as GitHub secrets, causing failures when these secrets were missing or invalid (401 Unauthorized errors).

## Solution

The Azure DevOps NuGet feed configuration has been made **optional** in the CI/CD workflow. The workflow will:

1. **Check if Azure DevOps secrets exist** - Uses conditional logic to detect if secrets are configured
2. **Skip feed configuration if secrets are missing** - Continues with only nuget.org as the package source
3. **Continue on error** - Even if feed configuration fails, the workflow proceeds

## Implementation Details

### CI Workflow Changes

The `.github/workflows/ci.yml` file was updated in three jobs (lint, test, build):

**Before:**
```yaml
- name: Validate NuGet Feed Secrets
  run: |
    # Fail if secrets are missing
    exit 1

- name: Configure NuGet Feed
  run: |
    dotnet nuget add source ...
```

**After:**
```yaml
- name: Configure NuGet Feed (Optional)
  if: ${{ secrets.MYSTIRA_DEVOPS_AZURE_ORG != '' && secrets.MYSTIRA_DEVOPS_AZURE_PAT != '' }}
  run: |
    echo "Configuring Azure DevOps NuGet feed..."
    dotnet nuget add source ...
  continue-on-error: true
```

### Key Changes

1. **Removed mandatory secret validation** - No longer fails if secrets are missing
2. **Added conditional execution** - Step only runs if secrets exist
3. **Added continue-on-error** - Allows workflow to proceed even if feed configuration fails
4. **Updated step names** - Renamed to "Configure NuGet Feed (Optional)" for clarity

## Configuration Options

### Option 1: Using nuget.org Only (Default)

If you don't configure Azure DevOps secrets, the project will use only nuget.org:

```xml
<!-- NuGet.config -->
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

**Note**: This will work if all Mystira.App.* packages are published to nuget.org. Otherwise, restore will fail with NU1101 (package not found).

### Option 2: Using Azure DevOps Feed

If you have internal packages in Azure DevOps, configure these GitHub secrets:

| Secret | Description | Example |
|--------|-------------|---------|
| `MYSTIRA_DEVOPS_AZURE_ORG` | Organization name | `phoenixvc` |
| `MYSTIRA_DEVOPS_AZURE_PROJECT` | Project name | `Mystira` |
| `MYSTIRA_DEVOPS_AZURE_PAT` | Personal Access Token | `***` (with Packaging:Read scope) |
| `MYSTIRA_DEVOPS_NUGET_FEED` | Feed name | `Mystira-Internal` |

The workflow will automatically detect and configure the feed.

### Local Development

For local development, you can configure the Azure DevOps feed manually:

```bash
dotnet nuget add source https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v3/index.json \
  --name "Mystira-Internal" \
  --username {your-email} \
  --password {your-pat} \
  --store-password-in-clear-text
```

Or add it to your user-level `NuGet.config` file.

## Error Messages

### Before (401 Unauthorized)

```
error NU1301: Unable to load the service index for source https://pkgs.dev.azure.com/...
error NU1301: Response status code does not indicate success: 401 (Unauthorized).
```

This occurred when the workflow tried to access Azure DevOps without valid credentials.

### After (Package Not Found)

```
error NU1101: Unable to find package Mystira.App.Domain. No packages exist with this id in source(s): nuget.org
```

This occurs when packages are not published to nuget.org and Azure DevOps credentials are not configured. This is expected behavior and indicates that packages need to be either:
1. Published to nuget.org as public packages
2. Accessed via Azure DevOps with proper credentials

## Migration Path

If you need to transition from Azure DevOps to nuget.org:

1. **Publish packages to nuget.org** - Make internal packages available publicly
2. **Update package versions** - Ensure .csproj references correct versions
3. **Remove Azure DevOps secrets** - Clean up GitHub repository secrets
4. **Update NuGet.config** - Already configured to use only nuget.org

## Benefits

✅ **No authentication failures** - CI won't fail due to missing or invalid credentials  
✅ **Flexible configuration** - Supports both public and private package sources  
✅ **Clear error messages** - NU1101 clearly indicates missing packages  
✅ **Backward compatible** - Existing configurations with secrets continue to work  
✅ **Easier onboarding** - New contributors don't need Azure DevOps access

## Related Files

- `.github/workflows/ci.yml` - CI workflow with optional feed configuration
- `NuGet.config` - Package source configuration
- `README.md` - Setup instructions
- `docs/cicd/README.md` - CI/CD documentation
