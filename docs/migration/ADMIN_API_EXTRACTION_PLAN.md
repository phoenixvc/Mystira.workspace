# Admin API Extraction Migration Plan

**Date**: 2025-12-14  
**Status**: In Progress  
**Target**: Extract `Mystira.App.Admin.Api` to separate repositories `Mystira.Admin.Api` and `Mystira.Admin.UI`

## Executive Summary

This migration plan outlines the steps to extract the Admin API and Admin UI from the `Mystira.App` monorepo into separate repositories, enabling better security isolation, independent release cycles, and modern frontend architecture.

**Repositories Created**:

- ✅ `Mystira.Admin.Api` - Admin API backend (pure REST/gRPC)
- ✅ `Mystira.Admin.UI` - Admin frontend (modern SPA)

## Prerequisites

- [ ] Internal NuGet feed configured (Azure DevOps, GitHub Packages, or private NuGet server)
- [x] Access to create new repository `Mystira.Admin.Api` - **COMPLETED**
- [x] Access to create new repository `Mystira.Admin.UI` - **COMPLETED**
- [ ] CI/CD pipeline access for new repositories
- [x] Team alignment on extraction decision - **COMPLETED**

## Phase 1: Setup Shared Packages (Week 1)

### 1.1 Create Internal NuGet Feed

**Decision**: Use Azure DevOps Artifacts (see [ADR-0007: NuGet Feed Strategy](../architecture/adr/0007-nuget-feed-strategy-for-shared-libraries.md))

**Feed Name**: `Mystira-Internal`

**Steps**:

```bash
# Configure NuGet source
dotnet nuget add source https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v3/index.json \
  --name "Mystira-Internal" \
  --username {username} \
  --password {PAT}
```

### 1.2 Prepare Shared Libraries for Publishing

**Libraries to Package**:

1. `Mystira.App.Domain`
2. `Mystira.App.Application`
3. `Mystira.App.Infrastructure.Azure`
4. `Mystira.App.Infrastructure.Data`
5. `Mystira.App.Infrastructure.Discord`
6. `Mystira.App.Infrastructure.StoryProtocol`
7. `Mystira.App.Shared`
8. `Mystira.App.Contracts`

**For each library** (see [ADR-0006](./../architecture/adr/0006-admin-api-repository-extraction.md) for complete list):

1. **Update `.csproj` files** with package metadata (see ADR-0007 for standards):

```xml
<PropertyGroup>
  <PackageId>Mystira.App.Domain</PackageId>
  <Version>1.0.0</Version>
  <Authors>Mystira Team</Authors>
  <Company>Phoenix VC</Company>
  <Description>Mystira platform domain models and business logic</Description>
  <RepositoryUrl>https://github.com/phoenixvc/Mystira.App</RepositoryUrl>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
</PropertyGroup>
```

2. **Create `.nuspec` files** (optional, for more control)

3. **Build and publish**:

```bash
dotnet pack src/Mystira.App.Domain/Mystira.App.Domain.csproj \
  --configuration Release \
  --output ./nupkg

dotnet nuget push ./nupkg/Mystira.App.Domain.1.0.0.nupkg \
  --source "Mystira-Internal" \
  --api-key {api-key}
```

### 1.3 Setup Automated Publishing

**Create GitHub Actions workflow** for automatic publishing:

```yaml
# .github/workflows/publish-shared-packages.yml
name: Publish Shared Packages

on:
  push:
    branches: [main]
    paths:
      - "src/Mystira.App.Domain/**"
      - "src/Mystira.App.Application/**"
      # ... other shared libraries

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6
      - uses: actions/setup-dotnet@v4
        with:
          nuget-version: "6.x"

      - name: Configure NuGet source
        run: |
          dotnet nuget add source https://pkgs.dev.azure.com/${{ secrets.AZURE_ORG }}/${{ secrets.AZURE_PROJECT }}/_packaging/${{ secrets.NUGET_FEED }}/nuget/v3/index.json \
            --name "Mystira-Internal" \
            --username ${{ secrets.AZURE_USER }} \
            --password ${{ secrets.AZURE_PAT }}

      - name: Pack libraries
        run: |
          dotnet pack src/Mystira.App.Domain/Mystira.App.Domain.csproj -c Release
          dotnet pack src/Mystira.App.Application/Mystira.App.Application.csproj -c Release
          # ... pack other libraries

      - name: Publish to NuGet
        run: |
          dotnet nuget push **/*.nupkg --source "Mystira-Internal" --skip-duplicate
```

## Phase 2: Create Admin API and Admin UI Repositories (Week 1-2)

### 2.1 Create Repositories

**Status**: ✅ **COMPLETED**

1. ✅ Created GitHub repository: `Mystira.Admin.Api`
2. ✅ Created GitHub repository: `Mystira.Admin.UI`
3. Initialize with:
   - `.gitignore` for .NET
   - `README.md`
   - `LICENSE`
   - Basic folder structure

