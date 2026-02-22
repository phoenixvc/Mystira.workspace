# PR Analysis: consolidate-branches

**Branch:** `claude/consolidate-branches-3QrMR`
**Base:** `origin/dev`
**Stats:** 358 files changed, +22,380 / -7,576 lines (86 test files, +13,786 / -370)

---

## What Was Done

| Phase | Commit | Description |
|-------|--------|-------------|
| A | `60b88c8` | Add `CancellationToken` to all 72 UseCase `ExecuteAsync` methods and service interfaces |
| B Wave 1 | `9ac5e8e` | Handler tests: Archetypes, EchoTypes, FantasyThemes (34 tests) |
| B Waves 1-2 | `a643781` | Handler tests: CompassAxes, AgeGroups, Attribution, CharacterMaps, Characters, Badges, ContentBundles, Avatars, MediaMetadata |
| B Waves 3-4 | `7883ae4` | Handler tests: Discord, Health, Royalties, Scenarios, UserBadges, UserProfiles |
| C | `6af8bc9` | Configuration extension method tests (Teams, WhatsApp, Payments, Caching, Data) |
| D1 | `a251177` | Bug fixes: AwardBadge missing duplicate check + profile update; MakeChoice missing scenario context |
| D3 | `1d9f6da` | Consolidate 3 handler/UseCase pairs (EndGameSession, AddCompletedScenario, CreateUserProfile) |
| E | `9f66fe5` | UseCase unit tests: CreateGameSession, Pause/Resume, AddProfile, RemoveProfile, RevokeBadge |

---

## Bugs Found & Fixed

### 1. AwardBadgeCommandHandler - Missing Duplicate Check & Profile Update (FIXED)
**Severity:** High
**File:** `CQRS/UserBadges/Commands/AwardBadgeCommandHandler.cs`

The handler was creating badges without checking if the user already had one for the same badge configuration. It also never updated the `UserProfile.EarnedBadges` list, causing the profile's badge collection to be stale.

**Fix:** Added `GetByUserProfileIdAndBadgeConfigIdAsync` check (returns existing badge if found) and `IUserProfileRepository` dependency to call `profile.AddEarnedBadge(badge)`.

### 2. MakeChoiceCommandHandler - Missing Scenario Context (FIXED)
**Severity:** Critical
**File:** `CQRS/GameSessions/Commands/MakeChoiceCommandHandler.cs`

The handler was directly mutating the session without loading the scenario, meaning it couldn't validate the choice branch, resolve the active character, update compass values, or detect session completion.

**Fix:** Rewrote to delegate to `MakeChoiceUseCase` which contains full business logic.

### 3. EndGameSessionCommandHandler - Missing ElapsedTime & Pause Cleanup (FIXED via D3)
**Severity:** Medium
**File:** `CQRS/GameSessions/Commands/EndGameSessionCommandHandler.cs`

The handler set `EndTime` but never calculated `ElapsedTime` or cleaned up pause state. The `EndGameSessionUseCase` handles both correctly.

**Fix:** Handler now delegates to `EndGameSessionUseCase`.

---

## Potential Bugs (Not Yet Fixed)

### 4. StartGameSessionCommandHandler - Stale CreateGameSessionUseCase
**Severity:** Medium
**File:** `CQRS/GameSessions/Commands/StartGameSessionCommandHandler.cs` (307 lines)

The handler contains mature, complex session-creation logic (duplicate cleanup, character assignments, compass init, starting scene detection). Meanwhile `CreateGameSessionUseCase` (40 lines) exists but is **never called** and has much simpler logic. If someone calls `CreateGameSessionUseCase` directly, they get an incomplete session.

**TODO:** Either delete the unused UseCase or make the handler delegate to it after porting the handler's logic.

### 5. CreateAccountCommandHandler - Property Name Mismatch
**Severity:** Low (correctness risk)
**File:** `CQRS/Accounts/Commands/CreateAccountCommandHandler.cs`

The `CreateAccountCommand` uses `ExternalUserId` while `CreateAccountRequest` (Mystira.Contracts NuGet) uses `Auth0UserId`. If someone consolidates without noticing, the external user ID won't be set.

**TODO:** Align property names when consolidating, or add explicit mapping.

### 6. GetInProgressSessionsQueryHandler - Zombie Session Filtering Not in UseCase
**Severity:** Low
**File:** `CQRS/GameSessions/Queries/GetInProgressSessionsQueryHandler.cs` (136 lines)

