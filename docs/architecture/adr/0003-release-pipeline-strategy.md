# ADR-0003: Release Pipeline Strategy

## Status

**Accepted** - 2025-01-XX

## Context

The Mystira workspace contains multiple services with different deployment models and requirements:

1. **Workspace-Level Packages**: JavaScript/TypeScript packages that may be published to npm
   - Managed via pnpm workspaces
   - Use Changesets for version management
   - Located in workspace root or `packages/`

2. **Containerized Services**: Chain, Publisher, Story-Generator
   - Deployed to Kubernetes (AKS)
   - Use Docker images
   - Infrastructure managed via Terraform
   - CI/CD workflows in workspace `.github/workflows/`

3. **App Services**: .NET applications with Azure PaaS deployment
   - API, Admin API, PWA
   - Deployed to Azure App Services and Static Web Apps
   - Infrastructure managed via Azure Bicep
   - CI/CD workflows in `packages/app/.github/workflows/`

### Problem Statement

Different services have different release requirements:

- **Workspace packages**: Version management, npm publishing, semantic versioning
- **Containerized services**: Image building, registry publishing, Kubernetes deployment
- **App services**: Build, test, deploy to Azure environments (dev/staging/prod)

We need a unified strategy for:

- Release coordination
- Version management
- Deployment automation
- Environment promotion
- Rollback procedures

## Decision

We will adopt a **hybrid release pipeline strategy** with clear separation and coordination:

### 1. Workspace-Level Release Pipeline

**Location**: `.github/workflows/release.yml`

**Purpose**: Version management and npm package publishing

**Features**:

- Changesets-based versioning
- Automatic PR creation for version bumps
- npm package publishing
- Workspace-wide dependency management

**Process**:

1. Developers create changeset files for changes
2. Release workflow runs on push to `main`
3. Changesets action creates version PR or publishes directly
4. Version bumps are coordinated across workspace packages

### 2. Service-Specific CI/CD Pipelines

Each service maintains its own CI/CD workflows with environment-specific deployments:

#### Containerized Services (Chain, Publisher, Story-Generator)

**Location**: `.github/workflows/{service}-ci.yml` and `infra-deploy.yml`

**Process**:

1. **CI Pipeline** (on PR/push):
   - Lint and test code
   - Build Docker images
   - Run security scans
   - No deployment

2. **Infrastructure Deployment** (on infrastructure changes):
   - Plan/apply Terraform
   - Deploy to Kubernetes
   - Environment-specific (dev/staging/prod)

3. **Service Deployment** (after infrastructure):
   - Build and push images
   - Update Kubernetes manifests
   - Rollout to cluster

#### App Services (API, Admin API, PWA)

**Location**: `packages/app/.github/workflows/mystira-app-*-cicd-{env}.yml`

**Process**:

1. **CI Pipeline** (on PR):
   - Build and test
   - Run tests
   - Code coverage

2. **CD Pipeline** (on merge to environment branch):
   - Build .NET application
   - Deploy to Azure App Service / Static Web App
   - Run smoke tests
   - Environment-specific workflows (dev/staging/prod)

**Infrastructure**: Separate `infrastructure-deploy-{env}.yml` workflows for Bicep deployments

### 3. Release Coordination

**Manual Coordination**:

- Workspace packages: Changesets PR review
- Service deployments: Environment branch merges
- Infrastructure: Manual approval for prod

**Automated Coordination**:

- Workspace release triggers npm publish
- Service CI/CD pipelines run independently
- Infrastructure changes trigger deployment workflows

## Rationale

### 1. Different Deployment Models Require Different Pipelines

- **Workspace packages**: Need semantic versioning and npm publishing
- **Containerized services**: Need Docker builds and Kubernetes deployments
- **App services**: Need .NET builds and Azure PaaS deployments

Each requires different tooling and processes.

### 2. Service Autonomy

- Services can release independently
- No tight coupling between releases
- Teams can manage their own release cadence

### 3. Environment Promotion

- Clear separation between dev/staging/prod
- Manual approval gates for production
- Rollback capabilities per service

### 4. Infrastructure as Code

- Infrastructure changes trigger separate workflows
- Infrastructure and application code deploy separately
- Reduces risk of infrastructure changes affecting application deployments

