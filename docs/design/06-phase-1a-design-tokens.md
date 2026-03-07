# Mystira Phase 1A — Design Tokens & Tailwind Mapping

## Overview

This document defines the canonical design token model, semantic naming conventions, and Tailwind CSS integration for the Mystira design system.

---

## 1. Canonical Token Model

### 1.1 Brand Tokens

| Token              | Value     | Name                |
|--------------------|-----------|---------------------|
| `brand.primary`    | `#5B3CC4` | Twilight Amethyst   |
| `brand.secondary`  | `#C7B8FF` | Starlight Lavender  |
| `brand.accent`     | `#F6C453` | Moonbeam Gold       |
| `brand.support`    | `#4ED7C8` | Dream Teal          |

### 1.2 Light Theme Tokens

#### Surface

| Token                      | Value     |
|----------------------------|-----------|
| `surface.background`       | `#F6F4FF` |
| `surface.card`             | `#ECE7FF` |
| `surface.elevated`         | `#E8E2FF` |
| `surface.hover`            | `#F1EDFF` |
| `surface.selected`         | `#DDD6FF` |
| `surface.border`           | `#DED9FF` |
| `surface.selectedBorder`   | `#B8A9FF` |

#### Text

| Token              | Value     |
|--------------------|-----------|
| `text.primary`     | `#2E1F66` |
| `text.secondary`   | `#5C4FA3` |
| `text.disabled`    | `#A8A3D6` |
| `text.gold`        | `#F6C453` |
| `text.selected`    | `#2E1F66` |

#### Focus

| Token          | Value     |
|----------------|-----------|
| `focus.ring`   | `#5B3CC4` |

#### Status — Success

| Token                          | Value     |
|--------------------------------|-----------|
| `status.success.background`    | `#E9F8EF` |
| `status.success.base`          | `#4CAF75` |
| `status.success.border`        | `#B6E2C6` |

#### Status — Info

| Token                       | Value     |
|-----------------------------|-----------|
| `status.info.background`    | `#EAF6FF` |
| `status.info.base`          | `#46B8DC` |
| `status.info.border`        | `#A7D3F2` |

#### Status — Warning

| Token                          | Value     |
|--------------------------------|-----------|
| `status.warning.background`    | `#FFF6D9` |
| `status.warning.base`          | `#E59C00` |
| `status.warning.border`        | `#F3D18A` |

#### Status — Error

| Token                        | Value     |
|------------------------------|-----------|
| `status.error.background`    | `#FFE9E9` |
| `status.error.base`          | `#D94B4B` |
| `status.error.border`        | `#F3B3B3` |

### 1.3 Dark Theme Tokens

#### Surface

| Token                      | Value     |
|----------------------------|-----------|
| `surface.background`       | `#141127` |
| `surface.card`             | `#1E1A38` |
| `surface.elevated`         | `#26225A` |
| `surface.hover`            | `#23204A` |
| `surface.selected`         | `#2F2866` |
| `surface.border`           | `#2B2750` |
| `surface.selectedBorder`   | `#3D3580` |

#### Text

| Token              | Value     |
|--------------------|-----------|
| `text.primary`     | `#F8F7FF` |
| `text.secondary`   | `#B9B4E6` |
| `text.disabled`    | `#6F6A9A` |
| `text.gold`        | `#F6C453` |
| `text.selected`    | `#F8F7FF` |

#### Focus

| Token          | Value     |
|----------------|-----------|
| `focus.ring`   | `#C7B8FF` |

#### Status — Success (Dark)

| Token                          | Value     |
|--------------------------------|-----------|
| `status.success.background`    | `#0F2A1C` |
| `status.success.base`          | `#5EDC91` |
| `status.success.border`        | `#1E5C3C` |

#### Status — Info (Dark)

| Token                       | Value     |
|-----------------------------|-----------|
| `status.info.background`    | `#10212B` |
| `status.info.base`          | `#59C7E6` |
| `status.info.border`        | `#1E4C66` |

#### Status — Warning (Dark)

| Token                          | Value     |
|--------------------------------|-----------|
| `status.warning.background`    | `#2B2108` |
| `status.warning.base`          | `#FFB84D` |
| `status.warning.border`        | `#7A5D1C` |

#### Status — Error (Dark)

| Token                        | Value     |
|------------------------------|-----------|
| `status.error.background`    | `#2A1515` |
| `status.error.base`          | `#FF6B6B` |
| `status.error.border`        | `#7A2E2E` |

---

## 2. Semantic Naming Map

All Mystira components must consume semantic tokens, never raw color values.

### Approved Semantic Groups