Handler filters out "zombie" sessions (no current scene, no history) and deduplicates by `(ScenarioId, ProfileId)`. The corresponding `GetInProgressSessionsUseCase` is a thin repo wrapper that does neither.

**TODO:** Move filtering logic to UseCase or document why it's handler-only.

---

## Architecture Issues

### Handler/UseCase Delegation Gap

The intended architecture is: `Controller -> IMessageBus -> Handler -> UseCase -> Repository`

**Current state after this PR:**
- **4 handlers** (4%) properly delegate to UseCases (EndGameSession, AddCompletedScenario, CreateUserProfile, MakeChoice)
- **96+ handlers** (96%) still implement their own business logic
- **40 UseCases** (56% of 72) are never referenced by any handler — effectively dead code

### Top Offenders (Handlers with Complex Logic That Should Be UseCases)

| Handler | Lines | Why It Needs Extraction |
|---------|-------|------------------------|
| `CalculateBadgeScoresQueryHandler` | 323 | DFS graph traversal, percentile math, cycle detection |
| `StartGameSessionCommandHandler` | 307 | Duplicate cleanup, compass init, character assignments, scene detection |
| `GetInProgressSessionsQueryHandler` | 136 | Zombie filtering, dedup, compass recalculation, DTO mapping |
| `GetProfileBadgeProgressQueryHandler` | 106 | 4-repo orchestration, tier matching, axis grouping |
| `UpdateUserProfileCommandHandler` | 104 | 10+ field-level updates with different validation rules |
| `FinalizeGameSessionCommandHandler` | 96 | Multi-profile scoring, cumulative axis calculation, badge awarding |
| `GetScenariosWithGameStateQueryHandler` | 94 | 2-repo join, game state inference |

---

## DRY Violations

### 1. CRUD Handler Boilerplate (~675 lines duplicated)
15 master-data handlers (AgeGroups, Archetypes, EchoTypes, FantasyThemes, CompassAxes x Create/Update/Delete) follow identical patterns:
```text
validate input -> create/update entity -> repo.Add/Update -> unitOfWork.Save -> cache.Invalidate -> log
```

**Fix:** Extract a generic `MasterDataCommandHandler<TEntity, TRepository>` base or factory.

### 2. GetByIdAsync + Null Check Pattern (17 occurrences)
```csharp
var entity = await repository.GetByIdAsync(id, ct);
if (entity == null) { logger.LogWarning("Not found: {Id}", id); return null; }
```

**Fix:** Create `GetByIdOrLogAsync<T>` extension method on `IRepository`.

### 3. Argument Validation (223 occurrences across 101 files)
`string.IsNullOrWhiteSpace` / `string.IsNullOrEmpty` repeated everywhere with slightly different error messages.

**Fix:** Create `Guard.NotEmpty(value, paramName)` or `ValidationHelper.ValidateRequiredIds(...)`.

### 4. Session DTO Mapping (duplicated in 2+ handlers)
`GetInProgressSessionsQueryHandler` and `GetSessionsByAccountQueryHandler` both map `GameSession -> GameSessionResponse` with ~45 lines of identical character/compass mapping.

**Fix:** Extract `GameSessionResponseMapper`.

### 5. Session Status Transition Logic (3 handlers)
Pause, Resume, and ProgressScene all validate status before transitioning. Similar pattern each time.

**Fix:** Create `SessionStateMachine` or domain method on `GameSession` (e.g. `session.Pause()`, `session.Resume()`).

---

## Test Coverage Gaps

### Untested UseCases by Priority

**CRITICAL (test immediately):**

| UseCase | Lines | Risk |
|---------|-------|------|
| `MakeChoiceUseCase` | 161 | Core gameplay: branch validation, compass tracking, echo logs, auto-completion |
| `ValidateScenarioUseCase` | 150 | Graph traversal, scene reachability, axis/archetype validation |
| `UploadMediaUseCase` | 149 | File validation, hash calculation, blob storage |

**HIGH (test this sprint):**

