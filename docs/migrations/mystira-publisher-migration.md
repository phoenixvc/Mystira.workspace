# Mystira.Publisher Migration Guide

**Target**: Migrate Publisher to use `@mystira/design-tokens` and shared utilities
**Prerequisites**: `@mystira/design-tokens` v0.2.0+ published to NPM
**Estimated Effort**: 0.5-1 day
**Last Updated**: December 2025
**Status**: 📋 Planned

---

## Overview

Publisher is a React/TypeScript frontend with the most comprehensive design token system in the workspace. Migration focuses on:

1. Adopting `@mystira/design-tokens` v0.2.0 (derived from Publisher's own tokens)
2. Migrating to unified `@mystira/shared-utils` v0.2.0
3. Standardizing HTTP client patterns with retry utilities
4. **Dark mode support** (new)
5. **Dockerfile migration** to submodule repo (ADR-0019)

Publisher's `variables.css` was the **reference implementation** for `@mystira/design-tokens`, so this migration is mostly about consuming the centralized package.

---

## Current State Analysis

### Publisher's Design System

Publisher has a comprehensive design token system at `/src/styles/variables.css`:

```css
/* Color System */
--color-primary-50 through --color-primary-900 (purple #9333ea gradient)
--color-neutral-50 through --color-neutral-900
--color-success, --color-warning, --color-danger, --color-info

/* Typography */
--font-family-sans: 'Inter', system-ui, sans-serif
--font-family-mono: 'Fira Code', monospace
--font-size-xs through --font-size-4xl (8 scales)

/* Spacing */
--spacing-0 through --spacing-16 (11-point scale)

/* Components */
--radius-sm/md/lg/xl/full
--shadow-sm/md/lg/xl
--transition-fast/base/slow
--z-dropdown/sticky/modal/tooltip/toast
```

### Current Dependencies

| Package | Version | Action |
|---------|---------|--------|
| `@mystira/shared-utils` | local | Update to workspace package |
| `zustand` | 5.x | Keep |
| `axios` | Latest | Keep |
| `zod` | Latest | Keep |
| `react-hook-form` | Latest | Keep |

---

## Phase 1: Install Design Tokens

### 1.1 Add Package

```bash
cd packages/publisher
pnpm add @mystira/design-tokens@0.2.0 @mystira/shared-utils@0.2.0
```

### 1.2 Update package.json

```json
{
  "dependencies": {
    "@mystira/design-tokens": "^0.2.0",
    "@mystira/shared-utils": "^0.2.0"
  }
}
```

### 1.3 Ensure Node.js Version

```json
// package.json
{
  "engines": {
    "node": ">=20.0.0"
  }
}
```

---

## Phase 2: CSS Variables Migration

### 2.1 Option A: Full Replacement

Replace `src/styles/variables.css` with the design tokens package:

```css
/* src/styles/index.css */
@import '@mystira/design-tokens/css/variables.css';

/* Publisher-specific overrides (if any) */
:root {
  /* Override primary color if Publisher needs different purple */
  --color-primary-600: #9333ea; /* Publisher's original purple */
}
```

### 2.2 Option B: Gradual Migration

Keep Publisher's variables.css but import shared tokens for new features:

```css
/* src/styles/variables.css */
/* Keep existing variables */

/* Import shared tokens for consistency with other apps */
@import '@mystira/design-tokens/css/variables.css';

/* Publisher overrides take precedence due to cascade */
:root {
  --color-primary-600: #9333ea;
}
```

### 2.3 Color Differences

Note: `@mystira/design-tokens` uses Mystira.App's purple (`#7c3aed`) as the default primary color. Publisher originally used `#9333ea`.

| Token | Design Tokens | Publisher Original |
|-------|---------------|-------------------|
| `--color-primary-600` | `#7c3aed` | `#9333ea` |
| `--color-primary-700` | `#7c3aed` | `#7c3aed` |

**Decision**: Either adopt the unified color or override in Publisher's CSS.

---

## Phase 3: TypeScript Token Usage

### 3.1 Import Tokens in Components

```typescript
// Using TypeScript tokens
import { colors, spacing, typography } from '@mystira/design-tokens';

// Example: styled-components or CSS-in-JS
const Button = styled.button`
  background-color: ${colors.primary[600]};
  padding: ${spacing[4]} ${spacing[6]};
  font-family: ${typography.fontFamily.sans};
`;
```

### 3.2 Using CSS Variables (Recommended)

```tsx
// Use CSS variables for runtime theming support
const Button: React.FC = () => (
  <button
    style={{
      backgroundColor: 'var(--color-primary-600)',
      padding: 'var(--spacing-4) var(--spacing-6)',
    }}
  >
    Click me
  </button>
);
```

---

## Phase 4: Shared Utils Migration

### 4.1 Update Imports

```typescript
// From (if using local copy)
import { retry, logger } from '../utils/shared';

// To
import { retry, logger, validateSchema } from '@mystira/shared-utils';
```

### 4.2 Available Utilities

| Utility | Description |
|---------|-------------|
| `retry(fn, options)` | Retry with exponential backoff |
| `logger` | Structured logging |
| `validateSchema(schema, data)` | Zod schema validation |
| `formatDate(date)` | Date formatting |
| `debounce(fn, ms)` | Debounce utility |

---

## Phase 5: HTTP Client Standardization (Optional)

### 5.1 Current Axios Setup

```typescript
// Current: Custom axios instance
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 30000,
});
```

### 5.2 Enhanced with Shared Utilities

```typescript
import { retry } from '@mystira/shared-utils';

// Add retry wrapper
const apiWithRetry = {
  get: <T>(url: string) => retry(() => api.get<T>(url), { maxRetries: 3 }),
  post: <T>(url: string, data: unknown) =>
    retry(() => api.post<T>(url, data), { maxRetries: 3 }),
};
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure @mystira/design-tokens is published
- [ ] Create feature branch
- [ ] Document current color usage

### Phase 1: Package Setup
- [ ] Install @mystira/design-tokens
- [ ] Install @mystira/shared-utils
- [ ] Verify build succeeds

### Phase 2: CSS Migration
- [ ] Decide on Option A (full replacement) or Option B (gradual)
- [ ] Update CSS imports
- [ ] Test dark mode if applicable
- [ ] Verify visual regression

### Phase 3: TypeScript Tokens
- [ ] Update component imports
- [ ] Replace hardcoded values with tokens

### Phase 4: Shared Utils
- [ ] Update utility imports
- [ ] Remove local duplicate utilities

### Phase 5: HTTP Client (Optional)
- [ ] Add retry wrapper
- [ ] Test API calls

### Post-Migration
- [ ] Visual regression testing
- [ ] Cross-browser testing
- [ ] Create PR

---

## Visual Regression Testing

Before and after screenshots for:

- [ ] Login page
- [ ] Dashboard
- [ ] Story editor
- [ ] Settings page
- [ ] Dark mode (all pages)

---

## File Changes Summary

| File | Action |
|------|--------|
| `package.json` | Add design-tokens, shared-utils |
| `src/styles/index.css` | Import design tokens |
| `src/styles/variables.css` | Remove or keep as overrides |
| `src/utils/*` | Remove duplicates of shared-utils |

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Primary color shift | Visual change | Override if needed |
| Variable names | Possible CSS breaks | Audit variable usage |

---

## Phase 6: Dockerfile Migration (ADR-0019)

Move Dockerfile from workspace to submodule repo:

### 6.1 Create Dockerfile in Submodule

```dockerfile
# packages/publisher/Dockerfile (new location)
FROM node:22-alpine AS build

WORKDIR /app

COPY package.json pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 6.2 Add CI/CD Workflow

```yaml
# .github/workflows/ci.yml (in Mystira.Publisher repo)
name: Publisher CI

on:
  push:
    branches: [main, dev]
  pull_request:

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: pnpm/action-setup@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'pnpm'
      - run: pnpm install --frozen-lockfile
      - run: pnpm build
      - run: pnpm test

  docker:
    needs: build
    if: github.ref == 'refs/heads/dev'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: docker/login-action@v3
        with:
          registry: myssharedacr.azurecr.io
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}
      - uses: docker/build-push-action@v5
        with:
          push: true
          tags: myssharedacr.azurecr.io/publisher:${{ github.sha }}
      - name: Trigger workspace deployment
        uses: peter-evans/repository-dispatch@v2
        with:
          token: ${{ secrets.WORKSPACE_PAT }}
          repository: phoenixvc/Mystira.workspace
          event-type: publisher-deploy
          client-payload: '{"sha": "${{ github.sha }}"}'
```

---

## Notes

Publisher was the **source** for `@mystira/design-tokens`. The main benefit of this migration is:

1. **Consistency**: Other apps (Admin-UI, DevHub) will match Publisher's design
2. **Single Source**: Design changes propagate to all apps
3. **Reduced Maintenance**: No need to maintain tokens in Publisher separately
4. **Faster CI/CD**: Docker builds run in submodule repo (ADR-0019)

---

## Related Documentation

- [ADR-0019: Dockerfile Location Standardization](../adr/ADR-0019-dockerfile-location-standardization.md)
- [Design Token Analysis](../analysis/package-inventory.md#design-system-analysis-phase-4e)
- [@mystira/design-tokens README](../../packages/design-tokens/README.md)
- [Mystira.Admin Migration Guide](./mystira-admin-migration.md)
- [Mystira.Admin.UI Migration Guide](./mystira-admin-ui-migration.md)
