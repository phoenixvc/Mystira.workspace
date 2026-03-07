# Mystira Figma Variables & Token JSON Specification

## Overview

This document defines the Figma variable collections, modes, styles, and the canonical JSON structure for token exchange between design and code.

---

## 1. Variable Collections

### Collection: Mystira Colors

**Modes:** Light, Dark

| Variable                      | Light       | Dark        |
|-------------------------------|-------------|-------------|
| `brand/primary`               | `#5B3CC4`   | `#5B3CC4`   |
| `brand/secondary`             | `#C7B8FF`   | `#C7B8FF`   |
| `brand/accent`                | `#F6C453`   | `#F6C453`   |
| `brand/support`               | `#4ED7C8`   | `#4ED7C8`   |
| `surface/background`          | `#F6F4FF`   | `#141127`   |
| `surface/card`                | `#ECE7FF`   | `#1E1A38`   |
| `surface/elevated`            | `#E8E2FF`   | `#26225A`   |
| `surface/hover`               | `#F1EDFF`   | `#23204A`   |
| `surface/selected`            | `#DDD6FF`   | `#2F2866`   |
| `surface/border`              | `#DED9FF`   | `#2B2750`   |
| `surface/selected-border`     | `#B8A9FF`   | `#3D3580`   |
| `text/primary`                | `#2E1F66`   | `#F8F7FF`   |
| `text/secondary`              | `#5C4FA3`   | `#B9B4E6`   |
| `text/disabled`               | `#A8A3D6`   | `#6F6A9A`   |
| `text/gold`                   | `#F6C453`   | `#F6C453`   |
| `text/selected`               | `#2E1F66`   | `#F8F7FF`   |
| `focus/ring`                  | `#5B3CC4`   | `#C7B8FF`   |
| `status/success/bg`           | `#E9F8EF`   | `#0F2A1C`   |
| `status/success/base`         | `#4CAF75`   | `#5EDC91`   |
| `status/success/border`       | `#B6E2C6`   | `#1E5C3C`   |
| `status/info/bg`              | `#EAF6FF`   | `#10212B`   |
| `status/info/base`            | `#46B8DC`   | `#59C7E6`   |
| `status/info/border`          | `#A7D3F2`   | `#1E4C66`   |
| `status/warning/bg`           | `#FFF6D9`   | `#2B2108`   |
| `status/warning/base`         | `#E59C00`   | `#FFB84D`   |
| `status/warning/border`       | `#F3D18A`   | `#7A5D1C`   |
| `status/error/bg`             | `#FFE9E9`   | `#2A1515`   |
| `status/error/base`           | `#D94B4B`   | `#FF6B6B`   |
| `status/error/border`         | `#F3B3B3`   | `#7A2E2E`   |

---

### Collection: Mystira Radius

**Modes:** Base

| Variable       | Value  |
|----------------|--------|
| `radius/sm`    | 8px    |
| `radius/md`    | 12px   |
| `radius/lg`    | 16px   |
| `radius/xl`    | 24px   |
| `radius/pill`  | 999px  |

---

### Collection: Mystira Motion

**Modes:** Base

| Variable          | Value                            |
|-------------------|----------------------------------|
| `motion/fast`     | 150ms                            |
| `motion/standard` | 250ms                            |
| `motion/slow`     | 400ms                            |
| `motion/ease`     | cubic-bezier(0.4, 0, 0.2, 1)    |

---

### Collection: Mystira Typography

**Modes:** Base

| Variable             | Value     |
|----------------------|-----------|
| `font/heading`       | Baloo 2   |
| `font/body`          | Nunito    |
| `type/h1/size`       | 36px      |
| `type/h1/weight`     | 700       |
| `type/h2/size`       | 28px      |
| `type/h2/weight`     | 700       |
| `type/h3/size`       | 22px      |
| `type/h3/weight`     | 600       |
| `type/h4/size`       | 18px      |
| `type/h4/weight`     | 600       |
| `type/body/size`     | 16px      |
| `type/body/weight`   | 400       |
| `type/small/size`    | 14px      |
| `type/caption/size`  | 12px      |

---

## 2. Suggested Figma Styles

### Color Styles

- Brand / Primary
- Brand / Secondary
- Brand / Accent
- Brand / Support
- Surface / Background
- Surface / Card
- Surface / Elevated
- Text / Primary
- Text / Secondary
- Status / Success
- Status / Info
- Status / Warning
- Status / Error

### Text Styles

- Display / Hero
- Heading / H1
- Heading / H2
- Heading / H3
- Heading / H4
- Body / Large
- Body / Default
- Body / Small
- Caption

### Effect Styles

- Shadow / Sm
- Shadow / Md
- Shadow / Lg
- Glow / Magical
- Glow / Reward

---

## 3. Figma-Ready Token JSON

