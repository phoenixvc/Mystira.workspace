# Mystira Package Inventory

**Last Updated**: 2025-12-24
**Purpose**: Analyze packages for potential consolidation and optimal placement

---

## Consolidation Status (ADR-0020)

| Status | Package | Action |
|--------|---------|--------|
| ✅ Complete | `@mystira/contracts` | Unified TypeScript contracts |
| ✅ Complete | `Mystira.Contracts` | Unified .NET contracts |
| ✅ Complete | `@mystira/shared-utils` | Moved to workspace level |
| ✅ Complete | `Mystira.Shared` | .NET shared auth/infrastructure |
| ⏳ Deprecated | `@mystira/app-contracts` | Use `@mystira/contracts/app` |
| ⏳ Deprecated | `@mystira/story-generator-contracts` | Use `@mystira/contracts/story-generator` |
| ⏳ Deprecated | `Mystira.App.Contracts` | Use `Mystira.Contracts.App` |
| ⏳ Deprecated | `Mystira.StoryGenerator.Contracts` | Use `Mystira.Contracts.StoryGenerator` |
| ⏳ Deprecated | `Mystira.App.Shared` | Use `Mystira.Shared` |

---

## Package Inventory Summary

### NPM Packages (Workspace Level)

| Package | Location | Status | Notes |
|---------|----------|--------|-------|
| `@mystira/contracts` | `packages/contracts` | ✅ Active | Unified API types with subpath exports |
| `@mystira/shared-utils` | `packages/shared-utils` | ✅ Active | Retry, logging, validation utilities |

### NPM Packages (Submodules)

| Package | Current Location | Type | Could Combine? | Optimal Destination | Notes |
|---------|------------------|------|----------------|---------------------|-------|
| `@mystira/app` | `packages/app` (submodule) | NPM | ❌ Keep separate | Submodule | Core app - linked with contracts |
| `@mystira/app-contracts` | `packages/app` (submodule) | NPM | ⏳ Deprecated | Use `@mystira/contracts/app` | Migrating to unified contracts |
| `@mystira/story-generator` | `packages/story-generator` (submodule) | NPM | ❌ Keep separate | Submodule | Core service - linked with contracts |
| `@mystira/story-generator-contracts` | `packages/story-generator` (submodule) | NPM | ⏳ Deprecated | Use `@mystira/contracts/story-generator` | Migrating to unified contracts |
| `@mystira/publisher` | `packages/publisher` (submodule) | NPM | ❌ Keep separate | Submodule | Independent service |

### NuGet Packages (Workspace Level)

| Package | Location | Status | Notes |
|---------|----------|--------|-------|
| `Mystira.Contracts` | `packages/contracts/dotnet` | ✅ Active | Unified .NET contracts |
| `Mystira.Shared` | `packages/shared/Mystira.Shared` | ✅ Active | Auth, authorization, telemetry |

### NuGet Packages (Submodules)

| Package | Current Location | Type | Could Combine? | Optimal Destination | Notes |
|---------|------------------|------|----------------|---------------------|-------|
| `Mystira.App.Domain` | `packages/app/src/` | NuGet | ❌ Keep | Submodule | Core domain - consumed by Admin.Api via NuGet |
| `Mystira.App.Application` | `packages/app/src/` | NuGet | ❌ Keep | Submodule | Application layer - consumed by Admin.Api |
| `Mystira.App.Shared` | `packages/app/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Shared` | Migrating to workspace-level shared |
| `Mystira.App.Contracts` | `packages/app/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Contracts.App` | Migrating to unified contracts |
| `Mystira.StoryGenerator.Contracts` | `packages/story-generator/src/` | NuGet | ⏳ Deprecated | Use `Mystira.Contracts.StoryGenerator` | Migrating to unified contracts |

### Docker Images

| Image | Current Location | Could Combine? | Optimal Destination | Notes |
|-------|------------------|----------------|---------------------|-------|
| `publisher` | `infra/docker/publisher/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `chain` | `infra/docker/chain/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `story-generator` | `infra/docker/story-generator/` | ❌ Keep | Move to submodule (ADR-0019) | Independent service |
| `admin-api` | Submodule | ❌ Keep | Submodule | Already in correct location |

### Services (Non-Package)

| Service | Current Location | Could Combine? | Optimal Destination | Notes |
|---------|------------------|----------------|---------------------|-------|
| Mystira.App.Api | `packages/app` | ❌ Keep | Submodule | Public API - Azure App Service |
| Mystira.App.PWA | `packages/app` | ❌ Keep | Submodule | Blazor WASM - Static Web App |
| Mystira.StoryGenerator.Api | `packages/story-generator` | ❌ Keep | Submodule | API service - Kubernetes |
| Mystira.StoryGenerator.Web | `packages/story-generator` | ❌ Keep | Submodule | Blazor WASM - Static Web App |
| Mystira.Admin.Api | `packages/admin-api` | ❌ Keep | Separate submodule | Security isolation required |
| Mystira.Admin.UI | `packages/admin-ui` | ❌ Keep | Separate submodule | Security isolation required |
| Mystira.Chain | `packages/chain` | ❌ Keep | Submodule | Blockchain - Kubernetes |
| Mystira.DevHub | `packages/devhub` | ❌ Keep | Submodule | Developer portal |

---

## Consolidation Recommendations

### High Priority - Should Consolidate

#### 1. Create `@mystira/contracts` (Unified TypeScript Types)

**Combine:**
- `@mystira/app-contracts`
- `@mystira/story-generator-contracts`

**Benefits:**
- Single source of truth for API types
- Simplified dependency management
- Consistent versioning across all TypeScript consumers
- Reduces duplication in client apps

