# ADR-0022: Test Project Organization Strategy

## Status
ACCEPTED

## Date
2026-02-28

## Context
Our monorepo currently has 34 projects with varying test project organization patterns:
- Some projects have dedicated test projects (e.g., `Mystira.Core.Tests`)
- Some have multiple test projects (e.g., `Mystira.App.Infrastructure.*.Tests`)
- Some have no test projects
- Test projects follow different naming conventions and structures

We need to establish a consistent strategy for test project organization that aligns with best practices for each language (C#, TypeScript, Rust) and provides clear guidance on when to use single vs multiple test projects.

## Problem
1. **Inconsistent Organization**: No clear pattern for when to create separate test projects vs inline tests
2. **Naming Confusion**: Mix of naming conventions (`*.Tests`, `Test.*`, infrastructure-specific patterns)
3. **Language Differences**: C#, TypeScript, and Rust have different testing paradigms and conventions
4. **Scale Considerations**: Need guidance on when multiple test projects become necessary
5. **Maintenance Overhead**: Too many test projects can increase build complexity

## Decision

### C# Projects

#### Single Test Project Pattern
**Use when:**
- Project has ≤ 3 major components/layers
- Test suite is ≤ 10k lines of code
- All tests can run together efficiently
- Project is self-contained with minimal external dependencies

**Structure:**
```
Project.Source/
├── Project.Source.csproj
└── Tests/
    ├── Project.Source.Tests.csproj
    ├── Unit/
    ├── Integration/
    └── Functional/
```

**Examples:**
- `Mystira.Core.Tests` (single test project for core domain)
- `Mystira.Shared.Tests` (shared utilities)
- `Mystira.Contracts.Tests` (contracts/validation)

#### Multiple Test Projects Pattern
**Use when:**
- Project has > 3 major components with distinct testing needs
- Test suite is > 10k lines of code
- Different test layers have different dependencies or performance requirements
- Need to run test subsets independently (e.g., unit vs integration)

**Structure:**
```
Project.Source/
├── Project.Source.csproj
├── Project.Source.Domain.Tests/
├── Project.Source.Application.Tests/
├── Project.Source.Infrastructure.Tests/
└── Project.Source.Integration.Tests/
```

**Examples:**
- `Mystira.App.*.Tests` (multiple infrastructure concerns)
- `Mystira.StoryGenerator.*.Tests` (multiple layers)

### TypeScript Projects

#### Single Test Project Pattern
**Use when:**
- Frontend application or library
- Tests can run with Jest/Vitest efficiently
- Shared test configuration works for all test types

**Structure:**
```
package/
├── src/
├── package.json
├── jest.config.js
└── __tests__/
    ├── unit/
    ├── integration/
    └── e2e/
```

#### Multiple Test Projects Pattern
**Use when:**
- Microfrontend architecture
- Different build/test requirements per component
- Need separate CI pipelines

**Structure:**
```
packages/
├── component-a/
│   ├── src/
│   └── __tests__/
├── component-b/
│   ├── src/
│   └── __tests__/
└── e2e-tests/
```

### Rust Projects

#### Single Test Project Pattern (Integrated)
**Use when:**
- Standard Rust library/application
- Tests can run with `cargo test`
- No special test dependencies

**Structure:**
```
crate/
├── src/
├── Cargo.toml
├── tests/
│   ├── integration_tests.rs
│   └── common/
└── benches/
```

#### Multiple Test Projects Pattern
**Use when:**
- Workspace with multiple crates
- Different test configurations needed
- Performance-critical tests that need isolation

**Structure:**
```
workspace/
├── crate1/
│   ├── src/
│   └── tests/
├── crate2/
│   ├── src/
│   └── tests/
├── integration-tests/
│   └── Cargo.toml
└── benchmarks/
    └── Cargo.toml
```

## Decision Matrix

| Project Type | Lines of Code | Components | Recommended Pattern |
|--------------|---------------|------------|---------------------|
| C# Domain Library | < 5k | 1-2 | Single Test Project |
| C# Application | 5k-20k | 3-5 | Single Test Project with folders |
| C# Microservice | > 20k | > 5 | Multiple Test Projects |
| TypeScript Library | < 10k | 1-2 | Single Test Project |
| TypeScript App | > 10k | > 3 | Multiple Test Projects (if microfrontends) |
| Rust Library | < 10k | 1-3 | Integrated Tests |
| Rust Workspace | > 10k | > 3 | Multiple Test Projects |

## Implementation Plan

### Phase 1: Analysis (Week 1)
1. Audit all current test projects
2. Categorize by size, complexity, and language
3. Identify projects that need restructuring

### Phase 2: C# Projects (Week 2-3)
1. Consolidate small test projects where appropriate
2. Split large test projects where needed
3. Standardize naming conventions:
   - Single: `{Project}.Tests`
   - Multiple: `{Project}.{Layer}.Tests`

### Phase 3: TypeScript Projects (Week 4)
1. Implement consistent test directory structure
2. Standardize Jest/Vitest configurations
3. Establish e2e test separation

### Phase 4: Rust Projects (Week 5)
1. Implement integrated test patterns
2. Set up workspace-level test organization
3. Establish benchmark separation

### Phase 5: Documentation & Tooling (Week 6)
1. Update project templates
2. Create test project generators
3. Update CI/CD pipelines

## Consequences

### Positive
- **Consistency**: Clear patterns for test organization
- **Maintainability**: Easier to understand test structure
- **Performance**: Optimized test execution based on project needs
- **Scalability**: Clear guidance for growing projects

### Negative
- **Migration Effort**: Requires restructuring existing test projects
- **Learning Curve**: Team needs to understand new patterns
- **Tooling Updates**: CI/CD pipelines may need adjustments

### Risks
- **Over-engineering**: Creating too many test projects for small projects
- **Under-testing**: Consolidating might hide test coverage gaps
- **Migration Bugs**: Moving tests might introduce temporary issues

## References
- [Microsoft Testing Guidelines](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [Jest Best Practices](https://jestjs.io/docs/getting-started)
- [Rust Testing Book](https://doc.rust-lang.org/book/ch11-00-testing.html)
- [xUnit Testing Patterns](https://xunit.net/docs/getting-started)
- [Vitest Project Structure](https://vitest.dev/guide/)

## Related ADRs
- ADR-0001: Monorepo Structure
- ADR-0015: CI/CD Pipeline Strategy
- ADR-0019: Dockerfile Location Standardization
