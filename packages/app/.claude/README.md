# Claude Code Configuration for Mystira.App

This directory contains the Claude Code configuration for the Mystira Application Suite, including custom slash commands, hooks, and project settings.

## Quick Reference

| What             | Where                             | How to Use                          |
| ---------------- | --------------------------------- | ----------------------------------- |
| Slash Commands   | `.claude/commands/*.md`           | Type `/command-name` in Claude Code |
| Hooks            | `.claude/settings.json` → `hooks` | Run automatically on events         |
| MCP Servers      | `.mcp/config.json` → `mcpServers` | Available to MCP-compatible tools   |
| MCP Prompts      | `.mcp/config.json` → `prompts`    | Reusable prompt templates           |
| Project Settings | `.claude/settings.json`           | Auto-loaded by Claude Code          |
| Setup Script     | `.claude/install_tools.sh`        | Run manually for environment setup  |

---

## Custom Slash Commands

Invoke these by typing the command name in Claude Code. Each command is a markdown prompt template in `.claude/commands/`.

### `/create-endpoint`

Scaffolds a complete API endpoint across all architectural layers.

**Usage:**

```
/create-endpoint GameSession Create
/create-endpoint UserProfile Update --admin
```

**What it generates:**

1. Request/Response DTOs in `Mystira.Contracts.App/`
2. CQRS Command/Query + Handler in `Mystira.App.Application/`
3. Controller method in the appropriate API project
4. DI registration if needed

**Why:** Prevents the 80+ architectural violations documented in PERF-4 by ensuring every endpoint follows the hexagonal architecture pattern from the start.

---

### `/create-use-case`

Generates a CQRS Command or Query with Handler in the Application layer.

**Usage:**

```
/create-use-case CreateGameSession --description "Creates a new game session for a user"
/create-use-case GetUserBadges --type query
```

**What it generates:**

1. Command or Query record (auto-detected from name prefix)
2. Handler class with repository injection
3. Verifies/creates required port interfaces

**Why:** Enforces consistent CQRS patterns with Wolverine's static handler convention and ensures handlers only depend on port interfaces, never Infrastructure.

---

### `/add-tests`

Generates xUnit tests for a source file or feature.

**Usage:**

```
/add-tests src/Mystira.App.Application/CQRS/GameSessions/Commands/CreateGameSessionCommandHandler.cs
/add-tests GameSession --unit
/add-tests Badges
```

**What it generates:**

- Test class with Arrange/Act/Assert pattern
- Uses FluentAssertions + Moq
- Follows `MethodName_Scenario_ExpectedResult` naming
- Happy path, null input, not-found, and edge case tests

**Why:** Coverage is ~4.3%. This command makes it trivial to add well-structured tests following project conventions.

---

### `/architecture-check`

Scans for hexagonal architecture violations.

**Usage:**

```
/architecture-check src/Mystira.App.Api/
/architecture-check --full
```

**What it checks:**

- No business logic in Controllers
- No services in API layer
- Application layer has no Infrastructure references
- Domain layer has zero dependencies
- DTOs only in Contracts project
- Correct API routing (`/api` vs `/adminapi`)

**Output:** Compliance report with violations categorized by severity (CRITICAL, MAJOR, MINOR).

---

### `/coppa-audit`

Audits code for COPPA compliance -- critical for a children's platform.

**Usage:**

```
/coppa-audit registration
/coppa-audit src/Mystira.App.Api/Controllers/UserProfilesController.cs
/coppa-audit --full
```

**What it checks:**

- PII collection without consent
- Missing age verification
- Data retention without TTL
- PII in logs (BUG-4)
- Third-party data sharing
- Missing parental access controls

**Why:** COPPA violations carry $53,088 per violation (FTC adjustment effective Jan 17, 2025). This is a launch blocker documented in FEAT-INC-1.

---

### `/refactor-program-cs`

Extracts inline service registrations from Program.cs into extension methods.

**Usage:**

```
/refactor-program-cs --analyze
/refactor-program-cs authentication
```

**What it does:**

- Identifies inline registrations not yet extracted
- Creates extension methods in `src/Mystira.App.Api/Configuration/`
- Follows the established `Add{Feature}` / `Use{Feature}` pattern

---

## Hooks

Hooks run automatically in response to Claude Code events. Configured in `.claude/settings.json`.

### PreToolUse: Architecture Reminder

**Trigger:** Before any `Edit` or `Write` tool call

**What it does:** Prints a reminder to verify hexagonal architecture compliance before modifying files. This is a lightweight guardrail that prompts Claude to consider layer dependencies before every edit.

### PostToolUse: Auto-Format

**Trigger:** After any `Edit` or `Write` tool call on `.cs` files

**What it does:** Automatically runs `dotnet format` on the modified file to enforce code style from `.editorconfig`. This mirrors the Husky pre-commit hook but catches style drift immediately rather than at commit time.

### Notification

**Trigger:** On any Claude Code notification event

**What it does:** Echoes notifications to the terminal for visibility.

---

## MCP Servers

Configured in `.mcp/config.json`. These provide additional capabilities to MCP-compatible AI tools.

### Existing Servers (6)

