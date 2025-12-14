# Repository Extraction Analysis

**Date**: 2025-12-14  
**Status**: Analysis Complete - Recommendations Provided

## Executive Summary

This document analyzes all repositories in the Mystira workspace to determine which should remain as submodules, which should be extracted to standalone repositories, and which should be consolidated.

## Current Repository Structure

| Repository                 | Path                       | Tech Stack       | Deployment     | Status           |
| -------------------------- | -------------------------- | ---------------- | -------------- | ---------------- |
| **Mystira.Chain**          | `packages/chain`           | Python (gRPC)    | Kubernetes     | Submodule        |
| **Mystira.App**            | `packages/app`             | .NET 9           | Azure PaaS     | Submodule        |
| **Mystira.DevHub**         | `packages/app/tools`       | .NET             | Desktop App    | Nested Submodule |
| **Mystira.Publisher**      | `packages/publisher`       | TypeScript/React | Kubernetes     | Submodule        |
| **Mystira.StoryGenerator** | `packages/story-generator` | .NET             | Kubernetes     | Submodule        |
| **Mystira.INFRA**          | `infra`                    | Terraform/K8s    | Infrastructure | Submodule        |

## Analysis Criteria

We evaluate each repository based on:

1. **Independence**: Can it be developed and deployed independently?
2. **Coupling**: Code dependencies vs. network-based dependencies
3. **Technology Stack**: Different stacks benefit from separate repos
4. **Release Cycle**: Independent vs. synchronized releases
5. **Team Ownership**: Different teams/services
6. **Deployment Model**: Shared vs. independent infrastructure
7. **Code Sharing**: Shared libraries vs. service boundaries

---

## Detailed Analysis

### ✅ **KEEP AS SUBMODULE**: Mystira.Chain

**Current Status**: Independent Python gRPC service

**Rationale**:

- ✅ **Technology Independence**: Python stack, completely different from other services
- ✅ **Deployment Independence**: Deployed to Kubernetes independently via INFRA
- ✅ **Service Boundary**: Clean gRPC API interface (network coupling, not code coupling)
- ✅ **Release Cycle**: Can release independently without affecting other services
- ✅ **Team Ownership**: Likely owned by blockchain/Web3 team

**Recommendation**: **Keep as submodule** - Well-isolated service with clear boundaries.

**Coupling**:

- Network-based coupling via gRPC (Publisher calls Chain)
- No shared code dependencies
- Protocol-defined interface (`.proto` files)

---

### ⚠️ **EXTRACT TO STANDALONE**: Mystira.DevHub

**Current Status**: Nested submodule inside `packages/app/tools/`

**Rationale**:

- ⚠️ **Poor Structure**: Nested submodule is complex and hard to manage
- ⚠️ **Different Purpose**: Desktop application vs. web services
- ✅ **Technology Independence**: .NET desktop app (different from web APIs)
- ✅ **Deployment Independence**: Desktop application, no cloud deployment needed
- ✅ **Release Cycle**: Independent release cycle from App services
- ✅ **Team Ownership**: Likely owned by DevOps/Developer Tools team

**Recommendation**: **Extract to standalone repository** at root level (`packages/devhub/` or `tools/devhub/`)

**Action Items**:

1. Move submodule from `packages/app/tools/` to `packages/devhub/`
2. Update `.gitmodules`
3. Update workspace configuration
4. Update documentation

**Benefits**:

- Cleaner structure (no nested submodules)
- Easier to discover and maintain
- Better alignment with other packages at same level

---

### ✅ **KEEP AS SUBMODULE**: Mystira.Publisher

**Current Status**: Independent TypeScript/React application

**Rationale**:

- ✅ **Technology Independence**: TypeScript/React (different from .NET services)
- ✅ **Deployment Independence**: Deployed to Kubernetes independently
- ✅ **Service Boundary**: Frontend application, communicates via network (gRPC to Chain)
- ✅ **Release Cycle**: Can release independently
- ✅ **Team Ownership**: Likely owned by frontend/web team

**Recommendation**: **Keep as submodule** - Independent frontend service.

**Coupling**:

- Network-based coupling to Chain service (gRPC)
- No shared code with other services
- Standalone React application

---

### ⚠️ **CONSIDER EXTRACTING**: Mystira.StoryGenerator

**Current Status**: .NET service deployed to Kubernetes

**Rationale**:

