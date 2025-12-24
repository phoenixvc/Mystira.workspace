# ADR-0020: Package Consolidation Strategy

## Status

**Proposed** - 2025-12-24

## Context

The Mystira platform has evolved with multiple package repositories (submodules) that each publish their own contracts and shared utilities. This has led to:

### Current State

| Package | Location | Type | Published To |
|---------|----------|------|--------------|
| `@mystira/app-contracts` | `packages/app` | NPM | npmjs.org |
| `@mystira/story-generator-contracts` | `packages/story-generator` | NPM | npmjs.org |
| `Mystira.App.Contracts` | `packages/app` | NuGet | GitHub Packages + NuGet.org |
| `Mystira.StoryGenerator.Contracts` | `packages/story-generator` | NuGet | GitHub Packages + NuGet.org |
| `@mystira/shared-utils` | `packages/publisher` | NPM | npmjs.org |

### Problems

1. **Scattered Contracts**: API type definitions are spread across multiple submodules
2. **Versioning Complexity**: Each contracts package versions independently, requiring careful coordination
3. **Dependency Confusion**: Consumers must reference multiple packages for complete type coverage
4. **Shared Utils Misplacement**: `@mystira/shared-utils` lives in the Publisher submodule but is meant to be truly shared
5. **Publishing Overhead**: Multiple workflows and triggers needed for each contracts package

### Requirements

- Simplify dependency management for API consumers
- Maintain version synchronization across NPM and NuGet
- Support the existing bidirectional publishing flow (Changesets ↔ NuGet)
- Enable independent service evolution while sharing contracts

## Decision

We will consolidate contracts and shared utilities into workspace-level packages:

### 1. Create `@mystira/contracts` (NPM) and `Mystira.Contracts` (NuGet)

Combine all API contracts into a single package per ecosystem:

```
packages/
└── contracts/                    # NEW - Workspace native
    ├── package.json              # @mystira/contracts
    ├── src/
    │   ├── app/                  # App API types
    │   │   ├── index.ts
    │   │   └── types.ts
    │   ├── story-generator/      # Story Generator types
    │   │   ├── index.ts
    │   │   └── types.ts
    │   └── index.ts              # Unified exports
    └── dotnet/
        └── Mystira.Contracts/
            ├── App/              # App contracts
            ├── StoryGenerator/   # Story Generator contracts
            └── Mystira.Contracts.csproj
```

### 2. Move `@mystira/shared-utils` to Workspace

```
packages/
└── shared-utils/                 # MOVED from publisher
    ├── package.json              # @mystira/shared-utils
    └── src/
        └── index.ts
```

### 3. Update Changesets Configuration

```json
{
  "linked": [
    ["@mystira/contracts"],
    ["@mystira/app"],
    ["@mystira/story-generator"],
    ["@mystira/publisher"],
    ["@mystira/shared-utils"]
  ]
}
```

### 4. Simplified Publishing Flow

```
                          BEFORE
┌─────────────────────────────────────────────────────────────────┐
│  @mystira/app → triggers → Mystira.App.Contracts               │
│  @mystira/story-generator → triggers → Mystira.StoryGenerator.Contracts │
│  (4 packages, 4 workflows, complex coordination)                │
└─────────────────────────────────────────────────────────────────┘

                          AFTER
┌─────────────────────────────────────────────────────────────────┐
│  @mystira/contracts → triggers → Mystira.Contracts              │
│  (2 packages, 1 workflow, simple coordination)                  │
└─────────────────────────────────────────────────────────────────┘
```

## Consequences

### Positive

1. **Single Source of Truth**: One package for all API types in each ecosystem
2. **Simplified Dependencies**: Consumers add one package instead of multiple
3. **Easier Versioning**: Single version for all contracts
4. **Cleaner CI/CD**: One publishing workflow for contracts
5. **Better Discoverability**: All types in one searchable package
6. **Proper Placement**: shared-utils at workspace level where it belongs

### Negative

1. **Breaking Change**: Existing consumers must update imports
2. **Migration Effort**: Need to move code and update references
3. **Larger Package**: Consumers get all contracts even if they only need some
4. **Coupled Releases**: All contracts release together (may be undesirable)

### Mitigations

1. **Gradual Migration**: Deprecate old packages, maintain for 2 versions
2. **Subpath Exports**: Use package exports for tree-shaking
   ```json
   {
     "exports": {
       "./app": "./dist/app/index.js",
       "./story-generator": "./dist/story-generator/index.js"
     }
   }
   ```
3. **Clear Documentation**: Migration guide with examples
4. **Automated Codemods**: Script to update imports

## Packages to Keep Separate

The following should NOT be consolidated:

| Package | Reason |
|---------|--------|
| `@mystira/app` | Core service - independent evolution |
| `@mystira/story-generator` | Core service - independent evolution |
| `@mystira/publisher` | Core service - independent evolution |
| `Mystira.App.Domain` | Domain logic - tightly coupled to App |
| `Mystira.App.Application` | Application layer - tightly coupled to App |
| `Mystira.App.Infrastructure.*` | Infrastructure - service-specific |

## Implementation Plan

### Phase 1: Create New Packages (Non-Breaking)

1. Create `packages/contracts` with unified structure
2. Create `packages/shared-utils` at workspace level
3. Set up publishing workflows for new packages
4. Publish initial versions

### Phase 2: Migration Period

1. Update workspace packages to use new contracts
2. Deprecation notices on old packages
3. Migration guide and codemods
4. Monitor adoption

### Phase 3: Cleanup

1. Remove contracts from submodules
2. Remove old publishing workflows
3. Archive deprecated packages
4. Update all documentation

## Import Changes

### TypeScript/NPM

```typescript
// Before
import { StoryRequest } from '@mystira/app-contracts';
import { GeneratorConfig } from '@mystira/story-generator-contracts';

// After
import { StoryRequest } from '@mystira/contracts/app';
import { GeneratorConfig } from '@mystira/contracts/story-generator';

// Or unified import
import { App, StoryGenerator } from '@mystira/contracts';
```

### C#/NuGet

```csharp
// Before
using Mystira.App.Contracts;
using Mystira.StoryGenerator.Contracts;

// After
using Mystira.Contracts.App;
using Mystira.Contracts.StoryGenerator;
```

## Alternatives Considered

### 1. Keep Separate Packages

**Rejected**: Doesn't solve the versioning and coordination complexity.

### 2. Monorepo with Package References

**Rejected**: Would require all submodules to be in the same repo.

### 3. Git Submodule for Contracts

**Rejected**: Adds complexity with submodule management.

### 4. Shared NuGet Feed Only

**Rejected**: Doesn't address NPM package consolidation.

## Related ADRs

- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0009: Further App Segregation Strategy](./0009-further-app-segregation-strategy.md)
- [ADR-0016: Monorepo Tooling and Multi-Repository Strategy](./0016-monorepo-tooling-and-multi-repository-strategy.md)

## References

- [Package Inventory Analysis](../../analysis/package-inventory.md)
- [Package Releases Guide](../../guides/package-releases.md)
- [Publishing Flow](../../cicd/publishing-flow.md)
- [Changesets Documentation](https://github.com/changesets/changesets)
