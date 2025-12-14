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

1. **Phase 1**: Setup NuGet feed and publish shared libraries (see [ADR-0007](./0007-nuget-feed-strategy-for-shared-libraries.md))
2. **Phase 2**: Create Admin API repository and update references
3. **Phase 3**: Setup CI/CD for new repository
4. **Phase 4**: (Optional) Extract Admin UI to separate frontend
5. **Phase 5**: Update documentation
6. **Phase 6**: Execute migration with testing
7. **Phase 7**: Cleanup old code from Mystira.App

### Shared Libraries to Package

All shared libraries from `Mystira.App` will be published as NuGet packages to the internal feed:

#### Core Libraries

1. **`Mystira.App.Domain`**
   - **Package ID**: `Mystira.App.Domain`
   - **Purpose**: Core domain models, entities, enumerations, business logic
   - **Dependencies**: Minimal (only external NuGet packages, no internal dependencies)
   - **Usage**: Base layer, referenced by all other libraries
   - **Initial Version**: `1.0.0`

2. **`Mystira.App.Application`**
   - **Package ID**: `Mystira.App.Application`
   - **Purpose**: Application layer - CQRS handlers, MediatR, use cases, business orchestration
   - **Dependencies**: `Mystira.App.Domain`, external packages (MediatR, etc.)
   - **Usage**: Application logic, referenced by API projects
   - **Initial Version**: `1.0.0`

3. **`Mystira.App.Contracts`**
   - **Package ID**: `Mystira.App.Contracts`
   - **Purpose**: Shared request/response DTOs, API contracts
   - **Dependencies**: `Mystira.App.Domain` (for domain models in DTOs)
   - **Usage**: API contracts, referenced by API projects and potentially clients
   - **Initial Version**: `1.0.0`

#### Infrastructure Libraries

4. **`Mystira.App.Infrastructure.Azure`**
   - **Package ID**: `Mystira.App.Infrastructure.Azure`
   - **Purpose**: Azure-specific infrastructure (Cosmos DB, Blob Storage, health checks)
   - **Dependencies**: `Mystira.App.Domain`, Azure SDK packages
   - **Usage**: Azure PaaS service implementations
   - **Initial Version**: `1.0.0`

5. **`Mystira.App.Infrastructure.Data`**
   - **Package ID**: `Mystira.App.Infrastructure.Data`
   - **Purpose**: Data access layer (repositories, unit of work, specifications)
   - **Dependencies**: `Mystira.App.Domain`, `Mystira.App.Application`, Entity Framework Core
   - **Usage**: Data persistence implementations
   - **Initial Version**: `1.0.0`

6. **`Mystira.App.Infrastructure.Discord`**
   - **Package ID**: `Mystira.App.Infrastructure.Discord`
   - **Purpose**: Discord bot integration
   - **Dependencies**: `Mystira.App.Domain`, Discord.NET
   - **Usage**: Discord bot functionality
   - **Initial Version**: `1.0.0`

7. **`Mystira.App.Infrastructure.StoryProtocol`**
   - **Package ID**: `Mystira.App.Infrastructure.StoryProtocol`
   - **Purpose**: Story Protocol blockchain integration
   - **Dependencies**: `Mystira.App.Domain`, Story Protocol SDK
   - **Usage**: Blockchain/IP asset management
   - **Initial Version**: `1.0.0`

#### Shared Services

8. **`Mystira.App.Shared`**
   - **Package ID**: `Mystira.App.Shared`
   - **Purpose**: Shared services and utilities (JWT, telemetry, middleware, logging)
   - **Dependencies**: `Mystira.App.Domain` (minimal), ASP.NET Core packages
   - **Usage**: Cross-cutting concerns, used by all API projects
   - **Initial Version**: `1.0.0`

### Package Dependency Graph

```
Mystira.App.Domain (base, no internal deps)
    ↑
    ├── Mystira.App.Application
    ├── Mystira.App.Contracts
    ├── Mystira.App.Infrastructure.Azure
    ├── Mystira.App.Infrastructure.Data
    ├── Mystira.App.Infrastructure.Discord
    ├── Mystira.App.Infrastructure.StoryProtocol
    └── Mystira.App.Shared

Mystira.App.Application
    ↑
    └── Mystira.App.Infrastructure.Data (implements application interfaces)
```

**Key Rules**:

