# Test Project Decisions by Language

## Executive Summary

Based on ADR-0022 and our comprehensive analysis, here are the specific decisions for each project category in our monorepo.

## C# Projects - FINAL DECISIONS

### ✅ Keep Current Structure (28 Test Projects)

Our C# test organization already follows best practices perfectly. No changes needed to the project structure.

#### Single Test Projects (7 projects) - KEEP AS IS
- `Mystira.Core.Tests` ✅
- `Mystira.Shared.Tests` ✅  
- `Mystira.Contracts.Tests` ✅
- `Mystira.Authoring.Tests` ✅
- `Mystira.Ai.Tests` ✅
- `Mystira.Admin.Api.Tests` ✅
- `Mystira.Integration.Tests` ✅

#### Multiple Test Projects (21 projects) - KEEP AS IS

**Story Generator Suite (6 projects):**
- `Mystira.StoryGenerator.Domain.Tests` ✅
- `Mystira.StoryGenerator.Application.Tests` ✅
- `Mystira.StoryGenerator.Infrastructure.Tests` ✅
- `Mystira.StoryGenerator.Api.Tests` ✅
- `Mystira.StoryGenerator.Llm.Tests` ✅
- `Mystira.StoryGenerator.GraphTheory.Tests` ✅

**App Infrastructure Suite (9 projects):**
- `Mystira.App.Api.Tests` ✅
- `Mystira.App.PWA.Tests` ✅
- `Mystira.App.Domain.Tests` ✅
- `Mystira.App.Application.Tests` ✅
- `Mystira.App.Infrastructure.WhatsApp.Tests` ✅
- `Mystira.App.Infrastructure.Payments.Tests` ✅
- `Mystira.App.Infrastructure.Teams.Tests` ✅
- `Mystira.App.Infrastructure.Data.Tests` ✅
- `Mystira.App.Infrastructure.Discord.Tests` ✅

**Infrastructure Suite (6 projects):**
- `Mystira.Infrastructure.WhatsApp.Tests` ✅
- `Mystira.Infrastructure.Data.Tests` ✅
- `Mystira.Infrastructure.Payments.Tests` ✅
- `Mystira.Infrastructure.Teams.Tests` ✅
- `Mystira.Infrastructure.Discord.Tests` ✅
- `Mystira.Infrastructure.StoryProtocol.Tests` ✅

### 🔄 Minor Enhancements Only

**Add Performance Test Projects (NEW):**
- `Mystira.StoryGenerator.Performance.Tests` (NEW)
- `Mystira.App.Api.Performance.Tests` (NEW)

**Standardize Internal Folder Structure:**
Add Unit/Integration/Functional subfolders to existing test projects.

## TypeScript Projects - FINAL DECISIONS

### 🔄 Standardize Test Structure (15 Projects)

#### High Priority - Add Complete Test Setup (3 projects)

**1. `@mystira/contracts`**
```
contracts/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── contract-validation.test.ts
│   │   └── schema-validation.test.ts
│   └── integration/
│       └── contract-integration.test.ts
├── vitest.config.ts
└── package.json (add test scripts)
```

**2. `@mystira/shared-utils`**
```
shared-utils/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── validation.test.ts (expand)
│   │   ├── retry.test.ts (expand)
│   │   └── logger.test.ts (expand)
│   └── integration/
│       └── utils-integration.test.ts
├── vitest.config.ts
└── package.json (update test scripts)
```

**3. `@mystira/core-types`**
```
core-types/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── result.test.ts (expand)
│   │   ├── errors.test.ts (expand)
│   │   └── types.test.ts (new)
│   └── integration/
│       └── type-integration.test.ts
├── vitest.config.ts
└── package.json (add test scripts)
```

#### Medium Priority - Add Integration/E2E Tests (3 projects)

**4. `@mystira/admin-ui`**
```
admin-ui/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── components/
│   │   │   ├── LoadingSpinner.test.tsx (existing)
│   │   │   └── TextInput.test.tsx (existing)
│   │   └── hooks/
│   ├── integration/
│   │   ├── auth-flow.test.tsx
│   │   └── admin-workflows.test.tsx
│   └── e2e/
│       ├── login.spec.ts
│       └── admin-operations.spec.ts
├── vitest.config.ts
├── playwright.config.ts (NEW)
└── package.json (add e2e scripts)
```

**5. `@mystira/devhub`**
```
devhub/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── stores/
│   │   │   ├── resourcesStore.test.ts (existing)
│   │   │   └── connectionStore.test.ts (existing)
│   │   └── components/
│   │       ├── Dashboard.test.tsx (existing)
│   │       └── infrastructure/
│   ├── integration/
│   │   ├── store-integration.test.ts
│   │   └── component-integration.test.ts
│   └── e2e/
│       ├── devhub-workflow.spec.ts
│       └── resource-management.spec.ts
├── vitest.config.ts
├── playwright.config.ts (NEW)
└── package.json (add e2e scripts)
```

