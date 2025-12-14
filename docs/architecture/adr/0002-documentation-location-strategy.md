# ADR-0002: Documentation Location Strategy

## Status

**Accepted** - 2025-01-XX

## Context

The Mystira workspace contains a large number of markdown documentation files scattered across multiple locations:

1. **Workspace-level documentation** (`docs/`):
   - Architecture, setup, environment guides
   - Infrastructure documentation
   - Workspace coordination docs

2. **Project-specific documentation** (within each submodule):
   - `packages/app/docs/` - Extensive App documentation (100+ files)
   - `packages/story-generator/docs/` - Story-Generator docs
   - `packages/publisher/docs/` - Publisher docs
   - `packages/chain/README.md` - Chain documentation
   - `infra/README.md` and `infra/*.md` - Infrastructure docs

3. **Root-level files**:
   - `README.md`, `CONTRIBUTING.md`, `SECURITY.md`, `CHANGELOG.md`

4. **Component-level READMEs**:
   - Individual project READMEs
   - Component/project READMEs within projects
   - Module/library READMEs

### Problem Statement

This scattered organization creates several issues:

- **Discoverability**: Hard to find relevant documentation
- **Inconsistency**: No clear pattern for where docs should live
- **Duplication risk**: Similar docs in multiple places
- **Maintenance burden**: Updates need to happen in multiple locations
- **New contributor confusion**: Unclear where to add new documentation

## Decision

We will adopt a **hybrid documentation strategy** with clear boundaries:

### 1. Workspace-Level Documentation (`docs/`)

**Location**: `docs/` at workspace root

**Scope**: Documentation that applies to the entire workspace or multiple projects

**Includes**:

- Architecture decisions (ADRs)
- Workspace setup and coordination
- Infrastructure coordination
- Cross-project workflows
- Getting started guides
- Environment configuration
- Contribution guidelines (workspace-level)

**Structure**:

```
docs/
├── README.md                    # Documentation index
├── QUICK_START.md              # Quick start guide
├── SETUP.md                    # Setup instructions
├── ARCHITECTURE.md             # System architecture
├── infrastructure/
│   └── infrastructure.md       # Infrastructure guide
├── ENVIRONMENT.md              # Environment variables
├── SUBMODULES.md               # Submodule management
├── COMMITS.md                  # Commit conventions
└── architecture/
    └── adr/                    # Architecture Decision Records
        ├── 0001-*.md
        └── 0002-*.md
```

### 2. Project-Specific Documentation

**Location**: `{project}/docs/` or `{project}/README.md`

**Scope**: Documentation specific to a single project/submodule

**Structure Pattern**:

```
{project}/
├── README.md                   # Project overview, quick start
├── docs/                       # Detailed project documentation
│   ├── README.md              # Documentation index for project
│   ├── architecture/          # Project-specific architecture
│   ├── setup/                 # Project setup guides
│   ├── api/                   # API documentation
│   └── [other categories]/
└── [component]/README.md       # Component-level docs (if needed)
```

**Examples**:

- `packages/app/docs/` - App-specific documentation
- `packages/story-generator/docs/` - Story-Generator docs
- `packages/publisher/docs/` - Publisher docs
- `infra/README.md` - Infrastructure overview (detailed docs in infra repo)

### 3. Root-Level Documentation

**Location**: Workspace root

**Scope**: Essential workspace entry points and policies

**Files**:

- `README.md` - Workspace overview, getting started
- `CONTRIBUTING.md` - Contribution guidelines
- `SECURITY.md` - Security policy
- `CHANGELOG.md` - Workspace changelog

### 4. Component-Level Documentation

**Location**: Near the code it documents

**Scope**: Documentation for specific modules, libraries, or components

**Pattern**: `{path-to-component}/README.md`

**Examples**:

- `packages/app/src/Mystira.App.Domain/README.md` - Domain model documentation
- `packages/app/infrastructure/README.md` - Infrastructure setup
- `packages/app/docs/domain/models/README.md` - Domain models index

## Rationale

### 1. Separation of Concerns

