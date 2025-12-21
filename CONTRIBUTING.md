# Contributing to Mystira

Thank you for your interest in contributing to the Mystira platform!

## Development Setup

### Prerequisites

- Node.js 18.x or higher
- pnpm 8.x or higher
- Docker and Docker Compose
- Git

### Getting Started

1. Fork and clone the repository with submodules:

   ```bash
   git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
   cd Mystira.workspace
   ```

   If already cloned:

   ```bash
   git submodule update --init --recursive
   ```

2. Install dependencies:
   ```bash
   pnpm install
   ```
3. Set up environment variables:
   ```bash
   cp .env.example .env.local
   ```
4. Start development servers:
   ```bash
   pnpm dev
   ```

## Project Structure

This workspace integrates multiple repositories as git submodules, managed with pnpm workspaces and Turborepo. Each repository has its own README with specific instructions.

- `packages/chain/` - Mystira.Chain repository (blockchain and smart contracts)
- `packages/app/` - Mystira.App repository (web and mobile applications)
- `packages/story-generator/` - Mystira.StoryGenerator repository (AI story generation engine)
- `packages/publisher/` - Mystira.Publisher repository (publisher web application)
- `packages/devhub/` - Mystira.DevHub repository (development operations desktop app)
- `infra/` - Mystira.Infra repository (infrastructure and DevOps)

See [Submodules Guide](./docs/guides/submodules.md) for detailed information on working with git submodules.

## Development Workflow

### Branch Naming

Use descriptive branch names with prefixes:

- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation updates
- `refactor/` - Code refactoring
- `test/` - Test additions/updates

Example: `feature/add-nft-marketplace`

### Commit Messages

Follow conventional commits format:

```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Examples:

- `feat(chain): add NFT minting contract`
- `fix(app): resolve auth token refresh issue`
- `docs(readme): update installation instructions`

### Pull Requests

1. Create a feature branch from `dev` branch
2. Make your changes with clear commits
3. Write/update tests as needed
4. Ensure all tests pass: `pnpm test`
5. Ensure linting passes: `pnpm lint`
6. Submit a PR to the `dev` branch with a clear description

### Code Review

All PRs require at least one approval before merging. Reviewers will check for:

- Code quality and style
- Test coverage
- Documentation updates
- Performance implications
- Security considerations

## Testing

```bash
# Run all tests
pnpm test

# Run tests for specific package
pnpm --filter @mystira/chain test

# Run tests in watch mode
pnpm test -- --watch
```

## Linting and Formatting

```bash
# Lint all packages
pnpm lint

# Format code
pnpm format
```

## Package-Specific Guidelines

### Mystira.Chain

- Smart contracts must have 100% test coverage
- Use Hardhat for development and testing
- Document all public functions with NatSpec
- Run security analysis before submitting PRs

### Mystira.App

- Follow React/Next.js best practices
- Use TypeScript strict mode
- Implement responsive designs
- Write unit and integration tests

### Mystira.StoryGenerator

- Document AI model integrations
- Include example prompts in tests
- Monitor token usage in development
- Follow responsible AI guidelines

### Mystira.Infra

- Test infrastructure changes in staging first
- Document all Terraform resources
- Use semantic versioning for releases
- Include rollback procedures

## GitHub Workflows

### Workflow Naming Convention

All workflows follow the "Category: Name" pattern for consistency. See [ADR-0012: GitHub Workflow Naming Convention](./docs/architecture/adr/0012-github-workflow-naming-convention.md) for details.

**Categories:**

| Category         | Description                           | Example                       |
| ---------------- | ------------------------------------- | ----------------------------- |
| `Components:`    | CI workflows for individual services  | `Components: Admin API - CI`  |
| `Infrastructure:`| Infrastructure provisioning           | `Infrastructure: Deploy`      |
| `Deployment:`    | Environment deployments               | `Deployment: Staging`         |
| `Workspace:`     | Workspace-level operations            | `Workspace: CI`               |
| `Utilities:`     | Support and helper workflows          | `Utilities: Check Submodules` |

### Adding New Components

When adding a new component workflow:

1. **Name format**: `Components: {Component Name} - CI`
2. **File naming**: `{component-name}-ci.yml` (lowercase, hyphens)
3. **Include standard jobs**: lint, test, build
4. **Add path filters** to only trigger on relevant changes
5. **Update README.md** badges if applicable

Example workflow structure:

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

GitHub repository descriptions and topics are synced from `scripts/repo-metadata.json`. To update:

1. Edit `scripts/repo-metadata.json` with new metadata
2. Run `./scripts/sync-repo-metadata.sh --dry-run` to preview changes
3. Run `./scripts/sync-repo-metadata.sh` to apply

## Getting Help

- Check existing issues and discussions
- Join our Discord community
- Reach out to maintainers

## Code of Conduct

Be respectful, inclusive, and professional. We're building something amazing together!
