# Fix for Admin-UI ESLint Errors

## Problem
The `packages/admin-ui` submodule is missing ESLint configuration for React and browser environments, causing lint failures:
- `'React' is not defined` errors
- Browser API errors (`window`, `document`, `localStorage`, `File`, `FormData`, `Blob`)
- Node.js API errors in config files (`__dirname`)

## Solution
Add the following `.eslintrc.cjs` file to the `Mystira.Admin.UI` repository (dev branch):

```javascript
module.exports = {
  root: true,
  env: { browser: true, es2020: true },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
  ],
  ignorePatterns: ['dist', '.eslintrc.cjs'],
  parser: '@typescript-eslint/parser',
  plugins: ['react-refresh'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/no-unused-vars': [
      'warn',
      { argsIgnorePattern: '^_' },
    ],
  },
  overrides: [
    {
      files: ['vite.config.ts', '*.config.ts'],
      env: {
        node: true,
      },
    },
  ],
}
```

## Additional Fixes Required in Admin-UI Code

### 1. React Hooks Rules Violations
Files with conditional hook calls need to be refactored:
- `src/pages/CreateMasterDataPage.tsx`
- `src/pages/EditMasterDataPage.tsx`
- `src/pages/MasterDataPage.tsx`

Example fix for conditional hook usage:
```typescript
// ❌ Bad - hooks called after conditional return
export function CreateMasterDataPage({ type }: { type?: string }) {
  if (!type) {
    return <div>No type specified</div>;
  }
  const form = useForm(); // Hook called conditionally!
  //...
}

// ✅ Good - all hooks called before any returns
export function CreateMasterDataPage({ type }: { type?: string }) {
  const form = useForm(); // Hook called unconditionally
  
  if (!type) {
    return <div>No type specified</div>;
  }
  //...
}
```

### 2. Unused Variables (Warnings)
Consider removing or using the `err` variables in catch blocks:
- `src/pages/BadgesPage.tsx:52`
- `src/pages/BundlesPage.tsx:52`
- `src/pages/CharacterMapsPage.tsx:52`
- `src/pages/MasterDataPage.tsx:150`
- `src/pages/MediaPage.tsx:52`
- `src/pages/ScenariosPage.tsx:52`

Use `_err` if the variable needs to exist but won't be used:
```typescript
} catch (_err) {
  // Error ignored
}
```

## Steps to Apply

1. Navigate to the Mystira.Admin.UI repository
2. Create `.eslintrc.cjs` in the root with the content above
3. Fix the React Hooks violations in the affected pages
4. Fix the unused variable warnings
5. Run `pnpm lint` to verify all errors are resolved
6. Commit and push to the `dev` branch
7. Update the workspace submodule reference to the new commit

## Verification

After applying these changes, running `pnpm lint` in the workspace should complete without errors.
