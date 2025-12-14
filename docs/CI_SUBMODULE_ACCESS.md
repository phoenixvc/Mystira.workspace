# CI/CD Submodule Access Configuration

## Issue

GitHub Actions workflows fail when trying to checkout private submodule repositories with errors like:

```
repository 'https://github.com/phoenixvc/Mystira.App.git/' not found
```

## Root Cause

The default `GITHUB_TOKEN` may not have sufficient permissions to access private submodule repositories, even within the same organization.

## Solution

### Option 1: Use Personal Access Token (Recommended)

1. **Create a PAT**:
   - Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate new token with `repo` scope
   - Copy the token

2. **Add as GitHub Secret**:
   - Go to repository Settings → Secrets and variables → Actions
   - Add new secret: `SUBMODULE_ACCESS_TOKEN`
   - Paste the PAT token

3. **Update Workflows**:
   - All workflows use `token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}`
   - Add the PAT as secret: `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN`

### Option 2: Create Missing Repositories (If Needed)

If repositories don't exist, create them:

1. **Create repositories in GitHub**:
   - Go to https://github.com/organizations/phoenixvc/repositories/new
   - Create each repository:
     - `Mystira.App`
     - `Mystira.Chain`
     - `Mystira.StoryGenerator`
     - `Mystira.Publisher`
     - `Mystira.DevHub`
     - `Mystira.Infra`

2. **Push initial commits** to each repository

3. **Verify `.gitmodules` URLs** match the created repositories

### Option 3: Repository Permissions

If repositories exist but are private:

1. Verify all submodule repositories exist and are accessible
2. Check repository visibility:
   - If private, ensure they're in the same organization (`phoenixvc`)
   - Verify the GitHub Actions service account has access
3. Check organization settings:
   - Settings → Actions → General → Workflow permissions
   - Ensure "Read and write permissions" is enabled

### Option 3: Workflow Permissions

Add explicit permissions to workflows:

```yaml
permissions:
  contents: read
  submodules: read
```

## Current Configuration

All workflows now include:

```yaml
- uses: actions/checkout@v6
  with:
    submodules: recursive
    token: ${{ secrets.GITHUB_TOKEN }}
```

## Troubleshooting

### If repositories don't exist:

1. Create the repositories in GitHub
2. Push initial commits
3. Update `.gitmodules` if repository names differ

### If access is denied:

1. Verify PAT has `repo` scope
2. Check repository permissions
3. Ensure repositories are in the same organization
4. Consider using a GitHub App instead of PAT

## Related Documentation

- [GitHub Actions: Checkout Action](https://github.com/actions/checkout)
- [Git Submodules in CI/CD](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idstepsuses)
