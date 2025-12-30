# DevHub CI/CD Documentation

This directory contains reference documentation for the Mystira.DevHub repository CI/CD setup.

## Current Status

**DevHub now has its own comprehensive CI/CD workflows** directly in the repository. The workflows support both the current React-based Tauri app and the upcoming Leptos frontend.

## DevHub Workflows (in repository)

| Workflow | Description |
|----------|-------------|
| `ci.yml` | Full CI for Rust workspace, React frontend, and .NET components |
| `build-tauri.yml` | Cross-platform Tauri builds for React and Leptos |
| `release.yml` | GitHub Releases with desktop binaries |

## Project Architecture

DevHub is a **Tauri desktop application** with:
- **React Frontend** (current): TypeScript/Vite + Tauri 1.x
- **Leptos Frontend** (upcoming): Rust/WASM + Tauri 2.0
- **.NET Components**: CLI tools and services

## CI Pipeline

```
Rust Workspace:
├── lint-rust (clippy, rustfmt)
├── test-rust (contracts, tauri backend)
└── build-leptos-wasm (trunk build)

React DevHub:
├── lint-react (tsc, eslint)
├── test-react (vitest)
└── build-react

.NET:
└── build-dotnet (CLI + Services)
```

## Build Matrix

| Platform | React (Tauri 1.x) | Leptos (Tauri 2.0) |
|----------|-------------------|---------------------|
| Linux x64 | AppImage, deb | AppImage, deb |
| Windows x64 | msi, exe | msi, exe |
| macOS Intel | dmg | dmg |
| macOS ARM | - | dmg |

## Release Tags

| Tag Pattern | Builds |
|-------------|--------|
| `v*` | Both React and Leptos |
| `leptos-v*` | Leptos only |
| `react-v*` | React only |

## Required Secrets (in DevHub repo)

| Secret | Description |
|--------|-------------|
| `MYSTIRA_WORKSPACE_DISPATCH_TOKEN` | GitHub PAT for workspace notifications |
| `TAURI_PRIVATE_KEY` | (Optional) Tauri signing key for auto-updates |
| `TAURI_KEY_PASSWORD` | (Optional) Password for signing key |

## Comparison with Other Projects

| Feature | DevHub (Tauri) | Other Projects |
|---------|----------------|----------------|
| Type | Desktop App | Web Services |
| Deployment | GitHub Releases | K8s / App Service |
| Artifacts | Desktop binaries | Docker images |
| Runtime | Native + WASM | Container / Serverless |

---

## Legacy Files

The files in this directory (`ci.yml`, `build-deploy.yml`, `release.yml`, `Dockerfile`) are legacy templates that were originally designed for a Leptos SSR web application. They are kept for reference only.

**For current DevHub CI/CD, see the workflows in the DevHub repository directly.**
