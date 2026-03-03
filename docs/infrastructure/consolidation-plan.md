# Infra Repository Consolidation Plan

**Status**: Complete
**Date**: 2025-12-19
**Completed**: 2025-12-19

## Overview

This plan consolidates the `Mystira.Infra` repository (currently a submodule) directly into the `Mystira.workspace` repository.

## Current State

### Repository Structure

```
Mystira.workspace/
├── infra/                    ← Submodule → Mystira.Infra (private repo)
├── packages/
│   ├── publisher/            ← Submodule → Mystira.Publisher
│   ├── chain/                ← Submodule → Mystira.Chain
│   ├── story-generator/      ← Submodule → Mystira.StoryGenerator
│   ├── app/                  ← Submodule → Mystira.App
│   ├── devhub/               ← Submodule → Mystira.DevHub
│   └── admin-ui/             ← Submodule → Mystira.AdminUI
├── .github/workflows/
└── docs/
```

### What's in Mystira.Infra

```
infra/
├── terraform/
│   ├── modules/
│   │   ├── chain/              # Chain service infrastructure
│   │   ├── publisher/          # Publisher service infrastructure
│   │   ├── story-generator/    # Story Generator infrastructure
│   │   ├── dns/                # DNS zone management
│   │   ├── front-door/         # Azure Front Door (WAF, CDN, DDoS)
│   │   └── shared/
│   │       ├── postgresql/     # Shared PostgreSQL database
│   │       ├── redis/          # Shared Redis cache
│   │       └── monitoring/     # Log Analytics + App Insights
│   └── environments/
│       ├── dev/                # Dev environment config
│       ├── staging/            # Staging environment config
│       └── prod/               # Production environment config
├── kubernetes/
│   ├── base/
│   │   ├── publisher/          # Publisher K8s manifests
│   │   ├── chain/              # Chain K8s manifests
│   │   └── story-generator/    # Story Generator K8s manifests
│   └── overlays/
│       ├── dev/                # Dev kustomize overlay
│       ├── staging/            # Staging kustomize overlay
│       └── prod/               # Production kustomize overlay
├── docker/
│   ├── chain/Dockerfile
│   ├── publisher/Dockerfile
│   └── story-generator/Dockerfile
├── scripts/
│   └── bootstrap-terraform-backend.sh
└── docs/
    ├── AZURE_SETUP.md
    ├── DNS_INGRESS_SETUP.md
    ├── FRONT_DOOR_DEPLOYMENT_GUIDE.md
    └── ...
```

---

## Target State (After Consolidation)

```
Mystira.workspace/
├── infra/                    ← DIRECT (no longer a submodule)
│   ├── terraform/
│   ├── kubernetes/
│   ├── docker/
│   └── scripts/
├── packages/
│   ├── publisher/            ← Keep as submodule (service code)
│   ├── chain/                ← Keep as submodule (service code)
│   ├── story-generator/      ← Keep as submodule (service code)
│   ├── app/                  ← Keep as submodule (different tooling - Bicep)
│   ├── devhub/               ← Keep as submodule (Tauri desktop app)
│   └── admin-ui/             ← Keep as submodule (UI component)
├── .github/workflows/
├── docs/
└── scripts/
```

---

## Migration Steps

### Phase 1: Preparation (Before Migration)

- [ ] **1.1** Ensure all pending infra changes are committed and pushed
- [ ] **1.2** Create backup of current infra submodule state
- [ ] **1.3** Document current infra submodule commit SHA
- [ ] **1.4** Notify team of upcoming consolidation
- [ ] **1.5** Ensure no active deployments in progress

### Phase 2: Migration Execution

```bash
# Step 2.1: Clone the infra repo separately
cd /tmp
git clone https://github.com/phoenixvc/Mystira.Infra.git mystira-infra-backup

# Step 2.2: Remove the submodule from workspace
cd /path/to/Mystira.workspace
git submodule deinit infra
git rm infra
rm -rf .git/modules/infra

# Step 2.3: Copy infra content directly into workspace
cp -r /tmp/mystira-infra-backup/* infra/

# Step 2.4: Remove .git from copied infra (it's no longer a separate repo)
rm -rf infra/.git

# Step 2.5: Update .gitmodules (remove infra entry)
# Edit .gitmodules and remove:
# [submodule "infra"]
#     path = infra
#     url = https://github.com/phoenixvc/Mystira.Infra.git
#     branch = main

# Step 2.6: Commit the consolidation
git add .
git commit -m "refactor: consolidate Mystira.Infra into workspace

- Remove infra submodule
- Add infra content directly to repository
- Simplifies CI/CD (no submodule token needed for infra)
- Enables atomic commits across infra + workflows"

git push
```

