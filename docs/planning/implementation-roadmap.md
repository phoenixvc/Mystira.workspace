# Implementation Roadmap

This roadmap outlines the strategic implementation plan for the Mystira workspace, covering infrastructure, documentation, tooling, and operational improvements.

## Current Status Summary

### ✅ Completed

1. **Infrastructure Organization (ADR-0001)**
   - Hybrid infrastructure approach documented
   - App infrastructure remains in `packages/app/infrastructure/`
   - Containerized services infrastructure in `infra/terraform/`
   - Shared resources strategy defined

2. **Documentation Strategy (ADR-0002)**
   - Documentation location strategy established
   - ADRs properly organized (workspace-level vs project-specific)
   - Documentation structure defined

3. **Infrastructure Gaps Addressed**
   - Story-Generator Terraform module created
   - Shared PostgreSQL module created
   - Shared Redis module created
   - Shared monitoring module created
   - Module documentation (READMEs) added

4. **Release Pipeline Strategy (ADR-0003)**
   - Release pipeline strategy documented
   - CI/CD workflows identified and categorized
   - Release coordination approach defined

5. **Distributed CI Model Migration (December 2025)**
   - Migrated dev CI workflows to individual component repositories
   - Updated all .NET components to .NET 9.0
   - Updated all Node.js components to Node.js 20
   - Workspace now focuses on staging/production deployments
   - Component repos: Admin API (#7), Admin UI (#12), Chain (#1), DevHub (#1), Publisher (#13), Story Generator (#56)
   - See [CI/CD Setup](../cicd/cicd-setup.md) for details

6. **ADR-0017: Resource Group Organization (December 2025)**
   - Implemented 3-tier resource group strategy
   - Tier 1: Core resource group with shared infrastructure (VNet, AKS, PostgreSQL, Redis, Service Bus)
   - Tier 2: Service-specific resource groups (chain, publisher, story, admin, app)
   - Tier 3: Cross-environment shared resources (ACR, Communications, Terraform state)
   - All environments (dev, staging, prod) aligned to new structure
   - See [ADR-0017](../architecture/adr/0017-resource-group-organization-strategy.md)

7. **Azure AI Foundry Integration (December 2025)**
   - Created shared Azure AI module for Azure OpenAI Service
   - Deployed gpt-4o and gpt-4o-mini models
   - Auto-populate AI secrets to Story-Generator Key Vault
   - Replaced direct OpenAI/Anthropic API keys

8. **Entra External ID Migration (December 2025)**
   - Migrated from Azure AD B2C to Entra External ID
   - Updated all Terraform modules and documentation
   - Modern CIAM solution with `*.ciamlogin.com` domains

## Implementation Phases

## Phase 1: Infrastructure Foundation (Months 1-2)

**Goal**: Establish core infrastructure patterns and complete basic infrastructure setup.
**Status**: ✅ Complete (December 2025)

### 1.1 Shared Infrastructure Deployment

**Priority**: High
**Dependencies**: None
**Estimated Effort**: 2 weeks
**Status**: ✅ Complete

**Tasks**:

- [x] Create Terraform environment configurations for shared modules
  - [x] `infra/terraform/environments/dev/main.tf` (integrate shared modules)
  - [x] `infra/terraform/environments/staging/main.tf`
  - [x] `infra/terraform/environments/prod/main.tf`
- [x] Deploy shared PostgreSQL module to dev environment
- [x] Deploy shared Redis module to dev environment
- [x] Deploy shared monitoring module to dev environment
- [x] Align staging/prod with dev (admin-api module, AAD auth, workload identity) - December 2025
- [x] Document shared resource usage patterns (see docs/guides/)

**Deliverables**:

- ✅ Shared infrastructure configured in all environments
- ✅ Terraform modules ready for deployment
- ✅ Documentation guides (authentication, networking, deployment)

### 1.2 Story-Generator Infrastructure Integration

**Priority**: High
**Dependencies**: 1.1 (Shared Infrastructure)
**Estimated Effort**: 1 week
**Status**: ✅ Complete

**Tasks**:

- [x] Integrate Story-Generator module into environment configurations
- [x] Configure Story-Generator to use shared PostgreSQL and Redis
- [x] Deploy Story-Generator infrastructure to dev
- [x] Create Kubernetes manifests for Story-Generator service
- [x] Standardize K8s naming to mys-* prefix (December 2025)

**Deliverables**:

- ✅ Story-Generator infrastructure configured
- ✅ Kubernetes manifests ready (infra/kubernetes/base/story-generator/)
- ✅ Deployment documentation (docs/guides/deployment-types-guide.md)

### 1.3 Infrastructure Testing and Validation

**Priority**: Medium
**Dependencies**: 1.1, 1.2
**Estimated Effort**: 1 week
**Status**: ✅ Complete (December 2025)

**Tasks**:

- [x] Create infrastructure validation scripts (`infra-validate.yml` workflow)
- [x] Set up infrastructure monitoring and alerting (shared monitoring module)
- [x] Create troubleshooting documentation (docs/guides/)
- [x] Fix kustomization patch targets to match resource names

**Deliverables**:

- ✅ Infrastructure validation workflow
- ✅ Monitoring with alert action groups
- ✅ Guides: authentication, networking, deployment

## Phase 2: Pipeline Enhancement (Months 2-3)

**Goal**: Enhance CI/CD pipelines and automate release processes.

### 2.1 Pipeline Standardization

**Priority**: Medium
**Dependencies**: None
**Estimated Effort**: 2 weeks
**Status**: ✅ Partially Complete (December 2025)

**Tasks**:

- [x] Create reusable workflow templates for common patterns
  - [x] Container build template (`_docker-build.yml` - December 2025)
  - [x] Terraform deployment template (`_terraform.yml` - December 2025)
  - [x] .NET build/test template (standardized across admin-api, story-generator)
  - [x] Node.js build/test template (standardized across admin-ui, devhub, publisher)
  - [x] Python build/test template (chain)
- [x] Standardize pipeline naming conventions (all use `ci.yml` with consistent job names)
- [ ] Implement consistent error handling across pipelines
- [ ] Add pipeline metrics and reporting

**Deliverables**:

- ✅ Reusable workflow templates (dev CI in component repos)
- ✅ Pipeline documentation (updated cicd-setup.md)
- ✅ Standardized naming conventions

### 2.2 Automated Testing Integration

**Priority**: High  
**Dependencies**: None  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Enhance CI pipelines with comprehensive test coverage
- [ ] Integrate security scanning (SAST/DAST) into pipelines
- [ ] Add performance testing to critical paths
- [ ] Implement test result reporting and notifications
- [ ] Set up test failure analysis dashboards

**Deliverables**:

- Enhanced test coverage
- Security scanning integration
- Test reporting dashboards

### 2.3 Deployment Automation

**Priority**: High  
**Dependencies**: 2.1, 2.2  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Automate environment promotion workflows
- [ ] Implement blue-green deployment for critical services
- [ ] Add automated rollback triggers
- [ ] Create deployment approval workflows
- [ ] Integrate deployment notifications (Slack/Teams)

**Deliverables**:

- Automated deployment workflows
- Rollback automation
- Deployment notifications

## Phase 3: Monitoring and Observability (Months 3-4)

**Goal**: Establish comprehensive monitoring and observability across all services.

### 3.1 Unified Monitoring Setup

**Priority**: High  
**Dependencies**: Phase 1 (Shared Monitoring Module)  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Integrate Chain service with shared Log Analytics
- [ ] Integrate Publisher service with shared Log Analytics
- [ ] Integrate Story-Generator with shared Log Analytics
- [ ] Configure Application Insights for all services
- [ ] Set up unified logging aggregation

**Deliverables**:

- All services integrated with shared monitoring
- Unified log aggregation
- Application Insights dashboards

### 3.2 Alerting and Incident Response

**Priority**: High
**Dependencies**: 3.1
**Estimated Effort**: 1 week
**Status**: ✅ Complete (December 2025)

**Tasks**:

- [x] Define alert severity levels and thresholds
- [x] Create alert rules for critical metrics (December 2025)
  - [x] High error rate (>5% failures)
  - [x] Slow response times (P95 >2s)
  - [x] Unhandled exceptions (>10/5min)
  - [x] Dependency failures (>10% failure rate)
  - [x] High data ingestion (>1GB/hour)
- [x] Action group with email notifications

**Deliverables**:

- ✅ Alert configuration (in Terraform)

> **Deferred to Future Phase**: On-call rotation, incident management integration, and alert runbooks will be addressed as operational maturity increases. These operational procedures are beyond the current infrastructure setup phase and will be implemented when the platform reaches production readiness.

### 3.3 Dashboards and Reporting

**Priority**: Medium  
**Dependencies**: 3.1  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Create service health dashboards
- [ ] Build infrastructure monitoring dashboards
- [ ] Implement performance metrics dashboards
- [ ] Set up deployment tracking dashboards
- [ ] Create executive reporting dashboards

**Deliverables**:

- Monitoring dashboards
- Performance dashboards
- Reporting dashboards

## Phase 4: Documentation and Knowledge Management (Months 4-5)

**Goal**: Complete documentation migration and establish knowledge management practices.

### 4.1 Documentation Migration

**Priority**: Medium  
**Dependencies**: ADR-0002  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Audit all existing documentation
- [ ] Identify and relocate misplaced documentation
- [ ] Update cross-references and links
- [ ] Create documentation templates
- [ ] Establish documentation review process

**Deliverables**:

- Reorganized documentation structure
- Updated documentation index
- Documentation templates

### 4.2 API Documentation

**Priority**: Medium  
**Dependencies**: None  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Generate API documentation for all services
- [ ] Set up API documentation hosting
- [ ] Implement API versioning documentation
- [ ] Create API integration guides
- [ ] Add code examples and tutorials

**Deliverables**:

- API documentation site
- Integration guides
- Code examples

### 4.3 Runbooks and Operational Guides

**Priority**: High  
**Dependencies**: Phase 3 (Monitoring)  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Create service-specific runbooks
- [ ] Document common troubleshooting procedures
- [ ] Create disaster recovery procedures
- [ ] Document escalation procedures
- [ ] Create operational checklists

**Deliverables**:

- Service runbooks
- Troubleshooting guides
- Disaster recovery plan

## Phase 5: Security and Compliance (Months 5-6)

**Goal**: Strengthen security posture and ensure compliance.
**Status**: 🔄 In Progress (December 2025)

### 5.0 Authentication Implementation (Entra ID & External ID)

**Priority**: High
**Dependencies**: None
**Estimated Effort**: 3 weeks
**Status**: ✅ Infrastructure Complete (December 2025)

**Reference**: [ADR-0011: Entra ID Integration](../architecture/adr/0011-entra-id-authentication-integration.md)

**Tasks**:

#### Phase 5.0.1: Microsoft Entra ID (Admin)
- [x] Create Entra ID Terraform module (`infra/terraform/modules/entra-id/` - December 2025)
- [x] Define App Roles (Admin, SuperAdmin, Moderator, Viewer) - in Terraform module
- [x] Define API Scopes (Admin.Read, Admin.Write, Users.Manage, Content.Moderate) - in Terraform module
- [x] Add Entra ID module to all environment configs (dev, staging, prod - December 2025)
- [x] Auto-populate Entra ID secrets to Admin-API Key Vault (December 2025)
- [ ] Deploy Entra ID app registrations (run Terraform)
- [ ] Configure MSAL in Admin UI (React)
- [ ] Add Microsoft.Identity.Web to Admin API
- [ ] Configure group-to-role mapping
- [ ] Create Conditional Access policies (MFA requirement)
- [ ] Test admin authentication flow end-to-end

#### Phase 5.0.2: Microsoft Entra External ID (Consumer)
- [x] Create Entra External ID Terraform module (`infra/terraform/modules/entra-external-id/` - December 2025)
  - [x] Public API app registration with exposed scopes
  - [x] PWA/SPA app registration for Blazor WASM and React clients
  - [x] API scopes: API.Access, Stories.Read, Stories.Write, Profile.Read
- [ ] Create Entra External ID tenant (manual - via Azure Portal)
- [ ] Configure sign-in experience (Email/password, Social)
- [ ] Set up Google identity provider
- [ ] Set up Discord identity provider (OpenID Connect)
- [ ] Update PWA for External ID authentication (Blazor WASM)
- [ ] Customize External ID UI branding
- [ ] Test consumer sign-up/sign-in flow

#### Phase 5.0.3: Service-to-Service Authentication (Managed Identity)
- [x] Create shared identity RBAC module (`infra/terraform/modules/shared/identity/` - December 2025)
  - [x] AKS to ACR role assignments (AcrPull)
  - [x] Key Vault Secrets User role assignments
  - [x] PostgreSQL and Redis access roles
  - [x] Log Analytics contributor roles
  - [x] AKS workload identity federation support
- [x] Add identity module to all environment configs (dev, staging, prod - December 2025)
- [x] Enable OIDC issuer and workload identity on AKS clusters
- [x] Add Admin API module with managed identity to all environments (December 2025)
- [x] Add PostgreSQL Azure AD authentication for passwordless access (December 2025)
- [x] Configure workload_identities for all services in staging/prod (December 2025)
- [ ] Deploy identity infrastructure (run Terraform)
- [ ] Test service-to-service auth

**Deliverables**:

- ✅ Entra ID Terraform module with app registrations, roles, scopes
- ✅ Entra External ID Terraform module for consumer auth
- Entra ID authentication for Admin UI/API (pending app deployment)
- External ID authentication with social login (Google, Discord)
- ✅ Managed Identity for all Azure resources
- MFA enabled for admin users (pending Conditional Access policy)

### 5.1 Secrets Management

**Priority**: High
**Dependencies**: Phase 1 (Infrastructure), Phase 5.0 (Authentication)
**Estimated Effort**: 1 week
**Status**: ✅ Complete (December 2025)

**Tasks**:

- [x] Audit all secrets and credentials
- [x] Centralize secrets in Azure Key Vault (per-service Key Vaults)
- [x] Auto-populate infrastructure secrets from Terraform (December 2025)
  - [x] PostgreSQL connection strings
  - [x] Redis connection strings
  - [x] Service Bus connection strings
  - [x] Application Insights connection strings
  - [x] Azure AI Foundry endpoint and API key
  - [x] Entra ID tenant/client IDs
- [x] Zero GitHub Secrets required for infrastructure
- [x] Document secrets management procedures (docs/infrastructure/secrets-management.md)
- [ ] Store social provider secrets (Google, Discord) in Key Vault
- [ ] Implement secrets rotation policies
- [ ] Set up secrets access auditing

**Deliverables**:

- ✅ All secrets auto-populated by Terraform
- ✅ Secrets management documentation
- ✅ CI/CD secret validation workflow
- Social provider secrets (pending External ID setup)
- Rotation policies (future enhancement)

### 5.2 Security Scanning and Compliance

**Priority**: High
**Dependencies**: None
**Estimated Effort**: 2 weeks
**Status**: ✅ Partially Complete (December 2025)

**Tasks**:

- [x] Implement automated security scanning in CI/CD (December 2025)
  - [x] Reusable security workflow (`_security-scan.yml`)
  - [x] CodeQL SAST analysis for .NET, JavaScript, Python
  - [x] Dependency vulnerability scanning (dotnet, npm, pip-audit)
  - [x] Container image scanning (Trivy)
  - [x] Secret detection (TruffleHog)
- [x] Set up dependency vulnerability scanning
- [x] Implement infrastructure security scanning (tfsec, checkov)
- [x] Scheduled weekly security scans (`security-scan.yml`)
- [ ] Create compliance checklists
- [ ] Set up security incident response procedures

**Deliverables**:

- ✅ Security scanning integration
- [ ] Compliance checklists
- [ ] Security incident procedures

### 5.3 Access Control and IAM

**Priority**: Medium  
**Dependencies**: 5.1  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Review and audit IAM policies
- [ ] Implement least-privilege access
- [ ] Set up access review procedures
- [ ] Configure RBAC for all services
- [ ] Document access management procedures

**Deliverables**:

- IAM audit report
- Access policies
- Access management documentation

## Phase 6: Performance and Scalability (Months 6-7)

**Goal**: Optimize performance and prepare for scale.

### 6.1 Performance Optimization

**Priority**: Medium  
**Dependencies**: Phase 3 (Monitoring)  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Conduct performance baseline analysis
- [ ] Identify performance bottlenecks
- [ ] Implement performance optimizations
- [ ] Set up performance testing
- [ ] Create performance benchmarks

**Deliverables**:

- Performance analysis report
- Optimization implementations
- Performance benchmarks

### 6.2 Scalability Planning

**Priority**: Medium  
**Dependencies**: 6.1  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Define scalability requirements
- [ ] Implement auto-scaling for services
- [ ] Set up load testing infrastructure
- [ ] Create capacity planning procedures
- [ ] Document scaling runbooks

**Deliverables**:

- Auto-scaling configuration
- Capacity planning documentation
- Scaling runbooks

### 6.3 Database Optimization

**Priority**: Medium  
**Dependencies**: Phase 1 (Shared PostgreSQL)  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Analyze database performance
- [ ] Optimize database queries
- [ ] Implement database indexing strategies
- [ ] Set up database monitoring
- [ ] Create database maintenance procedures

**Deliverables**:

- Database optimization report
- Indexing strategy
- Database maintenance procedures

## Phase 7: Developer Experience (Months 7-8)

**Goal**: Improve developer productivity and onboarding.

### 7.1 Local Development Environment

**Priority**: High  
**Dependencies**: Phase 1 (Infrastructure)  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Enhance docker-compose setup
- [ ] Create local development scripts
- [ ] Document local setup procedures
- [ ] Create development environment validation
- [ ] Set up local debugging tools

**Deliverables**:

- Enhanced docker-compose
- Local development scripts
- Developer onboarding guide

### 7.2 Developer Tools and Automation

**Priority**: Medium  
**Dependencies**: None  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Create code generation tools
- [ ] Implement development automation scripts
- [ ] Set up pre-commit hooks
- [ ] Create developer utilities
- [ ] Document development workflows

**Deliverables**:

- Developer tools
- Automation scripts
- Development workflow documentation

### 7.3 Onboarding and Training

**Priority**: Medium  
**Dependencies**: Phase 4 (Documentation)  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Create onboarding checklist
- [ ] Develop training materials
- [ ] Create architecture diagrams
- [ ] Set up knowledge base
- [ ] Conduct onboarding sessions

**Deliverables**:

- Onboarding checklist
- Training materials
- Architecture diagrams

## Phase 8: Advanced Features (Months 8-9)

**Goal**: Implement advanced operational features.

### 8.1 Feature Flags

**Priority**: Medium  
**Dependencies**: None  
**Estimated Effort**: 1 week

**Tasks**:

- [ ] Evaluate feature flag solutions
- [ ] Implement feature flag infrastructure
- [ ] Integrate with CI/CD pipelines
- [ ] Create feature flag management procedures
- [ ] Document feature flag usage

**Deliverables**:

- Feature flag infrastructure
- Integration with pipelines
- Feature flag documentation

### 8.2 Canary Deployments

**Priority**: Low  
**Dependencies**: Phase 2 (Deployment Automation)  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Implement canary deployment infrastructure
- [ ] Create canary deployment workflows
- [ ] Set up traffic splitting
- [ ] Implement automatic rollback on errors
- [ ] Document canary deployment procedures

**Deliverables**:

- Canary deployment infrastructure
- Deployment workflows
- Canary deployment documentation

### 8.3 Chaos Engineering

**Priority**: Low  
**Dependencies**: Phase 3 (Monitoring)  
**Estimated Effort**: 2 weeks

**Tasks**:

- [ ] Evaluate chaos engineering tools
- [ ] Implement chaos experiments
- [ ] Set up chaos testing infrastructure
- [ ] Create chaos engineering runbooks
- [ ] Document chaos engineering practices

**Deliverables**:

- Chaos engineering setup
- Experiment runbooks
- Chaos engineering documentation

## Success Metrics

### Infrastructure Metrics

- **Infrastructure deployment time**: < 30 minutes
- **Infrastructure deployment success rate**: > 95%
- **Infrastructure test coverage**: > 80%

### Pipeline Metrics

- **CI pipeline duration**: < 15 minutes
- **CD pipeline duration**: < 30 minutes
- **Pipeline success rate**: > 90%
- **Deployment frequency**: Multiple per day

### Monitoring Metrics

- **Alert response time**: < 5 minutes
- **Mean time to resolution (MTTR)**: < 1 hour
- **System uptime**: > 99.9%
- **Monitoring coverage**: 100% of services

### Documentation Metrics

- **Documentation completeness**: > 90%
- **Documentation accuracy**: > 95%
- **Time to find information**: < 5 minutes
- **Documentation freshness**: Updated within 1 week of changes

## Risk Management

### High-Risk Areas

1. **Infrastructure deployment failures**
   - Mitigation: Comprehensive testing, rollback procedures, staged rollouts
2. **Service downtime during migrations**
   - Mitigation: Blue-green deployments, feature flags, staged migrations
3. **Documentation gaps during migration**
   - Mitigation: Parallel documentation, thorough audits, gradual migration

### Dependencies and Blockers

- External dependencies (Azure services, npm packages)
- Team capacity and availability
- Budget approvals for infrastructure resources
- Security and compliance approvals

## Resource Requirements

### Infrastructure

- Azure subscription with appropriate quotas
- Kubernetes cluster capacity
- Database and cache resources
- Monitoring and logging storage

### Tools and Services

- GitHub Actions (CI/CD)
- Azure services (App Services, AKS, etc.)
- Monitoring tools (Application Insights, Log Analytics)
- Security scanning tools

### Team

- DevOps engineers (infrastructure, pipelines)
- Developers (service integration, testing)
- Documentation writers (documentation migration)
- Security engineers (security and compliance)

## Review and Adaptation

This roadmap should be reviewed and updated:

- **Monthly**: Progress review and adjustments
- **Quarterly**: Strategic alignment and priority adjustments
- **Annually**: Comprehensive review and planning for next year

## Related Documentation

- [Infrastructure Guide](../infrastructure/infrastructure.md)
- [Secrets Management](../infrastructure/secrets-management.md)
- [Entra ID Best Practices](../infrastructure/entra-id-best-practices.md)
- [ADR-0001: Infrastructure Organization](../architecture/adr/0001-infrastructure-organization-hybrid-approach.md)
- [ADR-0002: Documentation Location Strategy](../architecture/adr/0002-documentation-location-strategy.md)
- [ADR-0003: Release Pipeline Strategy](../architecture/adr/0003-release-pipeline-strategy.md)
- [ADR-0010: Authentication Strategy](../architecture/adr/0010-authentication-and-authorization-strategy.md)
- [ADR-0011: Entra ID Integration](../architecture/adr/0011-entra-id-authentication-integration.md)
- [ADR-0017: Resource Group Organization](../architecture/adr/0017-resource-group-organization-strategy.md)
