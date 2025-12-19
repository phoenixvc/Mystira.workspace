# PR Summary: ESLint Configuration Fix for Admin-UI

## What This PR Fixes

This PR implements a workaround for ESLint configuration issues in the `packages/admin-ui` submodule:

✅ **Fixed (66 → ~6 errors)**:
- Added browser environment configuration (fixes `window`, `document`, `localStorage` undefined errors)
- Added ES2020 environment
- Added React support (fixes `React` undefined errors)
- Added TypeScript ESLint configuration
- Added support for browser APIs (`File`, `FormData`, `Blob` undefined errors)
- Added Node.js environment for config files (fixes `__dirname` undefined in `vite.config.ts`)

## How It Works

1. ESLint configuration is stored in `configs/admin-ui.eslintrc.cjs`
2. CI workflow automatically copies it to `packages/admin-ui/.eslintrc.cjs` before running lint
3. Local setup script also copies the config when initializing submodules

## What Still Needs Fixing

❌ **Remaining Issues (Requires code changes in Mystira.Admin.UI repository)**:

### React Hooks Violations (6 errors)
Files with hooks called after conditional returns:
- `src/pages/CreateMasterDataPage.tsx` (lines 151, 156)
- `src/pages/EditMasterDataPage.tsx` (lines 158, 167, 173, 190)  
- `src/pages/MasterDataPage.tsx` (lines 120, 130)

### Warnings (14 warnings - not blocking if `--max-warnings` flag is adjusted)
- Unused variables in catch blocks (6 occurrences)
- TypeScript `any` type usage (8 occurrences)

## Why Can't This PR Fix Everything?

The `packages/admin-ui` directory is a Git submodule pointing to the `Mystira.Admin.UI` repository. The remaining issues are in the source code of that repository, which requires:

1. Access to the Mystira.Admin.UI repository
2. Code changes to fix the React Hooks violations
3. Committing those changes to the admin-ui repository
4. Updating the workspace submodule reference

## Next Steps

### Option 1: Merge This PR + Manual Fix
1. Merge this PR to get the ESLint config workaround
2. Apply `admin-ui-eslint-config.patch` to the Mystira.Admin.UI repository
3. Fix the React Hooks violations in the admin-ui code
4. Update workspace submodule reference

### Option 2: Complete Fix Before Merge
1. Apply the patch to Mystira.Admin.UI repository
2. Fix React Hooks violations there  
3. Update this PR's submodule reference
4. Then merge

## Testing

⚠️ **Important**: The CI will still fail after merging this PR because of the React Hooks violations in the admin-ui code.

However, the error count will be dramatically reduced:
- **Before**: 66 errors, 14 warnings
- **After this PR**: ~6 errors, ~14 warnings

The remaining 6 errors are React Hooks rule violations that require code changes in the Mystira.Admin.UI repository.

### To Make CI Pass Completely

After merging this PR, you must also:
1. Fix the React Hooks violations in the Mystira.Admin.UI repository (see ADMIN_UI_LINT_FIX.md for details)
2. OR temporarily disable the react-hooks/rules-of-hooks rule (not recommended)

This PR provides the foundation - the ESLint configuration - but the code fixes must be done separately in the admin-ui repository.

## Files Changed

- `.github/workflows/ci.yml` - Added step to copy ESLint config before linting
- `scripts/setup-submodules.sh` - Added logic to copy ESLint config during setup
- `configs/admin-ui.eslintrc.cjs` - ESLint configuration for admin-ui
- `configs/README.md` - Documentation for the configs directory
- `admin-ui-eslint-config.patch` - Patch file to apply to admin-ui repository
- `ADMIN_UI_LINT_FIX.md` - Detailed fix documentation
- `.gitignore` - Added package-lock.json
