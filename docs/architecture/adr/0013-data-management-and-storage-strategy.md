# ADR-0013: Data Management and Storage Strategy

## Status

**Proposed** - 2025-12-22

## Context

The Mystira platform consists of multiple services with diverse data storage needs:

### Current Architecture

| Service                     | Database     | Storage    | Purpose                            |
| --------------------------- | ------------ | ---------- | ---------------------------------- |
| **Mystira.App** (Main API)  | Cosmos DB    | Azure Blob | Game app, user profiles, scenarios |
| **Mystira.App** (Admin API) | Cosmos DB    | Azure Blob | Content management, admin tools    |
| **Story-Generator**         | PostgreSQL   | -          | AI scenario generation             |
| **Publisher**               | -            | -          | Blockchain publishing service      |
| **Chain**                   | File storage | -          | Blockchain node data               |

### Data Categories

#### 1. Transactional Data (High Write, Moderate Read)

- User profiles and accounts
- Game sessions and progress
- Player scores and achievements
- Pending signups

#### 2. Content/Master Data (Low Write, High Read)

- Scenarios (YAML-based, complex nested documents)
- Character maps and metadata
- Badge configurations
- Archetype/Echo/FantasyTheme definitions
- Compass axis definitions

#### 3. Binary/Media Assets (Write Once, Read Many)

- Character images and avatars
- Background images
- Music tracks (MP3/OGG)
- Sound effects
- Scenario artwork

#### 4. Analytical/Tracking Data (Append-Only, Aggregate Read)

- Compass tracking events
- Player session analytics
- Usage metrics

#### 5. AI/ML Data (Batch Write, Complex Query)

- Generated scenario content
- Training data
- Prompt templates
- Generation history

#### 6. Blockchain Data (Append-Only, Immutable)

- Story IP registrations
- Royalty distributions
- On-chain metadata

### Current Pain Points

1. **Cosmos DB Partition Key Inconsistency**: Different containers use different partition strategies
2. **No Clear Separation**: Admin API and App API share same database without isolation
3. **PostgreSQL Underutilized**: Only used by Story-Generator, could serve relational needs
4. **Media Storage Costs**: Large binary files in single blob container
5. **Cross-Service Data Access**: No clear API boundaries for data sharing
6. **Scalability Concerns**: Cosmos DB serverless has RU limits

## Decision Drivers

| Driver                   | Weight | Description                                                  |
| ------------------------ | ------ | ------------------------------------------------------------ |
| **Cost Efficiency**      | 25%    | Minimize operational costs while scaling                     |
| **Performance**          | 20%    | Low latency for real-time game interactions                  |
| **Data Consistency**     | 15%    | ACID where needed, eventual consistency acceptable elsewhere |
| **Developer Experience** | 15%    | Easy to develop, test, and maintain                          |
| **Scalability**          | 15%    | Handle growth from 100 to 100K+ users                        |
| **Extensibility**        | 10%    | Easy to add new data types and services                      |

---

## Options Analysis

### Option 1: Cosmos DB Primary (Current State Enhanced)

**Description**: Keep Cosmos DB as primary store, optimize partitioning, add PostgreSQL for specific use cases.

