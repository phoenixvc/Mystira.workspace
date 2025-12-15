# Setup Admin-UI Submodule Action

This composite action handles the initialization of the `packages/admin-ui` submodule.

## Why This Action Exists

The admin-ui submodule reference in the git index was pointing to a commit that no longer exists in the remote repository (`b32a79a56384d72d3ccd65fa307c86435dc407dd`). This caused CI workflows to fail during the checkout step.

Rather than fixing the commit reference directly (which requires access to determine the correct commit hash), this action provides a workaround by:
1. Removing the invalid submodule reference from the git index
2. Manually cloning the admin-ui repository from the `dev` branch during CI runs

## Usage

```yaml
- uses: actions/checkout@v6
  with:
    submodules: recursive
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}

- name: Setup admin-ui submodule
  uses: ./.github/actions/setup-admin-ui
  with:
    token: ${{ secrets.MYSTIRA_GITHUB_SUBMODULE_ACCESS_TOKEN }}
```

## Inputs

- `token` (required): GitHub token with access to the private Mystira.Admin.UI repository

## Future Improvements

Once the correct commit reference is determined, this workaround can be replaced by:
1. Updating the submodule reference to point to a valid commit
2. Removing this action and its calls from all workflows
3. Relying on the standard `submodules: recursive` checkout option

## Notes

- The action clones from the `dev` branch with `--depth 1` for faster checkouts
- It only clones if the directory doesn't already exist (idempotent)
- All workflows that use submodules have been updated to use this action
