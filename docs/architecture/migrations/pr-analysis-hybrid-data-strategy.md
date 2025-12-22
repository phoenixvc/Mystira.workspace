# PR Analysis: Hybrid Data Strategy Infrastructure

**PR Branch**: `claude/import-acr-terraform-IDdgg`
**Review Date**: 2025-12-22
**Commits Analyzed**: 10 commits (941571f → cc61bb8)

---

## Summary

This PR implements Phase 1 of ADR-0013 (Data Management and Storage Strategy), adding PostgreSQL/Redis infrastructure, blob storage tiering, and migration documentation. Overall, the changes are solid but have several issues that should be addressed.

---

## Bugs

### BUG-1: PostgreSQL Connection String Format (Critical)

**File**: `infra/terraform/modules/mystira-app/main.tf:537`

**Issue**: The PostgreSQL connection string uses incorrect parameter format for Npgsql:
```hcl
value = "Host=${var.shared_postgresql_server_fqdn};Port=5432;Username=${var.shared_postgresql_admin_login};Password=${var.shared_postgresql_admin_password};Database=${var.postgresql_database_name};SSLMode=Require;Trust Server Certificate=true"
```

**Problem**:
- `Trust Server Certificate=true` should be `Trust Server Certificate=True` or `TrustServerCertificate=True`
- Npgsql uses PascalCase for boolean values

**Fix**:
```hcl
value = "Host=${var.shared_postgresql_server_fqdn};Port=5432;Username=${var.shared_postgresql_admin_login};Password=${var.shared_postgresql_admin_password};Database=${var.postgresql_database_name};SSL Mode=Require;Trust Server Certificate=True"
```

---

### BUG-2: Storage Lifecycle Rule Path Mismatch (High)

**File**: `infra/terraform/modules/mystira-app/main.tf:254-256`

**Issue**: Lifecycle rules use folder prefixes, but containers are root-level:
```hcl
filters {
  prefix_match = ["audio/"]  # This won't match blobs in the "audio" container
  blob_types   = ["blockBlob"]
}
```

**Problem**: The `prefix_match` filter expects blobs with paths like `audio/file.mp3` within a single container, but we created separate containers (`avatars`, `audio`, `content-bundles`). The rules won't apply correctly.

**Fix**: Either:
1. Use a single container with folder prefixes, OR
2. Create separate lifecycle policies per container without prefix filters

```hcl
# Option 2 - Per container rule
rule {
  name    = "audio-tiering"
  enabled = true

  filters {
    blob_types = ["blockBlob"]  # No prefix - applies to all blobs in container
  }

  actions {
    base_blob {
      tier_to_cool_after_days_since_modification_greater_than = var.storage_tier_to_cool_days
    }
  }
}
```

**Note**: This requires restructuring to one lifecycle policy per container, or moving to a single container with folder structure.

---

### BUG-3: PostgreSQL Secret Created Without Server (Medium)

**File**: `infra/terraform/modules/mystira-app/main.tf:533`

**Issue**: The condition only checks `enable_postgresql && use_shared_postgresql`, but if `use_shared_postgresql=false`, no database is created yet secret reference exists:
```hcl
resource "azurerm_key_vault_secret" "postgresql_connection_string" {
  count = var.enable_postgresql && var.use_shared_postgresql ? 1 : 0
```

**Problem**: If `enable_postgresql=true` but `use_shared_postgresql=false`, the app settings still reference the Key Vault secret that doesn't exist.

**Fix**: Update app settings to check both conditions:
```hcl
"ConnectionStrings__PostgreSQL" = var.enable_postgresql && var.use_shared_postgresql ? "@Microsoft.KeyVault(...)" : ""
```

---

### BUG-4: Missing depends_on for PostgreSQL Database (Medium)

**File**: `infra/terraform/modules/mystira-app/main.tf:701`

**Issue**: PostgreSQL database resource doesn't explicitly depend on Key Vault, but its connection string is stored there.

**Fix**: Add explicit dependency:
```hcl
resource "azurerm_postgresql_flexible_server_database" "mystira_app" {
  ...
  depends_on = [azurerm_key_vault.main]
}
```

---

## Missed Opportunities

### MISS-1: Admin API Not Configured for PostgreSQL/Redis

**Impact**: High

The App Service configuration only sets up PostgreSQL/Redis for the main API. The Admin API will need similar configuration but is missing from this PR.

**Recommendation**: Add variables for Admin API App Service name and update its app settings, or document that Admin API configuration is out of scope.

---

### MISS-2: No Health Check Endpoints Documentation

**Impact**: Medium

PostgreSQL and Redis are added but no guidance on health checks. The application should expose:
- `/health/postgresql` - Database connectivity
- `/health/redis` - Cache connectivity
- `/health/migration` - Current migration phase status

**Recommendation**: Add health check contract to `internal-api-contracts.md`.

---

### MISS-3: Missing Redis Configuration Options

**Impact**: Medium

Redis is provisioned but missing:
- **Connection pooling**: `StackExchange.Redis` multiplexer configuration
- **Backup**: No `rdb_backup_enabled` for production
- **Geo-replication**: Not mentioned for multi-region

**Recommendation**: Add variables:
```hcl
variable "redis_enable_backup" {
  description = "Enable Redis RDB backup (Standard/Premium only)"
  type        = bool
  default     = false
}

variable "redis_backup_frequency" {
  description = "Backup frequency in minutes (15, 30, 60, etc.)"
  type        = number
  default     = 60
}
```

---

### MISS-4: No Application Insights Custom Metrics for Migration

**Impact**: Medium

The migration phase is tracked in app settings but not exposed as Application Insights custom metrics. This would enable:
- Dashboard showing current migration phase per environment
- Alerts when phase transitions occur
- Telemetry for dual-write latency comparison

