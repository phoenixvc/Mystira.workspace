# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**Mystira** — AI-powered interactive storytelling platform combining blockchain, generative AI, and immersive narratives. Unified monorepo workspace.

## Tech Stack

- **Backend**: .NET (Mystira.sln, Directory.Build.props for shared build config)
- **Infrastructure**: Atlantis (atlantis.yaml), Codecov
- **Apps**: Multiple apps in `apps/`
- **Config**: Shared configs in `configs/`

## Key Commands

```bash
dotnet build Mystira.sln  # Build entire solution
dotnet test               # Run all tests
dotnet format             # Format code
```

## Architecture

- `Mystira.sln` — Root solution file
- `Directory.Build.props` — Shared MSBuild properties across all projects
- `apps/` — Individual application projects
- `configs/` — Shared configuration files
- `atlantis.yaml` — Terraform automation via Atlantis

## AgentKit Forge

This project has not yet been onboarded to [AgentKit Forge](https://github.com/phoenixvc/agentkit-forge). To request onboarding, [create a ticket](https://github.com/phoenixvc/agentkit-forge/issues/new?title=Onboard+Mystira.workspace&labels=onboarding).
