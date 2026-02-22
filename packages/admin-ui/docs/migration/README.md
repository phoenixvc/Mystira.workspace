# Migration Documentation

This directory contains migration documentation for the Mystira Admin UI project.

## Contents

- [Migration Phases](./phases.md) - Detailed migration progress tracking
- [Migration Strategy](./strategy.md) - Technical approach and decisions
- [Contracts Migration](./contracts-migration.md) - Guide for migrating to unified contracts packages
- [Admin UI Migration](./admin-ui-migration.md) - Guide for shared packages, Node.js 22, and dark mode

## Migration Overview

The Admin UI is being migrated from the `Mystira.App` monorepo into an independent repository to enable:

- Independent deployment and versioning
- Modern frontend stack (React instead of Razor Pages)
- Better separation of concerns
- Improved developer experience

## Current Status

**Phase 3 (Code Migration): ~98% Complete**

See [phases.md](./phases.md) for detailed progress.

## Architecture

### Before Migration

```
Mystira.App/
├── src/Mystira.App.Admin.Api/Views/   ← Razor Pages (migrated from)
├── src/Mystira.App.Admin.Api/wwwroot/ ← Static assets
└── [Shared libraries]
```

### After Migration

```
Mystira.Admin.UI/          ← Modern React SPA (this repo)
Mystira.Admin.Api/         ← Pure REST API
Mystira.App/               ← Main app (Admin code to be removed)
```

## Related Documentation

- [Implementation Roadmap](../planning/implementation-roadmap.md)
- [Testing Checklist](../operations/TESTING_CHECKLIST.md)
- [COMPLETION_STATUS.md](../../COMPLETION_STATUS.md)
