# ADR-0001: Infrastructure Organization - Hybrid Approach

## Status

**Accepted** - 2025-01-XX

## Context

The Mystira workspace consists of multiple services with different deployment models:

1. **Chain & Publisher Services**: Containerized Python/TypeScript services deployed to Kubernetes (AKS)
   - Use Docker containers
   - Deployed via Kubernetes manifests
   - Require orchestration capabilities
   - Located in `infra/terraform/modules/` (Terraform) and `infra/kubernetes/`

2. **App Services**: .NET applications using Azure PaaS services
   - Azure App Services (for APIs)
   - Azure Static Web Apps (for PWA)
   - Cosmos DB, Azure Storage, Key Vault
   - Located in `packages/app/infrastructure/` (Azure Bicep)

3. **Story-Generator**: Currently lacks infrastructure templates
   - May require PostgreSQL and Redis
   - Deployment model TBD

### Decision Point

We need to decide whether to:

- **Option A**: Centralize all infrastructure into `infra/` directory
- **Option B**: Keep App infrastructure separate in `packages/app/infrastructure/`
- **Option C**: Hybrid approach with coordination

## Decision

We will **keep the hybrid approach (Option B with enhancements)**:

1. **Keep App infrastructure separate** at `packages/app/infrastructure/` using Azure Bicep
2. **Keep containerized services** in `infra/terraform/` and `infra/kubernetes/` using Terraform
3. **Coordinate shared resources** through well-defined integration points
4. **Document coordination** in infrastructure guide

## Rationale

### 1. Different Deployment Models Require Different Tools

- **Containerized services (Chain/Publisher)**:
  - Best served by Terraform + Kubernetes
  - Require container orchestration
  - Benefit from Terraform's infrastructure management

- **Azure PaaS services (App)**:
  - Best served by Azure Bicep
  - Native Azure service integration
  - Bicep provides better Azure-specific features and faster iteration

### 2. Service Independence

- App services operate independently with their own:
  - Deployment pipelines
  - Release cycles
  - Resource management
- Keeping infrastructure co-located with code improves:
  - Team autonomy
  - Change locality
  - Service ownership

### 3. Tool Fit

| Service Type        | Tool      | Why                                                                |
| ------------------- | --------- | ------------------------------------------------------------------ |
| Containerized (K8s) | Terraform | Industry standard for K8s infrastructure, mature ecosystem         |
| Azure PaaS          | Bicep     | Native Azure support, faster deployments, better Azure integration |
| Mixed               | Both      | Each tool optimized for its use case                               |

### 4. Migration Risk

- App infrastructure is already:
  - Production-ready
  - Well-documented
  - Actively deployed
- Centralization would require:
  - Significant migration effort
  - Re-testing of deployment pipelines
  - Risk to production stability

## Consequences

### Positive

1. **Right tool for the job**: Each service uses the most appropriate infrastructure tool
2. **Service autonomy**: Teams can manage their infrastructure independently
3. **Reduced risk**: No need to migrate working infrastructure
4. **Flexibility**: Services can evolve their infrastructure independently
5. **Co-location**: Infrastructure lives near the code it supports

### Negative

1. **Multiple locations**: Infrastructure code is in two places
2. **Coordination required**: Shared resources need explicit coordination
3. **Documentation overhead**: Need to document how services coordinate
4. **Learning curve**: Developers need to understand both Terraform and Bicep

### Mitigations

1. **Shared resources module**: Create `infra/terraform/modules/shared/` for cross-service resources
2. **Documentation**: Comprehensive infrastructure guide explaining organization
3. **Coordination workflows**: GitHub Actions workflows that coordinate deployments
4. **Integration points**: Well-defined contracts for shared resources (DNS, networking, monitoring)

## Alternatives Considered

### Alternative 1: Fully Centralized (Rejected)

**Approach**: Move all infrastructure to `infra/` directory

**Pros**:

- Single source of truth
- Easier to see all resources at once
- Consistent tooling (if standardized on one)

**Cons**:

- Forces one tool for all scenarios (poor fit)
- Breaks service independence
- High migration risk
- Loss of team autonomy

### Alternative 2: All Separate (Rejected)

**Approach**: Each service maintains its own infrastructure with no coordination

**Pros**:

- Maximum independence
- No coordination overhead

**Cons**:

- Duplication of shared resources (DNS, monitoring, networking)
- Inconsistent patterns
- Harder to manage cross-service dependencies
- Fragmented documentation

## Implementation

1. **Keep existing structure**:
   - `infra/terraform/` - Containerized services (Chain, Publisher)
   - `packages/app/infrastructure/` - Azure PaaS services (App)

2. **Create shared resources module**:
   - `infra/terraform/modules/shared/` - DNS, networking, monitoring coordination

3. **Document coordination**:
   - Infrastructure guide in `docs/INFRASTRUCTURE.md`
   - Integration points clearly documented
   - Shared resource management procedures

4. **Workflow coordination**:
   - Workspace-level workflows that can deploy both when needed
   - Environment-specific coordination

## Related ADRs

- [ADR-0002: Documentation Location Strategy](./0002-documentation-location-strategy.md)
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md)
- [ADR-0004: Branching Strategy and CI/CD Process](./0004-branching-strategy-and-cicd.md)
- [ADR-0005: Service Networking and Communication](./0005-service-networking-and-communication.md)

## References

- [Infrastructure Guide](../INFRASTRUCTURE.md)
- [Azure Bicep Documentation](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/)
- [Terraform Documentation](https://www.terraform.io/docs)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
