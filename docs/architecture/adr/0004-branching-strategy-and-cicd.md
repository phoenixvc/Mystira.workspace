# ADR-0004: Branching Strategy and CI/CD Process

## Status

Accepted

## Context

The Mystira workspace requires a standardized branching strategy and CI/CD process that:

- Enforces code quality through automated checks
- Provides a clear development-to-production workflow
- Prevents direct pushes to the main branch
- Supports environment promotion (dev → staging → production)
- Implements approval gates for production deployments
- Maintains separation between development, staging, and production environments

Key requirements:

- All changes must go through code review
- CI checks must pass before merging to main
- Staging should automatically deploy when code is merged to main
- Production deployments require explicit approval
- Developers should work on feature branches or a development branch

## Decision

We adopt a **two-branch strategy with pull request workflow**:

### Branch Strategy

1. **`dev` branch**: Development branch where all feature work is integrated
   - **Requires pull requests** from feature branches (recommended)
   - All CI/CD pipelines run automatically on PR and push
   - Docker images are built and pushed to container registry
   - CI checks must pass before merge
   - Approval optional (0 approvals) for faster iteration, or require 1 approval for stricter quality
   - **Alternative**: Allow direct pushes if team prefers (requires CI checks via branch protection)

2. **`main` branch**: Production-ready code
   - Protected branch with strict rules
   - Only accepts changes via pull requests from `dev`
   - Requires all CI checks to pass
   - Requires at least one approval
   - Requires conversation resolution
   - Automatic staging deployment on merge

### CI/CD Workflow

#### 1. Development Workflow (Push to `dev`)

```
Developer pushes to dev
  ↓
CI workflows trigger automatically:
  - Lint checks
  - Test execution
  - Build verification
  - Docker image build and push to ACR
  - Service-specific CI (chain, publisher)
```

**Workflows triggered**:

- `ci.yml` - Workspace-level lint, test, build
- `chain-ci.yml` - Chain service CI (if chain files changed)
- `publisher-ci.yml` - Publisher service CI (if publisher files changed)

**Docker images**: Tagged with branch name (`dev`) and commit SHA

#### 2. Pull Request Workflow (PR from `dev` to `main`)

```
Developer creates PR from dev → main
  ↓
Same CI checks run as dev push
  ↓
Requires:
  - All CI checks pass
  - At least 1 approval
  - Branch up to date with main
  - All conversations resolved
  ↓
Ready for merge
```

**Key differences from dev push**:

- Docker images are **not** pushed (only built for validation)
- Branch protection rules enforce requirements
- PR status must be "Ready for review"

#### 3. Staging Deployment (Merge to `main`)

```
PR merged to main
  ↓
Staging Release workflow triggers automatically
  ↓
Deploys to staging:
  - Terraform infrastructure deployment
  - Kubernetes service deployment
  - All services updated
  ↓
Staging environment updated
```

**Workflow**: `staging-release.yml`

- Automatic trigger on push to `main`
- No manual approval required
- Deploys infrastructure and services to staging environment
- Uses `staging` GitHub environment

#### 4. Production Deployment (Manual with Approval)

```
Manual trigger from Actions
  ↓
Confirmation required: "DEPLOY TO PRODUCTION"
  ↓
Environment approval required (if configured)
  ↓
Production Release workflow executes:
  - Terraform infrastructure deployment
  - Kubernetes service deployment
  - Verification checks
  ↓
Production environment updated
```

**Workflow**: `production-release.yml`

- Manual workflow dispatch only
- Requires explicit confirmation input
- Uses `production` GitHub environment with approval gates
- Deploys infrastructure and services to production environment

### Branch Protection Rules

The `main` branch is protected with the following rules:

1. **Require pull request before merging**
   - Required approvals: 1
   - Dismiss stale reviews when new commits pushed
   - Require review from code owners (if CODEOWNERS exists)

2. **Require status checks to pass**
   - All CI jobs must pass
   - Branch must be up to date before merging

3. **Require conversation resolution**
   - All PR conversations must be resolved

4. **Administrator restrictions**
   - Administrators cannot bypass protection (recommended)

### Environment Strategy