```
┌─────────────────────────────────────────────────────────────────┐
│                        Data Architecture                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    COSMOS DB (NoSQL)                      │   │
│  │  ┌─────────────────┐  ┌─────────────────┐                │   │
│  │  │  User Domain    │  │  Game Domain    │                │   │
│  │  │  • UserProfiles │  │  • Scenarios    │                │   │
│  │  │  • Accounts     │  │  • GameSessions │                │   │
│  │  │  • PendingSignup│  │  • Scores       │                │   │
│  │  └─────────────────┘  └─────────────────┘                │   │
│  │  ┌─────────────────┐  ┌─────────────────┐                │   │
│  │  │  Content Domain │  │  Analytics      │                │   │
│  │  │  • ContentBundle│  │  • Tracking     │                │   │
│  │  │  • MediaAssets  │  │  • Metrics      │                │   │
│  │  └─────────────────┘  └─────────────────┘                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────┐  ┌──────────────────────────────────┐ │
│  │   POSTGRESQL         │  │   BLOB STORAGE                   │ │
│  │   • StoryGenerator   │  │   • Images (CDN-backed)          │ │
│  │   • Audit logs       │  │   • Music (CDN-backed)           │ │
│  │   • Reports          │  │   • User uploads                 │ │
│  └──────────────────────┘  └──────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:

- Minimal migration effort
- Cosmos DB excels at document storage (scenarios are complex JSON)
- Serverless scales automatically
- Global distribution if needed later

**Cons**:

- Higher cost at scale compared to PostgreSQL
- No relational integrity for cross-references
- Complex queries require denormalization
- RU cost unpredictable for complex queries

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Cost Efficiency | 2 | 0.50 |
| Performance | 4 | 0.80 |
| Data Consistency | 3 | 0.45 |
| Developer Experience | 4 | 0.60 |
| Scalability | 5 | 0.75 |
| Extensibility | 4 | 0.40 |
| **Total** | | **3.50** |

---

### Option 2: PostgreSQL Primary + Cosmos DB for Documents

**Description**: Move transactional data to PostgreSQL, keep Cosmos DB for document-heavy content.

```
┌─────────────────────────────────────────────────────────────────┐
│                        Data Architecture                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    POSTGRESQL (Relational)                │   │
│  │  ┌─────────────────┐  ┌─────────────────┐                │   │
│  │  │  User Domain    │  │  Game Domain    │                │   │
│  │  │  • users        │  │  • game_sessions│                │   │
│  │  │  • accounts     │  │  • scores       │                │   │
│  │  │  • profiles     │  │  • achievements │                │   │
│  │  │  • badges       │  │  • progress     │                │   │
│  │  └─────────────────┘  └─────────────────┘                │   │
│  │  ┌─────────────────┐  ┌─────────────────┐                │   │
│  │  │  Analytics      │  │  StoryGenerator │                │   │
│  │  │  • events       │  │  • prompts      │                │   │
│  │  │  • metrics      │  │  • generations  │                │   │
│  │  └─────────────────┘  └─────────────────┘                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────┐  ┌──────────────────────────────────┐ │
│  │   COSMOS DB          │  │   BLOB STORAGE                   │ │
│  │   • Scenarios (JSON) │  │   • Images (CDN-backed)          │ │
│  │   • CharacterMaps    │  │   • Music (CDN-backed)           │ │
│  │   • ContentBundles   │  │   • User uploads                 │ │
│  └──────────────────────┘  └──────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:

- PostgreSQL is cheaper for structured data
- ACID transactions for user/account data
- SQL joins for complex queries (reports, analytics)
- Cosmos DB optimized for read-heavy document content
- PostgreSQL JSONB can handle semi-structured data

**Cons**:

- Significant migration effort
- Two databases to manage
- Cross-database consistency challenges
- EF Core split context complexity

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Cost Efficiency | 4 | 1.00 |
| Performance | 4 | 0.80 |
| Data Consistency | 5 | 0.75 |
| Developer Experience | 3 | 0.45 |
| Scalability | 4 | 0.60 |
| Extensibility | 4 | 0.40 |
| **Total** | | **4.00** |

---

### Option 3: PostgreSQL Only (Full Migration)

**Description**: Consolidate everything to PostgreSQL using JSONB for document storage.

