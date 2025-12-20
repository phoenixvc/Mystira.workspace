# ADR-0007: NuGet Feed Strategy for Shared Libraries

## Status

**Accepted** - 2025-12-14

## Context

As we extract services from the `Mystira.App` monorepo (starting with Admin API), we need a strategy for managing shared libraries that are used across multiple repositories.

### Current State

**Shared Libraries in Mystira.App**:

- `Mystira.App.Domain` - Core domain models and business logic
- `Mystira.App.Application` - Application layer (CQRS, MediatR handlers)
- `Mystira.App.Infrastructure.Azure` - Azure-specific infrastructure
- `Mystira.App.Infrastructure.Data` - Data access layer
- `Mystira.App.Infrastructure.Discord` - Discord integration
- `Mystira.App.Infrastructure.StoryProtocol` - Story Protocol integration
- `Mystira.App.Shared` - Shared services (JWT, telemetry, middleware)
- `Mystira.App.Contracts` - Shared request/response DTOs

**Current Usage**:

- Referenced via project references (`<ProjectReference>`) within `Mystira.App`
- Both Public API and Admin API reference the same shared libraries
- No version management required (same solution)

### Problem Statement

When extracting Admin API to a separate repository:

1. **Dependency Management**: Admin API needs access to shared libraries
2. **Version Control**: Need explicit version management for shared code
3. **Release Coordination**: Shared library changes must be coordinated
4. **Development Workflow**: Developers need easy access to shared packages

**Options Considered**:

- Git submodules (not ideal for compiled .NET libraries)
- NuGet packages (standard .NET practice)
- Copy shared code (violates DRY principle)

## Decision

We will use **NuGet packages** hosted on an **internal NuGet feed** for shared libraries.

### NuGet Feed Provider

**Selected**: GitHub Packages

**Feed URL**: `https://nuget.pkg.github.com/phoenixvc/index.json`

**Rationale**:

- ✅ Integrated with GitHub (our source control and CI/CD)
- ✅ Built-in authentication via GITHUB_TOKEN in workflows
- ✅ Free for public repositories, included storage for private
- ✅ Simple CI/CD integration with GitHub Actions
- ✅ Private feed with access control via GitHub permissions

**Alternative Options**:

- **Azure DevOps Artifacts**: Good for Azure-centric teams, but adds another system to manage
- **Private NuGet Server**: Self-hosted, more maintenance overhead
- **NuGet.org (public)**: Not appropriate for private/internal libraries

### Feed Structure

**Single Feed**: `Mystira-Internal`

**Organization**: All Mystira shared libraries in one feed

**Benefits**:

- Single source of truth
- Unified access control
- Easier discovery
- Simplified authentication

### Package Naming Convention

**Format**: `{Organization}.{Project}.{Library}`

**Examples**:

- `Mystira.App.Domain`
- `Mystira.App.Application`
- `Mystira.App.Infrastructure.Azure`
- `Mystira.App.Infrastructure.Data`
- `Mystira.App.Infrastructure.Discord`
- `Mystira.App.Infrastructure.StoryProtocol`
- `Mystira.App.Shared`
- `Mystira.App.Contracts`

**Rationale**:

- Consistent with existing namespace structure
- Clear ownership and organization
- Easy to identify related packages

### Versioning Strategy

**Semantic Versioning** (SemVer): `MAJOR.MINOR.PATCH`

**Rules**:

- **MAJOR**: Breaking changes (incompatible API changes)
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

**Examples**:

- `1.0.0` - Initial release
- `1.1.0` - New feature added
- `1.1.1` - Bug fix
- `2.0.0` - Breaking change

**Pre-release Versions**:

- Use for development/alpha/beta: `1.1.0-alpha.1`, `1.1.0-beta.2`
- Useful for testing before stable release

### Package Publishing Workflow

#### Automated Publishing

**Trigger**: Push to `main` branch in `Mystira.App` repository

**Condition**: Changes to shared library projects

**Workflow**: `.github/workflows/publish-shared-packages.yml`

**Process**:

1. Detect changes to shared library projects
2. Build and pack libraries
3. Determine version (see versioning strategy below)
4. Publish to NuGet feed
5. Create GitHub release/tag

#### Manual Publishing

**Use Cases**:

- Hotfix releases
- Testing pre-release versions
- Publishing after manual version bumps

**Command**:

```bash
dotnet pack src/Mystira.App.Domain/Mystira.App.Domain.csproj -c Release
dotnet nuget push **/*.nupkg --source "Mystira-Internal" --api-key {PAT}
```

