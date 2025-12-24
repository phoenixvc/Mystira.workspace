# ADR-0019: Dockerfile Location Standardization

**Status**: Proposed
**Date**: 2024-12-24
**Decision Makers**: DevOps, Platform Team

## Context

Currently, Dockerfiles are inconsistently located across the Mystira ecosystem:

| Service | Code Location | Dockerfile Location | Built By |
|---------|---------------|---------------------|----------|
| Admin.Api | `packages/admin-api` (submodule) | Submodule repo | Submodule CI |
| Admin.UI | `packages/admin-ui` (submodule) | Submodule repo | Submodule CI |
| Chain | `packages/chain` (submodule) | `infra/docker/chain/` (workspace) | Workspace CI |
| Publisher | `packages/publisher` (submodule) | `infra/docker/publisher/` (workspace) | Workspace CI |
| Story-Generator.Api | `packages/story-generator` (submodule) | `infra/docker/story-generator/` (workspace) | Workspace CI |

> **Note**: Story-Generator follows the same pattern as Mystira.App:
> - **API** (`Mystira.StoryGenerator.Api`) → Kubernetes via `story-generator-deploy`
> - **Web** (`Mystira.StoryGenerator.Web`, Blazor WASM) → Static Web App (separate deployment)

This inconsistency causes:
- Confusion about where to find/update Dockerfiles
- Inconsistent CI/CD patterns across services
- Slower feedback loops for Chain/Publisher/Story-Generator (changes require workspace build)

## Decision

**Standardize on Dockerfiles living in their respective submodule repositories.**

All services will:
1. Have their Dockerfile in the submodule repo root
2. Build and push Docker images via submodule CI/CD
3. Trigger workspace deployment via `repository_dispatch`

## Migration Plan

### Phase 1: Move Dockerfiles to Submodule Repos

#### Chain (Python)

1. Copy `infra/docker/chain/Dockerfile` to `Mystira.Chain` repo
2. Update paths:
   ```dockerfile
   # Before (workspace context)
   COPY packages/chain/ ./

   # After (submodule context)
   COPY . ./
   ```
3. Add CI/CD workflow to `Mystira.Chain` repo (use `submodule-cicd-setup.md` template)

#### Publisher (Node.js)

1. Copy `infra/docker/publisher/Dockerfile` to `Mystira.Publisher` repo
2. Update paths and remove pnpm workspace references:
   ```dockerfile
   # Before (workspace context)
   COPY package.json ./
   COPY pnpm-lock.yaml ./
   COPY pnpm-workspace.yaml ./
   COPY packages/publisher/package.json ./packages/publisher/

   # After (submodule context - standalone)
   COPY package.json ./
   COPY pnpm-lock.yaml ./
   ```
3. Ensure Publisher has its own `pnpm-lock.yaml`
4. Add CI/CD workflow

#### Story-Generator (.NET)

1. Copy `infra/docker/story-generator/Dockerfile` to `Mystira.StoryGenerator` repo
2. Update to build **API** project (`src/Mystira.StoryGenerator.Api/`), not Web
3. Add CI/CD workflow

> **Important**: The current workspace Dockerfile incorrectly builds `Mystira.StoryGenerator.Web` (Blazor WASM).
> The `story-generator-deploy` event targets Kubernetes, which should run the **API**.
> Blazor WASM should deploy to Static Web Apps (if needed), similar to `app-swa-deploy`.

### Phase 2: Update Workspace CI/CD

1. Remove Docker build steps from `infra-deploy.yml`:
   - Remove "Build and Push Publisher" step
   - Remove "Build and Push Chain" step
   - (Story-Generator was already missing)

2. Delete workspace Dockerfiles:
   - `infra/docker/chain/Dockerfile`
   - `infra/docker/publisher/Dockerfile`
   - `infra/docker/story-generator/Dockerfile`

### Phase 3: Verify Submodule CI/CD

Each submodule should have a workflow that:
1. Builds Docker image on push to `dev`
2. Pushes to ACR (`myssharedacr.azurecr.io`)
3. Triggers workspace via `repository_dispatch`

## Consequences

### Positive
- Consistent CI/CD pattern across all services
- Faster feedback (Docker build runs on submodule PR)
- Teams own their Dockerfiles
- Clearer separation of concerns

### Negative
- Migration effort required
- Dockerfiles spread across repos (harder to see all at once)

### Neutral
- No change to deployment flow (still uses `repository_dispatch`)

## Action Items

- [ ] Create Dockerfile + workflow in `Mystira.Chain`
- [ ] Create Dockerfile + workflow in `Mystira.Publisher`
- [ ] Create Dockerfile + workflow in `Mystira.StoryGenerator`
- [ ] Remove Docker build steps from workspace `infra-deploy.yml`
- [ ] Delete `infra/docker/` directory from workspace
- [ ] Update documentation
