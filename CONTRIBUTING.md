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

| Tool     | Version                    | Purpose            |
| -------- | -------------------------- | ------------------ |
| Node.js  | 18.x+ (24 recommended)     | JavaScript runtime |
| pnpm     | 8.x+ (10.27.0 recommended) | Package manager    |
| .NET SDK | 10.0+                      | C# development     |
| Docker   | Latest                     | Local services     |
| Git      | Latest                     | Version control    |

**Optional (for infrastructure work):**

| Tool      | Version | Purpose                   |
| --------- | ------- | ------------------------- |
| Azure CLI | Latest  | Azure resource management |
| Terraform | 1.5.0+  | Infrastructure as Code    |
| kubectl   | Latest  | Kubernetes management     |

### Installing Prerequisites

```bash
# Node.js (use nvm)
nvm install 24
nvm use 24

# pnpm
npm install -g pnpm@10.27.0

# .NET SDK 10.0 (see https://dotnet.microsoft.com/download)
# Windows: winget install Microsoft.DotNet.SDK.10
# macOS: brew install dotnet-sdk
# Linux: See https://learn.microsoft.com/en-us/dotnet/core/install/linux

# Verify installations
node --version    # Should be 18+
pnpm --version    # Should be 8+
dotnet --version  # Should be 10.0+
```

### Getting Started

1. Fork and clone the repository:

   ```bash
   git clone https://github.com/phoenixvc/Mystira.workspace.git
   cd Mystira.workspace
   ```

2. Install dependencies:

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

This is a monorepo managed with pnpm workspaces and Turborepo. Each package has its own README with specific instructions.

- `packages/chain/` - Blockchain and smart contracts (Python, gRPC)
- `packages/app/` - Main storytelling application (.NET)
- `packages/story-generator/` - AI story generation engine (.NET)
- `packages/publisher/` - Publisher web application (TypeScript, React)
- `packages/devhub/` - Developer portal and tools (TypeScript)
- `packages/admin-api/` - Admin backend API (ASP.NET Core)
- `packages/admin-ui/` - Admin dashboard frontend (TypeScript, React)
- `packages/contracts/` - Shared contracts (TypeScript + .NET)
- `packages/shared/` - Shared .NET libraries
- `packages/shared-utils/` - Shared TypeScript utilities
- `infra/` - Infrastructure as Code (Terraform, Kubernetes)

## Development Workflow

### Branch Naming

Use descriptive branch names with prefixes:

| Prefix      | Purpose          | Example                       |
| ----------- | ---------------- | ----------------------------- |
| `feature/`  | New features     | `feature/add-nft-marketplace` |
| `fix/`      | Bug fixes        | `fix/auth-token-refresh`      |
| `docs/`     | Documentation    | `docs/update-setup-guide`     |
| `refactor/` | Code refactoring | `refactor/api-client`         |
| `test/`     | Test additions   | `test/add-payment-tests`      |
| `chore/`    | Maintenance      | `chore/update-dependencies`   |

### Commit Messages

We use [Conventional Commits](https://www.conventionalcommits.org/) format, enforced via commitlint:

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Types:**

| Type       | Description                               |
| ---------- | ----------------------------------------- |
| `feat`     | New feature                               |
| `fix`      | Bug fix                                   |
| `docs`     | Documentation only                        |
| `style`    | Code style (formatting, semicolons, etc.) |
| `refactor` | Code refactoring                          |
| `perf`     | Performance improvement                   |
| `test`     | Adding or updating tests                  |
| `build`    | Build system or dependencies              |
| `ci`       | CI configuration                          |
| `chore`    | Other changes                             |
| `revert`   | Revert a previous commit                  |

**Scopes:**

| Scope             | Repository/Package      |
| ----------------- | ----------------------- |
| `chain`           | Mystira.Chain           |
| `app`             | Mystira.App             |
| `story-generator` | Mystira.StoryGenerator  |
| `publisher`       | Mystira.Publisher       |
| `devhub`          | Mystira.DevHub          |
| `admin-api`       | Mystira.Admin.Api       |
| `admin-ui`        | Mystira.Admin.UI        |
| `contracts`       | Mystira.Contracts       |
| `shared`          | Mystira.Shared          |
| `infra`           | Infrastructure          |
| `workspace`       | Workspace configuration |
| `deps`            | Dependencies            |

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

| Type    | When to Use                       | Example           |
| ------- | --------------------------------- | ----------------- |
| `patch` | Bug fixes, minor updates          | `1.0.0` → `1.0.1` |
| `minor` | New features, backward compatible | `1.0.0` → `1.1.0` |
| `major` | Breaking changes                  | `1.0.0` → `2.0.0` |

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

| Category          | Description                          | Example                      |
| ----------------- | ------------------------------------ | ---------------------------- |
| `Components:`     | CI workflows for individual services | `Components: Admin API - CI` |
| `Infrastructure:` | Infrastructure provisioning          | `Infrastructure: Deploy`     |
| `Deployment:`     | Environment deployments              | `Deployment: Staging`        |
| `Workspace:`      | Workspace-level operations           | `Workspace: CI`              |
| `Utilities:`      | Support and helper workflows         | `Utilities: Link Checker`    |

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
- Use .NET 10.0 features where appropriate
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
- [Environment Variables](./docs/guides/environment-variables.md)
- [Package Releases](./docs/guides/package-releases.md)
- [Commit Conventions](./docs/guides/commit-conventions.md)
- [Architecture Overview](./docs/guides/architecture.md)
