# Admin-UI Submodule Fix Summary

## Problem
The repository's git index contained a reference to the `packages/admin-ui` submodule pointing to commit `b32a79a56384d72d3ccd65fa307c86435dc407dd`, which doesn't exist in the remote Mystira.Admin.UI repository. This caused all CI workflows to fail during checkout with:

```
Error: fatal: remote error: upload-pack: not our ref b32a79a56384d72d3ccd65fa307c86435dc407dd
Error: fatal: Fetched in submodule path 'packages/admin-ui', but it did not contain b32a79a56384d72d3ccd65fa307c86435dc407dd. Direct fetching of that commit failed.
```

## Solution Implemented

### 1. Removed Invalid Submodule Reference
- Deleted the invalid commit reference from the git index
- Kept the `.gitmodules` entry intact for future use

### 2. Created Reusable Composite Action
Created `.github/actions/setup-admin-ui/` with:
- `action.yml`: Composite action that clones admin-ui from the dev branch
- `README.md`: Documentation explaining the workaround and future improvements

### 3. Security Best Practices
The action implements secure credential handling:
- Uses `GIT_ASKPASS` environment variable for authentication
- Creates temporary askpass script with unique name (includes run ID and job ID)
- No token exposure in URLs or command line
- Cleans up temporary files after use
- Uses local git config (doesn't affect global state)

### 4. Updated All Workflows
Modified 6 workflow files to use the new action:
- `.github/workflows/ci.yml` (3 jobs: lint, test, build)
- `.github/workflows/release.yml`
- `.github/workflows/staging-release.yml` (2 jobs)
- `.github/workflows/production-release.yml` (2 jobs)
- `.github/workflows/chain-ci.yml` (6 jobs)
- `.github/workflows/publisher-ci.yml` (6 jobs)

## How It Works

1. Workflow checks out main repository with `submodules: recursive`
2. Calls `setup-admin-ui` action with GitHub token
3. Action checks if `packages/admin-ui` exists and is a valid git repository
4. If not, creates temporary GIT_ASKPASS script and clones from dev branch
5. Configures git user for the submodule locally
6. Cleans up temporary files

## Benefits

1. **Centralized**: Single action can be modified in one place
2. **Maintainable**: Easy to update or replace when permanent fix is implemented
3. **Secure**: Follows GitHub Actions security best practices
4. **Comprehensive**: All workflows use the same approach
5. **Documented**: Clear explanation of why and how to improve

## Future Improvements

Once the correct commit reference is determined:
1. Update the submodule to point to a valid commit
2. Remove the composite action
3. Remove action calls from all workflows
4. Rely on standard `submodules: recursive` checkout

## Testing

The CI workflows should now successfully:
- Check out the main repository
- Clone the admin-ui submodule from the dev branch HEAD
- Continue with normal build/test/lint operations

## Files Changed

- `.github/actions/setup-admin-ui/action.yml` (new)
- `.github/actions/setup-admin-ui/README.md` (new)
- `.github/workflows/ci.yml` (modified)
- `.github/workflows/release.yml` (modified)
- `.github/workflows/staging-release.yml` (modified)
- `.github/workflows/production-release.yml` (modified)
- `.github/workflows/chain-ci.yml` (modified)
- `.github/workflows/publisher-ci.yml` (modified)
- `packages/admin-ui` (removed from git index)
