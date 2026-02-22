# .NET 9 Upgrade Verification Checklist

Tracking document for verifying that all projects have been successfully upgraded to .NET 9.

> **Context**: This checklist was originally part of the repository README and has been relocated here as a reference artifact.

## Verified Projects

| Project | Target Framework | Notes |
|---------|-----------------|-------|
| `src/Mystira.App.Api` | `net9.0` | Public API upgraded for C# 13 features and ASP.NET Core performance improvements |
| `src/Mystira.App.PWA` | `net9.0` | Blazor host upgraded; WebAssembly assets run on the latest runtime |
| `src/Mystira.App.Domain` | `net9.0` | Core domain layer |
| `src/Mystira.App.Application` | `net9.0` | CQRS commands, queries, and Wolverine handlers |
| `src/Mystira.App.Infrastructure.Data` | `net9.0` | EF Core 9 with Cosmos DB and PostgreSQL providers |
| `src/Mystira.App.Infrastructure.Azure` | `net9.0` | Blob Storage, email, health checks |
| `src/Mystira.App.Infrastructure.Discord` | `net9.0` | Discord bot integration |
| `src/Mystira.App.Infrastructure.Teams` | `net9.0` | Teams integration |
| `src/Mystira.App.Infrastructure.WhatsApp` | `net9.0` | WhatsApp integration |
| `src/Mystira.App.Infrastructure.Payments` | `net9.0` | Payment processing |

## Package Updates

Blazor WebAssembly client libraries (`Microsoft.AspNetCore.Components.WebAssembly`, DevServer, `Microsoft.Extensions.Http`, `System.Text.Json`) target version 9.0.x to match the runtime upgrade.

## SDK Version

Controlled by `global.json`:

```json
{
  "sdk": {
    "version": "9.0.310",
    "rollForward": "latestMinor",
    "allowPrerelease": false
  }
}
```

## Tips

- Run `dotnet workload update` after upgrading to keep WebAssembly and MAUI workloads in sync with the 9.0 SDK.
- Dependency updates are managed automatically by [Renovate](../../renovate.json).

---

**Last Updated**: 2025-11-24