- **Brand:** `brand.primary`, `brand.secondary`, `brand.accent`, `brand.support`
- **Surface:** `surface.background`, `surface.card`, `surface.elevated`, `surface.hover`, `surface.selected`, `surface.border`, `surface.selectedBorder`
- **Text:** `text.primary`, `text.secondary`, `text.disabled`, `text.gold`, `text.selected`
- **Focus:** `focus.ring`
- **Status:** `status.{success|info|warning|error}.{background|base|border}`

### Radius

| Token        | Value  |
|--------------|--------|
| `radius.sm`  | 8px    |
| `radius.md`  | 12px   |
| `radius.lg`  | 16px   |
| `radius.xl`  | 24px   |
| `radius.pill`| 999px  |

### Motion

| Token             | Value                            |
|-------------------|----------------------------------|
| `motion.fast`     | 150ms                            |
| `motion.standard` | 250ms                            |
| `motion.slow`     | 400ms                            |
| `motion.ease`     | cubic-bezier(0.4, 0, 0.2, 1)    |

### Shadow

| Token        | Purpose          | Value                                          |
|--------------|------------------|-------------------------------------------------|
| `shadow.sm`  | Subtle elevation | `0 1px 3px rgba(0,0,0,0.08)`                  |
| `shadow.md`  | Card elevation   | `0 4px 12px rgba(0,0,0,0.12)`                 |
| `shadow.lg`  | Modal elevation  | `0 8px 24px rgba(0,0,0,0.16)`                 |

### Typography

| Token          | Value     |
|----------------|-----------|
| `font.heading` | Baloo 2   |
| `font.body`    | Nunito    |

| Token          | Size  | Weight | Description         |
|----------------|-------|--------|---------------------|
| `type.h1`      | 36px  | 700    | Hero headings       |
| `type.h2`      | 28px  | 700    | Section headings    |
| `type.h3`      | 22px  | 600    | Subsection headings |
| `type.h4`      | 18px  | 600    | Card headings       |
| `type.body`    | 16px  | 400    | Body text           |
| `type.small`   | 14px  | 400    | Small text          |
| `type.caption` | 12px  | 400    | Captions            |

---

## 3. Tailwind Token Mapping

### 3.1 CSS Variable Contract

All semantic tokens are expressed as CSS custom properties with RGB channel values:

```css
:root {
  --brand-primary: 91 60 196;
  --brand-secondary: 199 184 255;
  --brand-accent: 246 196 83;
  --brand-support: 78 215 200;

  --surface-bg: 246 244 255;
  --surface-card: 236 231 255;
  --surface-elevated: 232 226 255;
  --surface-hover: 241 237 255;
  --surface-selected: 221 214 255;
  --surface-border: 222 217 255;
  --surface-selected-border: 184 169 255;

  --text-primary: 46 31 102;
  --text-secondary: 92 79 163;
  --text-disabled: 168 163 214;
  --text-gold: 246 196 83;
  --text-selected: 46 31 102;

  --focus-ring: 91 60 196;

  --status-success-bg: 233 248 239;
  --status-success-base: 76 175 117;
  --status-success-border: 182 226 198;

  --status-info-bg: 234 246 255;
  --status-info-base: 70 184 220;
  --status-info-border: 167 211 242;

  --status-warning-bg: 255 246 217;
  --status-warning-base: 229 156 0;
  --status-warning-border: 243 209 138;

  --status-error-bg: 255 233 233;
  --status-error-base: 217 75 75;
  --status-error-border: 243 179 179;
}
```

Dark theme overrides the same semantic variables via `.dark` class or `@media (prefers-color-scheme: dark)`. The `.dark` class is the canonical integration path; `@media` provides automatic OS-level switching.

```css
.dark,
@media (prefers-color-scheme: dark) {
  :root {
    --brand-primary: 91 60 196;
    --brand-secondary: 199 184 255;
    --brand-accent: 246 196 83;
    --brand-support: 78 215 200;

    --surface-bg: 20 17 39;
    --surface-card: 30 26 56;
    --surface-elevated: 38 34 90;
    --surface-hover: 35 32 74;
    --surface-selected: 47 40 102;
    --surface-border: 43 39 80;
    --surface-selected-border: 61 53 128;

    --text-primary: 248 247 255;
    --text-secondary: 185 180 230;
    --text-disabled: 111 106 154;
    --text-gold: 246 196 83;
    --text-selected: 248 247 255;

    --focus-ring: 199 184 255;

    --status-success-bg: 15 42 28;
    --status-success-base: 94 220 145;
    --status-success-border: 30 92 60;

    --status-info-bg: 16 33 43;
    --status-info-base: 89 199 230;
    --status-info-border: 30 76 102;

    --status-warning-bg: 43 33 8;
    --status-warning-base: 255 184 77;
    --status-warning-border: 122 93 28;

    --status-error-bg: 42 21 21;
    --status-error-base: 255 107 107;
    --status-error-border: 122 46 46;
  }
}
```

