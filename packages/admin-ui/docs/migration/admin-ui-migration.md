# Mystira.Admin.UI Migration Guide

**Target**: Migrate Admin UI to use shared packages and latest infrastructure
**Runtime**: Node.js 22+ / React / TypeScript
**Estimated Effort**: 0.5 day
**Last Updated**: December 2025
**Status**: Planned

---

## Overview

Admin.UI is a React/TypeScript admin dashboard. Migration focuses on:

1. **Node.js 22 upgrade** (recommended)
2. Adopting `@mystira/design-tokens` v0.2.0
3. Adopting `@mystira/shared-utils` v0.2.0
4. **Dark mode support** (new)
5. Aligning with Admin.Api authentication (Microsoft Entra External ID)

---

## Current State Analysis

### Technology Stack

| Component | Current | Target |
|-----------|---------|--------|
| Node.js | 20.x | 22.x LTS |
| React | 18.x | 18.x (latest) |
| TypeScript | 5.x | 5.x (latest) |
| pnpm | 9.x | 9.15+ |

### Package Dependencies

| Current | Action | Replacement |
|---------|--------|-------------|
| Custom design tokens | Replace | `@mystira/design-tokens` |
| Custom utilities | Replace | `@mystira/shared-utils` |
| @azure/msal-react | Update | Latest for Entra External ID |

---

## Phase 1: Update Runtime

### 1.1 Update package.json

```json
{
  "engines": {
    "node": ">=22.0.0",
    "pnpm": ">=9.0.0"
  }
}
```

### 1.2 Update Dependencies

```bash
pnpm update
```

---

## Phase 2: Install Shared Packages

### 2.1 Add Packages

```bash
cd packages/admin-ui
pnpm add @mystira/design-tokens@0.2.0 @mystira/shared-utils@0.2.0
```

### 2.2 Update package.json

```json
{
  "dependencies": {
    "@mystira/design-tokens": "^0.2.0",
    "@mystira/shared-utils": "^0.2.0"
  }
}
```

---

## Phase 3: Design Tokens Migration

### 3.1 Import CSS Variables

```css
/* src/index.css or styles/globals.css */
@import '@mystira/design-tokens/css/variables.css';
@import '@mystira/design-tokens/css/dark-mode.css';
```

### 3.2 Update Tailwind Config

```javascript
// tailwind.config.js
const mystiraPreset = require('@mystira/design-tokens/tailwind/preset');

module.exports = {
  presets: [mystiraPreset],
  content: ['./src/**/*.{ts,tsx}', './index.html'],
  darkMode: 'class',
  theme: {
    extend: {
      // Admin-specific overrides if needed
    },
  },
};
```

### 3.3 Replace Custom Color Variables

```tsx
// Before
const buttonClass = 'bg-blue-500 hover:bg-blue-600';

// After (using design token classes)
const buttonClass = 'bg-primary-500 hover:bg-primary-600';
```

---

## Phase 4: Shared Utils Migration

### 4.1 HTTP Client with Auth

```typescript
// Before
import axios from 'axios';

const api = axios.create({
  baseURL: '/api',
  headers: { Authorization: `Bearer ${token}` },
});

// After
import { httpClient } from '@mystira/shared-utils';

const api = httpClient.create({
  baseURL: '/api',
  authProvider: async () => getAccessToken(),
});
```

### 4.2 Form Validation

```typescript
// Before
function validateEmail(email: string) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

// After
import { validateSchema } from '@mystira/shared-utils';
import { z } from 'zod';

const emailSchema = z.string().email();
const result = validateSchema(emailSchema, email);
```

### 4.3 Error Handling

```typescript
// Before
try {
  await api.post('/content', data);
} catch (error) {
  console.error('Failed to save', error);
}

// After
import { logger } from '@mystira/shared-utils';

try {
  await api.post('/content', data);
} catch (error) {
  logger.error('Failed to save content', { error, contentId: data.id });
}
```

---

## Phase 5: Dark Mode Support

### 5.1 Theme Provider

```tsx
// contexts/ThemeContext.tsx
import { createContext, useContext, useEffect, useState } from 'react';

type Theme = 'light' | 'dark' | 'system';

const ThemeContext = createContext<{
  theme: Theme;
  setTheme: (theme: Theme) => void;
}>({ theme: 'system', setTheme: () => {} });

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = useState<Theme>(() => {
    const stored = localStorage.getItem('admin-theme') as Theme;
    return stored || 'system';
  });

  useEffect(() => {
    const root = document.documentElement;
    const isDark =
      theme === 'dark' ||
      (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches);

    root.classList.toggle('dark', isDark);
    localStorage.setItem('admin-theme', theme);
  }, [theme]);

  return (
    <ThemeContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeContext.Provider>
  );
}

export const useTheme = () => useContext(ThemeContext);
```

### 5.2 Theme Selector Component

```tsx
// components/ThemeSelector.tsx
import { useTheme } from '../contexts/ThemeContext';

export function ThemeSelector() {
  const { theme, setTheme } = useTheme();

  return (
    <select value={theme} onChange={(e) => setTheme(e.target.value as any)}>
      <option value="light">Light</option>
      <option value="dark">Dark</option>
      <option value="system">System</option>
    </select>
  );
}
```

---

## Phase 6: Microsoft Entra External ID (Optional)

If Admin.Api migrates to Microsoft Entra External ID:

### 6.1 Update MSAL Configuration

```typescript
// authConfig.ts
import { Configuration } from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: process.env.VITE_ENTRA_CLIENT_ID!,
    authority: `https://${process.env.VITE_ENTRA_TENANT_SUBDOMAIN}.ciamlogin.com/${process.env.VITE_ENTRA_TENANT_ID}`,
    redirectUri: window.location.origin,
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: ['openid', 'profile', 'api://admin-api/.default'],
};
```

### 6.2 Update API Scopes

```typescript
// After Entra External ID migration
const apiScopes = ['api://admin-api/Content.Read', 'api://admin-api/Content.Write'];
```

---

## Migration Checklist

### Pre-Migration

- [ ] Review current color usage
- [ ] Identify custom utility functions
- [ ] Document MSAL configuration

### Phase 1: Runtime

- [ ] Update Node.js version
- [ ] Update dependencies

### Phase 2: Packages

- [ ] Install @mystira/design-tokens
- [ ] Install @mystira/shared-utils

### Phase 3: Design Tokens

- [ ] Import CSS variables
- [ ] Import dark mode CSS
- [ ] Update Tailwind config
- [ ] Remove custom color definitions
- [ ] Test color consistency

### Phase 4: Shared Utils

- [ ] Replace custom HTTP client
- [ ] Replace validation utilities
- [ ] Replace logging

### Phase 5: Dark Mode

- [ ] Add ThemeProvider
- [ ] Add theme selector
- [ ] Test dark mode styling

### Phase 6: Entra External ID (Optional)

- [ ] Update MSAL configuration
- [ ] Update API scopes
- [ ] Test authentication flow

### Post-Migration

- [ ] Run all tests
- [ ] Visual regression testing
- [ ] Cross-browser testing
- [ ] Create PR

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Node.js 20 → 22 | Runtime upgrade | Test in CI first |
| CSS variable names | Possible style breaks | Audit usage |
| Custom utils removal | Import path changes | Find/replace |
| Entra External ID | Auth flow changes | Gradual rollout |

---

## Related Documentation

- [Migration Overview](./README.md)
- [Migration Strategy](./strategy.md)
- [Contracts Migration](./contracts-migration.md)
