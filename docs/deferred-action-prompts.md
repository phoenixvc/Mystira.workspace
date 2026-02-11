# Deferred Audit Items — Actionable AI Prompts

Use these prompts in Claude Code sessions in the appropriate repo to execute each fix.

---

## 1. BUG-03: Story Protocol gRPC Adapter (Mystira.Chain repo)

> **Repo**: `justaghost/mystira.chain`
> **Severity**: High | **Effort**: Large

```
Implement a gRPC Story Protocol service per ADR-0010 and ADR-0013 from Mystira.App.

The .NET side already has:
- Port interface `IStoryProtocolService` with 7 methods: RegisterIpAssetAsync, IsRegisteredAsync, GetRoyaltyConfigurationAsync, UpdateRoyaltySplitAsync, PayRoyaltyAsync, GetClaimableRoyaltiesAsync, ClaimRoyaltiesAsync
- Configuration in `ChainServiceOptions` (GrpcEndpoint: localhost:50051, TimeoutSeconds: 120, MaxRetryAttempts: 3)
- Domain model `StoryProtocolMetadata` with IpAssetId, RegistrationTxHash, RegisteredAt, RoyaltyModuleId, Contributors (splits must sum to 100%)
- Use cases: RegisterScenarioIpAssetUseCase, RegisterBundleIpAssetUseCase

Implement:
1. Proto file defining `ChainService` with RPCs matching all 7 IStoryProtocolService methods
2. FastAPI + grpcio server implementing each RPC using the Story Protocol Python SDK
3. IP Asset registration: accept content metadata (title, description, content hash), call Story Protocol SDK to register, return IpAssetId + TxHash
4. Royalty operations: configure splits per contributor list, pay/claim royalties via WIP token (address: 0x1514000000000000000000000000000000000000)
5. Health check endpoint for gRPC reflection
6. Docker compose for local development (expose port 50051)
7. Tests using pytest with mocked Story Protocol SDK calls

The .NET consumer will call this via GrpcChainServiceAdapter (not yet implemented on .NET side — just focus on the Python gRPC server).
```

---

## 2. UX-01 + UX-02: PWA Visual Polish (Mystira.App repo)

> **Repo**: `phoenixvc/Mystira.App`
> **Severity**: Med/Low | **Effort**: Small

```
Fix two PWA visual issues:

**UX-01: Add music transition loading state**

In `src/Mystira.App.PWA/Services/Music/SceneAudioOrchestrator.cs`:
- The `EnterSceneAsync()` method (line 24) performs music transitions (stop/crossfade/play) but exposes no loading state to the UI
- `MusicContext` tracks CurrentTrackId, CurrentProfile, CurrentEnergy but NOT transition state

Fix:
1. Add to MusicContext: `bool IsTransitioning`, `string? TransitionError`
2. In EnterSceneAsync: set IsTransitioning=true at entry, false on completion (after line 108), catch errors into TransitionError
3. Raise a state change event (or use existing INotifyPropertyChanged pattern) so UI can react
4. Create a small `MusicTransitionIndicator.razor` component that shows a subtle loading spinner when IsTransitioning=true — use the existing LoadingIndicator.razor pattern at `src/Mystira.App.PWA/Components/LoadingIndicator.razor`

**UX-02: Enable Golden Tubes SVG in HeroSection**

In `src/Mystira.App.PWA/Components/HeroSection.razor`:
- CSS is fully defined in HeroSection.razor.css (lines 42-336): .golden-lights container, .light-tube-1 through .light-tube-4 with flow/pulse/twirl animations, video-mode vs logo-mode timing
- But the actual SVG markup is completely absent from the razor template

Fix:
1. After the particle canvas (line 9 in HeroSection.razor), add a `<div class="golden-lights @(showVideo ? "video-mode" : "logo-mode")">` containing an SVG with viewBox="0 0 600 500"
2. Create 4 `<g class="light-tube-group light-tube-group-{n}">` groups, each with a bezier `<path>` radiating from center (300,250) outward at different angles
3. Add glow filter in `<defs>` using Gaussian blur + color matrix for the purple/violet gradient (#A78BFA → #7C3AED → #5B21B6)
4. No CSS changes needed — animations are already defined (tubeFlow 3-4s, tubePulse 3.5-5s, twineTwirl 18-25s)
```

---

## 3. REF-01 + REF-02: Naming & Mapping Cleanup (Mystira.App repo)

> **Repo**: `phoenixvc/Mystira.App`
> **Severity**: Low/Med | **Effort**: Small/Medium

```
Fix two refactoring items:

**REF-01: InMemoryStoreService is orphaned dead code**

The service at `src/Mystira.App.PWA/Services/InMemoryStoreService.cs` (388 lines) was already renamed from IndexedDbService, but it's registered in DI (`Program.cs` line 201 as `IInMemoryStoreService`) and NEVER actually used by any component or page.

Decision needed:
- If this service has a planned use case: document it in a TODO comment at the registration site
- If not: remove InMemoryStoreService.cs, IInMemoryStoreService.cs, and the DI registration from Program.cs — it's dead code

**REF-02: Extract YamlScenario mapping to a dedicated mapper**

In `src/Mystira.App.Domain/Models/YamlScenario.cs` (388 lines), 10 Yaml* classes each have a `ToDomainModel()` method totaling ~142 lines of repetitive mapping boilerplate.

Key duplication:
- Legacy alias fallback logic is copy-pasted: `var nextSceneId = !string.IsNullOrWhiteSpace(NextScene) ? NextScene : LegacyNextSceneId;` appears in both YamlScene and YamlBranch
- `ParseEnum<T>` helper is defined only in YamlSceneMusicSettings but needed elsewhere
- Hardcoded `Timestamp = DateTime.UtcNow` in YamlEchoLog.ToDomainModel() should be injectable

Refactor:
1. Create `src/Mystira.App.Domain/Mapping/YamlScenarioMapper.cs` implementing `IYamlScenarioMapper`
2. Move all 10 ToDomainModel() methods into the mapper as `Map(YamlScenario) → Scenario`, `Map(YamlScene) → Scene`, etc.
3. Extract shared helpers: `ResolveLegacyAlias(string? current, string? legacy)` and `ParseEnum<T>(string? value, T defaultValue)`
4. Strip YamlScenario.cs classes down to pure YAML DTOs (properties + YamlMember attributes only)
5. Register IYamlScenarioMapper in DI; update callers (ImportCharacterMapUseCase, scenario import/create flows)
6. Add unit tests for the mapper, especially legacy alias resolution and enum parsing edge cases
```
