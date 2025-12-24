# Mystira.DevHub Migration Guide

**Target**: Migrate DevHub to use shared packages and latest infrastructure
**Runtime**: Node.js 22+ / TypeScript
**Estimated Effort**: 0.5 day
**Last Updated**: December 2025
**Status**: 📋 Planned

---

## Overview

DevHub is a TypeScript developer portal. Migration focuses on:

1. **Node.js 22 upgrade** (recommended)
2. Adopting `@mystira/design-tokens` v0.2.0
3. Adopting `@mystira/shared-utils` v0.2.0
4. **Dark mode support** (new)
5. Infrastructure alignment with ADR-0017

---

## Current State Analysis

### Technology Stack

| Component | Current | Target |
|-----------|---------|--------|
| Node.js | 20.x | 22.x LTS |
| TypeScript | 5.x | 5.x (latest) |
| pnpm | 9.x | 9.15+ |

### Package Dependencies

| Current | Action | Replacement |
|---------|--------|-------------|
| Custom design tokens | Replace | `@mystira/design-tokens` |
| Custom utilities | Replace | `@mystira/shared-utils` |

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

### 1.2 Update CI/CD

```yaml
# .github/workflows/ci.yml
- uses: actions/setup-node@v4
  with:
    node-version: '22'
```

---

## Phase 2: Install Shared Packages

### 2.1 Add Packages

```bash
cd packages/devhub
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
/* styles/globals.css */
@import '@mystira/design-tokens/css/variables.css';
@import '@mystira/design-tokens/css/dark-mode.css';
```

### 3.2 Update Tailwind Config (if applicable)

```javascript
// tailwind.config.js
const mystiraPreset = require('@mystira/design-tokens/tailwind/preset');

module.exports = {
  presets: [mystiraPreset],
  content: ['./src/**/*.{ts,tsx}', './pages/**/*.{ts,tsx}'],
  darkMode: 'class', // Enable dark mode support
};
```

### 3.3 Replace Custom Color Variables

```css
/* Before */
:root {
  --color-primary: #3b82f6;
  --color-background: #ffffff;
}

/* After - use imported variables */
/* Variables are imported from @mystira/design-tokens */
```

---

## Phase 4: Shared Utils Migration

### 4.1 HTTP Client

```typescript
// Before
import axios from 'axios';

const response = await axios.get('/api/docs');

// After
import { httpClient } from '@mystira/shared-utils';

const response = await httpClient.get('/api/docs');
```

### 4.2 Retry Logic

```typescript
// Before
async function fetchWithRetry(url: string, retries = 3) {
  // Custom retry logic
}

// After
import { retry } from '@mystira/shared-utils';

const result = await retry(() => fetch(url), {
  retries: 3,
  backoff: 'exponential',
});
```

### 4.3 Logging

```typescript
// Before
console.log('User action:', action);

// After
import { logger } from '@mystira/shared-utils';

logger.info('User action', { action });
```

### 4.4 Date Formatting

```typescript
// Before
const formatted = new Date(timestamp).toLocaleDateString();

// After
import { formatDate } from '@mystira/shared-utils';

const formatted = formatDate(timestamp, 'long');
```

---

## Phase 5: Dark Mode Support

### 5.1 Theme Toggle Component

```tsx
// components/ThemeToggle.tsx
import { useState, useEffect } from 'react';

export function ThemeToggle() {
  const [isDark, setIsDark] = useState(false);

  useEffect(() => {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    const stored = localStorage.getItem('theme');
    setIsDark(stored === 'dark' || (!stored && prefersDark));
  }, []);

  useEffect(() => {
    document.documentElement.classList.toggle('dark', isDark);
    localStorage.setItem('theme', isDark ? 'dark' : 'light');
  }, [isDark]);

  return (
    <button onClick={() => setIsDark(!isDark)}>
      {isDark ? 'Light Mode' : 'Dark Mode'}
    </button>
  );
}
```

### 5.2 Tailwind Dark Mode Classes

```tsx
// Example component
<div className="bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100">
  {/* Content */}
</div>
```

---

## Migration Checklist

### Pre-Migration
- [ ] Review current color usage
- [ ] Identify custom utility functions

### Phase 1: Runtime
- [ ] Update Node.js version in package.json
- [ ] Update CI/CD workflows

### Phase 2: Packages
- [ ] Install @mystira/design-tokens
- [ ] Install @mystira/shared-utils

### Phase 3: Design Tokens
- [ ] Import CSS variables
- [ ] Update Tailwind config
- [ ] Remove custom color definitions
- [ ] Test color consistency

### Phase 4: Shared Utils
- [ ] Replace custom HTTP client
- [ ] Replace retry logic
- [ ] Replace logging
- [ ] Replace date formatting

### Phase 5: Dark Mode
- [ ] Add theme toggle
- [ ] Test dark mode styling
- [ ] Verify color contrast

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

---

## Related Documentation

- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
- [@mystira/design-tokens README](../../packages/design-tokens/README.md)
- [@mystira/shared-utils README](../../packages/shared-utils/README.md)
- [Mystira.Publisher Migration Guide](./mystira-publisher-migration.md)
