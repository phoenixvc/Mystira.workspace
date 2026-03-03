# Shared Configuration Presets

This directory contains shared config presets used by thin package-level
wrappers across the monorepo.

## ESLint presets

- `eslint/create-node-eslint-config.mjs`
  - Shared Node + TypeScript flat-config factory.
  - Used by non-React packages (for example `contracts`, `shared-utils`).
- `eslint/create-react-eslint-config.mjs`
  - Shared React + TypeScript flat-config factory.
  - Used by frontend packages (for example `admin-ui`, `publisher`).

Package `eslint.config.mjs` files should stay as thin wrappers that only define
package-specific ignores and rule tweaks.

## TypeScript presets

- `typescript/tsconfig.base.json`
  - Common strictness and compiler safety defaults.
- `typescript/tsconfig.lib.json`
  - Baseline for TypeScript library packages.
- `typescript/tsconfig.react-vite.json`
  - Baseline for React/Vite app packages.
- `typescript/tsconfig.vite-node.json`
  - Baseline for Vite node-side config files (`tsconfig.node.json`).

Package `tsconfig*.json` files should prefer `extends` and only keep
package-specific options (aliases, output paths, stricter linting toggles).
