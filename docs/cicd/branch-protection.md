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

### Protection for `dev` Branch

**Recommendation**: Require PRs to `dev` with lighter requirements than `main`.

#### Why Require PRs to `dev`?

1. **Code Quality**: Ensures all code is reviewed before integration, catching issues early
2. **Knowledge Sharing**: PRs facilitate team collaboration and code knowledge transfer
3. **CI Enforcement**: Guarantees CI checks pass before code enters `dev`
4. **Clean History**: Better git history with clear commits and PR context
5. **Practice for Production**: Developers practice PR workflow before `main` PRs

#### Required Settings for `dev`

- ✅ **Require pull request before merging**
  - Require approvals: `0` (or `1` if you want stricter quality)
  - Dismiss stale reviews: Optional
  - Require review from Code Owners: ❌ Disabled (not required for dev)

- ✅ **Require status checks to pass before merging**
  - Require branches to be up to date: ✅ Enabled
  - Required checks: All CI jobs (`CI / lint`, `CI / test`, `CI / build`, etc.)

- ❌ **Require conversation resolution**: Optional (not required for dev)

- ❌ **Allow force pushes**: Disabled (prevents accidental overwrites)

- ❌ **Allow deletions**: Disabled

### Protection for `main` Branch

Stricter protection rules for production code:

#### Required Settings for `main`

- ✅ **Require pull request before merging**
  - Require approvals: `1` (required)
  - Dismiss stale reviews: ✅ Enabled
  - Require review from Code Owners: ✅ Enabled (if CODEOWNERS file exists)

- ✅ **Require status checks to pass before merging**
  - Require branches to be up to date: ✅ Enabled
  - Required checks: All CI jobs

- ✅ **Require conversation resolution**: Enabled (all PR comments must be resolved)

- ❌ **Allow force pushes**: Disabled

- ❌ **Allow deletions**: Disabled

#### Comparison: `dev` vs `main`

| Setting                         | `dev`    | `main`       |
| ------------------------------- | -------- | ------------ |
| Require PR                      | ✅ Yes   | ✅ Yes       |
| Required Approvals              | 0 (or 1) | 1 (required) |
| Require Code Owner Review       | ❌ No    | ✅ Yes       |
| Require Conversation Resolution | ❌ No    | ✅ Yes       |
| Required CI Checks              | ✅ All   | ✅ All       |
| Allow Force Pushes              | ❌ No    | ❌ No        |

## Setting Up Branch Protection

### Via GitHub UI

1. Go to **Settings** → **Branches**
2. Click **Add rule**
3. Branch name pattern: `dev` or `main`
4. Configure appropriate settings (see above)
5. Click **Create**

### Via GitHub CLI

#### `dev` Branch Protection

```bash
gh api repos/:owner/:repo/branches/dev/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["CI / lint","CI / test","CI / build","Chain CI / lint","Chain CI / test","Chain CI / build","Publisher CI / lint","Publisher CI / test","Publisher CI / build"]}' \
  --field required_pull_request_reviews='{"required_approving_review_count":0,"dismiss_stale_reviews":false}' \
  --field enforce_admins=false \
  --field restrictions=null \
  --field allow_force_pushes=false \
  --field allow_deletions=false
```

#### `main` Branch Protection

```bash
gh api repos/:owner/:repo/branches/main/protection \
  --method PUT \
  --field required_status_checks='{"strict":true,"contexts":["CI / lint","CI / test","CI / build","Chain CI / lint","Chain CI / test","Chain CI / build","Publisher CI / lint","Publisher CI / test","Publisher CI / build"]}' \
  --field required_pull_request_reviews='{"required_approving_review_count":1,"dismiss_stale_reviews":true,"require_code_owner_reviews":true}' \
  --field enforce_admins=true \
  --field restrictions=null \
  --field allow_force_pushes=false \
  --field allow_deletions=false \
  --field required_conversation_resolution=true
```

## Workflow Impact

### With PRs Required to `dev`

```
Developer creates feature branch
  ↓
Developer pushes feature branch
  ↓
Developer creates PR: feature → dev
  ↓
CI runs automatically
  ↓
CI must pass before merge
  ↓
Developer merges PR (no approval needed if set to 0)
  ↓
Code in dev, CI runs again, Docker images pushed
```

### PR to `main` Workflow

```
Code in dev branch
  ↓
Developer creates PR: dev → main
  ↓
CI runs automatically
  ↓
CI must pass
  ↓
Code review required (1 approval minimum)
  ↓
Code owner approval required
  ↓
All conversations resolved
  ↓
PR merged to main
  ↓
Staging Release triggered automatically
  ↓
Manual approval → Production Release
```

## Benefits of PR Requirements

- Code review happens before integration
- CI checks are enforced
- Clean git history
- Team collaboration
- Better code quality

## Related Documentation

- [CI/CD Setup](./cicd-setup.md)
- [Workflow Permissions](./workflow-permissions.md)
- [ADR-0004: Branching Strategy](../architecture/adr/0004-branching-strategy-and-cicd.md)
