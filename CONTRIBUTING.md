# Contributing to Mystira

Thank you for your interest in contributing to the Mystira platform!

## Table of Contents

- [Development Setup](#development-setup)
- [Project Structure](#project-structure)
- [Package Management](#package-management)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Linting and Formatting](#linting-and-formatting)
- [Package Releases](#package-releases)
- [GitHub Workflows](#github-workflows)
- [Dependency Management](#dependency-management)
- [Getting Help](#getting-help)

---

## Development Setup

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Node.js | 18.x+ (24 recommended) | JavaScript runtime |
| pnpm | 8.x+ (10.27.0 recommended) | Package manager |
| .NET SDK | 9.0+ | C# development |
| Docker | Latest | Local services |
| Git | Latest | Version control |

**Optional (for infrastructure work):**

| Tool | Version | Purpose |
|------|---------|---------|
| Azure CLI | Latest | Azure resource management |
| Terraform | 1.5.0+ | Infrastructure as Code |
| kubectl | Latest | Kubernetes management |

### Installing Prerequisites

```bash
# Node.js (use nvm)
nvm install 24
nvm use 24

# pnpm
npm install -g pnpm@10.27.0

# .NET SDK 9.0 (see https://dotnet.microsoft.com/download)
# Windows: winget install Microsoft.DotNet.SDK.9
# macOS: brew install dotnet-sdk
# Linux: See https://learn.microsoft.com/en-us/dotnet/core/install/linux

# Verify installations
node --version    # Should be 18+
pnpm --version    # Should be 8+
dotnet --version  # Should be 9.0+
```

### Getting Started

1. **Clone the repository with submodules:**

   ```bash
   git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
   cd Mystira.workspace
   ```

   If already cloned without submodules:

   ```bash
   git submodule update --init --recursive
   ```

2. **Install dependencies:**

   ```bash
   pnpm install
   ```

3. **Set up GitHub Packages authentication** (required for @mystira packages):

   ```bash
   # Option 1: Environment variable (recommended)
   export NODE_AUTH_TOKEN=ghp_your_github_pat_here

   # Option 2: Use GitHub CLI
   export NODE_AUTH_TOKEN=$(gh auth token)
   ```

   For NuGet packages, see [NuGet Authentication](#nuget-packages).

4. **Start local services:**

   ```bash
   docker-compose up -d
   ```

5. **Start development:**

   ```bash
   pnpm dev
   ```

---

## Project Structure

This workspace uses a hybrid approach: Git submodules for independent services and pnpm workspaces with Turborepo for shared packages.

### Git Submodules

| Submodule | Path | Technology | Description |
|-----------|------|------------|-------------|
| Mystira.Chain | `packages/chain/` | Python, gRPC | Blockchain service |
| Mystira.App | `packages/app/` | .NET, Blazor | Main application |
| Mystira.StoryGenerator | `packages/story-generator/` | .NET | AI story generation |
| Mystira.Publisher | `packages/publisher/` | TypeScript, React | Content publishing |
| Mystira.DevHub | `packages/devhub/` | TypeScript | Developer portal |
| Mystira.Admin.Api | `packages/admin-api/` | .NET, ASP.NET Core | Admin backend |
| Mystira.Admin.UI | `packages/admin-ui/` | TypeScript, React | Admin dashboard |

See [Submodules Guide](./docs/guides/submodules.md) for detailed information.

### Workspace Packages

| Package | Path | Type | Description |
|---------|------|------|-------------|
| Mystira.Contracts | `packages/contracts/` | NPM + NuGet | Shared API contracts |
| Mystira.Shared | `packages/shared/` | NuGet | .NET shared infrastructure |
| Mystira.Core | `packages/core/` | NuGet | Core functionality |
| Mystira.Domain | `packages/domain/` | NuGet | Domain models |
| Mystira.Application | `packages/application/` | NuGet | Application services |
| @mystira/core-types | `packages/core-types/` | NPM | TypeScript types |
| @mystira/shared-utils | `packages/shared-utils/` | NPM | Utility functions |
| @mystira/design-tokens | `packages/design-tokens/` | NPM | Design system tokens |
| @mystira/api-spec | `packages/api-spec/` | NPM | API specifications |

### Infrastructure Packages

| Package | Path | Description |
|---------|------|-------------|
| Mystira.Infrastructure.Data | `packages/infrastructure/Mystira.Infrastructure.Data/` | Data access layer |
| Mystira.Infrastructure.Azure | `packages/infrastructure/Mystira.Infrastructure.Azure/` | Azure integrations |
| Mystira.Infrastructure.Discord | `packages/infrastructure/Mystira.Infrastructure.Discord/` | Discord integration |
| Mystira.Infrastructure.Teams | `packages/infrastructure/Mystira.Infrastructure.Teams/` | Teams integration |
| Mystira.Infrastructure.Payments | `packages/infrastructure/Mystira.Infrastructure.Payments/` | Payment processing |
| Mystira.Infrastructure.WhatsApp | `packages/infrastructure/Mystira.Infrastructure.WhatsApp/` | WhatsApp integration |
| Mystira.Infrastructure.StoryProtocol | `packages/infrastructure/Mystira.Infrastructure.StoryProtocol/` | Story Protocol |

### Infrastructure (IaC)

The `infra/` directory is **not** a submodule. It contains infrastructure code directly in the workspace:

- `infra/terraform/` - Terraform modules for Azure
- `infra/kubernetes/` - Kubernetes manifests and overlays
- `infra/docker/` - Dockerfiles for services
- `infra/scripts/` - Deployment scripts

---

## Package Management

Mystira uses GitHub Packages for both NPM (`@mystira/*`) and NuGet (`Mystira.*`) packages.

### NPM Packages

The workspace `.npmrc` is pre-configured for the `@mystira` scope:

```ini
@mystira:registry=https://npm.pkg.github.com
//npm.pkg.github.com/:_authToken=${NODE_AUTH_TOKEN}
```

**Local Development Authentication:**

```bash
# Add to your shell profile (~/.bashrc, ~/.zshrc)
export NODE_AUTH_TOKEN=ghp_your_github_pat_here

# Or use GitHub CLI
export NODE_AUTH_TOKEN=$(gh auth token)
```

**Creating a Personal Access Token (PAT):**

1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Name: `Mystira Package Access`
4. Scopes: `read:packages`, `write:packages`, `repo`
5. Copy and save the token

### NuGet Packages

NuGet packages are published to `https://nuget.pkg.github.com/phoenixvc/index.json`.

**Local Development Authentication:**

```bash
# Add the GitHub Packages source globally
dotnet nuget add source https://nuget.pkg.github.com/phoenixvc/index.json \
  --name github-mystira \
  --username phoenixvc \
  --password ghp_your_github_pat_here \
  --store-password-in-clear-text \
  --configfile ~/.nuget/NuGet/NuGet.Config
```

**Package Source Mapping (Required in nuget.config):**

To ensure NuGet resolves Mystira packages from GitHub (not nuget.org), add package source mapping:

```xml
<packageSourceMapping>
  <packageSource key="nuget.org">
    <package pattern="*" />
  </packageSource>
  <packageSource key="github">
    <package pattern="Mystira.*" />
    <package pattern="PhoenixVC.*" />
  </packageSource>
</packageSourceMapping>
```

> **Note**: Without this mapping, you may see "Unable to resolve 'Mystira.*'" errors.

**Verifying Access:**

```bash
# List NuGet sources
dotnet nuget list source

# Search for packages
dotnet package search Mystira --source github-mystira

# Restore packages (clear cache if issues)
dotnet nuget locals all --clear
dotnet restore
```

---

## Development Workflow

### Branch Naming

Use descriptive branch names with prefixes:

| Prefix | Purpose | Example |
|--------|---------|---------|
| `feature/` | New features | `feature/add-nft-marketplace` |
| `fix/` | Bug fixes | `fix/auth-token-refresh` |
| `docs/` | Documentation | `docs/update-setup-guide` |
| `refactor/` | Code refactoring | `refactor/api-client` |
| `test/` | Test additions | `test/add-payment-tests` |
| `chore/` | Maintenance | `chore/update-dependencies` |

### Commit Messages

We use [Conventional Commits](https://www.conventionalcommits.org/) format, enforced via commitlint:

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Types:**

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Code style (formatting, semicolons, etc.) |
| `refactor` | Code refactoring |
| `perf` | Performance improvement |
| `test` | Adding or updating tests |
| `build` | Build system or dependencies |
| `ci` | CI configuration |
| `chore` | Other changes |
| `revert` | Revert a previous commit |

**Scopes:**

| Scope | Repository/Package |
|-------|-------------------|
| `chain` | Mystira.Chain |
| `app` | Mystira.App |
| `story-generator` | Mystira.StoryGenerator |
| `publisher` | Mystira.Publisher |
| `devhub` | Mystira.DevHub |
| `admin-api` | Mystira.Admin.Api |
| `admin-ui` | Mystira.Admin.UI |
| `contracts` | Mystira.Contracts |
| `shared` | Mystira.Shared |
| `infra` | Infrastructure |
| `workspace` | Workspace configuration |
| `deps` | Dependencies |

**Examples:**

```bash
feat(chain): add NFT minting contract
fix(app): resolve auth token refresh issue
docs(workspace): update installation instructions
chore(deps): update pnpm to 10.27.0
build(contracts): add source generators
```

See [Commit Conventions](./docs/guides/commit-conventions.md) for detailed guidelines.

### Pull Requests

1. Create a feature branch from `dev` branch
2. Make your changes with clear commits
3. Write/update tests as needed
4. Ensure all checks pass:

   ```bash
   pnpm test
   pnpm lint
   pnpm build
   ```

5. Create a changeset if your change affects packages:

   ```bash
   pnpm changeset
   ```

6. Submit a PR to the `dev` branch with a clear description

### Code Review

All PRs require at least one approval before merging. Reviewers check for:

- Code quality and style
- Test coverage
- Documentation updates
- Performance implications
- Security considerations

---

## Testing

```bash
# Run all tests
pnpm test

# Run tests for specific package
pnpm --filter @mystira/contracts test

# Run .NET tests
dotnet test

# Run tests in specific .NET project
dotnet test packages/shared/Mystira.Shared.Tests/

# Run tests in watch mode
pnpm test -- --watch
```

---

## Linting and Formatting

```bash
# Lint all packages
pnpm lint

# Format code
pnpm format

# .NET formatting
dotnet format
```

Pre-commit hooks (via Husky) automatically run linting on staged files.

---

## Package Releases

We use [Changesets](https://github.com/changesets/changesets) for version management.

### Creating a Changeset

When your change affects a published package:

```bash
# Create a changeset
pnpm changeset

# Follow the prompts:
# 1. Select affected packages
# 2. Choose version bump type (patch/minor/major)
# 3. Write a summary (appears in changelog)
```

### Version Bump Types

| Type | When to Use | Example |
|------|-------------|---------|
| `patch` | Bug fixes, minor updates | `1.0.0` → `1.0.1` |
| `minor` | New features, backward compatible | `1.0.0` → `1.1.0` |
| `major` | Breaking changes | `1.0.0` → `2.0.0` |

### Release Process

1. Create PR with your changes + changeset
2. Merge to `main` after approval
3. Release workflow automatically:
   - Creates a "Version Packages" PR (if changesets exist)
   - Publishes packages when Version PR is merged

See [Package Releases Guide](./docs/guides/package-releases.md) for detailed information.

---

## GitHub Workflows

### Workflow Naming Convention

All workflows follow the "Category: Name" pattern for consistency. See [ADR-0012](./docs/architecture/adr/0012-github-workflow-naming-convention.md).

**Categories:**

| Category | Description | Example |
|----------|-------------|---------|
| `Components:` | CI workflows for individual services | `Components: Admin API - CI` |
| `Infrastructure:` | Infrastructure provisioning | `Infrastructure: Deploy` |
| `Deployment:` | Environment deployments | `Deployment: Staging` |
| `Workspace:` | Workspace-level operations | `Workspace: CI` |
| `Packages:` | Package publishing | `Packages - Contracts: Publish NuGet` |
| `Utilities:` | Support and helper workflows | `Utilities: Check Submodules` |

### Adding New Components

When adding a new component workflow:

1. **Name format**: `Components: {Component Name} - CI`
2. **File naming**: `{component-name}-ci.yml` (lowercase, hyphens)
3. **Include standard jobs**: lint, test, build
4. **Add path filters** to trigger only on relevant changes

Example:

```yaml
name: "Components: New Service - CI"

on:
  push:
    branches: [dev, main]
    paths:
      - "packages/new-service/**"
      - ".github/workflows/new-service-ci.yml"
```

### Repository Metadata Sync

GitHub repository descriptions and topics are synced from `scripts/repo-metadata.json`:

```bash
# Preview changes
./scripts/sync-repo-metadata.sh --dry-run

# Apply changes
./scripts/sync-repo-metadata.sh
```

---

## Dependency Management

### Renovate Bot

We use [Renovate](https://docs.renovatebot.com/) for automated dependency updates.

**How it works:**

1. Renovate creates PRs for dependency updates
2. Patch updates for non-Mystira packages are auto-merged
3. Major updates and Mystira packages require review

**Dependency Dashboard:**

Check the "Renovate Dependency Dashboard" issue in each repo for:

- Pending updates
- Open PRs
- Detected problems

See [Renovate Setup Guide](./docs/guides/renovate-setup.md) for configuration details.

### Manual Updates

```bash
# Update all dependencies
pnpm update

# Update specific package
pnpm update @mystira/contracts

# Update .NET packages
dotnet outdated
dotnet add package <PackageName>
```

---

## Package-Specific Guidelines

### Mystira.Chain (Python)

- Use gRPC for service communication
- Document all protocol buffers
- Include unit tests for all functions
- Follow PEP 8 style guide

### Mystira.App / Mystira.StoryGenerator (.NET)

- Follow C# coding conventions
- Use .NET 9.0 features where appropriate
- Implement unit and integration tests
- Use dependency injection

### Mystira.Publisher / Mystira.Admin.UI (React)

- Follow React best practices
- Use TypeScript strict mode
- Implement responsive designs
- Write unit and integration tests

### Mystira.Contracts

- Define contracts in both TypeScript and C#
- Keep contracts backward compatible when possible
- Document breaking changes
- Run contract generation: `pnpm --filter @mystira/contracts generate`

### Infrastructure

- Test infrastructure changes in dev first
- Document all Terraform resources
- Use semantic versioning for releases
- Include rollback procedures

---

## Getting Help

- Check existing [GitHub Issues](https://github.com/phoenixvc/Mystira.workspace/issues)
- Review documentation in `docs/` directory
- Check workflow logs in GitHub Actions
- Reach out to maintainers

## Code of Conduct

Be respectful, inclusive, and professional. We're building something amazing together!

---

## Related Documentation

- [Quick Start Guide](./docs/guides/quick-start.md)
- [Comprehensive Setup Guide](./docs/guides/setup.md)
- [Submodules Guide](./docs/guides/submodules.md)
- [Environment Variables](./docs/guides/environment-variables.md)
- [Package Releases](./docs/guides/package-releases.md)
- [Commit Conventions](./docs/guides/commit-conventions.md)
- [Architecture Overview](./docs/guides/architecture.md)
