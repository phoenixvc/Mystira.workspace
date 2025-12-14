# GitHub Workflow Permissions Explained

## The Problem

Your CI/CD workflows are failing with:

```
Error: Input required and not supplied: token
```

This happens when `actions/checkout@v6` tries to clone **private submodules** but doesn't have the necessary authentication.

## Two Solutions

### Solution 1: Use GITHUB_TOKEN (Easier)

**What to do:**

1. Go to **Repository Settings → Actions → General → Workflow permissions**
2. Select **"Read and write permissions"**
3. Click **Save**

**What this does:**

- Grants the default `GITHUB_TOKEN` full access to your repository
- Allows workflows to clone private submodules using the built-in token
- No need to create a Personal Access Token (PAT)

**When to use this:**

- ✅ You're okay with workflows having write access
- ✅ You want the simplest solution
- ✅ All your repos are in the same organization

**To use this in workflows:**

```yaml
- uses: actions/checkout@v6
  with:
    submodules: recursive
    # token: ${{ secrets.GITHUB_TOKEN }}  # This is automatic, can omit
```

### Solution 2: Use Personal Access Token (PAT) (More Secure)

**What to do:**

1. Create a PAT:
   - GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   - Generate new token with `repo` scope
   - Copy the token

2. Add as GitHub Secret:
   - Go to repository Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN`
   - Value: Paste your PAT
   - Click "Add secret"

**What this does:**

- Uses a specific token for submodule access
- More granular control over permissions
- Can restrict workflow permissions to "Read only" for better security

**When to use this:**

- ✅ You want more granular control
- ✅ You want to keep workflow permissions as "Read only" by default
- ✅ You're following security best practices

**Your workflows already use this:**

```yaml
- uses: actions/checkout@v6
  with:
    submodules: recursive
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
```

## Current Status

Your workflows are **already configured** to use Solution 2 (PAT). You just need to:

1. **Create the PAT** with `repo` scope
2. **Add it as a secret** named `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN`

## Which Should You Choose?

**Recommendation: Use Solution 2 (PAT)** because:

- Your workflows are already set up for it
- Better security (workflows remain read-only by default)
- More explicit control

But if you want the quickest fix: **Use Solution 1 (Read and write permissions)** and remove the `token:` line from checkout steps.

## The Documentation Confusion

The GitHub docs mention using `permissions:` in workflow YAML files. This is a **separate concept** from the repository-level "Workflow permissions" setting.

- **Repository-level setting** (what you see in the screenshot): Default permissions for all workflows
- **Workflow-level `permissions:` key**: Override permissions for specific workflows

For your use case, you don't need to add `permissions:` to workflows if you:

- Use "Read and write permissions" in repository settings, OR
- Use a PAT with `repo` scope

## Quick Fix Checklist

**If using PAT (Solution 2):**

- [ ] Create PAT with `repo` scope
- [ ] Add secret: `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN`
- [ ] Workflows already configured ✓

**If using GITHUB_TOKEN (Solution 1):**

- [ ] Set repository workflow permissions to "Read and write"
- [ ] Remove `token:` parameter from checkout steps in workflows
- [ ] Workflows will use `GITHUB_TOKEN` automatically

## Still Confused?

**TL;DR:**

- Go to Settings → Actions → General → Workflow permissions
- Select "Read and write permissions"
- Click Save
- OR create a PAT and add it as `MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN` secret

Either way works. The PAT approach is more secure, but the GITHUB_TOKEN approach is simpler.
