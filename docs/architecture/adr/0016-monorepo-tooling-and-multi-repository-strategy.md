# ADR-0016: Monorepo Tooling and Multi-Repository Strategy

## Status

**Accepted** - 2025-12-22

## Context

The Mystira workspace uses a hybrid approach combining multiple independent repositories (via git submodules) with monorepo tooling (pnpm workspaces + Turborepo). This raises the question: **Should we consider alternative monorepo tools, and can they effectively work with multiple separate repository locations?**

### Current Architecture

**Multi-Repository Structure**:
- 7 independent repositories, each with their own history, CI/CD, and release cycles
- Integrated into workspace via git submodules
- Each repository remains independently accessible at its own GitHub URL

**Monorepo Tooling**:
- **pnpm workspaces**: Dependency management across packages
- **Turborepo**: Build orchestration and caching
- **Changesets**: Version management and changelogs

### The Question

Can monorepo tools (like Nx, Lerna, Rush, etc.) effectively "fake" or manage multiple repository locations as if they were a monorepo? Or is the git submodules + Turborepo approach the most viable solution for this hybrid architecture?

## Research: Monorepo Tools and Multi-Repository Support

### Tool Categories

1. **Build Orchestration Tools**: Turborepo, Nx, Rush
2. **Package Management**: pnpm workspaces, Yarn workspaces, npm workspaces, Lerna
3. **Meta-Repository Tools**: Git submodules, Git subtree, meta, git-repo
4. **Hybrid Solutions**: Combinations of the above

### Analysis of Major Tools

#### 1. **Turborepo** (Current)

**Multi-Repo Support**: ⚠️ Limited

**How It Works**:
- Designed for monorepos where all code is in one repository
- Works with git submodules but treats them as local packages
- Requires all code to be checked out locally
- No native understanding of separate repositories

**With Git Submodules**:
- ✅ Works when all submodules are initialized
- ✅ Fast build orchestration and caching
- ✅ Simple configuration
- ❌ No awareness of submodule boundaries
- ❌ Requires full checkout of all submodules
- ❌ Cache invalidation doesn't respect submodule commits

**Verdict**: Works well for build orchestration but doesn't provide multi-repo intelligence.

---

#### 2. **Nx**

**Multi-Repo Support**: ⚠️ Limited (similar to Turborepo)

**How It Works**:
- Powerful build system with dependency graph analysis
- Designed for monorepos with all code in one repo
- Can work with git submodules when checked out locally
- No native multi-repository support

**With Git Submodules**:
- ✅ Advanced build orchestration
- ✅ Better visualization of dependencies
- ✅ More sophisticated caching strategies
- ✅ Task scheduling and parallelization
- ❌ No submodule-aware operations
- ❌ Requires full local checkout
- ❌ Heavier than Turborepo (more complex)

**Verdict**: More powerful than Turborepo but still treats submodules as local code. Better for complex build graphs but adds complexity.

---

#### 3. **Lerna**

**Multi-Repo Support**: ❌ No (Deprecated for new projects)

**How It Works**:
- Package management and versioning tool
- Designed for JavaScript monorepos
- Largely superseded by workspace features in pnpm/yarn/npm

**Status**: Maintenance mode, not recommended for new projects.

**Verdict**: Not suitable. Use pnpm workspaces instead (which we already do).

---

#### 4. **Rush**

**Multi-Repo Support**: ⚠️ Limited

**How It Works**:
- Enterprise-scale monorepo management from Microsoft
- Excellent for large JavaScript/TypeScript monorepos
- Strict dependency management and versioning
- No native multi-repository support

**With Git Submodules**:
- ✅ Enterprise-grade build orchestration
- ✅ Strict dependency policies
- ✅ Better for large teams
- ❌ More complex than Turborepo
- ❌ Overkill for current scale
- ❌ No submodule intelligence
- ❌ Requires full local checkout

**Verdict**: Too complex for our current needs. No advantage over Turborepo + submodules.

---

#### 5. **Meta** (Facebook's tool)

**Multi-Repo Support**: ✅ Yes (Designed for it)

**How It Works**:
- Manages multiple git repositories
- Executes commands across multiple repos
- Lightweight wrapper around git
- No build orchestration features