## Consequences

### Positive

1. **Service independence**: Each service can release on its own schedule
2. **Clear separation**: Workspace releases vs. service deployments
3. **Environment safety**: Manual gates prevent accidental prod deployments
4. **Flexibility**: Different services can use different deployment strategies
5. **Scalability**: Easy to add new services with their own pipelines

### Negative

1. **Multiple pipelines**: More workflows to maintain
2. **Coordination overhead**: Manual coordination for cross-service releases
3. **Complexity**: Different processes for different services
4. **Documentation**: Need to document each pipeline type

### Mitigations

1. **Pipeline templates**: Use reusable workflow templates where possible
2. **Documentation**: Comprehensive pipeline documentation
3. **Standardization**: Standard patterns within each pipeline type
4. **Monitoring**: Unified monitoring for all deployments

## Pipeline Details

### Workspace Release Pipeline

**Trigger**: Push to `main` branch

**Workflow**: `.github/workflows/release.yml`

**Steps**:

1. Checkout with submodules
2. Setup pnpm and Node.js
3. Install dependencies
4. Run Changesets action:
   - Create version PR if changesets exist
   - Publish packages if version PR merged

**Versioning**: Semantic versioning via Changesets

### Containerized Service Pipelines

**CI Workflow**: `.github/workflows/{service}-ci.yml`

- Lint
- Test
- Build (no publish)

**Infrastructure Workflow**: `.github/workflows/infra-deploy.yml`

- Terraform plan/apply
- Kubernetes deployment
- Manual approval for prod

**Environments**: dev (auto), staging (auto), prod (manual)

### App Service Pipelines

**CI Workflow**: `packages/app/.github/workflows/ci-tests-codecov.yml`

- Build
- Test
- Code coverage

**CD Workflows**: `packages/app/.github/workflows/mystira-app-*-cicd-{env}.yml`

- Build .NET application
- Deploy to Azure
- Smoke tests

**Infrastructure Workflows**: `packages/app/.github/workflows/infrastructure-deploy-{env}.yml`

- Azure Bicep deployment
- Manual approval for prod

**Environments**: dev, staging, prod (branch-based)

## Release Process

### For Workspace Packages

1. Make changes to packages
2. Create changeset: `pnpm changeset`
3. Commit changes and changeset
4. Push to PR
5. After merge, release workflow:
   - Creates version PR (if multiple changesets)
   - Or publishes directly (if single changeset)

### For Containerized Services

1. Make code changes
2. Create PR (triggers CI)
3. Merge to `main` (triggers infrastructure deployment for dev)
4. Promote to staging (merge or manual trigger)
5. Promote to prod (manual approval required)

### For App Services

1. Make code changes
2. Create PR (triggers CI)
3. Merge to environment branch:
   - `dev` → auto-deploy to dev
   - `staging` → auto-deploy to staging
   - `main` → manual approval for prod

## Rollback Procedures

### Workspace Packages

- Revert version PR
- Publish patch version with fix

### Containerized Services

- Revert Kubernetes manifest changes
- Rollback to previous image tag
- Use `kubectl rollout undo`

### App Services

- Azure App Service slot swap
- Git revert and redeploy
- Azure Static Web App rollback via portal

## Monitoring and Observability

- GitHub Actions workflow status
- Deployment notifications (Slack/Teams)
- Application health checks post-deployment
- Infrastructure deployment status

## Future Enhancements

1. **Unified Release Dashboard**: Single view of all service releases
2. **Cross-Service Release Coordination**: Coordinated multi-service releases
3. **Automated Rollback**: Automatic rollback on health check failures
4. **Release Notes Generation**: Automatic release notes from changesets/commits
5. **Feature Flags**: Integration with feature flag management
6. **Canary Deployments**: Gradual rollout for critical services

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0002: Documentation Location Strategy](./0002-documentation-location-strategy.md)
- [ADR-0004: Branching Strategy and CI/CD Process](./0004-branching-strategy-and-cicd.md)
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md)
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md)
- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md)

## References

- [Changesets Documentation](https://github.com/changesets/changesets)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Best Practices](https://docs.microsoft.com/en-us/azure/devops/)
- [Kubernetes Deployment Strategies](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
