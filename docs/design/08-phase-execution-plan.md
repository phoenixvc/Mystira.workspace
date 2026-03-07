# Mystira Design System — Phase Execution Plan

## Overview

The Mystira design system is delivered in two phases. Phase 1 locks system primitives. Phase 2 scales them into production tooling.

---

## Phase 1 — Foundation Stabilization

### Wave 1A: Token System

**Deliverables:**

- Canonical `tokens.ts`
- `theme.ts` with light/dark runtime mapping
- `tailwind.config.ts` wired to CSS variables
- `globals.css` semantic token layer
- Token naming conventions document

**Scope:** Brand, surfaces, text, status, spacing, radius, motion, shadows, typography

**Success Criteria:**

- No hardcoded hex in components
- Light/dark parity established
- Semantic classes available in Tailwind

### Wave 1A (continued): Figma Variables

**Deliverables:**

- Figma-ready token JSON
- Variable collections and modes
- Color / typography / spacing / radius / effect mapping
- Style naming conventions

**Success Criteria:**

- One-to-one mapping with code tokens
- No duplicate color definitions
- Figma and code share the same source structure

### Wave 1B: Core Component Library

**Deliverables:**

- Foundational Mystira primitives
- Artifact-first domain components

**Core Components (15):**

| Component         | Category     |
|-------------------|-------------|
| Button            | Primitive    |
| Card              | Primitive    |
| Badge             | Primitive    |
| Chip              | Primitive    |
| Input             | Primitive    |
| Tabs              | Primitive    |
| Navigation Item   | System       |
| Alert             | System       |
| Modal             | System       |
| Search Field      | System       |
| Filter Bar        | System       |
| Artifact Card     | Domain       |
| Lore Panel        | Domain       |
| Inspector Panel   | Domain       |
| Metric Badge      | Domain       |

**Success Criteria:**

- Token-driven only (no raw values)
- Usable in `/brand` page
- Storybook-ready

---

## Phase 2 — Productization & System Expansion

### Wave 2A: Full Mystira UI Kit Spec (30+ Components)

**Deliverables:**

- Full inventory with variants, states, behaviors, and usage rules

**Target Categories:**

- Navigation
- Content discovery
- Artifact presentation
- Lore exploration
- Progression / achievements
- Forms
- Overlays
- System feedback

### Wave 2B: Production Tailwind Config

**Deliverables:**

- Finalized Tailwind theme extension
- Plugin strategy (if needed)
- Motion utilities
- Elevation utilities
- Rarity / artifact utility patterns

Separate from Phase 1 because Phase 1 defines tokens; this step hardens production ergonomics.

### Wave 2C: Live `/brand` Design System Page

**Deliverables:**

- Real Mystira brand route
- Dark/light toggle
- Token preview
- Typography showcase
- Component gallery
- Status demo
- Motion demo
- Artifact UI section

**Purpose:**

- Internal source of truth
- Design review surface
- Engineering reference
- Investor / partner showcase

### Wave 2D: Storybook Architecture for Mystira PWA

**Deliverables:**

- Story structure
- Theme switching
- Token docs
- Component stories
- Interaction stories
- Visual regression hooks

**Purpose:**

- Component-level verification
- Designer / engineer collaboration
- Baseline for drift detection

---

## Execution Order

### Wave 1 (Foundation)

1. Tailwind design tokens
2. Figma variables + styles
3. Core component library

### Wave 2 (Productization)

1. Full UI kit spec
2. Production Tailwind hardening
3. `/brand` live page
4. Storybook architecture

---

## Concrete Work Breakdown

### Wave 1A — Token System

- Finalize Mystira canonical token map
- Generate Tailwind token config
- Generate Figma token JSON

### Wave 1B — Core Components

- Build primitives (Button, Card, Badge, Chip, Input, Tabs)
- Build system components (NavigationItem, Alert, Modal, SearchField, FilterBar)
- Build domain components (ArtifactCard, LorePanel, InspectorPanel, MetricBadge)
- Validate dark/light states

### Wave 2A — System Surfaces

- Build `/brand` page
- Set up Storybook shell
- Create interaction demos

### Wave 2B — Expansion

- Full UI kit inventory
- Visual regression coverage
- Refinement rules

---

## Output Package Per Wave

### After Wave 1:

- `tokens.ts`
- `theme.ts`
- `tailwind.config.ts`
- `globals.css`
- `figma.tokens.json`
- Core Mystira components

### After Wave 2:

- `/brand` route
- Storybook
- Expanded component catalog
- Production-grade design system docs

---

## Why This Split Is Correct

**Phase 1 defines truth. Phase 2 defines distribution and scale.**

If you start with `/brand` or Storybook before tokens and core components are stable, you'll document churn instead of a system.

---

## Recommended Execution Sequence

```
tokens → figma variables → core component library → full UI kit → production Tailwind hardening → /brand → storybook
```
