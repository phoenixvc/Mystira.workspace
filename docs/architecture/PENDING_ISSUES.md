# Pending Architecture Issues

> These issues should be created in GitHub. Generated from system architecture analysis on 2026-01-11.

---

## Issue 1: Remove or Commit to 4-Phase Cosmos-to-PostgreSQL Migration

**Labels:** `tech-debt`, `architecture`, `decision-needed`

### Problem
The codebase contains a complex 4-phase migration schema (Cosmos → PostgreSQL) with 530 lines of dual-write infrastructure in `PolyglotRepository.cs`, but it's been stuck at Phase 0 (Cosmos only) with no progress.

### Current State
- Phase 0: CosmosOnly (CURRENT)
- Phase 1: DualWriteCosmosRead (NOT IMPLEMENTED)
- Phase 2: DualWritePostgresRead (NOT IMPLEMENTED)
- Phase 3: PostgresOnly (THEORETICAL)

### Files affected:
- `packages/app/src/Mystira.App.Infrastructure.Data/Polyglot/PolyglotRepository.cs` (530 lines)
- `AdminDataMigrationOptions.cs`
- `hybrid-data-strategy-roadmap.md`

### Engineering Overhead
- Complex compensation logic for dual-write failures
- Consistency validation infrastructure (disabled)
- Metrics for secondary write successes/failures
- All this code provides no value while at Phase 0

### Options
1. **Commit to migration**: Set a timeline and progress to Phase 1
2. **Remove dual-write**: Simplify to Cosmos-only until PostgreSQL is required
3. **Keep as-is**: Accept tech debt and leave it dormant

### Recommendation
Either commit to a migration timeline or remove the dual-write machinery to reduce maintenance burden.

---

## Issue 2: Calibrate Retry Policies Based on Actual Service Behavior

**Labels:** `observability`, `performance`, `enhancement`

### Problem
Retry policies use industry defaults rather than values calibrated to actual service behavior.

### Current Configuration (ResilienceOptions.cs)
```csharp
MaxRetries: 3                          // Generic default
BaseDelaySeconds: 2                    // Generic default
CircuitBreakerThreshold: 5             // Generic default
CircuitBreakerDurationSeconds: 30      // Generic default
TimeoutSeconds: 30
LongRunningTimeoutSeconds: 300         // LLM-specific
```

### Questions to Answer
- How often do retries actually succeed on attempt 1 vs 2 vs 3?
- Is 30-second timeout appropriate for Cosmos DB operations?
- Should Anthropic API and Azure OpenAI have different retry strategies?

### Proposed Solution
1. Add metrics to track retry success rate by attempt number
2. Profile actual failure patterns over 2-4 weeks
3. Calibrate per-service retry configurations:
   ```csharp
   CosmosDbRetry: { MaxRetries: 5, BaseDelay: 1s }  // Fast retries
   LlmApiRetry: { MaxRetries: 2, BaseDelay: 5s }    // Slow retries
   ExternalApiRetry: { MaxRetries: 3, BaseDelay: 2s } // Standard
   ```

---

## Issue 3: Establish Load Test Baselines for HPA Thresholds

**Labels:** `performance`, `infrastructure`, `testing`

### Problem
Kubernetes HPA (Horizontal Pod Autoscaler) thresholds use industry defaults without measured baselines.

### Current Configuration
```yaml
# Admin API
minReplicas: 2
maxReplicas: 5
scaleAt:
  cpu: 70%
  memory: 80%

# Story Generator
minReplicas: 2
maxReplicas: 10
scaleAt:
  cpu: 70%
  memory: 80%
```

### Questions to Answer
- At what CPU % does latency actually degrade?
- At what memory % do OOM events occur?
- What's the actual memory usage under load for each service?
- How many concurrent users can be supported per replica?

### Proposed Solution
1. Set up load testing environment (k6, Locust, or similar)
2. Run progressive load tests to find degradation points
3. Update HPA thresholds based on actual measurements
4. Document findings for future reference

---

## Issue 4: Validate Cache TTLs Against Data Change Frequency

**Labels:** `observability`, `performance`, `data`

### Problem
Cache TTLs appear to be industry defaults rather than values based on actual data change frequency.

### Current Configuration
```csharp
ContentCacheMinutes: 30     // How often does content actually change?
UserCacheMinutes: 5         // Is 5 minutes appropriate?
MasterDataCacheMinutes: 60  // Master data rarely changes
```