### 2.2 Copy Admin API Code

**Steps**:

```bash
# In Mystira.App repository
mkdir ../Mystira.Admin.Api
cd ../Mystira.Admin.Api
git init

# Copy Admin API project (excluding Razor Pages Views)
cp -r ../Mystira.App/src/Mystira.App.Admin.Api ./src/Mystira.App.Admin.Api

# Remove Razor Pages Views folder (will be migrated to Mystira.Admin.UI)
rm -rf ./src/Mystira.App.Admin.Api/Views

# Copy solution file and create new one if needed
# Copy relevant configuration files
```

**Files to Copy**:

- `src/Mystira.App.Admin.Api/` (entire project, **excluding `Views/` folder**)
- Solution file (or create new)
- `.editorconfig`
- `.gitattributes`
- `Directory.Build.props` (if exists)

**Note**: Razor Pages UI (`Views/` folder) will be migrated to `Mystira.Admin.UI` as a modern frontend application.

### 2.3 Update Project References

**Replace Project References with NuGet Packages**:

```xml
<!-- Before -->
<ProjectReference Include="..\Mystira.App.Domain\Mystira.App.Domain.csproj" />

<!-- After -->
<PackageReference Include="Mystira.App.Domain" Version="1.0.0" />
```

**Update `Mystira.App.Admin.Api.csproj`**:

```xml
<ItemGroup>
  <PackageReference Include="Mystira.App.Domain" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Application" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.Azure" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.Data" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.Discord" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Infrastructure.StoryProtocol" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Shared" Version="1.0.0" />
  <PackageReference Include="Mystira.App.Contracts" Version="1.0.0" />
</ItemGroup>
```

### 2.4 Configure NuGet Source

**Create `NuGet.config`**:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="Mystira-Internal"
         value="https://pkgs.dev.azure.com/{org}/{project}/_packaging/{feed}/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <Mystira-Internal>
      <add key="Username" value="{username}" />
      <add key="ClearTextPassword" value="{pat}" />
    </Mystira-Internal>
  </packageSourceCredentials>
</configuration>
```

**For CI/CD**: Use secrets instead of hardcoded credentials.

### 2.5 Update Namespace (Optional)

Consider renaming namespaces from `Mystira.App.Admin.Api` to `Mystira.Admin.Api`:

```bash
# Use IDE refactoring tools or:
find . -type f -name "*.cs" -exec sed -i 's/Mystira.App.Admin.Api/Mystira.Admin.Api/g' {} +
```

## Phase 3: Setup CI/CD (Week 2)

### 3.1 Create GitHub Actions Workflows

**`.github/workflows/ci.yml`**:

```yaml
name: Admin API CI

on:
  push:
    branches: [main, dev]
  pull_request:
    branches: [main, dev]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v6

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          nuget-version: "6.x"

      - name: Configure NuGet
        run: |
          dotnet nuget add source https://pkgs.dev.azure.com/${{ secrets.AZURE_ORG }}/${{ secrets.AZURE_PROJECT }}/_packaging/${{ secrets.NUGET_FEED }}/nuget/v3/index.json \
            --name "Mystira-Internal" \
            --username ${{ secrets.AZURE_USER }} \
            --password ${{ secrets.AZURE_PAT }}

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build
```

**`.github/workflows/deploy.yml`**:

```yaml
name: Deploy Admin API

