# Git Submodules Guide

This workspace integrates multiple Mystira repositories as git submodules. This guide explains how to work with them.

## What are Submodules?

Git submodules allow you to include one Git repository as a subdirectory of another Git repository. Each submodule points to a specific commit in the external repository.

## Initial Setup

### Cloning the Workspace

When cloning this workspace for the first time, use:

```bash
git clone --recurse-submodules https://github.com/phoenixvc/Mystira.workspace.git
```

This will clone the workspace and all submodules in one command.

### If Already Cloned

If you've already cloned without submodules:

```bash
# Initialize and clone all submodules
git submodule update --init --recursive
```

## Working with Submodules

### Updating Submodules

To update all submodules to their latest commits:

```bash
git submodule update --remote
```

To update a specific submodule:

```bash
git submodule update --remote packages/chain
```

### Making Changes in Submodules

1. Navigate to the submodule directory:

   ```bash
   cd packages/chain
   ```

2. Check out a branch (if needed):

   ```bash
   git checkout main
   git pull origin main
   ```

3. Make your changes and commit them:

   ```bash
   git add .
   git commit -m "Your changes"
   git push origin main
   ```

4. Return to the workspace root and update the submodule reference:
   ```bash
   cd ../..
   git add packages/chain
   git commit -m "Update Mystira.Chain submodule"
   ```

### Pulling Latest Changes

To pull the latest changes from all submodules:

```bash
git submodule update --remote --merge
```

### Switching Branches

When switching branches in the main workspace, update submodules:

```bash
git checkout <branch>
git submodule update --init --recursive
```

## Submodule Repositories

| Submodule              | Path                        | Repository                         |
| ---------------------- | --------------------------- | ---------------------------------- |
| Mystira.Chain          | `packages/chain/`           | `phoenixvc/Mystira.Chain`          |
| Mystira.App            | `packages/app/`             | `phoenixvc/Mystira.App`            |
| Mystira.StoryGenerator | `packages/story-generator/` | `phoenixvc/Mystira.StoryGenerator` |
| Mystira.Publisher      | `packages/publisher/`       | `phoenixvc/Mystira.Publisher`      |
| Mystira.DevHub         | `packages/devhub/`          | `phoenixvc/Mystira.DevHub`         |
| Mystira.Infra          | `infra/`                    | `phoenixvc/Mystira.Infra`          |

## Troubleshooting

### Submodule Shows as Modified

If a submodule shows as modified but you haven't made changes:

```bash
# This usually means the submodule is on a different commit
cd packages/chain
git status
# If you want to update to the latest
git pull origin main
cd ../..
git add packages/chain
```

### Removing a Submodule

If you need to remove a submodule:

```bash
# Remove the submodule entry from .git/config
git submodule deinit -f packages/chain

# Remove the submodule from .gitmodules
# (edit .gitmodules manually)

# Remove the submodule from git index
git rm --cached packages/chain

# Remove the submodule directory
rm -rf packages/chain
```

### Cloning Without Submodules

If you want to clone the workspace without submodules:

```bash
git clone https://github.com/phoenixvc/Mystira.workspace.git
```

Then add submodules later as needed.

### Git Proxy Limitations

If you're using a local git proxy that only authorizes the main workspace repository, you may encounter errors when trying to push submodule changes:

```
remote: Proxy error: repository not authorized
fatal: unable to access 'http://127.0.0.1:22376/git/phoenixvc/Mystira.Chain/': The requested URL returned error: 502
```

**Solution**: If the proxy only authorizes `Mystira.workspace`, you'll need to push submodule changes directly to GitHub (not through the proxy) or ensure the proxy is configured to authorize all submodule repositories.

To work around this:

1. Push submodule changes from an environment with direct GitHub access
2. Or configure the proxy to authorize all required submodule repositories
3. Or use direct GitHub remotes instead of the proxy for submodule repositories

## Best Practices

1. **Always commit submodule updates**: When a submodule is updated, commit the change in the workspace
2. **Use specific commits**: The workspace tracks specific commits, not branches (unless using `--remote`)
3. **Document submodule versions**: Keep track of which versions of each submodule are compatible
4. **Test after updates**: Always test the workspace after updating submodules

## CI/CD Considerations

In CI/CD pipelines, ensure submodules are initialized:

```yaml
- name: Checkout with submodules
  uses: actions/checkout@v4
  with:
    submodules: recursive
```
