# CI/CD Documentation

## Overview

The Admin API uses GitHub Actions for continuous integration and deployment.

## Workflows

### ci.yml - Continuous Integration

**Trigger**: Push to `main`/`dev`, Pull Requests, Manual dispatch

**Jobs**:

| Job | Description | Dependencies |
|-----|-------------|--------------|
| `lint` | Code formatting check (`dotnet format`) | None |
| `test` | Unit tests with code coverage | None |
| `build` | Production build + artifact upload | lint, test |
| `validate` | Config files, documentation, Dockerfile | None |

**Features**:
- Concurrency control (cancels duplicate runs)
- Path filtering (only runs on source changes)
- Code coverage with Codecov
- Build artifact retention (7 days)
- NuGet feed authentication

### Required Secrets (Optional)

The following secrets are **optional**. Configure them only if you need access to a private Azure DevOps NuGet feed:

| Secret | Description |
|--------|-------------|
| `MYSTIRA_DEVOPS_AZURE_ORG` | Azure DevOps organization name |
| `MYSTIRA_DEVOPS_AZURE_PROJECT` | Azure DevOps project name |
| `MYSTIRA_DEVOPS_AZURE_PAT` | Personal Access Token (Packaging Read scope) |
| `MYSTIRA_DEVOPS_NUGET_FEED` | Artifacts feed name (e.g., `Mystira-Internal`) |

**Note**: If these secrets are not configured, the workflow will skip the Azure DevOps feed configuration and use only nuget.org. This is intentional to support environments without access to private feeds.

## Deployment

Deployment workflows are managed in the `mystira.workspace` repository:

- `infra-deploy.yml` - Infrastructure deployment
- `staging-release.yml` - Staging environment release
- `production-release.yml` - Production environment release

## Local Development

### Running CI Checks Locally

```bash
# Format check
dotnet format --verify-no-changes

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Build for release
dotnet build --configuration Release

# Publish
dotnet publish src/Mystira.Admin.Api/Mystira.Admin.Api.csproj -c Release -o ./publish
```

### NuGet Feed Configuration (Optional)

For local development with private packages, configure the internal NuGet feed:

```bash
dotnet nuget add source https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v3/index.json \
  --name "Mystira-Internal" \
  --username {your-email} \
  --password {your-pat}
```

Or update `NuGet.config` with your credentials.

**Note**: This is only needed if you're using packages from a private Azure DevOps feed. Public packages from nuget.org don't require any additional configuration.

## Alignment with Workspace

This CI workflow is aligned with `mystira.workspace` patterns:

- Uses same job structure (lint → test → build)
- Uses same action versions (checkout@v6, setup-dotnet@v5)
- Uses same code coverage integration (Codecov)
- Uses same artifact naming conventions

See [ALIGNMENT_REPORT.md](../ALIGNMENT_REPORT.md) for details.