```
┌─────────────────────────────────────────────────────────────────┐
│                        Data Architecture                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                 POSTGRESQL (Single Database)              │   │
│  │                                                           │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐ │   │
│  │  │ users       │ │ scenarios   │ │ game_sessions       │ │   │
│  │  │ accounts    │ │ (JSONB)     │ │ session_choices     │ │   │
│  │  │ profiles    │ │ characters  │ │ session_scores      │ │   │
│  │  │ badges      │ │ (JSONB)     │ │                     │ │   │
│  │  └─────────────┘ └─────────────┘ └─────────────────────┘ │   │
│  │                                                           │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐ │   │
│  │  │ content_    │ │ analytics   │ │ story_generator     │ │   │
│  │  │ bundles     │ │ events      │ │ prompts             │ │   │
│  │  │ (JSONB)     │ │ metrics     │ │ generations         │ │   │
│  │  └─────────────┘ └─────────────┘ └─────────────────────┘ │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                     BLOB STORAGE                          │   │
│  │   • Images (CDN-backed)  • Music (CDN-backed)            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:

- Single database to manage
- Lowest operational cost
- Full SQL capabilities + JSONB flexibility
- Unified backup/restore
- Simpler connection management

**Cons**:

- Major migration effort
- JSONB queries less performant than Cosmos DB for documents
- Lose Cosmos DB global distribution capability
- PostgreSQL requires capacity planning (not serverless)

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Cost Efficiency | 5 | 1.25 |
| Performance | 3 | 0.60 |
| Data Consistency | 5 | 0.75 |
| Developer Experience | 4 | 0.60 |
| Scalability | 3 | 0.45 |
| Extensibility | 3 | 0.30 |
| **Total** | | **3.95** |

---

### Option 4: Polyglot Persistence with Domain Boundaries

**Description**: Each service owns its data with clear API boundaries. Use the right database for each domain.

```
┌─────────────────────────────────────────────────────────────────┐
│                    Data Architecture (Polyglot)                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────┐    ┌────────────────────┐               │
│  │  MYSTIRA.APP       │    │  ADMIN SERVICE     │               │
│  │  (Player Domain)   │    │  (Content Domain)  │               │
│  │  ┌──────────────┐  │    │  ┌──────────────┐  │               │
│  │  │ Cosmos DB    │  │◄───┤  │ Cosmos DB    │  │               │
│  │  │ • Profiles   │  │API │  │ • Scenarios  │  │               │
│  │  │ • Sessions   │  │    │  │ • Characters │  │               │
│  │  │ • Scores     │  │    │  │ • Bundles    │  │               │
│  │  └──────────────┘  │    │  └──────────────┘  │               │
│  └────────────────────┘    └────────────────────┘               │
│                                                                  │
│  ┌────────────────────┐    ┌────────────────────┐               │
│  │  STORY-GENERATOR   │    │  ANALYTICS SVC     │               │
│  │  (AI Domain)       │    │  (Reporting)       │               │
│  │  ┌──────────────┐  │    │  ┌──────────────┐  │               │
│  │  │ PostgreSQL   │  │    │  │ PostgreSQL   │  │               │
│  │  │ • Prompts    │  │    │  │ • Events     │  │               │
│  │  │ • Generations│  │    │  │ • Aggregates │  │               │
│  │  └──────────────┘  │    │  └──────────────┘  │               │
│  └────────────────────┘    └────────────────────┘               │
│                                                                  │
│  ┌────────────────────┐    ┌────────────────────┐               │
│  │  PUBLISHER         │    │  CHAIN             │               │
│  │  (Blockchain)      │    │  (Ledger)          │               │
│  │  ┌──────────────┐  │    │  ┌──────────────┐  │               │
│  │  │ Redis Cache  │  │    │  │ LevelDB/File │  │               │
│  │  │ • Queue      │  │    │  │ • Blocks     │  │               │
│  │  │ • State      │  │    │  │ • State      │  │               │
│  │  └──────────────┘  │    │  └──────────────┘  │               │
│  └────────────────────┘    └────────────────────┘               │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                   SHARED BLOB STORAGE                     │   │
│  │   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐      │   │
│  │   │ /images     │  │ /music      │  │ /uploads    │      │   │
│  │   │ (CDN)       │  │ (CDN)       │  │ (private)   │      │   │
│  │   └─────────────┘  └─────────────┘  └─────────────┘      │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**:

- Clear domain boundaries
- Each service can scale independently
- Right tool for each job
- Supports microservices evolution
- Easier to reason about data ownership

