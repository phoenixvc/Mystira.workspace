# Admin UI - Hybrid Data Strategy Integration

This document outlines how the Admin UI integrates with the Mystira platform's hybrid data strategy as defined in the [workspace roadmap](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/planning/hybrid-data-strategy-roadmap.md).

## Impact Overview

The Admin UI has **LOW to MEDIUM** impact in the hybrid data strategy migration:

| Phase | Impact Level | Tasks |
|-------|--------------|-------|
| Phase 1 (Foundation) | Low | No changes required |
| Phase 2 (Migration) | Medium | API contract updates, migration dashboard |
| Phase 3 (Integration) | Medium | Event-driven updates, account list changes |
| Phase 4 (PostgreSQL Primary) | Low | No changes (API abstraction) |
| Phase 5 (Evolution) | High | Analytics dashboards, BI components |

## Phase 2: Admin UI Updates (Weeks 7-12)

### Migration Dashboard Component

A new dashboard component to monitor the Cosmos DB → PostgreSQL migration:

**Planned Features**:
- [ ] Migration phase indicator (Phase 0-3)
- [ ] Sync queue status display
- [ ] Data reconciliation metrics
- [ ] Error/warning alerts

**API Endpoints Required** (from Admin.Api):
```
GET /api/admin/migration/status
GET /api/admin/migration/sync-queue
GET /api/admin/migration/reconciliation
```

### Account List Updates

The account management pages will need to consume the new read-only PostgreSQL endpoints:

**Current**: Cosmos DB via `/api/admin/accounts`
**Future**: PostgreSQL via `/api/admin/accounts` (same contract, different backend)

**No UI changes required** - the API contract remains the same.

## Phase 3: Cross-Service Integration

### Event-Driven Updates

When the platform moves to Wolverine event-driven architecture:

1. **Real-time Updates**: Consider WebSocket/SSE for live data updates
2. **Cache Invalidation**: React Query cache may need manual invalidation on events
3. **Optimistic Updates**: Already implemented, will work with events

## Phase 5: Analytics & Reporting

### Business Intelligence Dashboards

New components planned for Phase 5:

- [ ] Player behavior analytics dashboard
- [ ] Scenario engagement metrics
- [ ] Revenue and purchase analytics
- [ ] User journey visualization

**Tech Stack Additions** (potential):
- Chart.js or Recharts for visualizations
- React Table for data grids
- Date range pickers for time-series analysis

## Implementation Notes

### API Client Updates

When new endpoints are added, update `/src/api/admin.ts`:

```typescript
// Future: Migration status endpoint
export const getMigrationStatus = async (): Promise<MigrationStatus> => {
  const response = await client.get('/admin/migration/status');
  return response.data;
};
```

### React Query Keys

New query keys for migration data:

```typescript
// Future query keys
export const migrationKeys = {
  status: ['migration', 'status'] as const,
  syncQueue: ['migration', 'sync-queue'] as const,
  reconciliation: ['migration', 'reconciliation'] as const,
};
```

## Dependencies

| Dependency | Required By | Status |
|------------|-------------|--------|
| Admin.Api PostgreSQL endpoints | Phase 2 | Pending |
| Migration status API | Phase 2 | Pending |
| Event subscriptions (WebSocket) | Phase 3 | Future |
| Analytics APIs | Phase 5 | Future |

## Related Documentation

- [Workspace Hybrid Data Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/planning/hybrid-data-strategy-roadmap.md)
- [ADR-0013: Data Management Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/architecture/adr/0013-data-management-and-storage-strategy.md)
- [ADR-0015: Event-Driven Architecture](https://github.com/phoenixvc/Mystira.workspace/blob/dev/docs/architecture/adr/0015-event-driven-architecture-framework.md)
- [Implementation Roadmap](./implementation-roadmap.md)
