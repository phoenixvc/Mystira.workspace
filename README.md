# Mystira Admin API

Admin API backend for the Mystira platform. This is a pure REST/gRPC API (no Razor Pages UI).

## ✅ Migration Status

**This repository was extracted from `Mystira.App` as part of the admin tooling separation.**

**Status**: ✅ **COMPLETE** - Admin API extraction is finished and operational.

See [Migration Phases Documentation](../../docs/MIGRATION_PHASES.md) for overall migration status.

The Admin API was separated from the `Mystira.App` monorepo to enable:
- Independent deployment and versioning
- Separate development workflows
- Pure API service without UI dependencies
- Better separation of concerns between admin tools and main application
- Support for modern frontend frameworks (React/Vue/etc) via REST/gRPC

## Overview

This repository contains the Admin API extracted from the `Mystira.App` monorepo. The Admin UI has been separated into the `Mystira.Admin.UI` repository, allowing the frontend to be built with modern web technologies while this API provides a clean REST/gRPC interface.

## Dependencies

This project depends on NuGet packages published from `Mystira.App`:

- `Mystira.App.Domain` (1.0.0)
- `Mystira.App.Application` (1.0.0)
- `Mystira.App.Contracts` (1.0.0)
- `Mystira.App.Infrastructure.Azure` (1.0.0)
- `Mystira.App.Infrastructure.Data` (1.0.0)
- `Mystira.App.Infrastructure.Discord` (1.0.0)
- `Mystira.App.Infrastructure.StoryProtocol` (1.0.0)
- `Mystira.App.Shared` (1.0.0)

## Setup

1. Configure NuGet feed in `NuGet.config` (update Azure DevOps org/project/feed name)
2. Restore packages: `dotnet restore`
3. Build: `dotnet build`
4. Run: `dotnet run --project src/Mystira.App.Admin.Api`

## CORS Configuration

The API is configured to accept requests from `Mystira.Admin.UI`. Update CORS settings in `Program.cs` if the Admin UI URL changes.

## Architecture

```
┌─────────────────┐
│  Admin UI (SPA) │  ← Mystira.Admin.UI repository
│  (React/Vue/etc)│
└────────┬────────┘
         │ REST/gRPC
         ▼
┌─────────────────┐
│  Admin API      │  ← This repository
│  (ASP.NET Core) │
└────────┬────────┘
         │ NuGet packages
         ▼
┌─────────────────┐
│  Mystira.App    │  ← Source of shared libraries
│  (Domain/Infra) │
└─────────────────┘
```

## Related Repositories

- **Mystira.App**: Source of shared libraries (published as NuGet packages)
- **Mystira.Admin.UI**: Admin frontend application (modern SPA)
- **Mystira.workspace**: Unified workspace containing all Mystira components
