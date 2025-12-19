# Configuration Files for Submodules

This directory contains configuration files that should be copied to submodules when they're missing.

## admin-ui.vite.config.ts

Vite/Vitest configuration for the `packages/admin-ui` submodule. This file is automatically copied to `packages/admin-ui/vite.config.ts` by the CI workflow before running tests.

### Why is this needed?

The admin-ui submodule currently has no test files. Vitest exits with code 1 when no test files are found, causing CI to fail. This configuration adds `passWithNoTests: true` to allow the test job to pass gracefully.

## admin-ui.eslintrc.cjs

ESLint configuration for the `packages/admin-ui` submodule. This file is automatically copied to `packages/admin-ui/.eslintrc.cjs` by:

1. The `scripts/setup-submodules.sh` script
2. The CI workflow before running lint

### Why is this needed?

The admin-ui submodule is a React/Vite application that requires specific ESLint configuration:
- Browser environment for DOM APIs
- ES2020 environment features
- React Hooks linting rules
- TypeScript support
- Node.js environment for config files (vite.config.ts)

### Permanent Solution

This is a temporary workaround. The permanent solution is to add this `.eslintrc.cjs` file directly to the [Mystira.Admin.UI repository](https://github.com/phoenixvc/Mystira.Admin.UI).

A patch file is available at `../admin-ui-eslint-config.patch` that can be applied to the admin-ui repository.

## Future

When the ESLint configuration is added to the admin-ui repository itself, this workaround can be removed.
