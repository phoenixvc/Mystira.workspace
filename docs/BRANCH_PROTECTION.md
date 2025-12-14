# Branch Protection and CI/CD Workflow

This document describes the branch protection rules and CI/CD workflow for the Mystira workspace.

## Branch Strategy

- **`dev`**: Development branch - all feature work is integrated here via PRs
- **`main`**: Main branch - production-ready code, protected

## Workflow Overview

```
dev (push) → CI/CD runs → PR to main → Staging Release → Production Release (manual approval)
```

## Branch Protection Rules

### Protection for `dev` Branch (Recommended)

To ensure code quality even in development, configure protection for `dev`:

1. **Require pull request before merging**
   - ✅ Enabled
   - ✅ Require approvals: 0 (or 1 for stricter quality)
   - ✅ Dismiss stale reviews: Optional
   - ❌ Require review from Code Owners: Optional (not required for dev)

2. **Require status checks to pass before merging**
   - ✅ Enabled
   - ✅ Require branches to be up to date before merging
   - ✅ Status checks required: All CI jobs (lint, test, build)

3. **Require conversation resolution before merging**
   - ❌ Not required (optional for dev)

4. **Allow force pushes**
   - ❌ Disabled (prevents accidental overwrites)

5. **Allow deletions**
   - ❌ Disabled

**Rationale**: Requiring PRs to `dev` ensures:

- All code is reviewed before integration
- CI checks run before merge
- Better collaboration and knowledge sharing
- Cleaner git history
- Still maintains fast iteration (no approval required if configured)

**Alternative**: If team prefers faster iteration, allow direct pushes but require CI checks to pass before push completes (via pre-push hooks or branch protection that allows pushes but requires CI).

### Protection for `main` Branch

The following branch protection rules **must be configured** in GitHub repository settings:

### Required Status Checks

All of the following checks must pass before merging:

- `CI / lint` - Code linting checks
- `CI / test` - Test execution
- `CI / build` - Build verification
- `Chain CI / lint` - Chain service linting (if chain changed)
- `Chain CI / test` - Chain service tests (if chain changed)
- `Chain CI / build` - Chain service build (if chain changed)
- `Publisher CI / lint` - Publisher service linting (if publisher changed)
- `Publisher CI / test` - Publisher service tests (if publisher changed)
- `Publisher CI / build` - Publisher service build (if publisher changed)

### Protection Rules

1. **Require a pull request before merging**
   - ✅ Enabled
   - ✅ Require approvals: 1
   - ✅ Dismiss stale pull request approvals when new commits are pushed
   - ✅ Require review from Code Owners (if CODEOWNERS file exists)

2. **Require status checks to pass before merging**
   - ✅ Enabled
   - ✅ Require branches to be up to date before merging
   - ✅ Status checks required: All listed above

3. **Require conversation resolution before merging**
   - ✅ Enabled

4. **Do not allow bypassing the above settings**
   - ✅ Enabled for administrators (optional, recommended)

5. **Restrict who can push to matching branches**
   - ⚠️ No direct pushes allowed (all changes via PR)

## Setting Up Branch Protection (GitHub UI)

1. Go to repository **Settings** → **Branches**
2. Click **Add rule** or edit existing rule for `main`
3. Configure the following:

### Branch name pattern

```
main
```

### Protect matching branches

#### ✅ Require a pull request before merging

- [x] Require a pull request before merging
- [x] Require approvals: `1`
- [x] Dismiss stale pull request approvals when new commits are pushed
- [x] Require review from Code Owners

#### ✅ Require status checks to pass before merging

- [x] Require branches to be up to date before merging
- [x] Require status checks to pass before merging
- Select all required checks from the list above

#### ✅ Require conversation resolution before merging

- [x] Require conversation resolution before merging

#### ✅ Include administrators

- [x] Do not allow bypassing the above settings (optional, recommended)

#### Restrict pushes that create files that match

- Leave empty (or configure if needed)

## Setting Up Branch Protection (GitHub CLI)

```bash
# Install gh CLI if not already installed
# https://cli.github.com/

# Configure branch protection for main
gh api repos/:owner/:repo/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["CI / lint","CI / test","CI / build","Chain CI / lint","Chain CI / test","Chain CI / build","Publisher CI / lint","Publisher CI / test","Publisher CI / build"]}' \
  --field enforce_admins=true \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}' \
  --field restrictions=null
```

## CI/CD Workflow Details

### 1. Push to `dev` Branch

**Triggers**:

- All CI workflows run automatically
- Lint, test, and build jobs execute
- Docker images are built and pushed to ACR (if applicable)

**Workflows**:

- `ci.yml` - Workspace-level lint, test, build
- `chain-ci.yml` - Chain service CI (if chain files changed)
- `publisher-ci.yml` - Publisher service CI (if publisher files changed)

**What happens**:

- Code is validated
- Tests are executed
- Build artifacts are created
- Docker images tagged with branch name (`dev`)