### 3.2 Tailwind Theme Extension

```js
// tailwind.config.ts — theme.extend.colors
colors: {
  brand: {
    primary: 'rgb(var(--brand-primary) / <alpha-value>)',
    secondary: 'rgb(var(--brand-secondary) / <alpha-value>)',
    accent: 'rgb(var(--brand-accent) / <alpha-value>)',
    support: 'rgb(var(--brand-support) / <alpha-value>)',
  },
  surface: {
    bg: 'rgb(var(--surface-bg) / <alpha-value>)',
    card: 'rgb(var(--surface-card) / <alpha-value>)',
    elevated: 'rgb(var(--surface-elevated) / <alpha-value>)',
    hover: 'rgb(var(--surface-hover) / <alpha-value>)',
    selected: 'rgb(var(--surface-selected) / <alpha-value>)',
    border: 'rgb(var(--surface-border) / <alpha-value>)',
    selectedBorder: 'rgb(var(--surface-selected-border) / <alpha-value>)',
  },
  text: {
    primary: 'rgb(var(--text-primary) / <alpha-value>)',
    secondary: 'rgb(var(--text-secondary) / <alpha-value>)',
    disabled: 'rgb(var(--text-disabled) / <alpha-value>)',
    gold: 'rgb(var(--text-gold) / <alpha-value>)',
    selected: 'rgb(var(--text-selected) / <alpha-value>)',
  },
  focus: {
    ring: 'rgb(var(--focus-ring) / <alpha-value>)',
  },
  status: {
    success: {
      bg: 'rgb(var(--status-success-bg) / <alpha-value>)',
      base: 'rgb(var(--status-success-base) / <alpha-value>)',
      border: 'rgb(var(--status-success-border) / <alpha-value>)',
    },
    info: {
      bg: 'rgb(var(--status-info-bg) / <alpha-value>)',
      base: 'rgb(var(--status-info-base) / <alpha-value>)',
      border: 'rgb(var(--status-info-border) / <alpha-value>)',
    },
    warning: {
      bg: 'rgb(var(--status-warning-bg) / <alpha-value>)',
      base: 'rgb(var(--status-warning-base) / <alpha-value>)',
      border: 'rgb(var(--status-warning-border) / <alpha-value>)',
    },
    error: {
      bg: 'rgb(var(--status-error-bg) / <alpha-value>)',
      base: 'rgb(var(--status-error-base) / <alpha-value>)',
      border: 'rgb(var(--status-error-border) / <alpha-value>)',
    },
  },
}
```

### 3.3 Tailwind Radius / Motion / Shadow / Typography

```js
// tailwind.config.ts — theme.extend
borderRadius: {
  sm: '8px',
  md: '12px',
  lg: '16px',
  xl: '24px',
  pill: '999px',
},
transitionDuration: {
  fast: '150ms',
  standard: '250ms',
  slow: '400ms',
},
transitionTimingFunction: {
  DEFAULT: 'cubic-bezier(0.4, 0, 0.2, 1)',
},
boxShadow: {
  sm: '0 1px 3px rgba(0,0,0,0.08)',
  md: '0 4px 12px rgba(0,0,0,0.12)',
  lg: '0 8px 24px rgba(0,0,0,0.16)',
},
fontFamily: {
  heading: ['Baloo 2', 'system-ui', 'sans-serif'],
  body: ['Nunito', 'system-ui', 'sans-serif'],
},
fontSize: {
  h1: ['36px', { lineHeight: '1.2', fontWeight: '700' }],
  h2: ['28px', { lineHeight: '1.3', fontWeight: '700' }],
  h3: ['22px', { lineHeight: '1.4', fontWeight: '600' }],
  h4: ['18px', { lineHeight: '1.4', fontWeight: '600' }],
  body: ['16px', { lineHeight: '1.5', fontWeight: '400' }],
  small: ['14px', { lineHeight: '1.5', fontWeight: '400' }],
  caption: ['12px', { lineHeight: '1.5', fontWeight: '400' }],
},
```

---

## 4. Phase 1A Exit Criteria

Wave 1A is complete when:

- [ ] Semantic token names are locked
- [ ] Tailwind consumes semantic CSS variables only
- [ ] Light and dark mode values are finalized
- [ ] Figma variable collections mirror code naming
- [ ] No raw color usage is required in components

---

## Related Documents

- [Canonical UI Architecture](05-canonical-ui-architecture.md) — token architecture and component hierarchy
- [Figma Variables & Token JSON](07-figma-variables-token-json.md) — Figma variable collections and export JSON
- [Phase Execution Plan](08-phase-execution-plan.md) — delivery plan and work breakdown
