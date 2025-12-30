# DevHub CI/CD Templates

This directory contains CI/CD workflow templates for the Mystira.DevHub repository (Leptos/Rust frontend).

## Files

| File | Description |
|------|-------------|
| `ci.yml` | Continuous integration - runs on PRs and pushes |
| `build-deploy.yml` | Build Docker image and deploy to dev |
| `release.yml` | Build versioned releases for staging/production |
| `Dockerfile` | Multi-stage Docker build for Leptos SSR app |

## Setup Instructions

### 1. Copy workflows to DevHub repo

```bash
# In the DevHub repository
mkdir -p .github/workflows
cp ci.yml .github/workflows/
cp build-deploy.yml .github/workflows/
cp release.yml .github/workflows/
cp Dockerfile ./
```

### 2. Required Secrets

Configure these secrets in the DevHub repository settings:

| Secret | Description |
|--------|-------------|
| `MYSTIRA_AZURE_CREDENTIALS` | Azure service principal credentials for ACR access |
| `MYSTIRA_WORKSPACE_DISPATCH_TOKEN` | GitHub PAT with `repo` scope for triggering workspace workflows |

### 3. Required Leptos Configuration

Ensure your `Cargo.toml` has the SSR feature:

```toml
[features]
ssr = ["leptos/ssr", "leptos_actix"]  # or leptos_axum
hydrate = ["leptos/hydrate"]

[[bin]]
name = "devhub"
path = "src/main.rs"
```

### 4. Health Endpoint

Add a `/health` endpoint for Kubernetes probes:

```rust
// Using Actix-web
#[get("/health")]
async fn health() -> impl Responder {
    HttpResponse::Ok().body("OK")
}

// Using Axum
async fn health() -> &'static str {
    "OK"
}
```

## Workflow Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         DevHub Repo                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   PR/Push to dev ──► ci.yml ──► Check, Test, Build              │
│                                                                 │
│   Push to dev ──► build-deploy.yml ──► Build Image ──► Push ACR │
│                           │                                     │
│                           ▼                                     │
│              repository_dispatch (devhub-deploy)                │
│                           │                                     │
└───────────────────────────┼─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Mystira.workspace                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   submodule-deploy-dev.yml ──► Deploy to K8s (dev)              │
│                                                                 │
│   staging-release.yml ──► Deploy to K8s (staging)               │
│                                                                 │
│   production-release.yml ──► Deploy to K8s (production)         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## Environment URLs

| Environment | URL |
|-------------|-----|
| Dev | https://dev.devhub.mystira.app |
| Staging | https://staging.devhub.mystira.app |
| Production | https://devhub.mystira.app |

## Docker Build Notes

The Dockerfile uses a multi-stage build:

1. **Builder stage**: Compiles Rust binary and WASM assets using trunk
2. **Runtime stage**: Minimal Debian image with only the compiled binary and assets

Build arguments:
- `LEPTOS_ENV`: Set to `production` for release builds

## Trunk Configuration

Create a `Trunk.toml` in the DevHub repo root:

```toml
[build]
target = "index.html"
dist = "dist"

[watch]
watch = ["src", "style", "assets"]

[[hooks]]
stage = "post_build"
command = "sh"
command_arguments = ["-c", "cp -r assets/* dist/ 2>/dev/null || true"]
```
