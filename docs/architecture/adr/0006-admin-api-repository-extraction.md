# ADR-0006: Admin API Repository Extraction

## Status

**Accepted** - 2025-12-14

## Context

The `Mystira.App` repository currently contains:

1. **Public API** (`Mystira.App.Api`) - Public-facing REST API
2. **Admin API** (`Mystira.App.Admin.Api`) - Internal admin REST API with Razor Pages UI
3. **PWA** (`Mystira.App.PWA`) - Blazor WebAssembly frontend
4. **Shared Libraries**: Domain, Application, Infrastructure, Contracts, Shared

Both APIs share extensive code dependencies:

- `Mystira.App.Domain` - Core domain models
- `Mystira.App.Application` - Application layer (CQRS, MediatR)
- `Mystira.App.Infrastructure.*` - Infrastructure libraries
- `Mystira.App.Shared` - Shared services
- `Mystira.App.Contracts` - Shared DTOs

### Current State

- Admin API and Public API are in the same repository
- Both APIs deploy to separate Azure App Services
- Admin API includes Razor Pages UI (mixed concerns)
- Admin API README explicitly states separation benefits (security, scaling, maintainability, deployment)
- Both APIs share the same domain model and business logic

### Problem Statement

1. **Architectural Inconsistency**: Admin API README describes separation benefits, but code is in same repo
2. **Security Isolation**: Admin and Public APIs have different security requirements but share codebase
3. **Release Cycle Coupling**: Changes to shared libraries affect both APIs simultaneously
4. **Team Autonomy**: Different teams may own Admin vs. Public APIs
5. **UI Architecture**: Admin UI is Razor Pages embedded in API (mixed concerns, not modern)

## Decision

We will **extract Admin API to a separate repository** (`Mystira.Admin.Api`).

### Extraction Strategy

1. **Create Separate Repository**: `Mystira.Admin.Api`
2. **Publish Shared Libraries as NuGet Packages**: Domain, Application, Infrastructure, Shared, Contracts
3. **Update Admin API**: Replace project references with NuGet package references
4. **Maintain API Compatibility**: No breaking changes to Admin API surface
5. **Future UI Separation**: Plan to extract Admin UI to separate frontend (optional Phase 2)

### Repository Structure After Extraction

**Mystira.App** (remains):

- Public API (`Mystira.App.Api`)
- PWA (`Mystira.App.PWA`)
- Shared Libraries (published to NuGet)
- Domain, Application, Infrastructure projects

**Mystira.Admin.Api** (new):

- Admin API backend (`Mystira.App.Admin.Api` or renamed to `Mystira.Admin.Api`)
- Admin UI (Razor Pages, to be modernized later)
- Depends on NuGet packages from `Mystira.App`

## Rationale

### 1. Security Isolation

**Benefits**:

- ✅ Separate codebase for admin functionality
- ✅ Independent security reviews
- ✅ Reduced attack surface for public API
- ✅ Different security postures can be maintained

**Evidence**: Admin API README explicitly states "Enhanced Security - Admin operations isolated from public endpoints"

### 2. Independent Release Cycles

**Benefits**:

- ✅ Admin API can iterate faster without affecting public API
- ✅ Public API can maintain stability while admin features evolve
- ✅ Independent versioning and releases
- ✅ Reduced coordination overhead

### 3. Team Autonomy

**Benefits**:

- ✅ Different teams can own Admin vs. Public APIs
- ✅ Independent development workflows
- ✅ Reduced merge conflicts
- ✅ Better code ownership

### 4. Modern Architecture

**Benefits**:

- ✅ Opportunity to separate Admin UI from API
- ✅ Can modernize to REST API + modern frontend
- ✅ Better separation of concerns
- ✅ Aligns with microservices principles

### 5. Shared Dependencies Management

**Solution**: NuGet Packages

**Benefits**:

- ✅ Explicit version dependencies
- ✅ Better dependency management
- ✅ Versioned shared libraries
- ✅ Standard .NET practice for shared code

**Trade-offs**:

- ⚠️ Need to publish packages when shared libraries change
- ⚠️ Package version coordination required
- ✅ This is standard practice in .NET ecosystem

### 6. Consistency with Architecture