**Features**:
- ✅ Native multi-repository support
- ✅ Execute git commands across repos
- ✅ Track relationships between repos
- ❌ No build orchestration
- ❌ No caching
- ❌ Requires separate build tool

**Verdict**: Could replace git submodules but doesn't provide build features. Would still need Turborepo.

---

#### 6. **Google's repo** (Android tool)

**Multi-Repo Support**: ✅ Yes (Designed for it)

**How It Works**:
- Manages multiple git repositories (used by Android)
- XML manifest defines repository relationships
- Git wrapper for multi-repo operations

**Features**:
- ✅ Native multi-repository support
- ✅ Powerful sync operations
- ✅ Branch management across repos
- ❌ More complex than submodules
- ❌ No build orchestration
- ❌ Android-centric design

**Verdict**: Overkill. More complex than submodules without clear benefits.

---

#### 7. **Pants** (Build system)

**Multi-Repo Support**: ⚠️ Limited

**How It Works**:
- Polyglot build system (Python, Java, Go, Shell, etc.)
- Designed for monorepos
- Advanced build graph and caching

**Features**:
- ✅ Polyglot support (matches our stack: Python, C#, TypeScript)
- ✅ Very sophisticated caching
- ✅ Remote execution support
- ❌ Steep learning curve
- ❌ No native multi-repository support
- ❌ Requires extensive configuration

**Verdict**: Too complex for our current needs. Turborepo is sufficient.

---

#### 8. **Bazel**

**Multi-Repo Support**: ⚠️ Partial (via WORKSPACE rules)

**How It Works**:
- Google's build system (used for Google's monorepo)
- Can reference external repositories
- Hermetic builds with explicit dependencies

**Features**:
- ✅ Can reference external git repositories
- ✅ Hermetic, reproducible builds
- ✅ Polyglot support
- ❌ Extremely complex
- ❌ Steep learning curve
- ❌ Requires rewriting all build logic
- ❌ Poor developer experience for small teams

**Verdict**: Massive overkill. Enterprise-scale complexity not justified.

---

### Summary Matrix

| Tool        | Multi-Repo Native | Build Orchestration | Caching | Complexity | Polyglot | Verdict          |
| ----------- | ----------------- | ------------------- | ------- | ---------- | -------- | ---------------- |
| Turborepo   | ❌ No             | ✅ Excellent        | ✅ Good | Low        | ✅ Yes   | ✅ **Best fit**  |
| Nx          | ❌ No             | ✅ Excellent        | ✅ Great| Medium     | ✅ Yes   | ⚠️ Overkill     |
| Lerna       | ❌ No             | ❌ No               | ❌ No   | Low        | ❌ No    | ❌ Deprecated    |
| Rush        | ❌ No             | ✅ Good             | ✅ Good | High       | ⚠️ JS only| ❌ Too complex  |
| Meta        | ✅ Yes            | ❌ No               | ❌ No   | Low        | ✅ Yes   | ⚠️ No build     |
| repo        | ✅ Yes            | ❌ No               | ❌ No   | Medium     | ✅ Yes   | ❌ Overkill      |
| Pants       | ❌ No             | ✅ Excellent        | ✅ Great| Very High  | ✅ Yes   | ❌ Too complex   |
| Bazel       | ⚠️ Partial        | ✅ Excellent        | ✅ Great| Extreme    | ✅ Yes   | ❌ Overkill      |
| Submodules  | ✅ Yes            | ❌ No               | ❌ No   | Low        | ✅ Yes   | ✅ **Current**   |

## Decision

**We will continue using git submodules + Turborepo + pnpm workspaces** as our monorepo tooling strategy.

### Rationale

#### 1. **No Tool Can "Fake" Multiple Repositories as a True Monorepo**

All monorepo build tools (Turborepo, Nx, Rush, Pants, Bazel) are designed for **single-repository monorepos** where all code lives in one git history. They can work with git submodules by treating checked-out submodules as local packages, but they provide **no special intelligence for multi-repository structures**.

Key limitations:
- Build tools don't understand submodule boundaries
- Caching doesn't respect individual submodule commits
- Dependency graph analysis treats all code as one repo
- CI/CD still needs to handle submodule updates separately