**Cons**:

- Most complex to implement
- Cross-service queries require APIs
- Data duplication for read optimization
- Higher operational overhead

**Scoring**:
| Criterion | Score (1-5) | Weighted |
|-----------|-------------|----------|
| Cost Efficiency | 3 | 0.75 |
| Performance | 5 | 1.00 |
| Data Consistency | 4 | 0.60 |
| Developer Experience | 3 | 0.45 |
| Scalability | 5 | 0.75 |
| Extensibility | 5 | 0.50 |
| **Total** | | **4.05** |

---

## Decision Matrix Summary

| Option                  | Cost | Perf | Consistency | DevEx | Scale | Extend | **Total** |
| ----------------------- | ---- | ---- | ----------- | ----- | ----- | ------ | --------- |
| 1. Cosmos DB Primary    | 2    | 4    | 3           | 4     | 5     | 4      | **3.50**  |
| 2. PostgreSQL + Cosmos  | 4    | 4    | 5           | 3     | 4     | 4      | **4.00**  |
| 3. PostgreSQL Only      | 5    | 3    | 5           | 4     | 3     | 3      | **3.95**  |
| 4. Polyglot Persistence | 3    | 5    | 4           | 3     | 5     | 5      | **4.05**  |

---

## Recommendation

### Short-Term (0-6 months): **Option 2 - PostgreSQL + Cosmos DB Hybrid**

Start migrating transactional data to PostgreSQL while keeping Cosmos DB for document storage:

1. **Phase 1: PostgreSQL for Analytics & Story-Generator** (Already done)
   - Story-Generator already uses PostgreSQL
   - Add analytics tables to shared PostgreSQL

2. **Phase 2: Migrate User Domain to PostgreSQL**
   - Users, Accounts, Profiles → PostgreSQL
   - Keep badge configurations in Cosmos DB (document-heavy)

3. **Phase 3: Keep Content in Cosmos DB**
   - Scenarios (complex nested JSON) stay in Cosmos DB
   - CharacterMaps, MediaMetadata stay in Cosmos DB

### Long-Term (6-18 months): **Evolve toward Option 4 - Polyglot Persistence**

As services mature:

1. Extract Analytics as separate service with dedicated PostgreSQL
2. Add Redis for caching frequently accessed content
3. Implement event-driven sync between domains

---

## API Boundaries for Data Access

### Service-to-Service Communication

```
┌───────────────────────────────────────────────────────────────────┐
│                       API Gateway / Front Door                     │
├───────────────────────────────────────────────────────────────────┤
│                                                                    │
│  ┌──────────────────┐      ┌──────────────────┐                   │
│  │   App API        │      │   Admin API      │                   │
│  │   /api/v1/*      │      │   /admin/api/*   │                   │
│  └────────┬─────────┘      └────────┬─────────┘                   │
│           │                         │                              │
│           ▼                         ▼                              │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │              INTERNAL SERVICE MESH                          │   │
│  │                                                             │   │
│  │   App API ──────► Admin API (Content Sync)                 │   │
│  │                   POST /internal/scenarios/publish          │   │
│  │                                                             │   │
│  │   Admin API ────► Story-Generator (Generate)               │   │
│  │                   POST /internal/generate                   │   │
│  │                                                             │   │
│  │   Publisher ────► Chain (Register IP)                      │   │
│  │                   POST /internal/register                   │   │
│  │                                                             │   │
│  │   Analytics ◄──── All Services (Events)                    │   │
│  │                   POST /internal/events                     │   │
│  └────────────────────────────────────────────────────────────┘   │
└───────────────────────────────────────────────────────────────────┘
```

### Data Ownership Rules

