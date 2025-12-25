# ADR-0009: Further App Project Segregation Strategy

## Status

**Accepted** - 2025-01-XX

## Context

Following [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md), the Admin API and Admin UI have been successfully extracted to separate repositories (`Mystira.Admin.Api` and `Mystira.Admin.UI`). This ADR addresses whether `Mystira.App` should be further segregated by extracting the Public API and/or PWA to separate repositories.

### Current State After Admin Extraction

**Mystira.App** (remains) contains:

1. **Public API** (`Mystira.App.Api`) - Public-facing REST API
2. **PWA** (`Mystira.App.PWA`) - Blazor WebAssembly frontend
3. **Shared Libraries**:
   - `Mystira.App.Domain` - Core domain models
   - `Mystira.App.Application` - Application layer (CQRS, MediatR)
   - `Mystira.App.Infrastructure.*` - Infrastructure libraries
   - `Mystira.App.Shared` - Shared services
   - `Mystira.App.Contracts` - Shared DTOs
4. **Ops Console** (`tools/Mystira.App.CosmosConsole`) - Command-line utility

### Relationship Between Public API and PWA

**Shared Dependencies**:

- Both depend on `Mystira.App.Domain` (core business logic)
- Both depend on `Mystira.App.Application` (use cases, handlers)
- Both depend on `Mystira.App.Infrastructure.*` (data access, Azure services)
- Both depend on `Mystira.App.Shared` (cross-cutting concerns)
- Both depend on `Mystira.App.Contracts` (DTOs)

**Operational Characteristics**:

- **Public API**: Backend service, deployed to Azure App Service
- **PWA**: Frontend application, deployed to Azure Static Web Apps
- **Deployment**: Already separate (different Azure resources)
- **Security**: Both are public-facing (same security posture)
- **Release Cycles**: Currently coupled (shared libraries require coordinated updates)
- **Team Ownership**: Likely same team (user-facing features)

### Key Differences from Admin Extraction

| Aspect                     | Admin API/UI                                  | Public API/PWA                           |
| -------------------------- | --------------------------------------------- | ---------------------------------------- |
| **Security Posture**       | Internal/admin (higher security requirements) | Public-facing (same security posture)    |
| **User Base**              | Administrators only                           | End users                                |
| **Operational Separation** | Already separate deployments                  | Already separate deployments             |
| **Team Ownership**         | Potentially different teams                   | Likely same team                         |
| **Release Coupling**       | Independent release cycles desired            | Coordinated releases acceptable          |
| **Code Sharing**           | Minimal (admin-specific logic)                | Extensive (shared domain/business logic) |

## Decision

We will **NOT further segregate** `Mystira.App` by extracting Public API or PWA to separate repositories at this time.

### Rationale

#### 1. Shared Domain Model and Business Logic

**Current State**:

- Public API and PWA share the same domain model (`Mystira.App.Domain`)
- Both implement the same business logic (`Mystira.App.Application`)
- Both use the same infrastructure (`Mystira.App.Infrastructure.*`)

**If Segregated**:

- Would require extensive NuGet package management
- Every domain change would require package publishing and updates in multiple repos
- Increased coordination overhead for business logic changes
- Higher risk of version drift between services

**Benefit of Monorepo**:

- ✅ Direct project references (simpler dependency management)
- ✅ Atomic changes across API and PWA when business logic changes
- ✅ Easier refactoring of shared code
- ✅ Single source of truth for domain model

#### 2. Coordinated Release Cycles Are Acceptable

**Current State**:

- Public API and PWA typically release together (user-facing features)
- Shared libraries change infrequently (domain model is relatively stable)
- Coordinated releases align with business requirements

**If Segregated**:

- Independent release cycles would be possible but not necessarily beneficial
- User-facing features often require coordinated API + frontend changes
- Forced independence could create deployment complexity

**Benefit of Monorepo**:

- ✅ Coordinated releases are natural and expected
- ✅ Single CI/CD pipeline for related changes
- ✅ Easier to maintain API contract compatibility

#### 3. Same Security Posture

**Current State**:

- Both Public API and PWA are public-facing
- Both have the same security requirements
- No need for security isolation between them

**Admin API Extraction Rationale** (from ADR-0006):

- Admin operations require different security posture
- Internal vs. public-facing separation justified extraction