### Version Determination Strategy

#### Automatic Versioning

##### **Option 1: Changesets (Recommended)**

Use Changesets for version management:

```bash
# Developer creates changeset
pnpm changeset

# CI automatically versions and publishes
pnpm changeset version
dotnet pack
dotnet nuget push
```

**Benefits**:

- Developer-driven version decisions
- Clear changelog generation
- Consistent with npm package versioning in workspace

##### **Option 2: GitVersion (Alternative)**

Use GitVersion for automatic semantic versioning:

- Version based on git tags and commits
- Automatic version calculation
- No manual version management

#### Manual Versioning

**For Manual Control**:

- Update version in `.csproj` files manually
- Commit version changes
- CI publishes with new version

### Package Configuration

#### Package Metadata

Each shared library `.csproj` should include:

```xml
<PropertyGroup>
  <PackageId>Mystira.App.Domain</PackageId>
  <Version>1.0.0</Version>
  <Authors>Mystira Team</Authors>
  <Company>Phoenix VC</Company>
  <Description>Mystira platform domain models and business logic</Description>
  <RepositoryUrl>https://github.com/phoenixvc/Mystira.App</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

#### Package Dependencies

**External Dependencies**: Include in `.nuspec` or let NuGet resolve from `.csproj`

**Internal Dependencies**: Reference other Mystira packages

**Example**:

```xml
<ItemGroup>
  <!-- External dependencies -->
  <PackageReference Include="MediatR" Version="12.4.1" />

  <!-- Internal dependencies -->
  <PackageReference Include="Mystira.App.Domain" Version="1.0.0" />
</ItemGroup>
```

### Consumer Configuration

#### NuGet.config

**Location**: Repository root or solution root

**Content**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github" value="https://nuget.pkg.github.com/phoenixvc/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="USERNAME" />
      <add key="ClearTextPassword" value="GITHUB_TOKEN_OR_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

**For CI/CD**: Use `GITHUB_TOKEN` secret (automatically available in GitHub Actions)

**For Local Development**: Create a Personal Access Token (PAT) with `read:packages` scope

#### Package References

**In Consuming Projects**:

```xml
<ItemGroup>
  <PackageReference Include="Mystira.App.Domain" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Application" Version="1.0.0" />
</ItemGroup>
```

**Version Pinning**:

- Pin to specific version: `Version="1.0.0"`
- Allow patch updates: `Version="1.0.0"` → `1.0.*` (use `VersionOverride` or wildcard if supported)
- Use `Version="[1.0.0,2.0.0)"` for range

**Recommendation**: Pin to specific versions, update explicitly

### Access Control

#### Feed Permissions

**Reader Role**:

- All developers
- CI/CD service principals
- Can install packages

**Contributor Role**:

- Team leads
- CI/CD workflows (via service principal)
- Can publish packages

**Owner Role**:

- Admins only
- Full feed management

#### Authentication

**For Local Development**:

- Azure DevOps Personal Access Token (PAT)
- Store in `NuGet.config` (excluded from git) or user-level config

**For CI/CD**:

- Service Principal with Contributor role
- Store PAT in GitHub Secrets
- Use in CI/CD workflows

### Breaking Changes Management

#### Strategy

**Major Version Bumps**:

1. Identify breaking changes
2. Update version to next major (e.g., `1.0.0` → `2.0.0`)
3. Publish new version
4. Update consuming projects to new major version
5. Coordinate migration across all consumers

**Backward Compatibility**:

- Prefer backward-compatible changes (minor/patch)
- Use deprecation warnings for planned breaking changes
- Provide migration guides

#### Communication

- **Changelog**: Maintain CHANGELOG.md for each package
- **Release Notes**: GitHub releases with detailed notes
- **Team Notification**: Announce breaking changes in team channels
- **Migration Guides**: Document migration steps for major versions

### Dependency Graph Management

#### Internal Package Dependencies

**Dependency Rules**:

- Domain: No internal dependencies (base layer)
- Application: Depends on Domain
- Infrastructure.\*: Depends on Domain, may depend on Application
- Shared: Minimal dependencies (can depend on Domain)
- Contracts: No internal dependencies (interfaces only)

**Circular Dependencies**: Avoid at all costs

#### Version Alignment

**Strategy**: Keep compatible versions aligned when possible

**Example**:

- All `Mystira.App.*` packages at version `1.0.0`
- When Domain changes to `2.0.0`, coordinate related packages
- Document compatibility matrix

### Development Workflow

#### Local Development

##### **Option 1: Project References (During Development)**

During active development of shared libraries:

1. Use project references locally
2. Test changes before publishing
3. Publish after changes are complete

##### **Option 2: Local NuGet Feed**

Use local folder as NuGet source:

```bash
# Pack to local folder
dotnet pack -o ./local-packages