**Recommendation**: Document recommended custom metrics:
```csharp
telemetryClient.TrackMetric("DataMigration.Phase", (int)currentPhase);
telemetryClient.TrackMetric("DataMigration.DualWriteLatencyMs", latency);
telemetryClient.TrackMetric("DataMigration.SyncQueueDepth", queueLength);
```

---

### MISS-5: No PostgreSQL Connection Pooling (PgBouncer)

**Impact**: Medium (for production)

Azure PostgreSQL Flexible Server supports built-in PgBouncer, but it's not enabled in this configuration.

**Recommendation**: Add for production:
```hcl
# In shared/postgresql module
resource "azurerm_postgresql_flexible_server_configuration" "pgbouncer" {
  count     = var.environment == "prod" ? 1 : 0
  name      = "pgbouncer.enabled"
  server_id = azurerm_postgresql_flexible_server.main.id
  value     = "true"
}
```

---

### MISS-6: Missing Blob Container Access Policies

**Impact**: Low

Containers are created with `blob` access (public read) but sensitive containers like `archives` and `uploads` should use SAS tokens with time-limited access.

**Recommendation**: Add a data resource or output for generating SAS tokens:
```hcl
output "archives_sas_url" {
  description = "SAS URL template for archives container (insert blob name)"
  value       = var.skip_storage_creation ? null : "https://${azurerm_storage_account.main[0].name}.blob.core.windows.net/archives?sv=..."
  sensitive   = true
}
```

---

## Oversights

### OVER-1: Internal API Contracts Reference Non-Existent Project

**File**: `docs/architecture/contracts/internal-api-contracts.md`

**Issue**: Document references `src/Mystira.App.Contracts/` which doesn't exist in the codebase (based on project listing, it's at `/tmp/Mystira.App/src/Mystira.App.Contracts/`).

**Fix**: Either:
1. Clarify this is a planned project structure, OR
2. Reference the existing contracts location

---

### OVER-2: Repository Architecture Assumes Separate DbContexts

**File**: `docs/architecture/migrations/repository-architecture.md`

**Issue**: The architecture document assumes `MystiraAppDbContext` (Cosmos) and `PostgreSqlDbContext` are separate, but the current codebase uses a single `MystiraAppDbContext` that can switch providers.

**Impact**: The migration path needs to clarify whether to:
1. Keep single context with provider abstraction
2. Create separate contexts per database

**Recommendation**: Add section explaining context strategy.

---

### OVER-3: No PWA Offline Sync Documentation

**Impact**: Medium

The Mystira.App PWA uses IndexedDB for offline storage. Migration documentation doesn't address:
- How offline data syncs with new PostgreSQL backend
- Whether client schema needs updates
- Conflict resolution during dual-write phase

**Recommendation**: Add "PWA Considerations" section to migration doc.

---

### OVER-4: ADR-0013 Missing Cross-References

**File**: `docs/architecture/adr/0013-data-management-and-storage-strategy.md`

**Issue**: ADR-0013 was created before ADR-0014 and ADR-0015 but doesn't reference them in the "References" section.

**Fix**: Add to References section:
```markdown
- [ADR-0014: Polyglot Persistence Framework Selection](./0014-polyglot-persistence-framework-selection.md)
- [ADR-0015: Event-Driven Architecture Framework](./0015-event-driven-architecture-framework.md)
```

---

### OVER-5: PostgreSQL Schema Missing Soft Delete

**File**: `docs/architecture/migrations/user-domain-postgresql-migration.md`

**Issue**: PostgreSQL schema doesn't include soft delete columns (`deleted_at`, `is_deleted`) but Cosmos DB entities may have these. This could cause data loss during migration.

**Recommendation**: Add soft delete columns:
```sql
ALTER TABLE accounts ADD COLUMN deleted_at TIMESTAMPTZ;
ALTER TABLE accounts ADD COLUMN is_deleted BOOLEAN NOT NULL DEFAULT FALSE;
CREATE INDEX idx_accounts_deleted ON accounts(is_deleted) WHERE is_deleted = FALSE;
```

---

## Recommendations Summary

### Critical (Fix Before Merge)
1. Fix PostgreSQL connection string format (BUG-1)
2. Fix storage lifecycle rule path mismatch (BUG-2)

### High Priority (Fix Soon After Merge)
3. Document Admin API configuration requirements (MISS-1)
4. Add health check contracts (MISS-2)
5. Update ADR cross-references (OVER-4)

### Medium Priority (Before Phase 2)
6. Add Redis backup configuration for production (MISS-3)
7. Add migration metrics to Application Insights (MISS-4)
8. Add PostgreSQL connection pooling (MISS-5)
9. Clarify DbContext strategy in repository architecture (OVER-2)
10. Add PWA offline sync documentation (OVER-3)

### Low Priority (Nice to Have)
11. Add SAS URL generation for private containers (MISS-6)
12. Add soft delete to PostgreSQL schema (OVER-5)

---

## Files Changed Summary

| File | Lines Added | Issues Found |
|------|-------------|--------------|
| `main.tf` | 362 | BUG-1, BUG-2, BUG-3, BUG-4 |
| `outputs.tf` | 126 | None |
| `variables.tf` | 143 | None |
| `0013-data-management-*.md` | 533 | OVER-4 |
| `internal-api-contracts.md` | 894 | OVER-1 |
| `repository-architecture.md` | 696 | OVER-2 |
| `user-domain-postgresql-*.md` | 532 | OVER-5 |

---

## Conclusion

The PR provides a solid foundation for the hybrid data strategy but has critical bugs in the Terraform configuration that must be fixed. The documentation is comprehensive but needs cross-referencing and clarification on existing codebase integration. Recommend addressing Critical and High Priority items before merging.