| Server                  | Package                                            | Purpose                                     |
| ----------------------- | -------------------------------------------------- | ------------------------------------------- |
| **filesystem**          | `@modelcontextprotocol/server-filesystem`          | Repository file and directory access        |
| **github**              | `@modelcontextprotocol/server-github`              | GitHub operations, issues, PRs              |
| **git**                 | `@modelcontextprotocol/server-git`                 | Version control operations                  |
| **playwright**          | `@playwright/mcp`                                  | Browser automation for PWA testing          |
| **memory**              | `@modelcontextprotocol/server-memory`              | Knowledge graph persistence across sessions |
| **sequential-thinking** | `@modelcontextprotocol/server-sequential-thinking` | Structured reasoning with reflection        |

### New Servers (2)

| Server     | Package                                  | Purpose                                                                                               |
| ---------- | ---------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| **fetch**  | `mcp-server-fetch` (via `uvx`)           | Fetch external web content -- .NET docs, Azure API reference, NuGet package info                      |
| **docker** | `mcp/docker` (official Docker MCP image) | Manage Docker containers for local dev (Cosmos DB emulator, API containers from `docker-compose.yml`) |

---

## MCP Prompts

Reusable prompt templates in `.mcp/config.json`. These can be invoked by MCP-compatible tools (GitHub Copilot, Cursor, etc.).

### Existing Prompts (3)

| Prompt                | Purpose                                          |
| --------------------- | ------------------------------------------------ |
| `architecture-check`  | Verify Hexagonal/Clean Architecture compliance   |
| `create-api-endpoint` | Generate API endpoint following project patterns |
| `create-use-case`     | Generate Application layer use case              |

### New Prompts (4)

| Prompt                   | Purpose                                                                                             |
| ------------------------ | --------------------------------------------------------------------------------------------------- |
| `coppa-compliance-check` | Audit for COPPA violations: PII handling, consent, data retention, children's privacy               |
| `generate-tests`         | Generate xUnit + FluentAssertions + Moq tests following `MethodName_Scenario_ExpectedResult` naming |
| `security-review`        | Review for OWASP Top 10, auth bypass, injection, secrets exposure                                   |
| `blazor-component`       | Scaffold Blazor component with scoped CSS, lifecycle, offline-first, WCAG 2.1 AA accessibility      |

---

## MCP Resources

Registered resources in `.mcp/config.json` that MCP tools can reference for context.

### Existing Resources (6)

| Resource             | Path                                         |
| -------------------- | -------------------------------------------- |
| `documentation`      | `./docs`                                     |
| `readme`             | `./README.md`                                |
| `architecture-rules` | `./docs/architecture/ARCHITECTURAL_RULES.md` |
| `best-practices`     | `./docs/best-practices.md`                   |
| `contributing`       | `./CONTRIBUTING.md`                          |
| `solution`           | `./Mystira.App.sln`                          |

### New Resources (3)

| Resource               | Path                                | Why                                                         |
| ---------------------- | ----------------------------------- | ----------------------------------------------------------- |
| `copilot-instructions` | `./.github/copilot-instructions.md` | 574 lines of AI guidance not previously exposed to MCP      |
| `claude-settings`      | `./.claude/settings.json`           | Lets MCP tools know the project config, hooks, and commands |
| `product-requirements` | `./docs/prd`                        | PRDs for context when implementing features                 |

---

## Project Settings

### Environment Variables

Set automatically when Claude Code runs:

| Variable                      | Value         | Purpose                |
| ----------------------------- | ------------- | ---------------------- |
| `DOTNET_CLI_TELEMETRY_OPTOUT` | `1`           | Disable .NET telemetry |
| `DOTNET_NOLOGO`               | `1`           | Suppress .NET CLI logo |
| `ASPNETCORE_ENVIRONMENT`      | `Development` | Use dev configuration  |

### Built-in Commands

Quick commands available in settings:

| Command         | Action                                      |
| --------------- | ------------------------------------------- |
| `build`         | `dotnet build Mystira.App.sln`              |
| `test`          | `dotnet test Mystira.App.sln --no-build`    |
| `test:coverage` | Run tests with Coverlet code coverage       |
| `clean`         | Clean solution + remove bin/obj/TestResults |
| `restore`       | `dotnet restore Mystira.App.sln`            |
| `format`        | `dotnet format Mystira.App.sln`             |
| `api:run`       | Run the public API                          |
| `pwa:run`       | Run the Blazor PWA                          |
| `watch:api`     | Run API with hot reload                     |
| `watch:pwa`     | Run PWA with hot reload                     |

### Permissions

Pre-approved Bash commands: `dotnet`, `git`, `make`, `docker`, `docker-compose`, `az`, `gh`, `npm`, `npx`, `curl`, `wget`, `ls`, `mkdir`, and cleanup commands for `bin`/`obj`/`TestResults`.

---

## Setup

For a fresh environment, run the setup script:

```bash
chmod +x .claude/install_tools.sh
./.claude/install_tools.sh
```

This installs .NET SDK 9.0, Docker (optional), Azure CLI (optional), GitHub CLI, and restores NuGet packages.

**Requirements:**

- .NET SDK 9.0.310+
- Docker (optional, for `docker-compose.yml` local dev)
- Azure CLI (optional, for deployment)
