# Admin-UI Submodule Setup - TODO for Next Agent

## Problem

`packages/admin-ui` directory exists but is NOT properly registered as a git submodule. It's pointing to the wrong remote (Mystira.workspace instead of Mystira.Admin.UI).

## Required Actions

### 1. Clean up the existing admin-ui directory

```powershell
cd C:\Users\smitj\repos\Mystira.workspace
Remove-Item -Recurse -Force packages/admin-ui
Remove-Item .git/index.lock -ErrorAction SilentlyContinue
```

### 2. Add admin-ui as a proper submodule

```powershell
cd C:\Users\smitj\repos\Mystira.workspace
git submodule add https://github.com/phoenixvc/Mystira.Admin.UI.git packages/admin-ui
```

### 3. If the repo is empty or main branch doesn't exist, try dev branch:

```powershell
cd packages/admin-ui
git fetch origin
git checkout dev  # or whatever branch exists
cd ../..
```

### 4. Update .gitmodules to add admin-ui entry

The .gitmodules file should have:

```
[submodule "packages/admin-ui"]
	path = packages/admin-ui
	url = https://github.com/phoenixvc/Mystira.Admin.UI.git
	branch = dev  # or main, whatever the default branch is
```

### 5. Register and commit

```powershell
git add .gitmodules packages/admin-ui
git commit -m "chore: add admin-ui as submodule"
git push origin dev
```

## Current Status

- `admin-api` submodule: ✅ Already working (on dev branch)
- `admin-ui` submodule: ✅ **COMPLETE** - Successfully registered as submodule
- Both repos exist: `Mystira.Admin.Api` and `Mystira.Admin.UI`

## ✅ Completed

**Phase 2 is now complete!**

The `Mystira.Admin.UI` repository has been:

1. ✅ Initialized with README.md commit
2. ✅ Pushed to remote repository (dev branch)
3. ✅ Successfully registered as git submodule in workspace
4. ✅ Verified in `git submodule status`

**Next Steps**: Proceed to Phase 3 - Admin UI Code Migration (extract code from `Mystira.App`)

## Verification

After completion, run:

```powershell
git submodule status
```

Should show both `packages/admin-api` and `packages/admin-ui` in the list.
