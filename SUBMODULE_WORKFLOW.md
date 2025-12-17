# Submodule Workflow Guide

This guide explains how to work with submodules in the Mystira workspace and avoid common pitfalls.

## 🚨 The Problem

When you commit changes to a submodule and then commit the workspace, the workspace references a specific commit SHA from that submodule. If that commit doesn't exist on the remote, CI will fail with:

```
fatal: remote error: upload-pack: not our ref <commit-sha>
fatal: Fetched in submodule path '<submodule>', but it did not contain <commit-sha>
```

## ✅ The Solution

**Always push submodules before pushing the workspace.**

## 📋 Recommended Workflow

### Option 1: Use Git's Built-in Feature (Recommended)

Git can automatically push submodules for you:

```bash
# Push workspace and all submodules in one command
git push --recurse-submodules=on-demand

# Or configure it permanently
git config push.recurseSubmodules on-demand
```

With this setting, Git will:

1. Check all submodules for unpushed commits
2. Push submodules first
3. Then push the workspace

### Option 2: Manual Push (When You Need Control)

```bash
# 1. Make changes in a submodule
cd packages/publisher
git add .
git commit -m "fix: something"
git push origin dev

# 2. Return to workspace
cd ../..
git add packages/publisher
git commit -m "chore: update publisher submodule"
git push
```

### Option 3: Push All Submodules at Once

```bash
# Push all submodules on current branch
git submodule foreach 'git push origin $(git branch --show-current)'

# Then push workspace
git push
```

## 🛡️ Safeguards We've Added

### 1. Pre-Push Hook

A Husky pre-push hook checks if all submodule commits are pushed before allowing workspace push.

**Location:** `.husky/pre-push`

**Behavior:**

- ✅ Allows push if all submodule commits exist on remote
- ❌ Blocks push if any submodule has unpushed commits
- 💡 Shows which submodules need pushing

**Bypass (not recommended):**

```bash
git push --no-verify
```

### 2. CI Workflow Check

A GitHub Actions workflow validates submodule commits on every PR and push.

**Location:** `.github/workflows/check-submodules.yml`

**Triggers:**

- Pull requests (opened, synchronized, reopened)
- Pushes to `dev` and `main` branches

**Behavior:**

- ✅ Passes if all submodule commits exist on their remotes
- ❌ Fails with clear error messages if commits are missing
- 📊 Shows which submodules and commits are problematic

## 🔍 Checking Submodule Status

### Check if submodules have unpushed commits

```bash
# Check all submodules
git submodule foreach 'echo "=== $name ===" && git status'

# Check if current submodule commits are pushed
git submodule foreach 'git branch -r --contains HEAD || echo "⚠️  NOT PUSHED"'
```

### Check what the workspace is pointing to

```bash
# Show all submodule commits referenced by workspace
git submodule status

# Example output:
# ee1c91bd... infra (heads/dev)
# f869fad1... packages/publisher (heads/dev)
```

### Verify a specific commit exists on remote

```bash
cd packages/publisher
git ls-remote origin | grep <commit-sha>
# If no output, commit doesn't exist on remote
```

## 🚀 Common Scenarios

### Scenario 1: You forgot to push a submodule

**Error:**

```
Submodule 'infra' references commit ee1c91b which does not exist on remote
```

**Fix:**

```bash
cd infra
git push origin dev
cd ..
# Workspace push will now succeed
```

### Scenario 2: Submodule is on wrong branch

**Symptom:** Submodule changes don't appear after pull

**Fix:**

```bash
# Update submodules to match workspace references
git submodule update --remote --merge

# Or initialize/update all submodules
git submodule update --init --recursive
```

### Scenario 3: Merge conflict in submodule reference

**Symptom:** Git shows conflict in submodule during merge

**Fix:**

```bash
# Go to the submodule
cd packages/publisher

# Merge or rebase as appropriate
git fetch origin
git merge origin/dev

# Return to workspace
cd ../..

# Add the resolved submodule reference
git add packages/publisher
git commit -m "chore: update publisher submodule after merge"
```

### Scenario 4: Need to update all submodules to latest

```bash
# Update all submodules to their latest remote commit
git submodule update --remote --merge

# Or for each submodule
git submodule foreach 'git pull origin $(git branch --show-current)'

# Commit the workspace changes
git add .
git commit -m "chore: update all submodules to latest"
git push
```

## 🔧 Troubleshooting

### Problem: Pre-push hook is blocking legitimate push

**Cause:** Hook might be incorrectly detecting unpushed commits

**Solution:**

1. Verify commits are actually pushed:

   ```bash
   cd <submodule>
   git log origin/dev..HEAD  # Should be empty if pushed
   ```

2. If pushed but hook still fails, update hook:

   ```bash
   git fetch origin  # In submodule
   ```

3. Last resort (not recommended):
   ```bash
   git push --no-verify
   ```

### Problem: CI check fails but local check passes

**Cause:** Local git might be using cached remote refs

**Solution:**

```bash
# Force fetch all submodule remotes
git submodule foreach 'git fetch origin --prune'

# Check again
git push --recurse-submodules=check
```

### Problem: Submodule is detached HEAD

**Symptom:** Can't push submodule (not on any branch)

**Fix:**

```bash
cd packages/publisher
git checkout dev  # Or appropriate branch
git pull origin dev
# Make your changes
cd ../..
```

## 📝 Best Practices

1. **Always work on branches in submodules**
   - Don't work in detached HEAD state
   - Use `git checkout dev` before making changes

2. **Configure automatic submodule push**

   ```bash
   git config push.recurseSubmodules on-demand
   ```

3. **Use descriptive commit messages**
   - Submodule: `fix: resolve test failure in publisher`
   - Workspace: `chore: update publisher submodule with test fix`

4. **Keep submodules in sync**
   - Regularly run `git submodule update --remote --merge`
   - Don't let submodules drift too far behind their remotes

5. **Review submodule changes in PRs**
   - GitHub shows submodule diffs as commit ranges
   - Click through to review actual changes

6. **Document why submodules are updated**
   - In workspace commit message, explain what changed in submodule
   - Link to submodule PR/commit if applicable

## 🔗 Quick Reference

```bash
# Clone workspace with submodules
git clone --recurse-submodules <repo-url>

# Update existing workspace submodules
git submodule update --init --recursive

# Push workspace and submodules together
git push --recurse-submodules=on-demand

# Check submodule status
git submodule status

# Update all submodules to remote
git submodule update --remote --merge

# Run command in all submodules
git submodule foreach '<command>'

# Check if submodules are pushed
git push --recurse-submodules=check
```

## 📚 Additional Resources

- [Git Submodules Documentation](https://git-scm.com/book/en/v2/Git-Tools-Submodules)
- [Working with Submodules in GitHub](https://github.blog/2016-02-01-working-with-submodules/)
- [Husky Git Hooks](https://typicode.github.io/husky/)

---

**Remember:** Submodules are powerful but require discipline. When in doubt, push submodules first, then push the workspace.