| Domain              | Owner Service   | Database            | Consumers           |
| ------------------- | --------------- | ------------------- | ------------------- |
| Users & Accounts    | App API         | PostgreSQL (future) | Admin API (read)    |
| Profiles & Progress | App API         | Cosmos DB           | Admin API (read)    |
| Scenarios & Content | Admin API       | Cosmos DB           | App API (read)      |
| Game Sessions       | App API         | Cosmos DB           | Analytics (events)  |
| Generated Content   | Story-Generator | PostgreSQL          | Admin API (read)    |
| Analytics           | Analytics Svc   | PostgreSQL          | Admin API (reports) |
| IP Registration     | Publisher       | Redis + Chain       | Admin API (status)  |

---

## Media Storage Strategy

### Tiered Storage

```
┌─────────────────────────────────────────────────────────────────┐
│                      BLOB STORAGE TIERS                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  HOT TIER (Frequently Accessed)                           │   │
│  │  Container: mystira-media-hot                             │   │
│  │  • Character avatars                                      │   │
│  │  • Active scenario images                                 │   │
│  │  • UI assets                                              │   │
│  │  CDN: Azure Front Door with caching                      │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  COOL TIER (Infrequently Accessed)                        │   │
│  │  Container: mystira-media-cool                            │   │
│  │  • Archived scenarios                                     │   │
│  │  • Historical user uploads                                │   │
│  │  • Backup media                                           │   │
│  │  Auto-tier after 30 days of no access                    │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  ARCHIVE TIER (Rarely Accessed)                           │   │
│  │  Container: mystira-media-archive                         │   │
│  │  • Deleted content (retention)                            │   │
│  │  • Audit logs                                             │   │
│  │  • Old analytics data                                     │   │
│  │  Auto-tier after 90 days of no access                    │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  MUSIC STORAGE (Optimized for Streaming)                  │   │
│  │  Container: mystira-music                                 │   │
│  │  • Background music (OGG/MP3)                             │   │
│  │  • Sound effects                                          │   │
│  │  • Audio sprites                                          │   │
│  │  CDN: Streaming-optimized with range requests            │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

### CDN Configuration

| Content Type          | Cache TTL | Origin Shield | Compression |
| --------------------- | --------- | ------------- | ----------- |
| Images (PNG/JPG/WebP) | 7 days    | Yes           | Yes         |
| Music (OGG/MP3)       | 30 days   | Yes           | No          |
| YAML/JSON configs     | 1 hour    | No            | Yes         |
| User uploads          | 1 day     | No            | Yes         |

---

## Implementation Phases

### Phase 1: Foundation (Month 1-2)

- [ ] Define PostgreSQL schema for user domain
- [ ] Set up database migration tooling (EF Core Migrations)
- [ ] Create internal API contracts between services
- [ ] Implement blob storage tiering

### Phase 2: User Domain Migration (Month 2-4)

- [ ] Migrate Users/Accounts to PostgreSQL
- [ ] Keep Cosmos DB profiles synced during transition
- [ ] Add Redis caching for session data
- [ ] Update App API to use dual-write pattern

### Phase 3: Analytics Extraction (Month 4-6)

- [ ] Create Analytics service
- [ ] Implement event ingestion from all services
- [ ] Build reporting database in PostgreSQL
- [ ] Create dashboards for Admin UI

### Phase 4: Optimization (Month 6+)

- [ ] Remove dual-write, cutover to PostgreSQL
- [ ] Optimize Cosmos DB for read-heavy workloads
- [ ] Implement CQRS for complex queries
- [ ] Add data archival policies

---

## Consequences

### Positive

- Clear data ownership and boundaries
- Cost optimization through right-sizing databases
- Better performance for each data type
- Improved maintainability with domain separation

### Negative

- Increased operational complexity initially
- Cross-service queries require API calls
- Data synchronization challenges
- Learning curve for polyglot persistence

### Neutral

- Requires discipline in maintaining boundaries
- May need to revisit as scale changes
- Documentation and training investment

---

## References

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0005: Service Networking](./0005-service-networking-and-communication.md)
- [Azure Cosmos DB vs PostgreSQL](https://learn.microsoft.com/en-us/azure/cosmos-db/)
- [Polyglot Persistence Pattern](https://martinfowler.com/bliki/PolyglotPersistence.html)