# Add local feed
dotnet nuget add source ./local-packages --name local

# Restore from local feed
dotnet restore
```

#### CI/CD Integration

**Publishing Workflow**:

- Trigger: Push to `main` with shared library changes
- Build and test shared libraries
- Pack libraries
- Publish to internal feed
- Create git tags for versions

**Consuming Workflow**:

- Restore packages from internal feed
- Build and test consuming projects
- Deploy services

### Monitoring and Maintenance

#### Package Usage Tracking

- Track which services use which package versions
- Monitor for outdated packages
- Security scanning for package dependencies

#### Feed Health

- Monitor feed availability
- Track package download metrics
- Monitor storage usage

#### Cleanup

- Archive old/unused package versions (keep last N versions)
- Remove deprecated packages
- Document retention policy

## Rationale

### Why NuGet Packages?

1. **Standard Practice**: Industry-standard for .NET shared libraries
2. **Version Management**: Explicit versioning and dependency management
3. **Tooling Support**: Excellent IDE and tooling support
4. **CI/CD Integration**: Well-integrated with build systems
5. **Access Control**: Fine-grained permissions and security

### Why GitHub Packages?

1. **GitHub Integration**: Aligns with our GitHub-based source control and CI/CD
2. **Cost**: Free for public repos, included with GitHub plans for private
3. **Security**: Leverages existing GitHub permissions and access control
4. **Ease of Use**: Simple setup, automatic GITHUB_TOKEN authentication in Actions
5. **Single Platform**: No additional accounts or systems to manage

### Why Single Feed?

1. **Simplicity**: Single authentication, single source
2. **Discovery**: Easy to find related packages
3. **Management**: Unified access control and policies
4. **Cost**: Single feed is more cost-effective

## Consequences

### Positive

1. **Clear Versioning**: Explicit version management
2. **Dependency Control**: Pin specific versions for stability
3. **Independent Releases**: Shared libraries can release independently
4. **Standard Practice**: Aligns with .NET ecosystem standards
5. **Better Isolation**: Services depend on packages, not source code

### Negative

1. **Publishing Overhead**: Must publish packages when shared code changes
2. **Version Coordination**: Need to coordinate version updates
3. **Breaking Changes**: Major versions require coordinated updates
4. **Feed Maintenance**: Need to manage feed, permissions, cleanup
5. **Initial Setup**: Requires feed setup and authentication configuration

### Mitigations

1. **Automated Publishing**: CI/CD workflows reduce manual overhead
2. **Versioning Tools**: Use Changesets or GitVersion for automation
3. **Documentation**: Clear versioning and breaking change policies
4. **Monitoring**: Track package usage and versions
5. **Clear Processes**: Documented workflows for publishing and updates

## Implementation

### Phase 1: Setup Feed

1. GitHub Packages feed is automatically available at: `https://nuget.pkg.github.com/phoenixvc/index.json`
2. Permissions are inherited from GitHub repository access
3. CI/CD uses automatic `GITHUB_TOKEN` authentication
4. Local development uses Personal Access Tokens (PATs)

### Phase 2: Package Shared Libraries

1. Update `.csproj` files with package metadata
2. Test package creation locally
3. Setup automated publishing workflow
4. Publish initial versions (1.0.0)

### Phase 3: Update Consumers

1. Update Admin API to use NuGet packages (during extraction)
2. Update Public API to use NuGet packages (optional, can keep project refs)
3. Test package consumption
4. Document consumption workflow

### Phase 4: Ongoing Management

1. Establish versioning workflow
2. Setup monitoring and alerts
3. Document breaking change process
4. Train team on package management

## Related ADRs

- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md) - Drives need for NuGet packages, contains package consumption details
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md) - Release coordination
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md) - Service boundaries

## References

- [NuGet Package Management](https://docs.microsoft.com/en-us/nuget/)
- [Azure DevOps Artifacts](https://docs.microsoft.com/en-us/azure/devops/artifacts/)
- [Semantic Versioning](https://semver.org/)
- [Changesets](https://github.com/changesets/changesets) - Version management tool