**Implementation:**
```
packages/
└── contracts/           # NEW workspace package
    ├── package.json     # @mystira/contracts
    ├── src/
    │   ├── app/         # App API types
    │   ├── story-generator/  # Story Generator types
    │   └── index.ts     # Unified exports
    └── tsconfig.json
```

#### 2. Create `Mystira.Contracts` (Unified NuGet Package)

**Combine:**
- `Mystira.App.Contracts`
- `Mystira.StoryGenerator.Contracts`

**Benefits:**
- Single NuGet package for all API contracts
- Simplified reference management in consuming projects
- Consistent versioning across .NET consumers

**Implementation:**
```
packages/
└── contracts/           # NEW workspace package (or separate submodule)
    └── src/
        └── Mystira.Contracts/
            ├── App/     # App contracts
            ├── StoryGenerator/  # Story Generator contracts
            └── Mystira.Contracts.csproj
```

#### 3. Move `@mystira/shared-utils` to Workspace

**Current:** Inside `packages/publisher` submodule
**Optimal:** Workspace-level `packages/shared-utils`

**Benefits:**
- Truly shared across all packages
- Independent versioning from publisher
- Cleaner dependency graph

---

### Medium Priority - Consider Consolidating

#### 4. Create `Mystira.Shared` NuGet Package

**Evaluate combining:**
- `Mystira.App.Shared`
- Common infrastructure utilities

**Benefits:**
- Shared cross-cutting concerns
- Reduces duplication

**Considerations:**
- May increase coupling between services
- Need to carefully separate app-specific from truly shared code

---

### Keep Separate (No Consolidation)

| Package | Reason |
|---------|--------|
| `Mystira.App.Domain` | Core domain logic - tightly coupled to App |
| `Mystira.App.Application` | Application layer - tightly coupled to App |
| `Mystira.App.Infrastructure.*` | Infrastructure implementations - service-specific |
| Service containers | Each has unique runtime requirements |
| Admin API/UI | Security isolation required |

---

## Recommended Package Structure

### Current State
```
packages/
├── app/                          # Submodule (Mystira.App)
│   └── src/
│       ├── Mystira.App.Contracts/  ← Could extract
│       └── ...
├── story-generator/              # Submodule (Mystira.StoryGenerator)
│   └── src/
│       ├── Mystira.StoryGenerator.Contracts/  ← Could extract
│       └── ...
├── publisher/                    # Submodule (Mystira.Publisher)
│   └── packages/
│       └── shared-utils/         ← Could move to workspace
├── admin-api/                    # Submodule
├── admin-ui/                     # Submodule
├── chain/                        # Submodule
└── devhub/                       # Submodule
```

### Proposed State (After Consolidation)
```
packages/
├── contracts/                    # NEW - Workspace native
│   ├── package.json              # @mystira/contracts (NPM)
│   ├── src/
│   │   ├── typescript/           # TypeScript types
│   │   │   ├── app/
│   │   │   ├── story-generator/
│   │   │   └── index.ts
│   │   └── dotnet/               # .NET contracts
│   │       └── Mystira.Contracts/
│   │           ├── App/
│   │           └── StoryGenerator/
│   └── Mystira.Contracts.csproj  # NuGet package
│
├── shared-utils/                 # NEW - Workspace native
│   ├── package.json              # @mystira/shared-utils
│   └── src/
│
├── app/                          # Submodule (unchanged structure)
├── story-generator/              # Submodule (unchanged structure)
├── publisher/                    # Submodule (remove shared-utils)
├── admin-api/                    # Submodule
├── admin-ui/                     # Submodule
├── chain/                        # Submodule
└── devhub/                       # Submodule
```

---

## Migration Impact

### If We Consolidate Contracts

| Impact Area | Changes Required |
|-------------|------------------|
| **NPM Consumers** | Update imports from `@mystira/app-contracts` → `@mystira/contracts/app` |
| **NuGet Consumers** | Update package reference to `Mystira.Contracts` |
| **Changesets Config** | Update linked packages |
| **Release Workflow** | Simplify - single contracts package instead of multiple |
| **Submodule Dispatch** | Update to trigger `contracts-publish` instead of separate events |

### Versioning After Consolidation

```json
// .changeset/config.json
{
  "linked": [
    ["@mystira/contracts"],  // Unified contracts
    ["@mystira/app"],
    ["@mystira/story-generator"],
    ["@mystira/publisher", "@mystira/shared-utils"]
  ]
}
```

---

## Decision Matrix

| Package | Keep Separate | Consolidate | Move to Workspace |
|---------|:-------------:|:-----------:|:-----------------:|
| `@mystira/app` | ✅ | | |
| `@mystira/app-contracts` | | ✅ | ✅ |
| `@mystira/story-generator` | ✅ | | |
| `@mystira/story-generator-contracts` | | ✅ | ✅ |
| `@mystira/publisher` | ✅ | | |
| `@mystira/shared-utils` | | | ✅ |
| `Mystira.App.Domain` | ✅ | | |
| `Mystira.App.Application` | ✅ | | |
| `Mystira.App.Contracts` | | ✅ | ✅ |
| `Mystira.StoryGenerator.Contracts` | | ✅ | ✅ |

---

## Next Steps

1. **Create `packages/contracts`** workspace package
2. **Migrate TypeScript types** from submodules
3. **Migrate NuGet contracts** from submodules
4. **Move `shared-utils`** to workspace level
5. **Update Changesets config** for new package structure
6. **Update CI/CD workflows** for unified contracts publishing
7. **Update submodule workflows** to remove contracts publishing

---

## Related Documentation

- [Publishing Flow](../cicd/publishing-flow.md)
- [Package Releases Guide](../guides/package-releases.md)
- [ADR-0007: NuGet Feed Strategy](../architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0009: App Segregation Strategy](../architecture/adr/0009-further-app-segregation-strategy.md)