- Domain is the base layer (no internal dependencies)
- Application depends on Domain
- Infrastructure.\* depends on Domain (and Application if implementing interfaces)
- Shared has minimal dependencies
- Contracts depends on Domain (for domain models in DTOs)

### Version Strategy

**Semantic Versioning** (SemVer): `MAJOR.MINOR.PATCH`

**Version Rules**:

- **MAJOR** (`2.0.0`): Breaking changes (incompatible API changes)
  - Examples: Removing public methods, changing method signatures, breaking data contracts
  - **Action Required**: Update all consuming projects, coordinate migration
- **MINOR** (`1.1.0`): New features (backward compatible)
  - Examples: Adding new methods, new properties, new functionality
  - **Action Required**: Optional update, consuming projects can adopt when ready
- **PATCH** (`1.0.1`): Bug fixes (backward compatible)
  - Examples: Bug fixes, performance improvements, documentation updates
  - **Action Required**: Recommended update, typically low risk

**Initial Versions**: All packages start at `1.0.0`

**Version Alignment**:

- When possible, keep related packages at compatible versions
- Domain changes may require version bumps in dependent packages
- Document compatibility matrix

**Pre-release Versions**:

- Use for testing: `1.1.0-alpha.1`, `1.1.0-beta.1`
- Publish to feed for testing before stable release
- Example: `1.1.0-alpha.1` for Admin API team to test before `1.1.0` stable

### Package Metadata Standards

Each package `.csproj` must include:

```xml
<PropertyGroup>
  <PackageId>Mystira.App.Domain</PackageId>
  <Version>1.0.0</Version>
  <Authors>Mystira Team</Authors>
  <Company>Phoenix VC</Company>
  <Description>Mystira platform domain models and business logic. Core domain layer with entities, value objects, and domain services.</Description>
  <RepositoryUrl>https://github.com/phoenixvc/Mystira.App</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageLicenseExpression>PROPRIETARY</PackageLicenseExpression>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
  <PackageTags>mystira;domain;internal</PackageTags>
</PropertyGroup>
```

### Consumption in Admin API

After extraction, Admin API will reference packages like:

```xml
<ItemGroup>
  <!-- Core packages -->
  <PackageReference Include="Mystira.App.Domain" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Application" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Contracts" Version="1.0.0" />

  <!-- Infrastructure packages -->
  <PackageReference Include="Mystira.App.Infrastructure.Azure" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.Data" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.Discord" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.StoryProtocol" Version="1.0.0" />

  <!-- Shared services -->
  <PackageReference Include="Mystira.App.Shared" Version="1.0.0" />
</ItemGroup>
```

### Breaking Changes Management

**Process for Breaking Changes**:

1. **Identify Breaking Change**: Document what will break
2. **Plan Migration**: Create migration guide
3. **Version Bump**: Increment major version (e.g., `1.0.0` → `2.0.0`)
4. **Coordinate**: Notify all consuming teams
5. **Publish**: Publish new major version
6. **Migrate**: Update all consuming projects
7. **Document**: Update changelog and release notes

**Example Scenario**:

- Domain package removes deprecated method → `1.0.0` → `2.0.0`
- Admin API and Public API both need to update to `2.0.0`
- Coordinate update across both repositories
- Test thoroughly before deploying

### Package Update Workflow

**When Shared Library Changes**:

1. Developer makes changes to shared library in `Mystira.App`
2. Tests changes locally
3. Creates PR with changeset (if using Changesets)
4. After merge, CI publishes new package version
5. Consuming projects (Admin API, Public API) update package reference
6. Test and deploy updated services

**Automated Updates**:

- Consider Dependabot for package update notifications
- Manual updates preferred for control over timing

See [ADR-0007: NuGet Feed Strategy](./0007-nuget-feed-strategy-for-shared-libraries.md) for detailed feed setup and publishing workflow.

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
- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md) - Package management strategy
- [Admin API Extraction Plan](../migration/ADMIN_API_EXTRACTION_PLAN.md) - Implementation details

## References

- [NuGet Package Management](https://docs.microsoft.com/en-us/nuget/)
- [Semantic Versioning](https://semver.org/)
- [Microservices Architecture](https://microservices.io/)
- [Admin API README](../../packages/app/src/Mystira.App.Admin.Api/README.md) - Current separation rationale