#### 2. **Git Submodules are the Right Tool for Multi-Repository Management**

Git submodules are explicitly designed to:
- Reference external repositories at specific commits
- Maintain independent repository histories
- Allow independent CI/CD pipelines
- Enable team autonomy per repository
- Support selective checkout (only needed repos)

Alternative multi-repo tools (Meta, repo) provide similar functionality but with more complexity and no clear advantages.

#### 3. **Turborepo Provides Optimal Build Orchestration**

For build orchestration within the workspace, Turborepo is ideal:
- ✅ Simple configuration (`turbo.json`)
- ✅ Fast builds with excellent caching
- ✅ Polyglot support (works with C#, Python, TypeScript)
- ✅ Works seamlessly with pnpm workspaces
- ✅ Minimal learning curve
- ✅ Sufficient for our current scale (7 repos)

More complex tools (Nx, Pants, Bazel) offer:
- More sophisticated build graphs (not needed)
- Remote execution (not needed at current scale)
- Advanced features (unused complexity)

#### 4. **pnpm Workspaces Handle Dependency Management**

For JavaScript/TypeScript packages:
- ✅ Fast and efficient
- ✅ Native workspace support
- ✅ Works across submodule boundaries
- ✅ Standard tool in ecosystem

No need for Lerna or other package management overlays.

#### 5. **Our Hybrid Approach is Intentional**

We deliberately chose to keep repositories separate because:
1. **Different technology stacks**: Python, C#, TypeScript, Terraform
2. **Independent deployment cycles**: Each service deploys separately
3. **Team autonomy**: Different teams own different repos
4. **Clear boundaries**: Services communicate via network, not code
5. **Independent CI/CD**: Each repo has its own workflows

A true monorepo would sacrifice these benefits without providing equivalent value.

## Implementation

### Current Stack (No Changes)

1. **Multi-Repository**: Git submodules
   - Each component is an independent repository
   - Integrated at specific commits
   - Managed via `.gitmodules`

2. **Build Orchestration**: Turborepo
   - Defined in `turbo.json`
   - Handles build dependencies and caching
   - Executes tasks across all packages

3. **Package Management**: pnpm workspaces
   - Defined in `pnpm-workspace.yaml`
   - Manages JavaScript/TypeScript dependencies
   - Links packages across submodules

4. **Versioning**: Changesets
   - Defined in `.changeset/`
   - Manages version bumps and changelogs
   - Works with pnpm workspaces

### Best Practices

#### 1. Submodule Management

```bash
# Initialize all submodules
git submodule update --init --recursive

# Update specific submodule to latest
git submodule update --remote packages/<name>

# Update all submodules
git submodule update --remote --merge

# Commit submodule pointer updates
git add packages/<name>
git commit -m "chore: update <name> submodule to latest"
```

#### 2. Turborepo Usage

```bash
# Build all packages (respects dependencies)
pnpm build

# Build specific package and dependencies
pnpm --filter <package> build

# Run all tests
pnpm test

# Run tests for specific package
pnpm --filter <package> test
```

#### 3. Selective Checkout

For developers who don't need all repositories:

```bash
# Clone without submodules
git clone https://github.com/phoenixvc/Mystira.workspace.git

# Initialize only needed submodules
git submodule update --init packages/publisher
git submodule update --init packages/admin-ui

# Build only initialized packages
pnpm install
pnpm build
```

## Consequences

### Positive

1. **Right Tool for the Job**: Each tool is used for its intended purpose
   - Git submodules for multi-repository management
   - Turborepo for build orchestration
   - pnpm for package management

2. **Simple and Maintainable**: Minimal complexity, standard tools
3. **Scalable**: Can add more repositories without tool limitations
4. **Team Autonomy**: Each repo maintains independence
5. **Flexible**: Can work with partial checkouts
6. **Standard**: Uses well-understood, widely-adopted tools

### Negative

1. **No "Magic" Multi-Repo Intelligence**: Tools don't understand repository boundaries natively
2. **Manual Submodule Updates**: Need to explicitly update submodule pointers
3. **CI/CD Complexity**: Workflows must handle submodule updates explicitly
4. **Partial Feature Overlap**: Some features require coordination between tools

### Mitigations

1. **Submodule Automation**: Use GitHub Actions workflows to automate submodule updates
2. **Documentation**: Clear guides on submodule management
3. **Scripts**: Utility scripts for common submodule operations
4. **Workspace Scripts**: Centralized scripts for cross-repo operations

## Alternatives Considered

### Alternative 1: True Monorepo (Rejected)

**Approach**: Merge all repositories into one via git subtree or migration

**Pros**:
- Single git history
- Simpler tooling (no submodules)
- Atomic cross-repo changes

**Cons**:
- ❌ Loss of repository independence
- ❌ Loss of independent CI/CD
- ❌ Loss of team autonomy
- ❌ Complex migration path
- ❌ No selective checkout
- ❌ Massive repository size

**Verdict**: Sacrifices too many benefits of our current architecture.

---

### Alternative 2: Nx Instead of Turborepo (Rejected)

**Approach**: Replace Turborepo with Nx

**Pros**:
- More powerful build graph
- Better visualization
- More sophisticated caching

**Cons**:
- ❌ More complex configuration
- ❌ Steeper learning curve
- ❌ Doesn't solve multi-repo challenges
- ❌ Overkill for current scale

**Verdict**: Adds complexity without solving our actual challenges.

---

### Alternative 3: Meta or repo for Multi-Repo (Rejected)

**Approach**: Replace git submodules with Meta or repo

**Pros**:
- Better multi-repo commands
- Easier cross-repo operations

**Cons**:
- ❌ Additional tooling to learn
- ❌ Less mature/less common than submodules
- ❌ Still need Turborepo for builds
- ❌ Doesn't integrate with GitHub UI as well

**Verdict**: Marginal improvements don't justify switching cost.

---

### Alternative 4: Pants or Bazel (Rejected)

**Approach**: Replace entire build system with Pants or Bazel

**Pros**:
- Very sophisticated build system
- Hermetic builds
- Remote execution possible

**Cons**:
- ❌ Extreme complexity
- ❌ Steep learning curve
- ❌ Requires rewriting all build logic
- ❌ Poor developer experience
- ❌ Overkill for 7 repositories

**Verdict**: Enterprise-scale complexity not justified for our team size.

## Conclusion

**Answer to the Original Question**: No, there is no monorepo tool that can effectively "fake" or manage multiple repository locations as if they were a true monorepo. All monorepo build tools are designed for single-repository structures and provide no special intelligence for multi-repository setups.

**Our Solution**: The combination of **git submodules + Turborepo + pnpm workspaces** is the right approach because:
1. Git submodules handle multi-repository management (their intended purpose)
2. Turborepo handles build orchestration (simple and effective)
3. pnpm workspaces handle dependency management (fast and standard)

This hybrid approach gives us the benefits of both worlds:
- **Multi-repository**: Independence, autonomy, selective checkout
- **Monorepo-like experience**: Unified builds, dependency management, orchestration

Alternative tools either:
- Don't solve the multi-repository challenge (Nx, Rush, Pants, Bazel)
- Don't provide build orchestration (Meta, repo)
- Add unnecessary complexity (all advanced build systems)

## Related ADRs

- [ADR-0001: Infrastructure Organization - Hybrid Approach](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md)
- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md)
- [ADR-0009: Further App Segregation Strategy](./0009-further-app-segregation-strategy.md)

## References

- [Turborepo Documentation](https://turbo.build/repo/docs)
- [Nx Documentation](https://nx.dev/)
- [Git Submodules Documentation](https://git-scm.com/book/en/v2/Git-Tools-Submodules)
- [pnpm Workspaces](https://pnpm.io/workspaces)
- [Meta (Facebook's multi-repo tool)](https://github.com/mateodelnorte/meta)
- [repo (Google's multi-repo tool)](https://gerrit.googlesource.com/git-repo/)
- [Pants Build System](https://www.pantsbuild.org/)
- [Bazel Build System](https://bazel.build/)
- [Rush](https://rushjs.io/)
- [Monorepo Tools Comparison](https://monorepo.tools/)