### 2. Pull Request to `main`

**Triggers**:

- PR opened, updated, or marked ready for review
- Same CI checks run as on `dev` push

**Required**:

- All CI checks must pass
- At least 1 approval required
- Branch must be up to date with `main`
- All conversations must be resolved

**Workflows**:

- Same as push to `dev`, but:
  - Docker images are **not** pushed (only built for validation)
  - PR checks must pass before merge is allowed

### 3. Merge to `main` (Automatic Staging Release)

**Triggers**:

- PR merged to `main`
- Automatic deployment to staging

**Workflows**:

- `staging-release.yml` - Deploys to staging environment
  - Terraform infrastructure deployment
  - Kubernetes service deployment
  - All services updated to latest from `main`

**What happens**:

1. Infrastructure is deployed via Terraform
2. Kubernetes manifests are applied to staging AKS cluster
3. Services are rolled out with zero-downtime deployment
4. Deployment status is reported

### 4. Production Release (Manual with Approval)

**Triggers**:

- Manual workflow dispatch from Actions tab
- Requires explicit confirmation: `DEPLOY TO PRODUCTION`

**Workflows**:

- `production-release.yml` - Deploys to production environment
  - Requires environment approval (configured in GitHub)
  - Terraform infrastructure deployment
  - Kubernetes service deployment

**What happens**:

1. Workflow is manually triggered
2. Confirmation input is validated
3. Environment approval is required (if configured)
4. Infrastructure is deployed to production
5. Services are deployed with verification
6. Deployment summary is reported

## Environment Configuration

GitHub Environments must be configured for staging and production:

### Staging Environment

1. Go to **Settings** → **Environments**
2. Create environment: `staging`
3. Configure:
   - Deployment branches: `main` only
   - Protection rules: Optional (no approval required for staging)

### Production Environment

1. Go to **Settings** → **Environments**
2. Create environment: `production`
3. Configure:
   - Deployment branches: `main` only
   - Protection rules:
     - [x] Required reviewers: Add at least 1 reviewer (recommended: 2)
     - [x] Wait timer: Optional (e.g., 5 minutes delay)
     - [ ] Deployment branches: Restrict to `main`

## Manual Production Deployment

To deploy to production:

1. Go to **Actions** → **Production Release**
2. Click **Run workflow**
3. Select branch: `main`
4. In the confirmation field, type exactly: `DEPLOY TO PRODUCTION`
5. Click **Run workflow**
6. If environment approval is required, designated reviewers will be notified
7. Once approved, deployment proceeds automatically

## Workflow Files

| File                                       | Purpose                           | Triggers                                                 |
| ------------------------------------------ | --------------------------------- | -------------------------------------------------------- |
| `.github/workflows/ci.yml`                 | Workspace CI (lint, test, build)  | Push to `dev`, PR to `main`                              |
| `.github/workflows/chain-ci.yml`           | Chain service CI                  | Push to `dev`, PR to `main` (if chain files changed)     |
| `.github/workflows/publisher-ci.yml`       | Publisher service CI              | Push to `dev`, PR to `main` (if publisher files changed) |
| `.github/workflows/release.yml`            | NPM package releases (Changesets) | Push to `main`                                           |
| `.github/workflows/staging-release.yml`    | Staging deployment                | Push to `main`                                           |
| `.github/workflows/production-release.yml` | Production deployment             | Manual workflow dispatch                                 |
| `.github/workflows/infra-deploy.yml`       | Infrastructure deployment         | Manual or on infra changes to `main`                     |

## Best Practices

1. **Always work on `dev` branch** - Never push directly to `main`
2. **Create PRs for all changes** - Use descriptive PR titles and descriptions
3. **Wait for CI checks** - Don't merge until all checks pass
4. **Test in staging** - Verify staging deployment before production
5. **Use semantic versioning** - For npm packages via Changesets
6. **Monitor deployments** - Check deployment status and logs
7. **Rollback procedure** - Know how to rollback if production issues occur

## Troubleshooting

### CI Checks Not Running

- Verify branch protection rules are configured
- Check workflow file syntax
- Ensure paths in workflow `on:` sections match changed files

### PR Cannot Be Merged

- All required status checks must pass
- At least 1 approval required
- Branch must be up to date (rebase or merge `main` into your branch)
- All PR conversations must be resolved

### Staging Deployment Failed

- Check Terraform plan output
- Verify Azure credentials and permissions
- Review Kubernetes deployment logs
- Check infrastructure resources availability

### Production Deployment Blocked

- Verify environment approval is configured
- Check that reviewers are available
- Ensure confirmation input is exactly `DEPLOY TO PRODUCTION`
- Review any protection rules configured for production environment

## Related Documentation

- [Infrastructure Guide](./INFRASTRUCTURE.md)
- [Implementation Roadmap](./IMPLEMENTATION_ROADMAP.md)
- [ADR-0003: Release Pipeline Strategy](./architecture/adr/0003-release-pipeline-strategy.md)