| UseCase | Lines | Risk |
|---------|-------|------|
| `CreateAccountUseCase` | 64 | Email uniqueness, initialization |
| `AwardBadgeUseCase` | 89 | Multi-repo orchestration, duplicate detection |
| `CheckBundleAccessUseCase` | 78 | Authorization: free tier, subscription, purchase checks |
| `GetScenariosUseCase` | 105 | Complex filtering/pagination |
| `CreateScenarioUseCase` | 94 | JSON schema validation, mapper chain |
| `UpdateScenarioUseCase` | 86 | JSON schema validation, mapper chain |
| `GetSessionStatsUseCase` | 74 | Data aggregation, compass recalculation |

**MEDIUM (65 UseCases):** Multi-repo orchestration, conditional updates, enum mapping

**LOW (36 UseCases):** Thin repository wrappers (Get/Delete/List patterns)

### Test Quality Issues in Existing Tests

1. **Missing `SaveChangesAsync` verification** in `AddUserProfileToAccountUseCaseTests` — the "already linked" test doesn't verify `SaveChangesAsync` was NOT called
2. **Fragile count assertion** in `CheckAchievementsUseCaseTests:265` — `result.Should().HaveCount(3)` breaks if a new achievement type is added; should assert on specific types
3. **No concurrent access tests** — `CreateGameSessionUseCase` auto-completes existing sessions but no test for race conditions
4. **Logger mocks never verified** — 50+ logger mocks created across the test suite but only 16 are ever verified
5. **Missing negative configuration tests** — Infrastructure tests (Teams, Payments) don't test missing required config values

---

## Missed Opportunities

### Phase D2: Complex Handler Logic Extraction (Deferred)
These handlers contain 100+ lines of business logic that should be UseCases but were too risky to extract without compilation:
- `StartGameSessionCommandHandler` (307 lines)
- `CalculateBadgeScoresQueryHandler` (323 lines)
- `FinalizeGameSessionCommandHandler` (96 lines)
- `GetInProgressSessionsQueryHandler` (136 lines)
- `GetProfileBadgeProgressQueryHandler` (106 lines)

### Phase D4: Dead UseCase Cleanup (Deferred)
40 UseCases are never called by any handler. These should be reviewed:
- **Delete if truly dead:** `GetAccountUseCase`, `GetAccountByEmailUseCase`, `GetUserBadgesUseCase`, `ListMediaUseCase`, etc.
- **Wire up if business logic differs from handler:** `CreateGameSessionUseCase`, `GetInProgressSessionsUseCase`

### CreateAccount Consolidation (Skipped)
`CreateAccountCommandHandler` still reimplements `CreateAccountUseCase` logic. Blocked by `ExternalUserId` vs `Auth0UserId` property name mismatch in the contracts NuGet package.

### Interface Segregation
- `IScenarioRepository.GetQueryable()` leaks EF Core abstraction into domain layer
- `IUserBadgeRepository` has 5 query methods; most consumers use only 1-2

---

## SOLID Refactoring Recommendations

### Priority 1 (High Impact, Low Risk)
1. **Create `Guard` / `ValidationHelper`** — Eliminate 223 scattered null/empty checks
2. **Create `GameSessionResponseMapper`** — Deduplicate ~90 lines across 2+ handlers
3. **Add domain methods to `GameSession`** — `Pause()`, `Resume()`, `Complete()` with built-in status validation

### Priority 2 (High Impact, Medium Risk)
4. **Extract `MasterDataCommandHandler<T>`** — Eliminate 675 lines of CRUD boilerplate
5. **Consolidate `StartGameSessionCommandHandler` into UseCase** — 307 lines of logic belongs in application layer
6. **Extract `BadgeScoreCalculationService`** from `CalculateBadgeScoresQueryHandler` — DFS algorithm is pure business logic

### Priority 3 (Medium Impact)
7. **Create `SessionStateMachine`** — Centralize status transitions with validation
8. **Split `IScenarioRepository`** — Separate `GetQueryable()` into `IScenarioQueryProvider`
9. **Consolidate remaining handlers** — `CreateAccount`, `LinkProfilesToAccount`, `UpdateUserProfile`
10. **Audit and clean 40 dead UseCases** — Remove or wire up

---

## Summary

| Category | Count |
|----------|-------|
| Bugs found & fixed | 3 |
| Potential bugs remaining | 3 |
| Handlers needing UseCase extraction | 7 (1,166 lines total) |
| Dead UseCases | 40 (56% of all UseCases) |
| DRY violations | 5 major patterns |
| Critical untested UseCases | 3 |
| High-priority untested UseCases | 7 |
| SOLID refactoring opportunities | 10 |
