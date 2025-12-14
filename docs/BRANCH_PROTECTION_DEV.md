# Branch Protection for `dev` Branch - Recommendation

## Recommendation: Require PRs to `dev` Branch

### Why Require PRs to `dev`?

1. **Code Quality**: Ensures all code is reviewed before integration, catching issues early
2. **Knowledge Sharing**: PRs facilitate team collaboration and code knowledge transfer
3. **CI Enforcement**: Guarantees CI checks pass before code enters `dev`
4. **Clean History**: Better git history with clear commits and PR context
5. **Practice for Production**: Developers practice PR workflow before `main` PRs

### Configuration

Configure branch protection for `dev` with **lighter requirements** than `main`:

#### Required Settings

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

#### Differences from `main` Protection

| Setting                         | `dev`    | `main`       |
| ------------------------------- | -------- | ------------ |
| Require PR                      | ✅ Yes   | ✅ Yes       |
| Required Approvals              | 0 (or 1) | 1 (required) |
| Require Code Owner Review       | ❌ No    | ✅ Yes       |
| Require Conversation Resolution | ❌ No    | ✅ Yes       |
| Required CI Checks              | ✅ All   | ✅ All       |
| Allow Force Pushes              | ❌ No    | ❌ No        |

### Workflow Impact

**With PRs required to `dev`:**

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

**Benefits:**

- Code review happens before integration
- CI checks are enforced
- Clean git history
- Team collaboration

**Trade-offs:**

- Slightly slower iteration (need to create PR)
- One extra step before code reaches dev

### Alternative: Direct Pushes with CI Checks

If your team prefers faster iteration and trusts automated checks:

**Configuration:**

- ❌ Do NOT require pull request
- ✅ Require status checks to pass before merge (but allow direct pushes that trigger CI)
- ⚠️ Note: GitHub branch protection can't prevent pushes that fail CI, only merges
- **Solution**: Use pre-push hooks or rely on CI feedback post-push

**This approach:**

- ✅ Faster iteration
- ❌ No code review before integration
- ❌ Can push broken code (though CI will fail)
- ❌ Less collaboration

### Recommendation

**For most teams**: Require PRs to `dev` with 0 approvals + CI checks required.

This provides:

- Code review opportunity (even if not required)
- CI enforcement
- Team collaboration
- Minimal overhead (no approval delay)

**For very small teams (1-2 developers)**: Direct pushes to `dev` may be acceptable, but still require PRs to `main`.

## Setting Up `dev` Branch Protection

### Via GitHub UI

1. Go to **Settings** → **Branches**
2. Click **Add rule**
3. Branch name pattern: `dev`
4. Configure:
   - ✅ Require a pull request before merging
     - Number of approvals: `0` (or `1`)
     - Dismiss stale reviews: Optional
   - ✅ Require status checks to pass before merging
     - Require branches to be up to date: ✅
     - Select required checks: All CI jobs
   - ❌ Require conversation resolution: Disabled
   - ❌ Allow force pushes: Disabled
   - ❌ Allow deletions: Disabled
5. Click **Create**

### Via GitHub CLI

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

## Related Documentation

- [Branch Protection Guide](./BRANCH_PROTECTION.md)
- [ADR-0004: Branching Strategy](./architecture/adr/0004-branching-strategy-and-cicd.md)