- **Workspace-level**: Cross-cutting concerns, coordination, patterns
- **Project-level**: Project-specific implementation details
- **Component-level**: Technical details for specific code modules

### 2. Discoverability

- Clear boundaries make it easier to find relevant docs
- Standardized structure across projects improves navigation
- Workspace `docs/` serves as the hub for cross-project knowledge

### 3. Autonomy with Coordination

- Projects can organize their docs as needed
- Workspace provides coordination patterns
- Each project owns its documentation structure

### 4. Scalability

- Pattern works as projects grow
- New projects follow established pattern
- Easy to add new documentation categories

## Rules and Guidelines

### Documentation Type Decision Tree

1. **Does it apply to multiple projects?** → `docs/` (workspace-level)
2. **Is it a workspace policy or standard?** → Root level (`CONTRIBUTING.md`, `SECURITY.md`)
3. **Is it project-specific?** → `{project}/docs/` or `{project}/README.md`
4. **Is it component-specific?** → Near the component code

### Workspace-Level Documentation Criteria

Documentation belongs in `docs/` if it:

- Applies to multiple projects/submodules
- Describes workspace-wide patterns or standards
- Coordinates between projects
- Documents workspace infrastructure
- Explains how to work with the workspace as a whole

### Project-Level Documentation Criteria

Documentation belongs in `{project}/docs/` if it:

- Is specific to that project only
- Describes project architecture
- Documents project APIs
- Explains project setup/configuration
- Provides project-specific guides

### Naming Conventions

- **README.md**: Entry point for a directory/project
- **ADRs**: `docs/architecture/adr/000N-*.md` (workspace) or `{project}/docs/architecture/adr/` (project-specific)
- **Guides**: Descriptive kebab-case names (e.g., `quick-start.md`, `deployment-guide.md`)
- **API Docs**: `api.md` or in `api/` subdirectory

## Consequences

### Positive

1. **Clear organization**: Easy to know where documentation belongs
2. **Better discoverability**: Standard structure across projects
3. **Reduced duplication**: Clear boundaries prevent overlap
4. **Maintainability**: Updates happen in the right place
5. **Scalability**: Pattern works as workspace grows
6. **Team autonomy**: Projects can structure their docs internally

### Negative

1. **Migration effort**: Existing docs may need to be reorganized
2. **Learning curve**: New contributors need to understand the pattern
3. **Boundary decisions**: Some docs may be ambiguous (workspace vs project)

### Mitigations

1. **Documentation index**: Maintain `docs/README.md` as navigation hub
2. **Cross-references**: Link between workspace and project docs where relevant
3. **Migration guide**: Document how to reorganize existing docs
4. **Examples**: Provide examples in ADR for common scenarios

## Implementation

### Phase 1: Establish Pattern (Current)

1. ✅ Create ADR documenting the strategy
2. ✅ Maintain workspace `docs/` structure
3. ✅ Keep project-specific docs in their projects

### Phase 2: Reorganize Existing Docs (Future)

1. Audit existing documentation locations
2. Identify misplaced documentation
3. Move documentation to appropriate locations
4. Update cross-references and links
5. Update documentation indices

### Phase 3: Documentation Standards (Future)

1. Create documentation templates
2. Add linting/rules for documentation location
3. Update contribution guidelines with doc location guidance
4. Add PR templates that prompt for documentation updates

## Examples

### Workspace-Level (Move to `docs/`)

- Architecture decisions affecting multiple projects
- Infrastructure coordination guides
- Workspace setup procedures
- Cross-project workflows

### Project-Level (Keep in `{project}/docs/`)

- `packages/app/docs/architecture/` - App-specific architecture
- `packages/app/docs/domain/` - App domain models
- `packages/story-generator/docs/` - Story-Generator guides
- `packages/publisher/docs/` - Publisher design docs

### Component-Level (Keep Near Code)

- `packages/app/src/Mystira.App.Domain/README.md` - Domain library docs
- `packages/app/infrastructure/README.md` - Infrastructure setup
- Module READMEs within projects

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md)

## References

- [Documentation Best Practices](https://www.writethedocs.org/guide/)
- [ADR Format](http://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions)
- [Workspace Documentation](./../README.md)
