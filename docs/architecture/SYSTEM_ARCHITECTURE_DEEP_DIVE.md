# Mystira System Architecture Deep Dive

> **Purpose**: This document answers fundamental questions about the Mystira platform architecture that are often lost in day-to-day development.

---

## Table of Contents

1. [Why the System is Structured This Way](#1-why-the-system-is-structured-this-way)
2. [Where State Actually Lives](#2-where-state-actually-lives)
3. [How Failure Propagates](#3-how-failure-propagates)
4. [What Changes Under Load or Latency](#4-what-changes-under-load-or-latency)
5. [Which Constraints Are Real vs Imagined](#5-which-constraints-are-real-vs-imagined)

---

## 1. Why the System is Structured This Way

### Overview

Mystira is an **AI-powered interactive storytelling platform** combining blockchain IP registration, generative AI, and immersive narratives. The architecture is a **polyglot microservices monorepo** with clear service boundaries.

### Component Map

| Component | Tech Stack | Purpose | Deployment |
|-----------|------------|---------|------------|
| **Mystira.App** | .NET 9 + Blazor WASM | Core platform API + PWA | Kubernetes/AKS |
| **Mystira.Admin.Api** | .NET 9 ASP.NET Core | Content management REST API | Kubernetes/AKS |
| **Mystira.Admin.UI** | React 18 + TypeScript | Admin dashboard SPA | Kubernetes/AKS |
| **Mystira.Publisher** | React 18 + TypeScript | On-chain story registration | Kubernetes/AKS |
| **Mystira.StoryGenerator** | .NET 9 + Blazor | AI story generation engine | Kubernetes/AKS |
| **Mystira.Chain** | Python + gRPC | Blockchain/Story Protocol ops | Kubernetes/AKS |
| **Mystira.DevHub** | Rust/Tauri + React | Desktop dev operations console | Desktop app |

### Why This Structure?

**1. Technology Choice Rationale**

| Decision | Reasoning |
|----------|-----------|
| **.NET 9 for APIs** | Strong async support, Azure integration, team expertise |
| **Python for Chain service** | Story Protocol SDK is Python-native; gRPC for cross-language |
| **React for frontends** | Component reuse between Admin UI and Publisher |
| **Blazor WASM for PWA** | Offline-first requirement, code sharing with backend |
| **Rust/Tauri for DevHub** | Native performance, small binary, cross-platform desktop |

**2. Architectural Patterns in Use**

```
┌─────────────────────────────────────────────────────────────┐
│                    API Layer (Controllers)                  │
│                         ↓ depends on                        │
├─────────────────────────────────────────────────────────────┤
│              Application Layer (CQRS + MediatR)             │
│    Commands (16) | Queries (20) | Specifications (32)       │
│                         ↓ depends on                        │
├─────────────────────────────────────────────────────────────┤
│                    Domain Layer (Core)                      │
│       Entities | Value Objects | Domain Events              │
│                         ↑ implements                        │
├─────────────────────────────────────────────────────────────┤
│                 Infrastructure Layer                        │
│   EF Core | Cosmos DB | Redis | Azure Blob | External APIs  │
└─────────────────────────────────────────────────────────────┘
```

- **Hexagonal Architecture (Ports & Adapters)**: Application layer has no direct infrastructure dependencies
- **CQRS**: Separate read/write paths enable independent optimization
- **Repository + Specification**: Encapsulated query logic, reusable specifications

**3. Service Communication**

| Path | Protocol | Auth | Use Case |
|------|----------|------|----------|
| Frontend → APIs | REST/HTTP | Entra ID JWT | Standard CRUD |
| Publisher → Chain | gRPC | Wallet key in metadata | Blockchain ops |
| PWA ↔ App.Api | SignalR | Entra External ID | Real-time updates |
| All services | Direct DB | Managed Identity | Internal data access |

---

## 2. Where State Actually Lives

### State Location Matrix

| Location | Durability | Consistency | Scope | TTL |
|----------|------------|-------------|-------|-----|
| **Cosmos DB** | Persistent | Strong (partition) / Eventual (global) | Application | Permanent |
| **PostgreSQL** | Persistent | ACID | Application | Permanent |
| **Redis** | Configurable (AOF/RDB) | Eventual | Distributed | 1-30 min |
| **Memory Cache** | None (process restart) | Process-local | Single instance | 1-30 min |
| **Blob Storage** | 11 nines | Strong (single blob) | Global | Permanent |
| **IndexedDB (PWA)** | Browser storage | Client-local | User session | Until cleared |
| **SignalR Hub** | Ephemeral | Real-time | Connection lifetime | Connection |
| **Story Protocol** | Immutable | Blockchain consensus | Global | Permanent |

### Primary Database: Cosmos DB

**Entities Stored:**
- User Profiles, Accounts, Game Sessions
- Scenarios, Content Bundles, Character Maps
- Badges, Media Assets, Compass Axes

**Partition Keys:**
- Most entities: `/id`
- GameSessions: `/AccountId`
- MediaAssets: `/MediaType`
- PlayerScenarioScores: `/ProfileId`

**Access Pattern:**
```
Controller → MediatR → CQRS Handler → Repository → DbContext → Cosmos
```

### Distributed Cache: Redis

**What's Cached:**
- Query results (ICacheableQuery implementers)
- Session state (transient)
- Distributed locks
- Rate limiting counters

**Key Pattern:** `mystira:<type>:<identifier>`

**Invalidation:** Prefix-based via `QueryCacheInvalidationService`

### Polyglot Persistence (Dual-Write)

```
┌────────────────────────────────────────────────────────────┐
│                    Write Operation                         │
├────────────────────────────────────────────────────────────┤
│  1. Write to Cosmos DB (primary) ─────────────→ MUST SUCCEED
│  2. Write to PostgreSQL (secondary) ──────────→ Best effort
│     └─ On failure: Log, increment metric, continue         │
└────────────────────────────────────────────────────────────┘
```

**Current Mode:** Phase 0 (Cosmos only) - dual-write infrastructure exists but is disabled.

---

## 3. How Failure Propagates

### Exception Hierarchy

```
MystiraException (base)
├── ValidationException         → 400 Bad Request
├── NotFoundException           → 404 Not Found
├── UnauthorizedException       → 401 Unauthorized
├── ForbiddenException          → 403 Forbidden
├── ConflictException           → 409 Conflict
├── ServiceUnavailableException → 503 Service Unavailable
└── RateLimitedException        → 429 Too Many Requests
```

### Error Response Format (RFC 7807)

```json
{
  "type": "https://httpstatuses.com/400",
  "status": 400,
  "title": "Validation Failed",
  "detail": "One or more validation errors occurred",
  "instance": "/api/game-sessions",
  "traceId": "0HMVG8FSJH9MD:00000001",
  "timestamp": "2026-01-11T15:30:45Z",
  "errorCode": "VALIDATION_FAILED",
  "errors": [{ "field": "ScenarioId", "message": "Required" }]
}
```

### Resilience Patterns

**Circuit Breaker (Polly v8):**
```
CLOSED → (5 consecutive failures) → OPEN → (30s cooldown) → HALF-OPEN → (success) → CLOSED
                                                         └─ (failure) → OPEN
```

**Retry with Exponential Backoff:**
```
Attempt 1: Wait 2s (± 20% jitter)
Attempt 2: Wait 4s (± 20% jitter)
Attempt 3: Wait 8s (± 20% jitter)
Max delay capped at 60 seconds
```

### Failure Propagation Flow

```
Request
  │
  ├─[Auth Middleware]─────────────────→ 401 Unauthorized
  ├─[Rate Limiting]───────────────────→ 429 Too Many Requests
  ├─[Validation Middleware]───────────→ 400 Bad Request
  │
  ├─[Handler/Use Case]
  │   ├─[Unit of Work Transaction]
  │   │   ├─ Cosmos DB ─────────────→ Retry (3x) then rollback
  │   │   └─ PostgreSQL ────────────→ Fail silently, log metric
  │   │
  │   └─[External Services]
  │       ├─ gRPC Chain ────────────→ Circuit breaker + MockService fallback
  │       ├─ Payment API ───────────→ Return failed result
  │       └─ Discord Bot ───────────→ NoOpChatBotService fallback
  │
  └─[GlobalExceptionHandler]
      ├─ MystiraException ──────────→ Domain-specific HTTP status
      └─ Unhandled ─────────────────→ 500 Internal Server Error
```

### Health Check Endpoints

| Endpoint | Purpose | Failure Response |
|----------|---------|------------------|
| `GET /health` | Full health with dependencies | 503 if any unhealthy |
| `GET /health/ready` | Kubernetes readiness | Blocks traffic routing |
| `GET /health/live` | Kubernetes liveness | Triggers pod restart |

---

## 4. What Changes Under Load or Latency

### Critical Load Points

| Component | Bottleneck | Current Limit | Impact |
|-----------|------------|---------------|--------|
| **LLM Rate Limiting** | API quotas | 50-250 req/min | Story generation queues |
| **Redis** | Memory/connections | Provider tier | Cache fallback to memory |
| **Database** | RU throughput (Cosmos) | Shared throughput | Query throttling |
| **SignalR** | Connection count | Redis backplane | Real-time lag |
| **Kubernetes** | Pod resources | 512Mi-1Gi per pod | Autoscaling |

### Rate Limiting Configuration

**Global (In-Memory Token Bucket):**
```
Default: 100 requests/minute per client
Window: 1 minute
Tracking: User ID or IP (with X-Forwarded-For support)
Headers: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset
```

**LLM-Specific (Leaky Bucket):**
```
PrefixSummaryRequests: 50/min → 1.2s interval
SrlRequests: 250/min → 0.24s interval
```

### Caching TTLs

| Data Type | TTL | Rationale |
|-----------|-----|-----------|
| Master data (badges, archetypes) | 60 min | Rarely changes |
| Content (scenarios) | 30 min | Moderate change frequency |
| User data (profiles) | 5 min | More dynamic |
| Query results | 1-5 min | Configurable per query |

### Kubernetes Autoscaling

**Admin API:**
```yaml
minReplicas: 2
maxReplicas: 5
scaleAt:
  cpu: 70%
  memory: 80%
```

**Story Generator:**
```yaml
minReplicas: 2
maxReplicas: 10  # Higher due to LLM workload
scaleAt:
  cpu: 70%
  memory: 80%
```

### Timeout Configuration

| Operation | Timeout | Reasoning |
|-----------|---------|-----------|
| Standard HTTP | 30s | General operations |
| LLM/AI calls | 300s (5 min) | Story generation with 25K tokens |
| Cosmos DB | 30s | HTTP layer timeout |
| PostgreSQL | 30s | Command timeout |
| Circuit breaker reset | 30s | Recovery window |

### Concurrency Controls

| Mechanism | Location | Purpose |
|-----------|----------|---------|
| `SemaphoreSlim(1,1)` | AI Model Settings | Serialize initialization |
| `SemaphoreSlim(N)` | Migration Service | Limit concurrent blob ops |
| `ConcurrentDictionary` | Rate Limiter | Thread-safe client tracking |
| Redis SET NX | Distributed Locks | Cross-instance coordination |

---

## 5. Which Constraints Are Real vs Imagined

### Definitely Real Constraints

| Constraint | Why It's Real | Evidence |
|------------|---------------|----------|
| **LLM 300s timeout** | GPT-4.1 with 25K tokens + chained operations | Measured workload requirement |
| **Cosmos eventual consistency** | Fundamental to multi-region distribution | Azure architecture property |
| **HTTP/2 for gRPC** | Protocol requirement | RFC 7540 |
| **Story Protocol finality** | Blockchain requires confirmations | Consensus mechanism |
| **Kubernetes resource limits** | Physical hardware constraints | Node capacity |

### Likely Imagined Constraints

| Constraint | Why It's Questionable | Recommendation |
|------------|----------------------|----------------|
| **4-phase Cosmos→PostgreSQL migration** | Phase 0 for months; 530-line PolyglotRepository unused | Remove or commit timeline |
| **Cache TTLs (30/5/60 min)** | No measured change frequency data | Add cache hit/miss metrics |
| **Retry policy defaults (3x, 2s base)** | Industry defaults, not calibrated | Profile actual failure rates |
| **HPA thresholds (70%/80%)** | Standard defaults, not measured | Load test to determine |
| **5000ms secondary write timeout** | PostgreSQL writes should be <100ms | Reduce to 500ms |
| **gRPC over REST for Chain** | No actual latency measurements | Benchmark before committing |
| **Mock implementations in config** | Dev-only feature flags in prod settings | Move to environment-specific |

### Gray Area: Potentially Over-Engineered

| Pattern | Current Justification | Reality Check |
|---------|----------------------|---------------|
| **CQRS** | "Read optimization needed" | At current scale, simpler patterns might suffice |
| **Hexagonal Architecture** | "Swap infrastructure easily" | Have we ever swapped Azure for AWS? |
| **32 Specification classes** | "Reusable query logic" | Many used only once |
| **Wolverine message bus** | "Event-driven future" | Most handlers are synchronous |

### Measurement Gaps

These values appear to be defaults rather than measured:

```yaml
# Kubernetes
cpu_trigger: 70%      # Is this where performance actually degrades?
memory_trigger: 80%   # Measured or industry default?

# Retry policies
max_retries: 3        # What's the actual success rate on retry 1 vs 2 vs 3?
base_delay: 2s        # Calibrated to actual service recovery time?

# Cache
content_ttl: 30min    # How often does content actually change?
```

### Recommendations

1. **Immediate**: Add observability for cache hit rates, retry success rates, timeout frequency
2. **Short-term**: Load test to establish evidence-based HPA and resource limits
3. **Medium-term**: Either commit to PostgreSQL migration timeline or remove dual-write machinery
4. **Ongoing**: Question each "industry default" - is it right for our specific workload?

---

## Quick Reference: Key File Locations

| Concern | Primary Files |
|---------|---------------|
| **Database Context** | `packages/app/src/Mystira.App.Infrastructure.Data/MystiraAppDbContext.cs` |
| **Polyglot Repository** | `packages/app/src/Mystira.App.Infrastructure.Data/Polyglot/PolyglotRepository.cs` |
| **Resilience Policies** | `packages/shared/Mystira.Shared/Resilience/ResiliencePipelineFactory.cs` |
| **Exception Handling** | `packages/shared/Mystira.Shared/Exceptions/GlobalExceptionHandler.cs` |
| **Rate Limiting** | `packages/shared/Mystira.Shared/Middleware/RateLimitingMiddleware.cs` |
| **Caching** | `packages/shared/Mystira.Shared/Caching/DistributedCacheService.cs` |
| **Health Checks** | `packages/shared/Mystira.Shared/Health/` |
| **Kubernetes Manifests** | `infra/kubernetes/base/*/deployment.yaml` |
| **API Configuration** | `packages/app/src/Mystira.App.Api/appsettings.json` |
| **DI Registration** | `packages/app/src/Mystira.App.Api/Program.cs` (~800 lines) |

---

## Appendix: Architecture Decision Records

| ADR | Topic | Status |
|-----|-------|--------|
| 0001 | Infrastructure organization | Accepted |
| 0004 | Branching strategy and CI/CD | Accepted |
| 0005 | Service networking | Accepted |
| 0006 | Admin API extraction | Accepted |
| 0010 | Authentication and authorization | Accepted |
| 0013 | Data management (Cosmos + PostgreSQL) | In Progress |
| 0014 | Polyglot persistence | Accepted |
| 0015 | Event-driven architecture | Proposed |

---

*Last updated: 2026-01-11*
*Generated from codebase analysis*