```json
{
  "meta": {
    "brand": "Mystira",
    "version": "1.0.0",
    "generatedAt": "2026-03-07"
  },
  "collections": [
    {
      "name": "Mystira Colors",
      "modes": ["Light", "Dark"],
      "variables": {
        "brand/primary": { "Light": "#5B3CC4", "Dark": "#5B3CC4" },
        "brand/secondary": { "Light": "#C7B8FF", "Dark": "#C7B8FF" },
        "brand/accent": { "Light": "#F6C453", "Dark": "#F6C453" },
        "brand/support": { "Light": "#4ED7C8", "Dark": "#4ED7C8" },
        "surface/background": { "Light": "#F6F4FF", "Dark": "#141127" },
        "surface/card": { "Light": "#ECE7FF", "Dark": "#1E1A38" },
        "surface/elevated": { "Light": "#E8E2FF", "Dark": "#26225A" },
        "surface/hover": { "Light": "#F1EDFF", "Dark": "#23204A" },
        "surface/selected": { "Light": "#DDD6FF", "Dark": "#2F2866" },
        "surface/border": { "Light": "#DED9FF", "Dark": "#2B2750" },
        "surface/selected-border": { "Light": "#B8A9FF", "Dark": "#3D3580" },
        "text/primary": { "Light": "#2E1F66", "Dark": "#F8F7FF" },
        "text/secondary": { "Light": "#5C4FA3", "Dark": "#B9B4E6" },
        "text/disabled": { "Light": "#A8A3D6", "Dark": "#6F6A9A" },
        "text/gold": { "Light": "#F6C453", "Dark": "#F6C453" },
        "text/selected": { "Light": "#2E1F66", "Dark": "#F8F7FF" },
        "focus/ring": { "Light": "#5B3CC4", "Dark": "#C7B8FF" },
        "status/success/bg": { "Light": "#E9F8EF", "Dark": "#0F2A1C" },
        "status/success/base": { "Light": "#4CAF75", "Dark": "#5EDC91" },
        "status/success/border": { "Light": "#B6E2C6", "Dark": "#1E5C3C" },
        "status/info/bg": { "Light": "#EAF6FF", "Dark": "#10212B" },
        "status/info/base": { "Light": "#46B8DC", "Dark": "#59C7E6" },
        "status/info/border": { "Light": "#A7D3F2", "Dark": "#1E4C66" },
        "status/warning/bg": { "Light": "#FFF6D9", "Dark": "#2B2108" },
        "status/warning/base": { "Light": "#E59C00", "Dark": "#FFB84D" },
        "status/warning/border": { "Light": "#F3D18A", "Dark": "#7A5D1C" },
        "status/error/bg": { "Light": "#FFE9E9", "Dark": "#2A1515" },
        "status/error/base": { "Light": "#D94B4B", "Dark": "#FF6B6B" },
        "status/error/border": { "Light": "#F3B3B3", "Dark": "#7A2E2E" }
      }
    },
    {
      "name": "Mystira Radius",
      "modes": ["Base"],
      "variables": {
        "radius/sm": { "Base": "8px" },
        "radius/md": { "Base": "12px" },
        "radius/lg": { "Base": "16px" },
        "radius/xl": { "Base": "24px" },
        "radius/pill": { "Base": "999px" }
      }
    },
    {
      "name": "Mystira Motion",
      "modes": ["Base"],
      "variables": {
        "motion/fast": { "Base": "150ms" },
        "motion/standard": { "Base": "250ms" },
        "motion/slow": { "Base": "400ms" },
        "motion/ease": { "Base": "cubic-bezier(0.4, 0, 0.2, 1)" }
      }
    },
    {
      "name": "Mystira Typography",
      "modes": ["Base"],
      "variables": {
        "font/heading": { "Base": "Baloo 2" },
        "font/body": { "Base": "Nunito" },
        "type/h1/size": { "Base": "36px" },
        "type/h1/weight": { "Base": "700" },
        "type/h2/size": { "Base": "28px" },
        "type/h2/weight": { "Base": "700" },
        "type/h3/size": { "Base": "22px" },
        "type/h3/weight": { "Base": "600" },
        "type/h4/size": { "Base": "18px" },
        "type/h4/weight": { "Base": "600" },
        "type/body/size": { "Base": "16px" },
        "type/body/weight": { "Base": "400" },
        "type/small/size": { "Base": "14px" },
        "type/caption/size": { "Base": "12px" }
      }
    }
  ]
}
```

---

## 4. Token Naming Conventions

### Rules

1. Use `/` as the hierarchy separator (Figma convention)
2. Use lowercase for all token names
3. Group by semantic category first, then specificity
4. Brand tokens are mode-invariant (same in Light and Dark)
5. Surface and text tokens are mode-dependent
6. Status tokens always include three sub-tokens: `bg`, `base`, `border`

### Naming Pattern

```text
{category}/{subcategory}/{property}
```

Examples:
- `surface/background`
- `status/success/base`
- `type/h1/size`
- `motion/fast`

---

## Related Documents

- [Canonical UI Architecture](05-canonical-ui-architecture.md) — token architecture and component hierarchy
- [Phase 1A Design Tokens](06-phase-1a-design-tokens.md) — token values and Tailwind mapping
- [Phase Execution Plan](08-phase-execution-plan.md) — delivery plan and work breakdown