**Public API/PWA**:

- ❌ No security isolation benefit from separation
- ✅ Same security posture supports keeping together

#### 4. Team Ownership and Development Workflow

**Current State**:

- Public API and PWA likely owned by the same team
- User-facing features require coordinated API + frontend work
- Shared development workflow is beneficial

**If Segregated**:

- Would require cross-repository PRs for feature work
- More complex development workflow
- Potential for merge conflicts and coordination overhead

**Benefit of Monorepo**:

- ✅ Single repository for user-facing features
- ✅ Easier cross-cutting changes
- ✅ Better developer experience

#### 5. Package Management Overhead

**Current State** (after Admin extraction):

- Shared libraries published as NuGet packages for Admin API
- Public API and PWA use direct project references

**If Further Segregated**:

- Public API would need NuGet packages (already done for Admin)
- PWA would need NuGet packages (new overhead)
- More package publishing and versioning complexity
- More dependency update coordination

**Trade-off**:

- ⚠️ Additional package management overhead
- ⚠️ More complex dependency graph
- ✅ But packages already exist for Admin API consumption

#### 6. Operational Reality

**Current State**:

- Public API and PWA already deploy separately (different Azure resources)
- Operational separation exists without repository separation
- No operational issues requiring repository extraction

**Admin API Extraction Rationale**:

- Different security requirements
- Different operational concerns
- Different team ownership (potentially)

**Public API/PWA**:

- ✅ Already operationally separate (different deployments)
- ✅ No operational issues requiring repository extraction

### When Further Segregation Might Make Sense

**Future Conditions** (not currently met):

1. **Different Team Ownership**:
   - If Public API and PWA teams become separate
   - If teams need independent development workflows
   - If coordination overhead becomes problematic

2. **Divergent Release Cycles**:
   - If Public API needs to release independently from PWA
   - If PWA needs to iterate faster than API
   - If release coordination becomes a bottleneck

3. **Technology Divergence**:
   - If PWA migrates to different technology stack (e.g., React/Next.js)
   - If API and frontend no longer share domain model
   - If architectural patterns diverge significantly

4. **Scale and Complexity**:
   - If repository becomes too large to manage effectively
   - If build times become problematic
   - If codebase complexity requires separation

5. **Business Requirements**:
   - If business requires independent versioning
   - If licensing or compliance requires separation
   - If different deployment models require separation

## Alternatives Considered

### Alternative 1: Extract Public API Only (Rejected)

**Approach**: Extract Public API to separate repository, keep PWA in `Mystira.App`

**Pros**:

- API and frontend separation
- Independent API release cycles

**Cons**:

- PWA still depends on shared libraries (package management still needed)
- Doesn't address PWA-specific concerns
- Creates asymmetry (API separate, PWA not)
- Increased coordination for user-facing features

**Decision**: Rejected - Partial extraction doesn't provide clear benefits

### Alternative 2: Extract PWA Only (Rejected)

**Approach**: Extract PWA to separate repository, keep Public API in `Mystira.App`

**Pros**:

- Frontend/backend separation
- Independent frontend release cycles

**Cons**:

