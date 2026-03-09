# Hero Section Selection Decision Record

**Date:** 2026-03-09
**Status:** Implemented
**Branch:** `claude/select-landing-hero-4ProI`

---

## Decision

Adopted a hybrid hero design combining:
- **Concept A's layout** — two-column (left text, right visual), headline, dual CTAs, trust pills
- **Concept C's visual identity** — ornate golden gallery frame around the M logo portal
- **Prototype's interactive story demo** — embedded branching story preview in the hero
- **Existing M logo + intro video** — integrated as a phased portal visual (video → static logo)

## Evaluation Criteria (Weighted Matrix)

| Dimension | Weight | Description |
|---|---|---|
| Narrative Immersion | 0.20 | Does the hero feel like entering a story? |
| Brand Uniqueness | 0.15 | Does it look distinctly "Mystira"? |
| Child-Friendly Tone | 0.15 | Inviting for ages 3-12 without being scary or generic? |
| Product Usability | 0.15 | Does a visitor understand what Mystira does? |
| Visual Hierarchy | 0.10 | Clear focal points, reading flow, CTA prominence? |
| Emotional Resonance | 0.10 | Does it evoke wonder, curiosity, discovery? |
| System Scalability | 0.10 | Does it use/extend the design system properly? |
| Engineering Simplicity | 0.05 | Implementation complexity vs. value? |

## Scoring Summary

| Variant | Total (weighted) |
|---|---|
| **Concept #6 (observatory + cards)** | **4.70** |
| **Hybrid A+C (adopted)** | **~4.80** |
| Concept #9 (portal orb) | 4.40 |
| Concept A (portal swirl) | 4.60 |
| Concept C (framed painting) | 4.05 |
| Concept B (crystal orb) | 4.00 |
| Current dev | 2.60 |
| Current prod | 1.85 |

## Why This Hybrid Wins

1. **Best headline across all concepts**: "Step Into Worlds Where Your Choices Matter" communicates Mystira's core mechanic directly.
2. **Golden frame = cosmic museum**: The ornate frame turns the M logo into an artifact on display, matching the brand guide's "magical observatory" personality.
3. **Interactive story demo**: The embedded branching preview lets visitors experience the product before signing up — the single most compelling conversion element.
4. **Trust pills**: Immediately answer "why should I care" for both kids and parents/facilitators.
5. **Two-column layout**: Separates "what is this" (left) from "wow, that's beautiful" (right), creating breathing room.
6. **Video → static transition**: First-time visitors get the cinematic intro; return visitors get the polished static hero instantly.

## What Changed

| Component | Before | After |
|---|---|---|
| Layout | Single centered column | Two-column grid (lg+) |
| Headline | "Step Into the Immersive Portal" | "Step Into Worlds Where Your Choices Matter" |
| Badge pill | "ANCIENT SECRETS AWAIT" | "NEW CHOICE-DRIVEN ADVENTURES" |
| CTAs | "Begin Your Journey" / "Explore The Lore" | "Start Adventure" / "Explore Worlds" |
| Trust pills | None | Choice-driven, Safe, Replayable |
| Portal visual | Centered M logo with SVG light tubes | Ornate gold-framed M logo with particles |
| Interactive demo | None | 6-node branching story preview |
| Feature cards | Bootstrap cards with FA icons | Glass-morphism cards with inline SVGs |
| Background | Flat gradient + 4 particles | Radial gradient + nebula blobs + star field |
| Scroll indicator | "SCROLL TO DISCOVER" | Removed (unnecessary) |

## New Components Created

| Component | File | Purpose |
|---|---|---|
| `HeroBadgePill` | `Components/HeroBadgePill.razor` | Animated badge pill with sparkle icon |
| `HeroTrustPills` | `Components/HeroTrustPills.razor` | Trust indicator pills (choice-driven, safe, replayable) |
| `HeroPortalVisual` | `Components/HeroPortalVisual.razor` | Ornate-framed M logo with video intro transition |
| `InteractiveStoryDemo` | `Components/InteractiveStoryDemo.razor` | Branching story preview state machine |

## Contributing Factors from Docs

- **Brand Guide Canvas**: "Cosmic museum", "magical observatory", "warm but not whimsical"
- **Unified Design Language**: Concept C identity + Concept A structure + Concept B atmosphere
- **Evaluation Matrix**: 8-dimension weighted scoring (narrative immersion highest at 0.20)
- **UI Concept Analysis**: Warned against "SaaS dashboard" feel (ruled out Concept #7)
- **Rarity System**: Gold frame maps to Moonbeam Gold (#F6C453) achievement accent
