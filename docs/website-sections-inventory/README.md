# Mystira Website Sections Inventory

> Comprehensive documentation of every section across the Mystira platform.
> Last updated: 2026-03-07

## Overview

Mystira comprises multiple web applications, each serving a distinct audience and purpose. This inventory documents every page, section, component, and interaction pattern across the entire platform.

## Applications

| # | Application | Technology | Audience | Document |
|---|-------------|-----------|----------|----------|
| 1 | [Mystira App (PWA)](01-app-pwa.md) | Blazor WebAssembly | Children, parents, group leaders | Consumer-facing interactive storytelling platform |
| 2 | [Mystira Publisher](02-publisher.md) | React + TypeScript | Story creators, authors, illustrators | Creator-facing story registration and attribution platform |
| 3 | [Mystira Admin UI](03-admin-ui.md) | React + Bootstrap 5 | Platform administrators, content managers | Internal content management and moderation dashboard |
| 4 | [Mystira DevHub](04-devhub.md) | Tauri + React | Developers, technical contributors | Desktop developer portal and tooling |
| 5 | [Mystira Story Generator](05-story-generator.md) | Blazor WebAssembly | Story designers, AI operators | Story generation and AI agent management tool |

## Cross-Application Patterns

Shared UI patterns, states, and conventions used across all applications are documented in:

- [Cross-Application Shared Patterns](06-shared-patterns.md)

## Document Structure

Each application document follows a consistent format:

- **Application overview** — technology, routes, audience
- **Global components** — header, footer, navigation, overlays
- **Per-page breakdown** — each page and its sections documented with:
  - Primary purpose
  - Target audience
  - Key message
  - Current layout/structure
  - Call-to-action

## Related Documentation

- [UI Concepts SWOT Analysis](../design/01-ui-concepts-swot-analysis.md)
- [Design Direction Decision Matrix](../design/03-design-direction-decision-matrix.md)
- [Package Inventory](../analysis/package-inventory.md)