- Public API still in monorepo (doesn't address API concerns)
- PWA would need NuGet packages for shared libraries
- Increased coordination for user-facing features
- Creates asymmetry

**Decision**: Rejected - Partial extraction doesn't provide clear benefits

### Alternative 3: Extract Both Public API and PWA (Rejected)

**Approach**: Extract both Public API and PWA to separate repositories

**Pros**:

- Complete separation
- Independent release cycles for both
- Maximum flexibility

**Cons**:

- Significant package management overhead
- Every domain change requires package updates in 3+ repositories
- Increased coordination complexity
- Loss of atomic changes across API + frontend
- No clear operational or security benefit
- Higher maintenance burden

**Decision**: Rejected - Costs outweigh benefits at current scale

### Alternative 4: Keep Together (Selected)

**Approach**: Keep Public API and PWA in `Mystira.App` monorepo

**Pros**:

- ✅ Simple dependency management (project references)
- ✅ Atomic changes across API + frontend
- ✅ Coordinated releases (natural for user-facing features)
- ✅ Single source of truth for domain model
- ✅ Better developer experience
- ✅ Easier refactoring
- ✅ No additional package management overhead

**Cons**:

- ⚠️ Coupled release cycles (acceptable for user-facing features)
- ⚠️ Single repository for both (acceptable at current scale)

**Decision**: Selected - Benefits clearly outweigh costs

## Consequences

### Positive

1. **Simplified Dependency Management**: Direct project references instead of NuGet packages
2. **Atomic Changes**: Can update API + PWA + domain in single commit
3. **Better Developer Experience**: Single repository, simpler workflow
4. **Easier Refactoring**: Can refactor shared code across API and PWA atomically
5. **Coordinated Releases**: Natural alignment for user-facing features
6. **Reduced Overhead**: No additional package publishing/versioning complexity

### Negative

1. **Coupled Release Cycles**: Public API and PWA release together (acceptable for user-facing features)
2. **Single Repository**: Larger codebase in one place (manageable at current scale)
3. **Build Times**: Potentially longer builds (mitigated by incremental builds)

### Mitigations

1. **Modular Structure**: Maintain clear separation within monorepo (API, PWA, libraries)
2. **Independent Deployments**: Continue deploying API and PWA separately
3. **Future Flexibility**: Structure allows future extraction if conditions change
4. **Package Infrastructure**: NuGet packages already exist (can extract later if needed)

## Implementation

### Current Structure (Maintained)

```
Mystira.App/
├── src/
│   ├── Mystira.App.Domain/              # Core domain (published as NuGet)
│   ├── Mystira.App.Application/         # Application layer (published as NuGet)
│   ├── Mystira.App.Infrastructure.*/     # Infrastructure (published as NuGet)
│   ├── Mystira.App.Shared/              # Shared services (published as NuGet)
│   ├── Mystira.App.Contracts/           # DTOs (published as NuGet)
│   ├── Mystira.App.Api/                 # Public API (project references)
│   └── Mystira.App.PWA/                 # PWA (project references)
└── tools/
    └── Mystira.App.CosmosConsole/       # Ops tool (project references)
```

### Package Consumption

**Admin API** (separate repository):

- Consumes shared libraries via NuGet packages
- Independent release cycle
- Security isolation

**Public API & PWA** (monorepo):

- Use direct project references
- Coordinated releases
- Shared development workflow

### Future Extraction Path

If conditions change and extraction becomes desirable:

1. **PWA Extraction**:
   - PWA would consume NuGet packages (already published)
   - Minimal additional overhead
   - Can be done incrementally

2. **Public API Extraction**:
   - Public API would consume NuGet packages (already published)
   - Similar to Admin API extraction
   - Well-understood process

3. **Both Extraction**:
   - Both would consume NuGet packages
   - Maximum flexibility
   - Maximum overhead

## Success Criteria

- [x] Public API and PWA remain in `Mystira.App` monorepo
- [x] Shared libraries continue to be published as NuGet packages (for Admin API)
- [x] Public API and PWA use direct project references
- [x] Coordinated releases work smoothly
- [x] Developer workflow remains efficient
- [x] No operational issues from monorepo structure

## Future Considerations

### Monitoring for Extraction Triggers

Monitor these conditions that might justify future extraction:

1. **Team Structure Changes**: If Public API and PWA teams become separate
2. **Release Cycle Conflicts**: If coordinated releases become problematic
3. **Repository Size**: If repository becomes too large to manage
4. **Build Performance**: If build times become unacceptable
5. **Technology Divergence**: If API and PWA technology stacks diverge significantly

### Extraction Decision Framework

If extraction is considered in the future, evaluate:

1. **Clear Operational Benefit**: Does extraction solve a real problem?
2. **Team Autonomy**: Do teams need independent workflows?
3. **Release Independence**: Is independent versioning required?
4. **Cost-Benefit**: Do benefits justify package management overhead?
5. **Business Requirements**: Does business need require separation?

## Related ADRs

- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md) - Admin extraction rationale and implementation
- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md) - Package management strategy
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md) - Service boundaries
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md) - Release processes
- [ADR-0016: Monorepo Tooling and Multi-Repository Strategy](./0016-monorepo-tooling-and-multi-repository-strategy.md) - Monorepo tooling decision and rationale

## References

- [Monorepo vs. Multi-repo Trade-offs](https://monorepo.tools/)
- [.NET Solution Structure Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Microservices: When to Split](https://microservices.io/patterns/decomposition/decompose-by-business-capability.html)
