# Mystira Migration Index

**Last Updated**: December 2025
**Status**: Active
**Owner**: jurie@phoenixvc.tech (Technical), eben@phoenixvc.tech (Business)

---

## Quick Status Dashboard

| Area | Status | Progress | Details |
|------|--------|----------|---------|
| **Infrastructure Foundation** | ✅ Complete | 100% | Azure, Terraform, CI/CD |
| **Mystira.Shared Package** | ✅ Complete | 100% | v0.2.0-alpha released |
| **@mystira/core-types** | ✅ Complete | 100% | v0.2.0-alpha released |
| **Domain Events** | ✅ Complete | 100% | 13 events defined |
| **API Domain Standardization** | ✅ Complete | 100% | `{env}.{service}.mystira.app` |
| **Service Migrations** | 🔄 In Progress | 0% | Week 1-2 target |
| **Monitoring & Alerts** | ✅ Complete | 100% | App Insights, availability tests |

---

## Service Migration Guides

Each service has a detailed migration guide for adopting `Mystira.Shared 0.2.0-alpha`:

| Service | Guide | Priority | Status |
|---------|-------|----------|--------|
| Mystira.App | [mystira-app-migration.md](./mystira-app-migration.md) | High | 📋 Ready to start |
| Mystira.Admin.Api | [mystira-admin-migration.md](./mystira-admin-migration.md) | High | 📋 Ready to start |
| Mystira.StoryGenerator | [mystira-storygenerator-migration.md](./mystira-storygenerator-migration.md) | Medium | 📋 Ready to start |
| Mystira.Publisher | [mystira-publisher-migration.md](./mystira-publisher-migration.md) | Medium | 📋 Needs update |
| Mystira.Chain | [mystira-chain-migration.md](./mystira-chain-migration.md) | Medium | 📋 Planned |
| Mystira.DevHub | [mystira-devhub-migration.md](./mystira-devhub-migration.md) | Low | 📋 Planned |
| Admin UI | [mystira-admin-ui-migration.md](./mystira-admin-ui-migration.md) | Low | 📋 Planned |

---

## What's Been Completed

### Mystira.Shared 0.2.0-alpha

All infrastructure patterns are implemented:

| Component | Status | Location |
|-----------|--------|----------|
| Polly v8 Resilience | ✅ | `Mystira.Shared.Resilience` |
| Wolverine + Service Bus | ✅ | `Mystira.Shared.Messaging` |
| Ardalis.Specification | ✅ | `Mystira.Shared.Data` |
| Redis Caching | ✅ | `Mystira.Shared.Caching` |
| EntityId (string IDs) | ✅ | `Mystira.Shared.Data.EntityId` |
| AsyncExtensions | ✅ | `Mystira.Shared.Extensions.AsyncExtensions` |
| RateLimitingMiddleware | ✅ | `Mystira.Shared.Middleware` |
| SecurityMetrics | ✅ | `Mystira.Shared.Telemetry` |
| BusinessMetrics | ✅ | `Mystira.Shared.Telemetry` |

### Domain Events (13 events)

| Category | Events |
|----------|--------|
| Account | `AccountCreated`, `AccountUpdated`, `AccountDeleted` |
| Session | `SessionStarted`, `SessionCompleted`, `SessionAbandoned` |
| Content | `ScenarioCreated`, `ScenarioUpdated`, `ScenarioPublished`, `ScenarioUnpublished`, `MediaUploaded` |
| Cache | `CacheInvalidated`, `CacheWarmupRequested` |

---

## Technical Debt - RESOLVED

These issues were identified in code reviews and have been addressed:

| Issue | Solution | Location |
|-------|----------|----------|
| `Guid.Parse` crashes | `EntityId` class | `Mystira.Shared.Data.EntityId` |
| Missing `CancellationToken` | `EnsureToken()` | `Mystira.Shared.Extensions.AsyncExtensions` |
| Fire-and-forget patterns | `SafeExecuteAsync()` | `Mystira.Shared.Extensions.AsyncExtensions` |
| Rate limiting | `RateLimitingMiddleware` | `Mystira.Shared.Middleware` |
| Rollback procedures | Service runbook | `docs/operations/runbooks/service-rollback.md` |

---

## Remaining Work

### High Priority (Before Jan 1, 2026)

- [ ] Migrate Mystira.App to Mystira.Shared 0.2.0-alpha
- [ ] Migrate Mystira.Admin.Api to Mystira.Shared 0.2.0-alpha
- [ ] Implement Wolverine event handlers
- [ ] Test cross-service events via Azure Service Bus
- [ ] Performance baselines (load testing)

### Medium Priority

- [ ] Migrate StoryGenerator, Publisher, Chain
- [ ] Add missing events (AI, notification, payment)
- [ ] Add unit tests for Mystira.Shared components

---

## Related Documents

- [2026 Roadmap](../../ROADMAP.md) - Single source of truth for planning
- [Architecture ADRs](../architecture/adr/) - Decision records
- [Architecture Migrations](../architecture/migrations/) - Technical specifications
- [Service Rollback](../operations/runbooks/service-rollback.md) - Rollback procedures

---

## Contacts

| Role | Contact |
|------|---------|
| Technical Lead | jurie@phoenixvc.tech |
| Business | eben@phoenixvc.tech |
