# @mystira/design-tokens

Unified design tokens for the Mystira platform. Provides consistent colors, typography, spacing, and component styles across all frontend packages.

## Installation

```bash
npm install @mystira/design-tokens
```

## Usage

### TypeScript/JavaScript

```typescript
import { colors, spacing, tokens } from '@mystira/design-tokens';

// Use individual tokens
const primaryColor = colors.primary[700]; // #7c3aed

// Or use the grouped tokens object
const { fontFamily, fontSize } = tokens;
```

### CSS Variables

```css
@import '@mystira/design-tokens/css';

.button {
  background-color: var(--color-primary-700);
  border-radius: var(--radius-md);
  padding: var(--spacing-2) var(--spacing-4);
  font-family: var(--font-family-sans);
  transition: all var(--transition-fast);
}
```

### Tailwind CSS

```javascript
// tailwind.config.js
module.exports = {
  presets: [require('@mystira/design-tokens/tailwind')],
  // your customizations...
};
```

Then use in your components:

```html
<button class="bg-primary-700 hover:bg-primary-800 rounded-md px-4 py-2">
  Click me
</button>
```

## Token Categories

### Colors

- **Primary**: Purple gradient (`#7c3aed` - main brand color from Mystira App)
- **Neutral**: Gray scale for text and backgrounds
- **Semantic**: Success (green), Warning (amber), Danger (red), Info (blue)

### Typography

- **Font Families**: `sans` (Inter), `mono` (Fira Code), `display` (Inter)
- **Font Sizes**: xs through 6xl (12px to 60px)
- **Font Weights**: normal, medium, semibold, bold

### Spacing

11-point scale from 0 to 96 (0 to 24rem)

### Components

- **Border Radius**: sm, md, lg, xl, 2xl, full
- **Box Shadows**: sm, md, lg, xl, 2xl
- **Transitions**: fast (150ms), base (200ms), slow (300ms)
- **Z-Index**: Named values for common UI layers

## Migration Guide

### From Publisher's variables.css

Replace direct CSS custom property references:

```css
/* Before (Publisher) */
--color-primary-500: #9333ea;

/* After (design-tokens) */
@import '@mystira/design-tokens/css';
/* Uses --color-primary-700: #7c3aed (App color) */
```

### From Tailwind defaults

Add the preset to your config:

```javascript
// Before
module.exports = {
  theme: {
    extend: {
      colors: {
        primary: '#0ea5e9', // DevHub sky blue
      },
    },
  },
};

// After
module.exports = {
  presets: [require('@mystira/design-tokens/tailwind')],
  // primary is now the unified purple from Mystira App
};
```

## License

MIT
