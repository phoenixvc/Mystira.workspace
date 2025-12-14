# Mystira Admin UI

Admin frontend application for the Mystira platform. A modern single-page application (SPA) for content moderation, administrative workflows, and platform management.

## 🚧 Migration Status

**This repository is currently being set up as part of a migration from `Mystira.App`.**

The Admin UI is being extracted from the `Mystira.App` monorepo into this dedicated repository to enable:

- Independent deployment and versioning
- Separate development workflows
- Modern frontend stack without .NET/Blazor dependencies
- Better separation of concerns between admin tools and main application

## Overview

This is a modern SPA frontend that connects to the `Mystira.Admin.Api` backend service. The Admin API provides a pure REST/gRPC interface (no Razor Pages UI), allowing this frontend to be built with modern web technologies.

## Architecture

```
┌─────────────────┐
│  Admin UI (SPA) │  ← This repository
│  (React/Vue/etc)│
└────────┬────────┘
         │ REST/gRPC
         ▼
┌─────────────────┐
│  Admin API      │  ← Mystira.Admin.Api repository
│  (ASP.NET Core) │
└─────────────────┘
```

## Related Repositories

- **Mystira.Admin.Api**: Backend API service (REST/gRPC endpoints)
- **Mystira.App**: Source repository where Admin UI currently exists (being migrated from)
- **Mystira.workspace**: Unified workspace containing all Mystira components

## Migration Status

**Current Phase**: Phase 2 - Repository Setup (In Progress)

See [Migration Phases Documentation](../../docs/MIGRATION_PHASES.md) for detailed status and progress tracking.

### Migration Plan

1. ✅ Repository created
2. ⏳ Extract Admin UI code from `Mystira.App`
3. ⏳ Set up modern frontend stack (React/Vue/Next.js/etc)
4. ⏳ Configure API integration with `Mystira.Admin.Api`
5. ⏳ Set up CI/CD pipeline
6. ⏳ Deploy and verify functionality
7. ⏳ Remove Admin UI from `Mystira.App` monorepo

**Note**: Repository is currently empty (no commits). First commit will be made once code extraction begins.

## Setup

_Setup instructions will be added once the initial codebase is migrated._

## Development

_Development instructions will be added once the initial codebase is migrated._

## Contributing

This repository is in active migration. Once the initial migration is complete, contribution guidelines will be added.
