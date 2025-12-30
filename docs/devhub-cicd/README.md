# DevHub CI/CD Documentation

This directory contains CI/CD documentation for the Mystira.DevHub repository (Tauri desktop application).

## Project Overview

DevHub is a **Tauri desktop application** that combines:
- **Frontend**: React/TypeScript with Vite
- **Backend**: Rust (Tauri native bindings)
- **Additional .NET Components**: CLI tools and services

## Workflow Files (in DevHub repo)

| File | Description |
|------|-------------|
| `ci.yml` | Main CI - builds Tauri app for all platforms (Linux, Windows, macOS) |
| `ci-dotnet.yml` | CI for .NET CLI and Services components |
| `build-deploy.yml` | Dev builds with workspace notification |
| `release.yml` | Creates GitHub releases with desktop binaries |

## CI/CD Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         DevHub Repo                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   PR/Push ──► ci.yml ──► Lint (TS + Rust) ──► Test ──► Build    │
│                                                                 │
│   Push to dev ──► build-deploy.yml ──► Build all platforms      │
│                           │                                     │
│                           ▼                                     │
│              repository_dispatch (devhub-deploy)                │
│                           │                                     │
│   Tag (v*) ──► release.yml ──► Build ──► GitHub Release         │
│                           │                                     │
│                           ▼                                     │
│              repository_dispatch (devhub-release)               │
│                                                                 │
└───────────────────────────┼─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Mystira.workspace                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   submodule-deploy-dev-appservice.yml                           │
│     ──► Updates submodule reference                             │
│     ──► Tracks deployed version                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Build Matrix

### Tauri Desktop App

| Platform | Target | Artifacts |
|----------|--------|-----------|
| Linux x64 | `x86_64-unknown-linux-gnu` | `.AppImage`, `.deb` |
| Windows x64 | `x86_64-pc-windows-msvc` | `.msi`, `.exe` |
| macOS Intel | `x86_64-apple-darwin` | `.dmg`, `.app.tar.gz` |
| macOS ARM | `aarch64-apple-darwin` | `.dmg`, `.app.tar.gz` |

## Required Secrets

Configure these secrets in the DevHub repository settings:

| Secret | Description |
|--------|-------------|
| `MYSTIRA_WORKSPACE_DISPATCH_TOKEN` | GitHub PAT with `repo` scope for triggering workspace workflows |
| `TAURI_PRIVATE_KEY` | (Optional) Tauri signing key for auto-updates |
| `TAURI_KEY_PASSWORD` | (Optional) Password for Tauri signing key |

## Project Structure

```
Mystira.DevHub/
├── packages/devhub/
│   ├── .github/workflows/
│   │   ├── ci.yml                  # Main Tauri CI
│   │   ├── ci-dotnet.yml           # .NET components CI
│   │   ├── build-deploy.yml        # Dev deployment
│   │   └── release.yml             # Desktop releases
│   │
│   ├── Mystira.DevHub/             # Tauri app
│   │   ├── src/                    # React/TypeScript frontend
│   │   ├── src-tauri/              # Rust backend
│   │   │   ├── Cargo.toml
│   │   │   └── src/
│   │   ├── package.json
│   │   └── vite.config.ts
│   │
│   ├── Mystira.DevHub.CLI/         # .NET CLI tool
│   ├── Mystira.DevHub.Services/    # .NET services
│   └── Mystira.App.CosmosConsole/  # .NET console app
```

## Local Development

```bash
# Install dependencies
cd Mystira.DevHub
pnpm install

# Development mode (hot reload)
pnpm tauri dev

# Build for current platform
pnpm tauri build

# Run tests
pnpm test                    # Frontend tests
cd src-tauri && cargo test   # Rust tests
```

## Release Process

1. Create a version tag: `git tag v1.0.0`
2. Push the tag: `git push origin v1.0.0`
3. The `release.yml` workflow will:
   - Build binaries for all platforms
   - Create a GitHub Release with all artifacts
   - Notify the workspace

## Comparison with Other Projects

| Feature | DevHub (Tauri) | Other Projects |
|---------|---------------|----------------|
| Type | Desktop App | Web Services |
| Deployment | GitHub Releases | K8s / App Service |
| Artifacts | Desktop binaries | Docker images |
| Runtime | Native + WASM | Container / Serverless |

---

## Legacy Notes

The original templates in this directory (`ci.yml`, `build-deploy.yml`, `release.yml`, `Dockerfile`) were designed for a **Leptos SSR** web application. The actual DevHub project uses **Tauri** for desktop deployment. The updated workflows in the DevHub repository reflect the correct architecture.
