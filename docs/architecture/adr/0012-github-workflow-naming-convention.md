# ADR-0012: GitHub Workflow Naming Convention

## Status

Accepted

## Context

The Mystira workspace contains 15 GitHub Actions workflows that handle CI/CD for components, infrastructure, deployments, and utilities. As the number of workflows grew, several issues emerged:

1. **Inconsistent naming**: Workflows used different naming patterns ("Chain CI", "CI", "Release", "Production Release")
2. **Poor discoverability**: In the GitHub Actions UI, related workflows were scattered alphabetically
3. **Lack of hierarchy**: No clear indication of workflow purpose or category
4. **Scaling concerns**: Adding new components would worsen the organization problem

**Example of the problem:**

```
All Workflows (old):
- Admin UI CI
- Chain CI
- CI
- Check Submodules
- Infrastructure Deploy
- Infrastructure Validation
- Production Release
- Publisher CI
- Release
- Staging Release
- Story Generator CI
```

In this list, component CIs are mixed with infrastructure workflows, deployment workflows, and utilities, making navigation difficult.

## Decision

We adopt a **hierarchical "Category: Name" naming convention** for all GitHub Actions workflows.

### Naming Pattern

```
[Category]: [Specific Name] - [Optional Modifier]
```

**Examples:**

- `Components: Publisher - CI`
- `Infrastructure: Deploy`
- `Deployment: Production`
- `Workspace: Release`
- `Utilities: Check Submodules`

### Categories

| Category           | Purpose                          | Examples                                                 |
| ------------------ | -------------------------------- | -------------------------------------------------------- |
| **Components**     | Component-specific CI/CD         | `Components: Chain - CI`                                 |
| **Infrastructure** | Infrastructure operations        | `Infrastructure: Deploy`                                 |
| **Deployment**     | Environment-specific deployments | `Deployment: Staging`                                    |
| **Workspace**      | Workspace-level operations       | `Workspace: CI`                                          |
| **Utilities**      | Helper workflows and checks      | `Utilities: Check Submodules`, `Utilities: Link Checker` |

### Implementation

All 15 workflows have been renamed following this convention:

#### Components (7 workflows)

- `Components: Admin API - CI` - Admin backend CI
- `Components: Admin UI - CI` - Admin frontend CI
- `Components: App - CI` - Main application CI
- `Components: Chain - CI` - Blockchain service CI
- `Components: Devhub - CI` - Developer hub CI
- `Components: Publisher - CI` - Publishing service CI
- `Components: Story Generator - CI` - Story engine CI

#### Infrastructure (2 workflows)

- `Infrastructure: Deploy` - Terraform + Kubernetes deployment
- `Infrastructure: Validate` - Infrastructure validation on PRs

#### Deployment (2 workflows)

- `Deployment: Production` - Manual production deployment
- `Deployment: Staging` - Auto-deploy to staging

#### Workspace (2 workflows)

- `Workspace: CI` - Workspace-level CI
- `Workspace: Release` - NPM package releases

#### Utilities (2 workflows)

- `Utilities: Check Submodules` - Validate submodule commits
- `Utilities: Link Checker` - Check markdown links in documentation

## Consequences

### Positive

1. **Better organization**: Related workflows group together alphabetically

   ```
   Components: Admin API - CI
   Components: Admin UI - CI
   Components: App - CI
   Components: Chain - CI
   ...
   Deployment: Production
   Deployment: Staging
   Infrastructure: Deploy
   Infrastructure: Validate
   ```

2. **Clear hierarchy**: Category prefix immediately indicates workflow purpose

3. **Scalable**: Adding new components follows the same pattern
   - New component: `Components: [Name] - CI`
   - New deployment: `Deployment: [Environment]`

4. **Improved discoverability**: Developers can quickly find:
   - All component CIs (filter by "Components:")
   - All deployments (filter by "Deployment:")
   - Infrastructure operations (filter by "Infrastructure:")

5. **Professional appearance**: Consistent naming creates a polished impression

6. **Badge-friendly**: Status badges in README.md are more descriptive

### Negative

1. **Migration effort**: All workflows had to be renamed (one-time cost)

2. **Longer names**: Some workflow names are longer than before
   - Old: "CI" (2 chars)
   - New: "Workspace: CI" (13 chars)
   - **Mitigation**: The extra clarity is worth the length

3. **Breaking change**: Any external references to workflow names need updating
   - **Mitigation**: Workflow file names remain unchanged (only `name:` field updated)

### Neutral

1. **Documentation updates required**: README.md and docs needed updating to reflect new names

2. **Learning curve**: Team needs to learn the new naming pattern
   - **Mitigation**: Pattern is intuitive and self-documenting

## Alternatives Considered

### 1. Prefix-only naming

```
CI: Chain
CI: Publisher
Deploy: Infrastructure
Deploy: Staging
```

**Rejected because:**

- Doesn't group as well (CI workflows would be far from each other)
- Less descriptive for complex workflows

### 2. Numbered prefixes

```
01-chain-ci
02-publisher-ci
10-infra-deploy
```

**Rejected because:**

- Hard to maintain numbering as workflows are added/removed
- Not semantic (numbers have no meaning)
- Poor UX in GitHub Actions UI

### 3. No changes (status quo)

**Rejected because:**

- Problems with discoverability and organization would continue
- Would worsen as more components are added

## Implementation Details

### Changes Made

1. Updated `name:` field in all 14 workflow YAML files
2. Updated README.md with new workflow names and status badges
3. Updated docs/README.md with current workflow list
4. Updated infra/README.md with new workflow references
5. Created this ADR to document the decision

### Workflow File Names

**Important**: Workflow **file names** remain unchanged (only the `name:` field is updated):

- File: `.github/workflows/chain-ci.yml`
- Name: `Components: Chain - CI`

This ensures:

- Git history is preserved
- URLs to workflows remain stable
- Automation referencing file paths still works

### Future Guidelines

When adding new workflows:

1. **Component CI**: `Components: [Component Name] - CI`
2. **Infrastructure**: `Infrastructure: [Action]`
3. **Deployment**: `Deployment: [Environment]`
4. **Workspace**: `Workspace: [Purpose]`
5. **Utility**: `Utilities: [Purpose]`

Use quotation marks in YAML:

```yaml
name: "Components: My Service - CI"
```

## References

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Workflow Organization Best Practices](https://docs.github.com/en/actions/using-workflows/about-workflows)
- Main README.md - CI/CD Pipeline section
- ADR-0004: Branching Strategy and CI/CD Process

## Date

2024-12-21

## Author

Claude (AI Assistant) with user approval
