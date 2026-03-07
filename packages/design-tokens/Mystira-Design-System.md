# Mystira Unified Design Language

This document serves as the canonical reference for the Mystira application's design system, merging the structured layout of SaaS applications with the premium, lore-rich atmosphere of a cosmic fantasy world.

## Design Direction Decision Matrix

Mystira's visual language was determined through a weighted matrix scoring Narrative storytelling, Magical atmosphere, Child-friendly clarity, Usability, and Scalability.

The resulting **Unified Mystira Design Language** is a combination of three concepts:
**Structure (A) + Atmosphere (B) + Story Identity (C) = Mystira System**

| Element        | Style                                                             |
| :------------- | :---------------------------------------------------------------- |
| **Base UI**    | Clean, structured layout (predictable patterns, card grids, tabs) |
| **Surface**    | Dark magical gradients (deep space, nebulas)                      |
| **Accents**    | Gold + Teal energy (for rarity and interactive states)            |
| **Artifacts**  | Glowing focal objects (highly premium artifact feeling)           |
| **Typography** | Elegant serif headings + highly readable sans-serif body          |

## Canonical Mystira UI Architecture

### Layout Regions

- **Left Sidebar:** Navigational hub for world traversal (UserIdentityCard, VaultStatus).
- **Top Command Bar:** Utility space (SearchBar, FilterChips, Alerts).
- **Content Canvas:** The primary narrative space (Artifact Grid, StoryCards, DiscoveryModules).
- **Inspector Panel:** Deep-dive storytelling space (ArtifactPreview, LoreDescription, ProvenanceDetails, EnergyMetrics).

### Token Architecture

**Colors**

- **Brand:** `amethyst`, `lavender`, `gold`, `teal`
- **Surface:** `background`, `panel`, `card`, `overlay` (dark magical themes)
- **Text:** `primary`, `secondary`, `muted`, `gold`

**Scale & Elevation**

- **Radius:** `sm` (8px), `md` (16px), `lg` (24px), `pill` (999px)
- **Elevation:** `card`, `panel`, `modal` (utilizing glow and shadow)

**Motion**

- `fast` (150ms), `base` (250ms), `emphasis` (400ms)

### Component Architecture

- **Primitive:** Button, Card, Badge, Chip, Input, Tabs.
- **System:** NavigationItem, ArtifactCard, LorePanel, StatusBanner.
- **Domain (Mystira-specific):** ArtifactSpecimen, EnergyOutputPanel, ProvenanceRegistry, StoryChoicePanel.

### The Artifact Card Pattern

1. RarityBadge (Top Left/Right)
2. ArtifactVisual (Centered, glowing)
3. ArtifactName (Serif, elegant)
4. ArtifactLoreSnippet (Muted text)
5. MetricsRow (Teal/Gold accents)
6. PrimaryAction

**Interaction Model**

- **Hover:** Card lift, soft glow, rarity highlight.
- **Selection:** Border accent, panel open.
- **Discovery:** Particle shimmer, subtle pulse.

> **Philosophical Goal:** Mystira interfaces should feel like a cosmic museum, a storybook archive, or a magical observatory. It should _never_ feel like a fintech dashboard, an NFT marketplace, or a developer console.