### Phase 3: Update CI/CD Workflows

Update workflows that reference submodules to remove infra-specific token requirements:

```yaml
# Before (in workflows)
- uses: actions/checkout@v6
  with:
    submodules: recursive
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}

# After (infra no longer needs token, but other submodules still do)
- uses: actions/checkout@v6
  with:
    submodules: recursive
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
# Note: Token still needed for other submodules (publisher, chain, etc.)
```

### Phase 4: Archive Old Repository

- [ ] **4.1** Mark `Mystira.Infra` repo as archived in GitHub
- [ ] **4.2** Update README in archived repo pointing to new location
- [ ] **4.3** Keep repo for historical reference (6 months minimum)

### Phase 5: Verification

- [ ] **5.1** Run all CI workflows to verify they pass
- [ ] **5.2** Deploy to dev environment successfully
- [ ] **5.3** Verify Terraform state is accessible
- [ ] **5.4** Verify kubectl access to clusters

---

## Benefits of Consolidation

| Benefit                     | Description                                   |
| --------------------------- | --------------------------------------------- |
| **Atomic commits**          | Infra + workflow changes in single commit     |
| **Simpler CI/CD**           | No submodule sync issues for infra            |
| **Better discoverability**  | All infra code visible in main repo           |
| **Easier onboarding**       | New developers see everything in one place    |
| **Reduced auth complexity** | One less private repo to manage access for    |
| **Unified history**         | Git log shows infra changes with related code |

## What Stays as Submodules

| Submodule                  | Reason                                   |
| -------------------------- | ---------------------------------------- |
| `packages/publisher`       | Service code, separate release cycle     |
| `packages/chain`           | Service code, separate release cycle     |
| `packages/story-generator` | Service code, separate release cycle     |
| `packages/app`             | Different tooling (Bicep), separate team |
| `packages/devhub`          | Desktop app, completely different build  |
| `packages/admin-ui`        | UI component library                     |

---

## Rollback Plan

If consolidation causes issues:

```bash
# Remove the direct infra directory
rm -rf infra

# Re-add as submodule
git submodule add https://github.com/phoenixvc/Mystira.Infra.git infra
git submodule update --init infra

# Commit rollback
git add .
git commit -m "revert: restore infra as submodule"
git push
```

---

## Timeline

| Phase                  | Duration     | Dependencies          |
| ---------------------- | ------------ | --------------------- |
| Phase 1: Preparation   | 1 hour       | Team notification     |
| Phase 2: Migration     | 30 minutes   | No active deployments |
| Phase 3: CI/CD Updates | 1 hour       | Phase 2 complete      |
| Phase 4: Archive       | 15 minutes   | Phase 3 verified      |
| Phase 5: Verification  | 2 hours      | Deploy to dev         |
| **Total**              | **~5 hours** |                       |

---

## Checklist

### Pre-Migration

- [x] All infra changes committed and pushed
- [x] Backup of infra repo created
- [x] Current commit SHA documented: `ee1c91b`
- [x] Team notified
- [x] No active deployments

### Migration

- [x] Submodule removed
- [x] Content copied (55 files)
- [x] .gitmodules updated
- [x] Changes committed and pushed

### Post-Migration

- [ ] CI workflows passing
- [ ] Dev deployment successful
- [ ] Terraform state accessible
- [ ] Old repo archived

---

## Notes

### Front Door Enabling

As part of this consolidation, enable Front Door in dev:

```bash
# After consolidation, in the workspace
cd infra/terraform/environments/dev
mv front-door-example.tf.disabled front-door.tf
git add front-door.tf
git commit -m "feat: enable Front Door for dev environment"
```

### Git History

The consolidation will not preserve the git history from `Mystira.Infra`. If history preservation is critical, consider using `git subtree` instead:

```bash
# Alternative: Using git subtree (preserves history)
git subtree add --prefix infra https://github.com/phoenixvc/Mystira.Infra.git main --squash
```

---

_Created: 2025-12-19_
_Author: Claude_
