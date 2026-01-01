# DevHub CI/CD Documentation

This directory contains the **canonical CI/CD workflow templates** for the Mystira.DevHub repository. Copy these files to `.github/workflows/` in the DevHub repository.

## Workflow Files to Copy

| File | Destination | Description |
|------|-------------|-------------|
| `ci.yml` | `.github/workflows/ci.yml` | Full CI for Tauri (React/Leptos frontend + Rust backend) |
| `ci-dotnet.yml` | `.github/workflows/ci-dotnet.yml` | .NET components CI (CLI, Services, CosmosConsole) |
| `build-deploy.yml` | `.github/workflows/build-deploy.yml` | Dev branch builds with workspace notification |
| `release.yml` | `.github/workflows/release.yml` | GitHub Releases with desktop binaries |

## Project Architecture

DevHub is a **Tauri desktop application** with:
- **React Frontend** (current): TypeScript/Vite + Tauri 1.x
- **Leptos Frontend** (upcoming): Rust/WASM + Tauri 2.0
- **.NET Components**: CLI tools and services

## CI Pipeline Overview

```
ci.yml:
├── lint-frontend (tsc, eslint)
├── lint-rust (clippy, rustfmt)
├── test-frontend (vitest)
├── test-rust (cargo test)
└── build (cross-platform Tauri builds)
    ├── Linux x64
    ├── Windows x64
    └── macOS x64

ci-dotnet.yml:
├── lint (dotnet format)
└── build
    ├── CLI
    ├── Services
    └── CosmosConsole

build-deploy.yml:
├── build (multi-platform)
└── notify (workspace dispatch)

release.yml:
├── build (multi-platform + ARM)
└── release (GitHub Release + workspace notify)
```

## Build Matrix

| Platform | CI Build | Release Build |
|----------|----------|---------------|
| Linux x64 | AppImage | AppImage, .deb |
| Windows x64 | .msi | .msi, .exe |
| macOS Intel | .dmg | .dmg, .app.tar.gz |
| macOS ARM | - | .dmg, .app.tar.gz |

## Release Tags

| Tag Pattern | Description |
|-------------|-------------|
| `v*` | Creates GitHub Release with all platform binaries |

## Required Secrets (in DevHub repo)

| Secret | Required | Description |
|--------|----------|-------------|
| `MYSTIRA_WORKSPACE_DISPATCH_TOKEN` | Yes | GitHub PAT for workspace notifications |
| `TAURI_PRIVATE_KEY` | Optional | Tauri signing key for auto-updates |
| `TAURI_KEY_PASSWORD` | Optional | Password for signing key |

## Comparison with Other Projects

| Feature | DevHub (Tauri) | Other Projects |
|---------|----------------|----------------|
| Type | Desktop App | Web Services |
| Deployment | GitHub Releases | K8s / App Service |
| Artifacts | Desktop binaries | Docker images |
| Runtime | Native + WASM | Container / Serverless |

## Workspace Integration

DevHub integrates with the workspace via `repository_dispatch` events:

1. **`devhub-deploy`** - Triggered by `build-deploy.yml` after successful dev builds
2. **`devhub-release`** - Triggered by `release.yml` after creating a GitHub Release

The workspace handles:
- Submodule reference updates
- MS Teams notifications
- Deployment tracking

## Additional Files

| File | Description |
|------|-------------|
| `Dockerfile` | Leptos SSR Docker build (for future web deployment) |
| `tauri-*.yml` | Legacy template variants (kept for reference) |
