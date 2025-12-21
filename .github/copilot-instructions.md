# Copilot Instructions for Mystira Workspace

## Project Overview

Mystira is an AI-powered interactive storytelling platform combining blockchain technology, generative AI, and immersive narratives. This is a monorepo workspace that integrates multiple components as git submodules, managed with pnpm workspaces and Turborepo.

## Architecture

The workspace consists of multiple independent repositories integrated as submodules:

- **Mystira.Chain** (Python, gRPC) - Blockchain integration & Story Protocol
- **Mystira.App** (C#, .NET) - Main storytelling application
- **Mystira.StoryGenerator** (C#, .NET) - AI-powered story generation engine
- **Mystira.Publisher** (TypeScript, Node.js) - Content publishing service
- **Mystira.DevHub** (TypeScript) - Developer portal and tools
- **Mystira.Admin.Api** (C#, ASP.NET Core) - Admin backend API
- **Mystira.Admin.UI** (TypeScript, React) - Admin dashboard frontend
- **Infrastructure** (Terraform, Kubernetes) - Infrastructure as Code

## Technology Stack

### Languages & Frameworks
- **TypeScript/JavaScript**: Node.js 18+, React, Next.js
- **C#/.NET**: .NET SDK 8.0+, ASP.NET Core
- **Python**: Python 3.11+ (for Chain component)
- **Infrastructure**: Terraform (HCL), Kubernetes (YAML)

### Package Management & Build Tools
- **pnpm** 8.0+ - Package manager (preferred)
- **Turborepo** - Monorepo build system
- **Husky** - Git hooks for pre-commit checks
- **Changesets** - Version management and changelogs

### Code Quality Tools
- **ESLint** - JavaScript/TypeScript linting
- **Prettier** - Code formatting
- **Commitlint** - Commit message validation

## Development Workflow

### Git Submodules
This repository uses git submodules extensively. Each component in `packages/` is a separate repository:
- Always initialize submodules after cloning: `git submodule update --init --recursive`
- Update specific submodules: `git submodule update --remote packages/<component>`
- See [Submodules Guide](../docs/guides/submodules.md) for details

### Branch Strategy
- **main** - Production-ready code
- **dev** - Development branch (default target for PRs)
- Feature branches: Use prefixes `feature/`, `fix/`, `docs/`, `refactor/`, `test/`

### Commit Conventions
Follow Conventional Commits format:
```
<type>(<scope>): <description>

[optional body]

[optional footer]
```

**Types**: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

**Scopes**: Use component names (e.g., `chain`, `app`, `admin-api`, `publisher`, `devhub`, `story-generator`, `infra`)

**Examples**:
- `feat(chain): add NFT minting contract`
- `fix(app): resolve auth token refresh issue`
- `docs(readme): update installation instructions`
- `refactor(publisher): optimize content delivery pipeline`

## Building and Testing

### Common Commands
```bash
# Install all dependencies
pnpm install

# Build all packages (uses Turborepo)
pnpm build

# Build specific package
pnpm --filter @mystira/publisher build

# Run tests
pnpm test

# Run linting
pnpm lint

# Format code
pnpm format

# Start development servers
pnpm dev

# Start specific service
pnpm --filter mystira-publisher dev
```

### Pre-commit Hooks
The repository uses Husky and lint-staged to run checks before commits:
- ESLint on TypeScript files
- Prettier formatting on all files
- Commitlint validation

## Code Style Guidelines

### TypeScript/JavaScript
- Use TypeScript strict mode
- Follow ESLint configuration in `.eslintrc.json`
- Use functional components and hooks in React
- Prefer named exports over default exports
- Use async/await over raw promises
- Use Prettier for consistent formatting

### C#/.NET
- Follow standard .NET conventions
- Use async/await for asynchronous operations
- Implement proper error handling and logging
- Write XML documentation for public APIs

### Python
- Follow PEP 8 style guide
- Use type hints
- Write docstrings for all public functions

### General
- Write clear, self-documenting code
- Add comments only when necessary to explain "why", not "what"
- Keep functions small and focused
- Use meaningful variable and function names

## Testing Standards

### Test Coverage
- Write unit tests for business logic
- Write integration tests for API endpoints
- Aim for meaningful test coverage (focus on critical paths)

### Test Organization
- Place tests near the code they test
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)

## Documentation

### Code Documentation
- Document all public APIs
- Keep README files up to date
- Document complex algorithms and business logic
- Use JSDoc/TSDoc for TypeScript, XML docs for C#

### Architecture Decision Records (ADRs)
- Located in `docs/architecture/adr/`
- Follow the established template
- Document significant architectural decisions

## CI/CD

### Workflow Naming Convention
All GitHub Actions workflows follow the "Category: Name" pattern:

**Categories**:
- `Components:` - CI for individual services (e.g., `Components: Admin API - CI`)
- `Infrastructure:` - Infrastructure operations (e.g., `Infrastructure: Deploy`)
- `Deployment:` - Environment deployments (e.g., `Deployment: Staging`)
- `Workspace:` - Workspace-level operations (e.g., `Workspace: CI`)
- `Utilities:` - Helper workflows (e.g., `Utilities: Check Submodules`)

### CI Requirements
All PRs must pass:
- Linting checks
- Unit tests
- Build verification
- Commit message validation

## Security Guidelines

- Never commit secrets or credentials
- All secrets are managed via Azure Key Vault and Kubernetes secrets
- Follow the security policy in `SECURITY.md`
- Use environment variables for configuration
- Validate and sanitize all user inputs

## Component-Specific Guidelines

### Admin UI (React/TypeScript)
- Use React hooks and functional components
- Implement responsive designs
- Follow accessibility best practices
- Use TypeScript strict mode

### Admin API (C#/ASP.NET Core)
- Follow RESTful API design principles
- Implement proper authentication and authorization
- Use dependency injection
- Write comprehensive API documentation

### Chain (Python)
- Document all blockchain interactions
- Include gas optimization considerations
- Write thorough tests for smart contract interactions
- Follow security best practices for blockchain code

### Publisher (TypeScript/Node.js)
- Implement proper error handling
- Use async/await patterns
- Follow Node.js best practices
- Optimize for performance

### Story Generator (C#/.NET)
- Document AI model integrations
- Monitor token usage
- Follow responsible AI guidelines
- Include example prompts in documentation

### Infrastructure (Terraform/Kubernetes)
- Test all infrastructure changes in staging first
- Document all resources and their purposes
- Use variables and modules for reusability
- Include rollback procedures

## Pull Request Guidelines

1. Create feature branches from `dev` branch
2. Write clear, descriptive PR titles and descriptions
3. Link related issues using keywords (fixes #123, closes #456)
4. Ensure all CI checks pass
5. Request review from at least one team member
6. Address review feedback promptly
7. Keep PRs focused and reasonably sized

## Common Pitfalls to Avoid

- Don't forget to update submodules when needed
- Don't commit `node_modules/`, `dist/`, or build artifacts
- Don't bypass pre-commit hooks
- Don't merge without PR approval
- Don't make changes directly to `main` branch
- Don't ignore failing tests or linting errors
- Don't add dependencies without considering workspace-wide impact

## Helpful Resources

- [Quick Start Guide](../docs/guides/quick-start.md)
- [Contributing Guide](../CONTRIBUTING.md)
- [Submodules Guide](../docs/guides/submodules.md)
- [Architecture Documentation](../docs/guides/architecture.md)
- [CI/CD Documentation](../docs/cicd/)
- [Infrastructure Guide](../docs/infrastructure/infrastructure.md)

## When in Doubt

1. Check existing code for patterns and conventions
2. Review documentation in the `docs/` directory
3. Ask questions in PR comments or team discussions
4. Follow the principle of least surprise
5. Prioritize code clarity over cleverness
