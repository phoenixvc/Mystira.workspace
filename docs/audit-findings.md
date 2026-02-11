# Core Audit Findings

## 1. Bugs (Logic & External Integrations)
| ID | Description | Severity | Impact | Effort | Evidence | Status |
|----|-------------|----------|--------|--------|----------|--------|
| **BUG-01** | DFS Cycle Logic Flaw: Recursion in `CalculateBadgeScoresQueryHandler` adds paths on cycle detection without leaf verification. | High | Med | M | `CalculateBadgeScoresQueryHandler.cs:204` | **FIXED** - Extracted to `ScenarioGraphTraversal`, cycle now preserves accumulated scores |
| **BUG-02** | WhatsApp Template Param Mapping: Logic for template parameters is outdated for Azure SDK 1.1.0+. | Med | Low | S | `WhatsAppBotService.cs:372` | **RESOLVED** - SDK updated to 1.1.0, template parameter mapping modernized |
| **BUG-03** | IP Asset Parsing: `StoryProtocolClient` lacks actual ABI and log parsing logic for IP Asset extraction. | High | High | L | `StoryProtocolClient.cs:73, 109` | Open - Requires Python sidecar microservice (ADR-0010) |
| **BUG-04** | Handler Coupling: `StartGameSessionCommandHandler` and `CreateAccountCommandHandler` inject concrete use case classes instead of interfaces. | Low | Low | S | `StartGameSessionCommandHandler.cs:15` | **FIXED** - Extracted `ICreateGameSessionUseCase` and `ICreateAccountUseCase` interfaces |

## 2. Performance & Structural Improvements
| ID | Description | Severity | Impact | Effort | Evidence |
|----|-------------|----------|--------|--------|----------|
| **PERF-01** | Image Cache Bloat: `imageCacheManager.js` lacks eviction/TTL strategy for Blobs in IndexedDB. | Med | High | M | `imageCacheManager.js` | **FIXED** - Already had MAX_ITEMS (100) + TTL (7d) + pruneCache(). Added TTL check on cache read to prevent serving expired items. |
| **STR-01** | Hardcoded Badge Thresholds: `CheckAchievementsUseCase` uses constants instead of dynamic config. | Low | Med | S | `CheckAchievementsUseCase.cs:42` | **RESOLVED** - Already loads thresholds dynamically from `IBadgeConfigurationRepository`; 3.0f is null-coalescing fallback only |
| **STR-02** | Hardcoded Explorer URLs: IP Explorer URLs are hardcoded in handlers instead of injected via Options. | Low | Low | S | `GetBundleIpStatusQueryHandler.cs:16` | **RESOLVED** - Already uses `IOptions<StoryProtocolOptions>.ExplorerBaseUrl` |

## 3. UI/UX Improvements
| ID | Description | Severity | Impact | Effort | Evidence |
|----|-------------|----------|--------|--------|----------|
| **UX-01** | Missing Loading States: Transition between music scenes in PWA lacks visual feedback. | Med | Med | S | `SceneAudioOrchestrator.cs` (logic gaps) |
| **UX-02** | Feature Flagged UI: 'Golden Tubes' in `HeroSection.razor` are commented out/disabled. | Low | Low | S | `HeroSection.razor:12` |

## 4. Refactoring Opportunities
| ID | Description | Severity | Impact | Effort | Evidence |
|----|-------------|----------|--------|--------|----------|
| **REF-01** | Misleading Naming: `IndexedDbService` is transient, not persistent. Rename to `InMemoryStore`. | Med | Med | S | `IndexedDbService.cs` |
| **REF-02** | Domain Mapping Duplication: Multiple `ToDomainModel` patterns in `YamlScenario.cs` could be simplified. | Low | Med | M | `YamlScenario.cs` |

## 5. Resolved Items (2026-02-10)

### Refactoring Completed
- **CalculateBadgeScoresQueryHandler** reduced from 323 lines/24 conditionals to ~158 lines by extracting `ScenarioGraphTraversal` and `PercentileCalculator` services
- **Create/UpdateScenarioUseCase** DRY violation fixed by extracting `ScenarioSchemaValidator`
- **BUG-01** fixed: DFS cycle detection now preserves accumulated compass scores

### Security Items Verified (Previously Documented as Open)
- **SEC-1** (Secrets): Config files use empty placeholders with warnings; Azure Key Vault integration present in Program.cs
- **SEC-2** (Hardcoded creds): No hardcoded secrets found in source; AdminAuth password requires User Secrets
- **SEC-3** (Swagger): Already guarded — only enabled in Development/Staging environments
- **SEC-4** (PII logging): `PiiMask.MaskEmail()` and `LogAnonymizer.HashId()` used consistently throughout
- **Rate limiting**: Configured — 100 req/min global, 5 req/15min for auth endpoints

### BUG-04 Fixed (2026-02-11)
- Extracted `ICreateGameSessionUseCase` and `ICreateAccountUseCase` interfaces
- Updated `StartGameSessionCommandHandler` and `CreateAccountCommandHandler` to use interfaces (dependency inversion)
- Updated DI registrations in `UseCaseExtensions.cs`
- Added handler tests that were previously impossible due to concrete class injection

### BUG-02 Verified Resolved (2026-02-11)
- WhatsApp SDK already updated to Azure.Communication.Messages 1.1.0
- Template parameter mapping modernized in WhatsAppBotService.cs (lines 368-376)
- Only remaining: integration testing with actual Azure ACS

### Test Coverage Added
- 60+ new test files covering Account, Scenario, ContentBundle, Badge, UserProfile, GameSession use cases
- CharacterMap use case tests (7): Create, Update, Delete, Get, GetAll, Import, Export
- Media use case tests (7): Upload, Download, Delete, Get, GetByFilename, List, UpdateMetadata
- Avatar use case tests (6): Create, Update, Delete, GetConfigurations, GetByAgeGroup, AssignToAgeGroup
- Infrastructure repository tests: GameSession, Account, Scenario, MediaAsset repositories + UnitOfWork
- EF Core InMemory integration tests for IQueryable-based use cases (GetScenarios, ListMedia)
- API controller tests for Bundles, BadgeImages, ProfileAxisScores
- Extracted service tests for PercentileCalculator and ScenarioGraphTraversal
- Handler tests for StartGameSession and CreateAccount (enabled by BUG-04 fix)
- PWA component tests (bUnit): EmptyState, SkeletonLoader, WarningModal, CoppaCompliancePill, ThemeToggle

## 6. New Features (High Value)
| Feature | Value Proposition | Integration Point |
|---------|-------------------|-------------------|
| **Real-time IP Attribution** | Transparent ownership tracking for story contributors. | `StoryProtocolClient` |
| **Advanced Character Sync** | Persistence of character choices across devices (PWA <-> Mobile). | `GameSessions` Command |
| **Adaptive Music Engine** | Dynamic cross-fading based on user engagement levels. | `AudioBus` / `SceneAudioOrchestrator` |
