# Package Management Strategy for Mystira Workspace

This guide explains the comprehensive package management strategy for the Mystira monorepo, which spans multiple languages and ecosystems.

## Overview

The Mystira workspace is a complex polyglot monorepo that includes:

- **TypeScript/JavaScript** (React, Node.js, Vite, etc.)
- **.NET/C#** (Web APIs, class libraries, test projects)
- **Python** (Blockchain integration)
- **Rust** (Desktop applications via Tauri)

## Primary Package Manager: pnpm

### Why pnpm?

1. **Efficient Disk Usage**: Shared dependencies across packages
2. **Fast Installation**: Parallel installation and caching
3. **Strict Lockfile**: Prevents dependency drift
4. **Monorepo Support**: Native workspace support with `pnpm-workspace.yaml`
5. **Security**: Built-in security auditing

### Configuration

```json
// package.json (root)
{
  "packageManager": "pnpm@10.30.2",
  "engines": {
    "node": ">=18.0.0",
    "pnpm": ">=8.0.0"
  }
}
```

## Language-Specific Strategies

### TypeScript/JavaScript Projects

**Package Manager**: pnpm (exclusively)

**Workspace Structure**:

```yaml
# pnpm-workspace.yaml
packages:
  - "packages/*"
  - "packages/*/src/*" # For nested projects like StoryGenerator.Web
```

**Scripts**:

```json
{
  "scripts": {
    "dev": "pnpm run dev",
    "build": "pnpm run build",
    "test": "pnpm run test",
    "lint": "pnpm run lint",
    "format": "pnpm run format",
    "clean": "turbo run clean"
  }
}
```

**Turbo Integration**:

```json
// turbo.json
{
  "pipeline": {
    "build": {
      "dependsOn": ["^build"],
      "outputs": ["dist/**", "build/**"]
    },
    "test": {
      "dependsOn": ["build"],
      "outputs": []
    },
    "lint": {
      "outputs": []
    }
  }
}
```

### .NET Projects

**Package Manager**: NuGet (via `dotnet CLI`)

**Integration with pnpm**:

- .NET projects are managed independently but coordinated through workspace scripts
- Use `Directory.Build.props` for shared configuration
- Central package management via `NuGet configuration`

**Key Files**:

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

**Scripts**:

```bash
# Run from workspace root
dotnet build                    # Build all .NET projects
dotnet test                     # Test all .NET projects
dotnet pack                     # Package all .NET projects
```

### Python Projects

**Package Manager**: Poetry (recommended) or pip + requirements.txt

**Current Setup**:

```toml
# packages/chain/pyproject.toml
[build-system]
requires = ["poetry-core>=1.0.0"]
build-backend = "poetry.core.masonry.api"

[tool.poetry]
name = "mystira-chain"
version = "0.5.2-alpha"

[tool.poetry.dependencies]
python = "^3.11"
```

**Integration**:

- Python projects are managed independently
- Use workspace scripts to coordinate with other languages
- Virtual environments managed per project

### Rust Projects

**Package Manager**: Cargo

**Current Setup**:

```toml
# packages/devhub/Mystira.DevHub/src-tauri/Cargo.toml
[package]
name = "mystira-devhub"
version = "0.1.0"
edition = "2021"

[dependencies]
tauri = { version = "2", features = [] }
reqwest = { version = "0.13", features = ["rustls-tls-native-roots"] }
```

**Integration**:

- Rust projects (Tauri apps) are managed independently
- Use workspace scripts for coordination
- Cross-compilation handled by Cargo

## Quality Assurance Scripts

### Comprehensive Quality Check

We provide both Bash and PowerShell scripts for comprehensive quality checks:

```bash
# Unix/Linux/macOS
./scripts/quality-check.sh

# Windows
./scripts/quality-check.ps1
```

**Features**:

- Runs linting, formatting, and tests across all languages
- Security audits for all package managers
- Dependency validation
- Code coverage reports
- Documentation generation

### Individual Language Commands

**TypeScript/JavaScript**:

```bash
pnpm install          # Install dependencies
pnpm run lint         # ESLint + Prettier
pnpm run format       # Format code
pnpm run test         # Run tests
pnpm run build        # Build projects
pnpm audit            # Security audit
```

**.NET**:

```bash
dotnet restore         # Restore NuGet packages
dotnet build          # Build projects
dotnet test           # Run tests
dotnet pack          # Create packages
```

**Python**:

```bash
poetry install        # Install dependencies
poetry run pytest     # Run tests
poetry build          # Build package
```

**Rust**:

```bash
cargo build           # Build project
cargo test            # Run tests
cargo audit           # Security audit (if installed)
```

## Dependency Management Best Practices

### 1. Version Pinning

**pnpm**: Use `pnpm-lock.yaml` (automatically generated)

```bash
pnpm install --frozen-lockfile  # CI/CD
pnpm update                    # Update dependencies
```

**.NET**: Use `Directory.Package.props` for central package management

```xml
<PackageVersion Include="Microsoft.AspNetCore.App" Version="8.0.0" />
```

**Python**: Pin versions in `pyproject.toml`

```toml
[tool.poetry.dependencies]
requests = "^2.31.0"
```

**Rust**: Pin versions in `Cargo.lock` (automatically generated)

### 2. Security Updates

**Automated**: Use Dependabot and Renovate

```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "npm"
    directory: "/"
    schedule:
      interval: "weekly"
```

**Manual**: Regular audits

```bash
pnpm audit              # Node.js
cargo audit             # Rust
safety check            # Python (if using safety)
```

### 3. Monorepo Coordination

**Root Scripts**: Coordinate all languages from workspace root

```json
{
  "scripts": {
    "clean": "turbo run clean && rm -rf node_modules",
    "dev": "pnpm run dev",
    "build": "pnpm run build && dotnet build",
    "test": "pnpm run test && dotnet test",
    "lint": "pnpm run lint",
    "quality": "./scripts/quality-check.sh"
  }
}
```

**Turbo Caching**: Efficient build caching

```json
{
  "pipeline": {
    "build": {
      "dependsOn": ["^build"],
      "outputs": ["dist/**"]
    }
  }
}
```

## CI/CD Integration

### GitHub Actions Workflow

```yaml
name: Quality Checks
on: [push, pull_request]

jobs:
  quality:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: pnpm/action-setup@v2
        with:
          version: 8.0.0
      - uses: actions/setup-node@v4
        with:
          node-version: "18"
          cache: "pnpm"

      - name: Install dependencies
        run: pnpm install --frozen-lockfile

      - name: Run quality checks
        run: ./scripts/quality-check.sh

      - name: .NET tests
        run: dotnet test --no-build --verbosity normal

      - name: Python tests
        run: |
          cd packages/chain
          python -m pytest tests/

      - name: Rust tests
        run: |
          cd packages/devhub/Mystira.DevHub/src-tauri
          cargo test
```

## Migration Guidelines

### From npm to pnpm

1. **Remove node_modules**: `rm -rf node_modules`
2. **Install pnpm**: `npm install -g pnpm`
3. **Install dependencies**: `pnpm install`
4. **Update CI/CD**: Replace `npm install` with `pnpm install --frozen-lockfile`

### Mixed Environment Cleanup

1. **Standardize on pnpm**: Replace all `npm run` with `pnpm run`
2. **Update package.json**: Ensure all scripts use pnpm
3. **Update documentation**: Reference pnpm commands
4. **Team training**: Ensure team knows pnpm basics

## Troubleshooting

### Common Issues

**pnpm lockfile out of sync**:

```bash
pnpm install  # Regenerates lockfile
```

**.NET restore failures**:

```bash
dotnet restore --force
```

**Python dependency conflicts**:

```bash
poetry install --no-dev  # Skip dev dependencies
```

**Rust compilation errors**:

```bash
cargo clean && cargo build  # Clean build
```

### Performance Optimization

1. **Turbo caching**: Ensure proper cache configuration
2. **Parallel execution**: Use `--parallel` flag where available
3. **Selective builds**: Use `--filter` to build only changed packages
4. **Dependency caching**: Cache dependencies in CI/CD

## Conclusion

This package management strategy provides:

- **Consistency**: Standardized approach across languages
- **Efficiency**: Optimized builds and installations
- **Security**: Regular audits and updates
- **Maintainability**: Clear documentation and automation
- **Scalability**: Works for small and large teams

The strategy balances the needs of different ecosystems while maintaining a cohesive development experience across the entire Mystira workspace.
