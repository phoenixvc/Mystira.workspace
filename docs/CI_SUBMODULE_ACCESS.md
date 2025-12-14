# CI/CD Submodule Access Configuration

## Issue

GitHub Actions workflows fail when trying to checkout private submodule repositories with errors like:

```
repository 'https://github.com/phoenixvc/Mystira.App.git/' not found
```

## Root Cause

The "repository not found" error typically indicates one of:

1. **Repositories don't exist**: The submodule repositories haven't been created in GitHub yet
2. **Private repository access**: The default `GITHUB_TOKEN` may not have sufficient permissions to access private submodule repositories, even within the same organization
3. **Organization permissions**: The repositories exist but are in a different organization or have restricted access

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
   - All workflows already have `token: ${{ secrets.GITHUB_TOKEN }}` configured
   - Change to: `token: ${{ secrets.SUBMODULE_ACCESS_TOKEN }}` if needed

### Option 2: Repository Permissions

Ensure the repositories exist and are accessible:

1. Verify all submodule repositories exist:
   - `Mystira.App`
   - `Mystira.Chain`
   - `Mystira.StoryGenerator`
   - `Mystira.Publisher`
   - `Mystira.DevHub`
   - `Mystira.Infra`

2. Check repository visibility:
   - If private, ensure they're in the same organization
   - Verify the GitHub Actions service account has access

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
