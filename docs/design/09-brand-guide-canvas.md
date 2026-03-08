# Mystira Brand Guide Canvas

## Brand Identity

### Brand Name
**Mystira**

### Brand Essence
A story-driven discovery platform where magical artifacts, cosmic lore, and enchanted worlds come alive.

### Brand Promise
Every interaction feels like uncovering a secret in a magical archive.

---

## Visual Identity

### Color Palette

#### Primary Brand Colors

| Name                | Hex       | Usage                              |
|---------------------|-----------|-------------------------------------|
| Twilight Amethyst   | `#5B3CC4` | Primary brand, CTAs, active states  |
| Starlight Lavender  | `#C7B8FF` | Secondary, highlights, soft accents |
| Moonbeam Gold       | `#F6C453` | Rewards, rare items, achievements   |
| Dream Teal          | `#4ED7C8` | Support, energy, discovery moments  |

#### Surface Palette (Light)

| Name        | Hex       | Usage              |
|-------------|-----------|---------------------|
| Cloud       | `#F6F4FF` | Page background     |
| Mist        | `#ECE7FF` | Card surfaces       |
| Haze        | `#E8E2FF` | Elevated panels     |
| Whisper     | `#F1EDFF` | Hover states        |
| Shimmer     | `#DDD6FF` | Selected states     |

#### Surface Palette (Dark)

| Name        | Hex       | Usage              |
|-------------|-----------|---------------------|
| Void        | `#141127` | Page background     |
| Abyss       | `#1E1A38` | Card surfaces       |
| Nebula      | `#26225A` | Elevated panels     |
| Eclipse     | `#23204A` | Hover states        |
| Rift        | `#2F2866` | Selected states     |

---

## Typography

### Font Families

| Role     | Font     | Fallback          |
|----------|----------|--------------------|
| Headings | Baloo 2  | system-ui, sans    |
| Body     | Nunito   | system-ui, sans    |

### Type Scale

| Style   | Size | Weight | Usage                |
|---------|------|--------|----------------------|
| H1      | 36px | 700    | Hero / page titles   |
| H2      | 28px | 700    | Section headings     |
| H3      | 22px | 600    | Subsection headings  |
| H4      | 18px | 600    | Card headings        |
| Body    | 16px | 400    | Default text         |
| Small   | 14px | 400    | Secondary text       |
| Caption | 12px | 400    | Labels, metadata     |

---

## Brand Voice

### Tone Attributes

- **Magical** — not whimsical, not dark; enchanted
- **Warm** — inviting for all ages, especially children
- **Premium** — crafted, not cheap
- **Story-driven** — every element hints at narrative
- **Discoverable** — rewards exploration and curiosity

### Voice Do's

- Use language of discovery: "uncover", "reveal", "explore"
- Reference lore, artifacts, and worlds naturally
- Keep descriptions concise but evocative
- Make achievements feel earned and meaningful

### Voice Don'ts

- Don't use corporate SaaS language
- Don't use crypto/NFT terminology
- Don't use dark or scary imagery
- Don't be condescending to younger users

---

## UI Personality

### Interfaces Should Feel Like

- A cosmic museum
- A storybook archive
- A magical observatory
- An enchanted discovery interface

### Interfaces Should Never Feel Like

- A fintech dashboard
- An NFT marketplace
- A developer console
- A generic SaaS product

---

## Iconography

### Style Guidelines

- Rounded, friendly shapes
- Consistent stroke weight
- Magical / cosmic motifs preferred
- Avoid overly technical or sharp styles

### Icon Categories

- Navigation (compass, map, scroll)
- Artifacts (crystal, amulet, tome)
- Actions (discover, collect, unlock)
- Status (energy, rarity, achievement)

---

## Motion & Animation

### Principles

- Motion should feel **enchanted**, not mechanical
- Transitions suggest discovery, not loading
- Hover effects create a sense of life and magic
- Particle effects used sparingly for high-value moments

### Motion Tokens

| Token             | Duration | Usage                     |
|-------------------|----------|---------------------------|
| `motion.fast`     | 150ms    | Micro-interactions        |
| `motion.standard` | 250ms    | Standard transitions      |
| `motion.slow`     | 400ms    | Emphasis / reveal moments |

### Easing

Default: `cubic-bezier(0.4, 0, 0.2, 1)` — smooth, natural deceleration.

---

## Spatial System

### Radius

| Token        | Value  | Usage                      |
|--------------|--------|----------------------------|
| `radius.sm`  | 8px    | Buttons, chips             |
| `radius.md`  | 12px   | Cards, inputs              |
| `radius.lg`  | 16px   | Panels, modals             |
| `radius.xl`  | 24px   | Hero cards, feature blocks |
| `radius.pill`| 999px  | Tags, pill buttons         |

### Elevation

| Level  | Usage                          |
|--------|--------------------------------|
| Small  | Subtle card lift               |
| Medium | Panels, dropdowns              |
| Large  | Modals, overlays               |

---

## Rarity System _(Future Scope)_

> **Note:** The rarity visual treatments described below do not yet have corresponding design tokens in the token system (docs 06/07). Rarity-specific tokens will be defined in a future phase when these treatments are implemented.

Artifacts in Mystira have rarity tiers that affect visual treatment:

| Tier       | Visual Treatment                              |
|------------|-----------------------------------------------|
| Common     | Standard card, no glow                        |
| Uncommon   | Subtle border shimmer                         |
| Rare       | Soft ambient glow                             |
| Epic       | Gold accent border + particle trail           |
| Legendary  | Full gold glow + animated particle effect     |

---

## Application

### Primary Product (Concept C)

The core Mystira experience uses the full brand language: cosmic surfaces, magical motion, artifact-first design, and narrative immersion.

### Admin / Creator Tools (Concept A)

Internal tools adopt the structural patterns (grids, navigation, filters) with a simplified visual treatment. Functional clarity over atmosphere.

### Premium Moments (Concept B)

Achievement reveals, vault screens, and rare artifact displays use the gold cosmic accent layer for heightened premium feeling.

---

## Related Documents

- [Unified Design Language](04-unified-design-language.md) — combined design language from all three concepts
- [Canonical UI Architecture](05-canonical-ui-architecture.md) — layout, tokens, and component hierarchy
- [Phase 1A Design Tokens](06-phase-1a-design-tokens.md) — token values and Tailwind mapping
