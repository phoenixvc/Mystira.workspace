# ADR-0024: True Monorepo Migration (Supersedes ADR-0016)

## Status

**Accepted** - 2026-02-22

**Supersedes**: [ADR-0016: Monorepo Tooling and Multi-Repository Strategy](./0016-monorepo-tooling-and-multi-repository-strategy.md)

## Context

ADR-0016 (December 2025) decided to maintain a **hybrid multi-repository approach** using git submodules integrated with pnpm workspaces and Turborepo. While this approach preserved repository independence, it created significant friction:

### Problems with Submodules

1. **Developer friction**: `git submodule update --init --recursive` required after every clone; easy to get into detached HEAD state
2. **Cross-repo changes**: Modifying shared code required coordinating commits across multiple repositories
3. **CI/CD complexity**: Every workflow needed submodule checkout steps, ESLint/config copying hacks, and careful submodule sync
4. **NuGet publishing overhead**: Internal C# packages (Mystira.Core, Mystira.Domain, Mystira.Shared, 7x Infrastructure packages) required a full publish-then-consume cycle with version coordination
5. **npm publishing overhead**: Internal TypeScript packages (@mystira/contracts, @mystira/core-types, @mystira/shared-utils) required GitHub Packages publishing with Changesets
6. **Inconsistent tooling**: Each submodule repo had its own `.eslintrc`, `.prettierrc`, CI workflows, creating drift and duplication
7. **History fragmentation**: Git blame/log couldn't follow changes across submodule boundaries

### Trigger

The workspace already had all the monorepo infrastructure in place (pnpm workspaces, Turborepo, shared configs). The submodule layer added complexity without providing meaningful benefits — the team was already treating the workspace as a single unit of development.

## Decision

**Migrate from git submodules to a true monorepo** where all 7 service repositories are inlined directly into `packages/` within Mystira.workspace.

### Migration Strategy

1. Use `git subtree add` to import each repo with full commit history preserved
2. Convert internal NuGet `<PackageReference>` entries to direct `<ProjectReference>` entries
3. Convert internal npm package references to `workspace:*` protocol
4. Consolidate CI/CD workflows with path-based triggers
5. Upgrade all C# projects to .NET 10 during the migration
6. Remove submodule infrastructure entirely

### Repositories Migrated

| Repository             | Target Path                 | Technology         |
| ---------------------- | --------------------------- | ------------------ |
| Mystira.Publisher      | `packages/publisher/`       | TypeScript/React   |
| Mystira.Admin.UI       | `packages/admin-ui/`        | TypeScript/React   |
| Mystira.DevHub         | `packages/devhub/`          | TypeScript/Tauri   |
| Mystira.Admin.Api      | `packages/admin-api/`       | C#/.NET 10         |
| Mystira.App            | `packages/app/`             | C#/.NET 10, Blazor |
| Mystira.StoryGenerator | `packages/story-generator/` | C#/.NET 10         |
| Mystira.Chain          | `packages/chain/`           | Python 3.12        |

## Consequences

### Positive

- **Simplified developer setup**: `git clone` + `pnpm install` — no submodule initialization
- **Atomic cross-cutting changes**: A single commit can modify shared packages and all consumers
- **Instant type checking**: C# `<ProjectReference>` provides compile-time validation across packages with no publish delay
- **Simplified CI/CD**: No submodule checkout steps, no internal package publishing pipelines
- **Unified tooling**: Single `.eslintrc`, `tsconfig`, `Directory.Build.props` for the whole workspace
- **Full git history**: `git log --follow` works across the entire codebase

### Negative

- **Larger repository**: All code in one repo increases clone size (mitigated by shallow clones in CI)
- **CODEOWNERS complexity**: Need explicit per-path ownership rules (`.github/CODEOWNERS` added)
- **Build times**: Full rebuilds take longer (mitigated by Turborepo caching and path-based CI triggers)

### Neutral

- **Original repos preserved**: Archived on GitHub with a redirect notice, preserving issues and PR history
- **External NuGet/npm packages unchanged**: Only internal Mystira-to-Mystira references changed; external dependencies remain as package references

## Implementation

The migration was executed in February 2026 across multiple phases:

1. TypeScript repos migrated via `git subtree add` (Publisher, Admin UI, DevHub)
2. C# repos migrated via `git subtree add` + .NET 10 upgrade (Admin API, App, StoryGenerator)
3. Python repo migrated via `git subtree add` (Chain)
4. All internal `<PackageReference>` converted to `<ProjectReference>` (12+ packages)
5. CI/CD consolidated: submodule workflows removed, path-based triggers added
6. `.gitmodules` removed, internal npm/NuGet publishing eliminated
7. CODEOWNERS and branch protection configured

## Related ADRs

- [ADR-0016](./0016-monorepo-tooling-and-multi-repository-strategy.md) — **Superseded** by this ADR
- [ADR-0020](./0020-package-consolidation-strategy.md) — Package consolidation strategy (complementary)
- [ADR-0007](./0007-nuget-feed-strategy-for-shared-libraries.md) — NuGet feed strategy (partially superseded: internal packages no longer published)