on:
  push:
    branches: [main]
    paths:
      - "src/**"

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: production
    steps:
      - uses: actions/checkout@v6

      - name: Setup .NET
        uses: actions/setup-dotnet@v4

      - name: Configure NuGet
        # ... (same as CI)

      - name: Build
        run: dotnet publish src/Mystira.App.Admin.Api/Mystira.App.Admin.Api.csproj -c Release

      - name: Deploy to Azure
        uses: azure/webapps-deploy@v3
        with:
          app-name: "prod-wus-app-mystira-api-admin"
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: "./src/Mystira.App.Admin.Api/bin/Release/net9.0/publish"
```

### 3.2 Update Infrastructure

**Update Azure Bicep/Terraform** (if applicable):

- Ensure Admin API deployment references new repository
- Update deployment pipelines to use new repo

## Phase 4: Migrate Admin UI to Modern Frontend (Week 3-4)

### 4.1 Admin UI Repository

**Status**: ✅ **REPOSITORY CREATED**

- ✅ `Mystira.Admin.UI` repository created
- [ ] Choose frontend framework (React/Vue/Blazor standalone)

### 4.2 Migrate Razor Pages to Modern Frontend

**Current**: Razor Pages in `Mystira.App.Admin.Api/Views/`

**Target**: Modern SPA (e.g., React + TypeScript) in `Mystira.Admin.UI`

**Migration Steps**:

1. [ ] Create new frontend application in `Mystira.Admin.UI`
2. [ ] Recreate UI components from Razor Pages
3. [ ] Connect to Admin API via REST/gRPC
4. [ ] Implement authentication/authorization
5. [ ] Deploy as static site (Azure Static Web Apps)

### 4.3 Update Admin API

- [ ] Remove Razor Pages (`Views/` folder) from `Mystira.Admin.Api`
- [ ] Ensure Admin API is pure API (REST/gRPC endpoints only)
- [ ] Add CORS configuration for `Mystira.Admin.UI`
- [ ] Update authentication for SPA

## Phase 5: Update Documentation (Week 2-3)

### 5.1 Update README Files

**Mystira.Admin.Api/README.md** and **Mystira.Admin.UI/README.md**:

- Document new repository structure
- Update setup instructions
- Document NuGet package dependencies
- Update deployment instructions

**Mystira.App/README.md**:

- Remove Admin API references
- Update repository overview
- Document that Admin API is now separate

### 5.2 Update Architecture Documentation

- Update ADRs (see below)
- Update architecture diagrams
- Update service communication documentation

### 5.3 Update Workspace Documentation

**Mystira.workspace**:

- Add `Mystira.Admin.Api` and `Mystira.Admin.UI` to submodules
- Update README with new repository
- Update setup scripts

## Phase 6: Migration Execution (Week 3)

### 6.1 Pre-Migration Checklist

- [ ] All shared packages published to NuGet
- [ ] Admin API repository created and configured
- [ ] CI/CD pipelines tested
- [ ] Documentation updated
- [ ] Team notified and trained

### 6.2 Execution Steps

1. **Freeze Admin API changes** in `Mystira.App`
2. **Copy code** to new repository
3. **Update references** to NuGet packages
4. **Test build** in new repository
5. **Deploy to staging** from new repository
6. **Verify functionality**
7. **Switch production** deployment to new repository
8. **Remove Admin API** from `Mystira.App` (after verification period)

### 6.3 Rollback Plan

If issues occur:

1. Keep `Mystira.App` Admin API code intact for 1-2 weeks
2. Can revert deployment to `Mystira.App` if needed
3. Fix issues in new repository
4. Redeploy

## Phase 7: Cleanup (Week 4)

### 7.1 Remove Admin API from Mystira.App

**After successful migration** (1-2 weeks):

1. Remove `src/Mystira.App.Admin.Api/` directory
2. Update solution file
3. Update documentation
4. Commit changes

### 7.2 Update Workspace Submodules

```bash
cd Mystira.workspace
git submodule add -b main https://github.com/phoenixvc/Mystira.Admin.Api.git packages/admin-api
git submodule add -b main https://github.com/phoenixvc/Mystira.Admin.UI.git packages/admin-ui
```

### 7.3 Archive Old Code

- Tag final version in `Mystira.App` with Admin API
- Document migration in CHANGELOG

## Risk Mitigation

### High-Risk Areas

1. **Package Version Management**
   - Risk: Breaking changes in shared packages
   - Mitigation: Semantic versioning, thorough testing

2. **Deployment Issues**
   - Risk: Deployment failures
   - Mitigation: Staging deployment first, rollback plan

3. **Team Coordination**
   - Risk: Confusion during migration
   - Mitigation: Clear communication, documentation

### Testing Strategy

1. **Unit Tests**: All existing tests should pass
2. **Integration Tests**: Test API endpoints
3. **E2E Tests**: Test full admin workflows
4. **Performance Tests**: Ensure no degradation

## Success Criteria

- [ ] Admin API builds and runs from new repository
- [ ] Admin UI builds and runs from new repository
- [ ] Admin UI successfully communicates with Admin API
- [ ] All tests pass (both repositories)
- [ ] Deployed to staging successfully (both services)
- [ ] Deployed to production successfully (both services)
- [ ] No functionality regression
- [ ] Documentation updated (both repositories)
- [ ] Team trained on new structure

## Timeline

| Phase                          | Duration  | Dependencies     | Status          |
| ------------------------------ | --------- | ---------------- | --------------- |
| Phase 1: Setup Shared Packages | 1 week    | NuGet feed setup | In Progress     |
| Phase 2: Create Repositories   | 1-2 weeks | Phase 1          | ✅ **COMPLETED** |
| Phase 3: Setup CI/CD           | 1 week    | Phase 2          | Pending         |
| Phase 4: Migrate Admin UI      | 2-3 weeks | Phase 3          | Pending         |
| Phase 5: Update Documentation  | 1 week    | Phase 2          | Pending         |
| Phase 6: Migration Execution   | 1 week    | Phases 1-3, 5    | Pending         |
| Phase 7: Cleanup               | 1 week    | Phase 6          | Pending         |

**Total Estimated Time**: 8-12 weeks (with Admin UI extraction included)

## Post-Migration

### Monitoring

- Monitor error rates
- Monitor deployment success
- Monitor package version conflicts
- Gather team feedback

### Continuous Improvement

- Iterate on package versioning strategy
- Optimize CI/CD pipelines
- Consider extracting other components if beneficial
