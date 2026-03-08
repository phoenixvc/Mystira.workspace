# Analysis Documentation

This directory contains analysis documents for repository structure, extraction decisions, architectural evaluations, and UI/UX quality assessments.
- [Repository Extraction Analysis](./repository-extraction-analysis.md) - Analysis of all repositories and extraction recommendations

## UI/UX Analysis Suite

Comprehensive evaluation of Mystira's user interface across both applications (PWA + Publisher). These documents form a layered analysis system — start with the research foundations, then read the evaluation and aesthetic analysis for actionable findings.

### Reading Order

| # | Document | Purpose | Start Here If... |
|---|----------|---------|-------------------|
| 1 | [UI Design Metrics Research](./ui-design-metrics-research.md) | Frameworks for measuring "looks good / feels good" — VisAWI, Hassenzahl, UEQ, Nielsen heuristics, Norman's emotional design, Kano model, HEART framework. Establishes the composite evaluation model and scoring system. | You want to understand the theoretical basis for how UI quality is measured |
| 2 | [UI Metrics Evaluation](./ui-metrics-evaluation.md) | Section-by-section behavioral/quantitative metrics for assessing UI suitability. Click rates, completion funnels, load times, error rates — with acceptable ranges and measurement methods. Includes P0-P3 priority matrix. | You want to instrument the app with measurable KPIs |
| 3 | [Metrics Discussion](./ui-metrics-discussion.md) | Meta-analysis of why specific metrics were chosen. Trade-offs, known gaps, metric relationships, implementation recommendations. Explains the methodology behind document #2. | You want to understand the reasoning behind metric selection |
| 4 | [Aesthetic & Experiential Analysis](./ui-aesthetic-analysis.md) | Section-by-section scoring of visual design, experiential quality, interaction quality, emotional impact, and domain fitness. Each section scored 1-5 across weighted dimensions. Includes composite scores and prioritized improvement recommendations. | You want to know which sections look/feel good and which need work |

### Key Findings at a Glance

- **Platform average aesthetic score: 3.2/5.0** (Adequate — functional but not exceptional)
- **Strongest sections:** Brand Style Guide (4.0), Hero Section (3.7), Game Session (3.7)
- **Weakest sections:** About page (2.4), Awards page (2.8), Feature Cards (2.9)
- **Biggest single issue:** The brand design system (`brand.css`) is not used by the main application — two separate token systems exist with different primary colors
- **Highest-leverage improvement:** Migrate the main app to use the brand design tokens. This would lift every section's visual score by 0.3-0.5 points
- **Telemetry gap:** Application Insights infrastructure exists via `ITelemetryService` but only tracks 1 event. ~80 recommended metrics need instrumentation

---

## Architecture & Migration Analysis


- [App Components Extraction](./app-components-extraction.md) - Analysis of Admin API/Public API extraction decision
- [Monorepo Migration Parity Matrix](./monorepo-migration-parity-matrix.md) - Legacy repo to monorepo artifact parity snapshot and recovery gaps
- [Legacy Workflow Mapping](./legacy-workflow-mapping.md) - Workflow responsibility mapping from legacy repos to monorepo workflows
- [Monorepo Deep Parity Report](./monorepo-deep-parity-report.md) - Recursive file, behavioral, and nested config parity findings
- [Monorepo Parity Issue Register](./monorepo-parity-issue-register.md) - Decision register for unresolved parity concerns and intentional drifts
- [Legacy Doc Migration Log](./legacy-doc-migration-log.md) - Source-to-target mapping for migrated legacy documentation artifacts
- [Package Inventory](./package-inventory.md) - Package and dependency inventory

## Related Documentation

- [Migration Plans](../migration/) - Detailed migration plans based on analysis
- [ADR-0006: Admin API Repository Extraction](../architecture/adr/0006-admin-api-repository-extraction.md) - Architecture decision record

## Archived Snapshot Note

Archived analysis snapshots were removed from the working tree to reduce repository bloat.
If historical snapshot artifacts are needed, retrieve them from git history.