**6. `@mystira/publisher`**
```
publisher/
├── src/
├── __tests__/
│   ├── unit/
│   │   ├── components/
│   │   │   └── Button.test.tsx (existing)
│   │   └── utils/
│   │       ├── validation.test.ts (existing)
│   │       └── format.test.ts (existing)
│   ├── integration/
│   │   └── publishing-workflow.test.ts
│   └── e2e/
│       ├── publish-flow.spec.ts
│       └── content-management.spec.ts
├── vitest.config.ts
├── playwright.config.ts (NEW)
└── package.json (add e2e scripts)
```

#### Low Priority - Add Basic Test Setup (6 projects)

**7. `@mystira/design-tokens`**
```
design-tokens/
├── src/
├── __tests__/
│   └── unit/
│       ├── token-validation.test.ts
│       └── theme-consistency.test.ts
├── vitest.config.ts
└── package.json (add test scripts)
```

**8. `@mystira/domain`**
```
domain/
├── src/
├── __tests__/
│   └── unit/
│       ├── domain-models.test.ts
│       └── business-rules.test.ts
├── vitest.config.ts
└── package.json (add test scripts)
```

**9. `@mystira/chain`**
```
chain/
├── src/
├── __tests__/
│   └── unit/
│       ├── blockchain-utils.test.ts
│       └── transaction-validation.test.ts
├── vitest.config.ts
└── package.json (add test scripts)
```

**10. `@mystira/core-types`** (already has basic tests, expand coverage)

#### No Tests Needed (2 projects)

**11. `@mystira/api-spec`** - OpenAPI specifications only
**12. `story-generator-web`** - CSS build package only

### 📋 Shared Configuration

**Create `configs/vitest/`:**
```
configs/vitest/
├── base.config.ts
├── component.config.ts
├── integration.config.ts
└── e2e.config.ts
```

## Rust Projects - FINAL DECISIONS

### 🔄 Add Complete Test Infrastructure (1 project)

**`mystira-devhub` (Tauri Application)**
```
src-tauri/
├── src/
│   ├── main.rs
│   ├── commands/
│   │   ├── mod.rs
│   │   ├── app.rs
│   │   └── #[cfg(test)] mod.rs
├── tests/
│   ├── integration_tests.rs
│   ├── app_commands_tests.rs
│   └── common/
│       └── mod.rs
├── benches/
│   ├── performance_bench.rs
│   └── command_bench.rs
└── Cargo.toml
```

**Test Categories:**
1. **Unit Tests**: `#[cfg(test)]` modules in `src/`
2. **Integration Tests**: `tests/` directory
3. **Benchmarks**: `benches/` directory

**Cargo.toml additions:**
```toml
[dev-dependencies]
tokio-test = "0.4"
criterion = "0.5"

[[bench]]
name = "performance_bench"
harness = false
```

## Implementation Priority Matrix

| Priority | Projects | Effort | Impact | Timeline |
|----------|----------|--------|--------|----------|
| P0 | C# Enhancements | Low | High | Week 1 |
| P1 | TypeScript Core (contracts, shared-utils, core-types) | Medium | High | Week 2 |
| P2 | TypeScript Apps (admin-ui, devhub, publisher) | High | High | Week 3 |
| P3 | Rust Test Setup | Medium | Medium | Week 4 |
| P4 | TypeScript Utilities (design-tokens, domain, chain) | Low | Low | Week 5 |

## Success Criteria

### C# Projects
- [x] All 28 existing test projects maintain current structure
- [ ] Add 2 new performance test projects
- [ ] Standardize internal folder structure

### TypeScript Projects
- [ ] Increase test coverage from 33% to 80% (12/15 packages)
- [ ] Standardize all test configurations
- [ ] Add E2E testing for 3 main applications

### Rust Projects
- [ ] Achieve 80% test coverage for Tauri application
- [ ] Set up benchmark infrastructure
- [ ] Establish integration testing

## Next Steps

1. **Week 1**: Create C# performance test projects and standardize folder structures
2. **Week 2**: Implement shared Vitest configuration and setup core TypeScript tests
3. **Week 3**: Add integration and E2E tests for main TypeScript applications
4. **Week 4**: Implement complete Rust test infrastructure
5. **Week 5**: Complete remaining TypeScript utility tests and documentation

This plan maintains our excellent C# test organization while significantly improving TypeScript and Rust testing consistency and coverage across the monorepo.
