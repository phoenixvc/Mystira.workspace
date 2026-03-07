# Mystira Canonical UI Architecture

## Overview

This document defines the recommended production UI architecture for Mystira, including layout structure, token architecture, component hierarchy, and interaction patterns.

---

## 1. Layout Architecture

### Primary App Layout

```
AppShell
├── Sidebar Navigation
├── Top Command Bar
├── Content Canvas
│     ├── Page Header
│     ├── Filter / Search Controls
│     ├── Artifact Grid
│     └── Lore Panels
└── Inspector Panel
```

---

### Layout Regions

#### Left Sidebar

**Purpose:** World navigation, vault sections

**Components:**

- `UserIdentityCard`
- `NavigationGroup`
- `VaultStatus`
- `PrimaryAction`

#### Top Command Bar

**Purpose:** Search, filters, notifications

**Components:**

- `SearchBar`
- `FilterChips`
- `Alerts`
- `UserMenu`

#### Content Canvas

**Purpose:** Primary narrative space

**Components:**

- `SectionHeader`
- `ArtifactGrid`
- `StoryCards`
- `DiscoveryModules`

#### Inspector Panel

**Purpose:** Artifact storytelling

**Components:**

- `ArtifactPreview`
- `LoreDescription`
- `ProvenanceDetails`
- `EnergyMetrics`
- `PrimaryAction`

---

## 2. Token Architecture

### Color Tokens

#### Brand

| Token            | Purpose              |
|------------------|----------------------|
| `brand.primary`  | Twilight Amethyst    |
| `brand.secondary`| Starlight Lavender   |
| `brand.accent`   | Moonbeam Gold        |
| `brand.support`  | Dream Teal           |

#### Surface

| Token                    | Purpose              |
|--------------------------|----------------------|
| `surface.background`     | Page background      |
| `surface.card`           | Card surfaces        |
| `surface.elevated`       | Elevated panels      |
| `surface.hover`          | Hover states         |
| `surface.selected`       | Selected items       |
| `surface.border`         | Borders              |
| `surface.selectedBorder` | Selected borders     |

#### Text

| Token            | Purpose              |
|------------------|----------------------|
| `text.primary`   | Main content         |
| `text.secondary` | Supporting text      |
| `text.muted`     | De-emphasized        |
| `text.gold`      | Special highlights   |

#### Status

| Token                        | Purpose              |
|------------------------------|----------------------|
| `status.success.background`  | Success bg           |
| `status.success.base`        | Success primary      |
| `status.success.border`      | Success border       |
| `status.info.background`     | Info bg              |
| `status.info.base`           | Info primary         |
| `status.info.border`         | Info border          |
| `status.warning.background`  | Warning bg           |
| `status.warning.base`        | Warning primary      |
| `status.warning.border`      | Warning border       |
| `status.error.background`    | Error bg             |
| `status.error.base`          | Error primary        |
| `status.error.border`        | Error border         |

### Radius Tokens

| Token        | Value  |
|--------------|--------|
| `radius.sm`  | 8px    |
| `radius.md`  | 12px   |
| `radius.lg`  | 16px   |
| `radius.xl`  | 24px   |
| `radius.pill`| 999px  |

### Elevation Tokens

| Token            | Purpose              |
|------------------|----------------------|
| `elevation.card` | Card shadow          |
| `elevation.panel`| Panel shadow         |
| `elevation.modal`| Modal shadow         |

### Motion Tokens

| Token             | Value  |
|-------------------|--------|
| `motion.fast`     | 150ms  |
| `motion.base`     | 250ms  |
| `motion.emphasis` | 400ms  |

---

## 3. Component Architecture

### Primitive Components

| Component | Purpose                    |
|-----------|----------------------------|
| `Button`  | Actions and CTAs           |
| `Card`    | Content containers         |
| `Badge`   | Status and counts          |
| `Chip`    | Tags and filters           |
| `Input`   | Text entry                 |
| `Tabs`    | Section switching          |
| `Avatar`  | User / entity identity     |
| `Icon`    | Iconography                |

### System Components

| Component        | Purpose                        |
|------------------|--------------------------------|
| `NavigationItem` | Sidebar / nav entries          |
| `ArtifactCard`   | Artifact display card          |
| `LorePanel`      | Lore description panel         |
| `StatusBanner`   | System status messages         |
| `SearchField`    | Search input with suggestions  |
| `FilterBar`      | Filter controls                |
| `MetricBadge`    | Numeric / status indicators    |

### Domain Components (Mystira-specific)

| Component              | Purpose                           |
|------------------------|-----------------------------------|
| `ArtifactSpecimen`     | Full artifact display             |
| `EnergyOutputPanel`    | Energy / power metrics            |
| `ProvenanceRegistry`   | Artifact origin tracking          |
| `DiscoveryLocation`    | Location-based discovery          |
| `AchievementBadge`     | Achievement display               |
| `StoryChoicePanel`     | Narrative branching UI            |

---

## 4. Artifact Card Pattern

Standard artifact card layout:

```
ArtifactCard
├── RarityBadge
├── ArtifactVisual
├── ArtifactName
├── ArtifactLoreSnippet
├── MetricsRow
└── PrimaryAction
```

---

## 5. Interaction Model

### Hover

- Card lift
- Soft glow
- Rarity highlight

### Selection

- Border accent
- Artifact glow
- Panel open

### Discovery

- Particle shimmer
- Subtle pulse
