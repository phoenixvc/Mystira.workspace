# Workspace Configuration Overrides

This directory contains configuration files that are copied to specific packages when needed (e.g., during CI).

## ESLint presets

Vite/Vitest configuration for `apps/admin/ui`. Copied to `apps/admin/ui/vite.config.ts` by CI before running tests.

Adds `passWithNoTests: true` so Vitest passes gracefully when no test files exist yet.

## admin-ui.eslint.config.mjs

ESLint configuration for `apps/admin/ui` and `apps/publisher`. Copied by CI before running lint.

Provides:

- Browser environment for DOM APIs
- ES2020 environment features
- React Hooks linting rules
- TypeScript support
- Node.js environment for config files (vite.config.ts)

## Future

These overrides should be moved into the packages themselves once their configurations are finalized.