- **Development**: Continuous deployment from `dev` branch (future enhancement)
- **Staging**: Automatic deployment on merge to `main`
- **Production**: Manual deployment with approval gates

### Required CI Checks

All of the following must pass before merging to `main`:

- `CI / lint`
- `CI / test`
- `CI / build`
- `Chain CI / lint` (if chain changed)
- `Chain CI / test` (if chain changed)
- `Chain CI / build` (if chain changed)
- `Publisher CI / lint` (if publisher changed)
- `Publisher CI / test` (if publisher changed)
- `Publisher CI / build` (if publisher changed)

## Consequences

### Positive

1. **Code Quality**: All code is reviewed and tested before reaching production
2. **Safety**: Production deployments require explicit approval
3. **Transparency**: Clear workflow from development to production
4. **Automation**: Staging deployments are automatic, reducing manual effort
5. **Traceability**: All production changes go through PRs with clear history
6. **Fast Feedback**: CI runs on every push to `dev`, catching issues early

### Negative

1. **Complexity**: More steps required to get code to production
2. **Slower Releases**: Multiple approval steps can slow down releases
3. **Branch Management**: Developers must manage feature branches and PRs
4. **PR Overhead**: Every change requires a PR (both to `dev` and `main`), which adds overhead
5. **Slower Dev Iteration**: Requiring PRs to `dev` adds a step that may slow rapid development cycles

### Neutral

1. **Learning Curve**: Team must understand and follow the workflow
2. **Tooling Dependency**: Relies on GitHub features (branch protection, environments)

## Alternatives Considered

### Alternative 1: Git Flow (feature/develop/main/hotfix branches)

**Pros**:

- More granular branch management
- Explicit hotfix process
- Clear release branch strategy

**Cons**:

- More complex for smaller teams
- Requires more branch management
- Overkill for our current scale

**Decision**: Rejected - Too complex for current team size and requirements

### Alternative 2: GitHub Flow (feature branches → main)

**Pros**:

- Simple and straightforward
- Fast feedback loop
- Less branch management

**Cons**:

- No staging environment in workflow
- All changes go directly to main
- Less separation between development and production

**Decision**: Rejected - Doesn't support staging environment promotion strategy

### Alternative 3: Trunk-Based Development

**Pros**:

- Simplest possible workflow
- Frequent small commits
- Fast integration

**Cons**:

- Requires strong CI/CD and feature flags
- Less separation for testing
- Doesn't fit our environment promotion needs

**Decision**: Rejected - Doesn't align with our staging/production environment strategy

## Implementation Notes

### GitHub Configuration Required

1. **Branch Protection Rules** (Settings → Branches):
   - **Configure protection for `dev` branch** (recommended):
     - Require pull request before merging
     - Require status checks to pass (all CI jobs)
     - Require approvals: 0 (or 1 for stricter quality)
     - Do not allow force pushes
   - **Configure protection for `main` branch**:
     - Require pull request before merging
     - Set required status checks
     - Configure approval requirements (1 approval)
     - Require conversation resolution

2. **GitHub Environments** (Settings → Environments):
   - Create `staging` environment (optional approval)
   - Create `production` environment (require approval, add reviewers)

3. **Create `dev` branch**:
   ```bash
   git checkout -b dev
   git push -u origin dev
   ```

### Workflow Files

All workflows are located in `.github/workflows/`:

- `ci.yml` - Workspace CI
- `chain-ci.yml` - Chain service CI
- `publisher-ci.yml` - Publisher service CI
- `staging-release.yml` - Staging deployment
- `production-release.yml` - Production deployment
- `release.yml` - NPM package releases (Changesets)

### Documentation

- [Branch Protection & CI/CD Workflow Guide](../../BRANCH_PROTECTION.md)
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md)

## References

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0002: Documentation Location Strategy](./0002-documentation-location-strategy.md)
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md)
- [GitHub Branch Protection](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/defining-the-mergeability-of-pull-requests/about-protected-branches)
- [GitHub Environments](https://docs.github.com/en/actions/deployment/targeting-different-environments/using-environments-for-deployment)

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0002: Documentation Location Strategy](./0002-documentation-location-strategy.md)
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md)
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md)