- ⚠️ **Technology Overlap**: Uses .NET (same as App), but different service boundary
- ✅ **Deployment Independence**: Kubernetes deployment (different from App's Azure PaaS)
- ✅ **Service Boundary**: Independent AI/story generation service
- ✅ **Release Cycle**: Likely independent release cycle
- ⚠️ **Team Ownership**: Could be same team as App (both .NET) or separate AI team

**Considerations**:

- If owned by different team or has different release cadence: **Extract**
- If tightly coupled with App domain logic: **Keep as submodule**

**Recommendation**: **Keep as submodule for now**, but monitor coupling.

**If extracting**:

- Evaluate shared .NET libraries (if any)
- Ensure no code dependencies on App
- Consider if it should share infrastructure patterns with App or Chain/Publisher

---

### ✅ **KEEP AS SUBMODULE**: Mystira.App

**Current Status**: Large .NET monorepo with multiple projects

**Rationale**:

- ✅ **Cohesive Domain**: All projects share domain model and .NET ecosystem
- ✅ **Deployment Model**: Azure PaaS deployment (different from K8s services)
- ✅ **Infrastructure Co-location**: Has its own infrastructure in `packages/app/infrastructure/` (per ADR-0001)
- ✅ **Shared Libraries**: Domain, infrastructure libraries shared across projects
- ✅ **Technology Consistency**: All .NET 9, benefits from unified build

**Recommendation**: **Keep as submodule** - Large cohesive monorepo with shared codebase.

**Note**: App is already being modularized internally (Web, Mobile, APIs). This is internal organization, not requiring extraction.

---

### ✅ **KEEP AS SUBMODULE**: Mystira.INFRA

**Current Status**: Infrastructure as Code repository

**Rationale**:

- ✅ **Clear Purpose**: Infrastructure management is distinct from application code
- ✅ **Multi-Service**: Manages infrastructure for multiple services (Chain, Publisher, StoryGenerator)
- ✅ **Different Tools**: Terraform, Kubernetes manifests, deployment scripts
- ✅ **Team Ownership**: DevOps/Platform team
- ✅ **Release Cycle**: Independent infrastructure updates

**Recommendation**: **Keep as submodule** - Essential for coordinating multi-service deployments.

**Note**: Some infrastructure for App lives in `packages/app/infrastructure/` (Azure Bicep) per ADR-0001 hybrid approach. This is acceptable separation of concerns.

---

## Summary Recommendations

### ✅ Keep as Submodules (No Changes)

1. **Mystira.Chain** - Well-isolated Python service
2. **Mystira.Publisher** - Independent frontend service
3. **Mystira.StoryGenerator** - Independent .NET service (monitor coupling)
4. **Mystira.App** - Cohesive .NET monorepo
5. **Mystira.INFRA** - Infrastructure coordination

### ⚠️ Extract/Reorganize

1. **Mystira.DevHub** - Move from nested submodule to root level

---

## Action Plan

### Phase 1: DevHub Extraction (Recommended)

**Priority**: Medium  
**Effort**: Low  
**Risk**: Low

**Steps**:

1. Create new submodule entry in `.gitmodules` for `packages/devhub`
2. Point to existing Mystira.DevHub repository
3. Update CI/CD workflows if they reference the nested path
4. Update documentation (README, CONTRIBUTING, etc.)
5. Remove old nested submodule reference
6. Test workspace build and CI/CD

**Commands**:

```bash
# Remove nested submodule
git submodule deinit packages/app/tools
git rm packages/app/tools
rm -rf .git/modules/packages/app/tools

# Add as root-level submodule
git submodule add -b main https://github.com/phoenixvc/Mystira.DevHub.git packages/devhub
```

---

## Future Considerations

### Potential Future Extractions

1. **Shared Libraries** (if they emerge):
   - If common libraries are created (e.g., shared TypeScript types, .NET contracts)
   - Consider extracting to separate packages: `@mystira/shared-types`, `Mystira.Contracts`
   - Publish to npm/NuGet for consumption

2. **StoryGenerator**:
   - Monitor coupling with App
   - If it grows significantly or gains separate team ownership, consider extraction
   - Current nested structure is acceptable if release cycles align

3. **App Internal Modularization**:
   - App is already being modularized (Web, Mobile, APIs)
   - Keep this as internal organization within the App repository
   - Only extract if release cycles diverge significantly

---

## Decision Matrix

| Repository     | Extract?   | Reason                           | Priority |
| -------------- | ---------- | -------------------------------- | -------- |
| Chain          | ❌ No      | Well-isolated Python service     | -        |
| Publisher      | ❌ No      | Independent frontend             | -        |
| StoryGenerator | ⚠️ Monitor | Independent but same tech as App | Low      |
| App            | ❌ No      | Cohesive monorepo                | -        |
| DevHub         | ✅ Yes     | Nested submodule complexity      | Medium   |
| INFRA          | ❌ No      | Multi-service coordination       | -        |

---

## Benefits of Current Structure

### Advantages of Submodules

1. **Service Independence**: Each service can evolve independently
2. **Clear Boundaries**: Network-based coupling (APIs) vs. code coupling
3. **Team Autonomy**: Different teams can own different repos
4. **Release Flexibility**: Independent release cycles
5. **Technology Diversity**: Different tech stacks (Python, TypeScript, .NET) managed separately

### When Submodules Make Sense

- ✅ Different technology stacks
- ✅ Independent deployment models
- ✅ Network-based coupling (APIs, gRPC)
- ✅ Different team ownership
- ✅ Independent release cycles

### When to Extract

- ❌ Nested submodules (harder to manage)
- ❌ Shared code dependencies (better as packages)
- ❌ Tight coupling requiring frequent cross-repo changes
- ❌ Same technology stack with no code sharing

---

## Conclusion

**Primary Recommendation**: Extract DevHub from nested submodule to root level.

The current structure is generally well-organized with clear service boundaries. The main issue is the nested submodule structure of DevHub, which should be flattened for better maintainability.

All other repositories should remain as submodules because they represent:

- Different technology stacks
- Independent services with network-based coupling
- Different deployment models
- Independent release cycles

**Next Steps**:

1. Review this analysis with team
2. Prioritize DevHub extraction
3. Plan and execute DevHub move
4. Update documentation
5. Monitor StoryGenerator coupling over time