### Questions to Answer
- How often does content actually change in production?
- What's the current cache hit/miss ratio? (Now trackable with CacheMetrics)
- Are we invalidating cache unnecessarily?
- Could we increase TTLs safely?

### Proposed Solution
1. Monitor CacheMetrics for 2-4 weeks (now implemented)
2. Analyze data change frequency patterns
3. Adjust TTLs based on actual usage:
   - If hit rate is low, content changes frequently → reduce TTL
   - If hit rate is high, content is stable → increase TTL
4. Consider per-entity-type TTLs if needed

---

## Issue 5: Evaluate CQRS and Hexagonal Architecture Overhead

**Labels:** `architecture`, `discussion`, `enhancement`

### Problem
The codebase uses CQRS and Hexagonal Architecture patterns that may be over-engineered for current scale.

### Current Implementation
- **CQRS**: 16 Commands, 20 Queries, 32 Specifications
- **Hexagonal**: Ports/Adapters pattern with full abstraction layers

### Justifications (from ADRs)
- "229 architectural violations identified"
- "138 direct dependencies from Application → Infrastructure"
- "Hard to swap infrastructure implementations"

### Questions to Consider
- At current scale, is CQRS providing measurable benefit?
- Has hexagonal architecture enabled any infrastructure swaps?
- Are all 32 Specifications actively reused?
- What's the maintenance cost vs benefit?

### Recommendation
- **Not recommending removal** - these patterns have value
- **Recommend evaluation** at scale milestones
- Document when these patterns provided concrete benefits

---

## Issue 6: Complete gRPC Implementation for Chain Service (When Needed)

**Labels:** `architecture`, `performance`, `blocked`

### Problem
ADR-0013 proposes gRPC for C#-to-Python Chain service communication but implementation is incomplete.

### Current State
- ADR-0013 approved
- Chain service still uses REST
- gRPC endpoint configured but not functional

### Claimed Benefits (from ADR)
- 4-5x faster latency
- 5-10x smaller payloads
- 60% CPU reduction

### Before Implementing
1. Benchmark current REST performance
2. Verify inter-service latency is actually a bottleneck
3. Consider complexity cost: Protocol Buffers, code generation, gRPC debugging
4. Browser clients still need REST/gRPC-Web proxy

### Recommendation
- Keep as low priority until latency is proven bottleneck
- Benchmark before committing significant effort
- Industry benchmarks don't necessarily apply to our workload

---

## Issue 7: Add Observability Dashboard for Cache and Retry Metrics

**Labels:** `observability`, `infrastructure`, `enhancement`

### Problem
New metrics are being collected (CacheMetrics, retry tracking) but no dashboard exists to visualize them.

### Metrics Now Available
```
# Cache Metrics (CacheMetrics.cs)
mystira.cache.hits
mystira.cache.misses
mystira.cache.errors
mystira.cache.sets
mystira.cache.removes
mystira.cache.latency

# Circuit Breaker Metrics (existing)
mystira.circuit_breaker.state_changes
mystira.circuit_breaker.rejections
mystira.circuit_breaker.successes
mystira.circuit_breaker.failures
```

### Proposed Solution
1. Create Application Insights workbook with:
   - Cache hit/miss ratio over time
   - Cache latency percentiles
   - Cache errors by operation type
   - Circuit breaker state timeline
2. Add alerts for:
   - Cache hit rate < 50%
   - Cache error rate > 5%
   - Circuit breaker open events

---

## Issue 8: Clean Up Unused Specification Classes

**Labels:** `tech-debt`, `cleanup`

### Problem
32 Specification classes exist, but many appear to be used only once.

### Investigation Needed
1. Audit all Specification classes
2. Identify which are used multiple times (true reuse)
3. Consider inlining single-use specifications
4. Document reusable specifications for team awareness

### Files to Review
- `packages/app/src/Mystira.App.Application/Specifications/`
- Usage across all handlers

---

## Summary Table

| Issue | Priority | Effort | Type |
|-------|----------|--------|------|
| #1 Migration Schema | High | Large | Decision |
| #2 Retry Calibration | Medium | Medium | Improvement |
| #3 Load Test Baselines | Medium | Medium | Testing |
| #4 Cache TTL Validation | Medium | Small | Improvement |
| #5 Architecture Evaluation | Low | Small | Discussion |
| #6 gRPC Implementation | Low | Large | Enhancement |
| #7 Observability Dashboard | Medium | Medium | Improvement |
| #8 Specification Cleanup | Low | Small | Cleanup |

---

*Generated: 2026-01-11*
*Source: System Architecture Deep Dive analysis*
