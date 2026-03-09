# Migration Documentation

Migration documentation for the Mystira.App package.

## Status

**Mystira.App Migration: ✅ 100% Complete**

All 10 migration phases are done:

1. .NET 10.0 upgrade
2. Infrastructure packages (ProjectReferences)
3. MediatR → Wolverine (111 handlers)
4. Custom resilience → Polly v8
5. IMemoryCache → IDistributedCache (Redis)
6. Custom exceptions → Mystira.Shared.Exceptions
7. Repository pattern → Ardalis.Specification 9.3.1
8. Distributed locking (Redis-backed)
9. Microsoft Entra External ID authentication
10. Source generators

## Centralized Documentation

See the workspace migration guides:

- [Migration Index](../../../../docs/migrations/MIGRATION_INDEX.md) - Overall migration status
- [Mystira.App Migration Guide](../../../../docs/migrations/mystira-app-migration.md) - Detailed migration guide
- [Architecture ADRs](../../../../docs/architecture/adr/) - Decision records

**Last Updated**: February 2026
