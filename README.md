# Mystira Admin API

Admin API backend for the Mystira platform. This is a pure REST/gRPC API (no Razor Pages UI).

## Overview

This repository contains the Admin API extracted from the `Mystira.App` monorepo. The Admin UI has been separated into the `Mystira.Admin.UI` repository.

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

## Related Repositories

- **Mystira.App**: Source of shared libraries (published as NuGet packages)
- **Mystira.Admin.UI**: Admin frontend application
