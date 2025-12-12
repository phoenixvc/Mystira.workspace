# Changesets

This workspace uses [Changesets](https://github.com/changesets/changesets) to manage versioning and changelogs.

## Adding a Changeset

When you make changes that should be included in a release:

```bash
pnpm changeset
```

This will prompt you to:
1. Select which packages are affected
2. Choose the type of change (major, minor, patch)
3. Write a summary of the change

## Releasing

Changesets will create a PR with version bumps and changelog updates. Once merged, the release workflow will publish the packages.

## Manual Release

```bash
# Version packages
pnpm changeset version

# Publish (if you have npm publish permissions)
pnpm changeset publish
```