**Current Evidence**: Admin API already deploys separately and README describes separation benefits. Repository extraction aligns code organization with operational reality.

## Consequences

### Positive

1. **Security**: Better isolation between admin and public functionality
2. **Independence**: Independent release cycles and team autonomy
3. **Clarity**: Clear service boundaries
4. **Scalability**: Services can evolve independently
5. **Modernization**: Opportunity to modernize admin UI architecture

### Negative

1. **Package Management**: Need to publish and version shared libraries
2. **Coordination**: Shared library changes require package updates
3. **Initial Effort**: Migration requires setup and testing
4. **Learning Curve**: Team must learn package management workflow

### Mitigations

1. **Automated Publishing**: CI/CD workflows for automatic package publishing
2. **Versioning Strategy**: Semantic versioning for shared packages
3. **Documentation**: Clear documentation on package management
4. **Phased Migration**: Incremental migration with rollback plan

## Implementation

See [Admin API Extraction Migration Plan](../migration/ADMIN_API_EXTRACTION_PLAN.md) for detailed implementation steps.

### Key Steps

1. **Phase 1**: Setup NuGet feed and publish shared libraries
2. **Phase 2**: Create Admin API repository and update references
3. **Phase 3**: Setup CI/CD for new repository
4. **Phase 4**: (Optional) Extract Admin UI to separate frontend
5. **Phase 5**: Update documentation
6. **Phase 6**: Execute migration with testing
7. **Phase 7**: Cleanup old code from Mystira.App

### Shared Libraries to Package

- `Mystira.App.Domain` → NuGet package
- `Mystira.App.Application` → NuGet package
- `Mystira.App.Infrastructure.Azure` → NuGet package
- `Mystira.App.Infrastructure.Data` → NuGet package
- `Mystira.App.Infrastructure.Discord` → NuGet package
- `Mystira.App.Infrastructure.StoryProtocol` → NuGet package
- `Mystira.App.Shared` → NuGet package
- `Mystira.App.Contracts` → NuGet package

### Version Strategy

- **Semantic Versioning**: Major.Minor.Patch
- **Breaking Changes**: Major version bump
- **New Features**: Minor version bump
- **Bug Fixes**: Patch version bump

## Alternatives Considered

### Alternative 1: Keep Together (Rejected)

**Approach**: Keep Admin API and Public API in same repository

**Pros**:

- Simple dependency management (project references)
- No package publishing overhead
- Easier cross-API changes

**Cons**:

- Doesn't align with operational separation (already deploy separately)
- Security isolation concerns
- Coupled release cycles
- Inconsistent with Admin API README description

**Decision**: Rejected - Operational reality (separate deployments) and security requirements justify extraction

### Alternative 2: Extract Admin UI Only (Rejected)

**Approach**: Extract only Admin UI, keep Admin API in same repo as Public API

**Pros**:

- UI/Backend separation
- Modern frontend architecture

**Cons**:

- Doesn't address security isolation
- Admin API still coupled with Public API
- Doesn't solve release cycle independence

**Decision**: Rejected - Doesn't address core concerns (security, independence)

## Success Criteria

- [ ] Admin API builds and runs from new repository
- [ ] All tests pass
- [ ] Deployed to staging successfully
- [ ] Deployed to production successfully
- [ ] No functionality regression
- [ ] Shared libraries published to NuGet
- [ ] Documentation updated
- [ ] Team trained on new structure

## Future Considerations

### Admin UI Modernization

After Admin API extraction, consider:

- Extract Admin UI to separate frontend repository
- Modernize to React/Vue/Blazor standalone
- REST API-only backend (remove Razor Pages)
- Better frontend/backend separation

### Service Mesh Integration

When Admin API is separate:

- Can integrate with service mesh if implemented
- Unified service-to-service communication
- Better observability

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md) - Deployment models
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md) - Release processes
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md) - Service boundaries
- [Admin API Extraction Plan](../migration/ADMIN_API_EXTRACTION_PLAN.md) - Implementation details

## References

- [NuGet Package Management](https://docs.microsoft.com/en-us/nuget/)
- [Semantic Versioning](https://semver.org/)
- [Microservices Architecture](https://microservices.io/)
- [Admin API README](../../packages/app/src/Mystira.App.Admin.Api/README.md) - Current separation rationale
