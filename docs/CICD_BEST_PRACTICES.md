# CI/CD Best Practices Guide

This document outlines CI/CD best practices adopted in the Mystira infrastructure and recommendations for future improvements.

## Current Implementation

### Authentication

**OIDC Authentication (Implemented)**

- All Azure authentication uses OIDC (Workload Identity Federation)
- No long-lived secrets for Azure access
- Short-lived tokens issued per workflow run
- See [OIDC_MIGRATION.md](OIDC_MIGRATION.md) for details

### Security Controls

**Feature Toggles (Implemented)**

```yaml
# Repository variables for enabling/disabling features
ENABLE_TFLINT: "true" # Terraform linting
ENABLE_TRIVY: "true" # IaC security scanning (Trivy)
ENABLE_CHECKOV: "true" # Policy-as-code checks
ENABLE_COST_ESTIMATION: "true" # Infracost analysis
```

**Graceful Degradation (Implemented)**

- Workflows check for secret availability before using them
- Missing optional secrets result in skipped steps, not failures
- Pre-flight checks validate configuration before execution

**Concurrency Controls (Implemented)**

```yaml
concurrency:
  group: terraform-${{ inputs.environment }}-${{ inputs.product }}
  cancel-in-progress: false # Don't cancel infrastructure operations
```

### Infrastructure as Code

**Terragrunt Product-Based Structure (Implemented)**

```
infra/terraform/
├── shared-infra/          # Shared resources (VNet, ACR, etc.)
├── products/              # Product-specific infrastructure
│   ├── mystira-app/
│   ├── story-generator/
│   ├── admin/
│   ├── publisher/
│   └── chain/
└── modules/               # Reusable Terraform modules
```

**State Protection (Implemented)**

- `prevent_destroy = true` on critical resources
- Separate state per product and environment
- State locking via Azure Storage

### Monitoring and Alerts

**Drift Detection (Implemented)**

- Daily scheduled drift detection
- Automatic issue creation when drift detected
- Artifact retention for drift reports

**Secret Rotation Monitoring (Implemented)**

- Weekly secret health checks
- Automated alerts for expiring credentials
- GitHub issue creation for action items

## Recommendations for Future Improvements

### 1. Governed Workflow Repository

**Recommendation**: Consider separating CI/CD workflow definitions into a dedicated governed repository.

**Why:**

- **Access Control**: Limit who can modify deployment workflows
- **Change Management**: Require approval for workflow changes
- **Audit Trail**: Clear history of deployment procedure changes
- **Consistency**: Share workflows across multiple repositories
- **Security**: Reduce attack surface in main codebase

**Proposed Structure:**

```
phoenixvc/mystira-workflows/
├── .github/
│   └── workflows/
│       ├── _azure-login.yml
│       ├── _infra-notify.yml
│       └── ...
├── actions/
│   └── custom-actions/
└── docs/
    └── workflow-guidelines.md
```

**Implementation Considerations:**

- Use `workflow_call` to invoke workflows from the governed repo
- Maintain backwards compatibility during migration
- Establish approval process for workflow changes
- Consider using GitHub CODEOWNERS

**Note**: This is a documentation-only recommendation. Implementation would require:

- Creating the new repository
- Migrating workflows gradually
- Updating references in main repository
- Setting up appropriate branch protection

### 2. Environment Promotion Pipeline

**Current State**: Manual promotion via separate workflow triggers

**Recommendation**: Implement automated promotion with gates

```yaml
# Proposed promotion flow
dev → [Tests Pass] → staging → [Approval + Soak] → production
```

**Gates to Consider:**

- Automated test suite pass
- Security scan pass
- Cost estimation within budget
- Manual approval for production
- Soak period in staging (e.g., 24 hours)

### 3. Deployment Strategies

**Current State**: Blue-green for App Service, rolling for Kubernetes

**Recommendations:**

- Implement canary deployments for high-traffic services
- Add automated rollback triggers based on error rates
- Implement feature flags for gradual rollouts

## Rollback Procedures

### Infrastructure Rollback

When an infrastructure deployment fails or causes issues, follow these procedures:

#### Immediate Rollback (Terraform)

1. **Identify the last known good state:**

   ```bash
   # Check recent Terraform state versions in Azure Storage
   az storage blob list --container-name tfstate --account-name mysterraformstate \
     --query "[?contains(name, 'prod')].{name:name,lastModified:properties.lastModified}" \
     --output table
   ```

2. **Rollback using saved plan:**

   ```bash
   # If you saved the previous plan, apply the reverse
   cd infra/terraform/products/<product>/environments/<env>
   terragrunt init
   terragrunt plan -target=<affected-resource>
   terragrunt apply
   ```

3. **Restore from state backup:**

   ```bash
   # Download previous state version
   az storage blob download --container-name tfstate --account-name mysterraformstate \
     --name "<product>/<env>.tfstate" --file backup.tfstate \
     --version-id <previous-version-id>

   # Push restored state (use with caution)
   terragrunt state push backup.tfstate
   ```

#### Workflow-Based Rollback

1. **Re-run previous successful deployment:**
   - Navigate to Actions > [CD] Infrastructure - Terragrunt Deploy
   - Find the last successful run for the affected environment
   - Click "Re-run all jobs"

2. **Promote from lower environment:**
   - If staging is stable, use the promotion workflow to re-deploy
   - Navigate to Actions > [CD] Infrastructure - Environment Promotion
   - Select source: staging, target: prod

#### Kubernetes Rollback

1. **Rollback deployment:**

   ```bash
   kubectl rollout undo deployment/<deployment-name> -n <namespace>
   ```

2. **Rollback to specific revision:**
   ```bash
   kubectl rollout history deployment/<deployment-name> -n <namespace>
   kubectl rollout undo deployment/<deployment-name> --to-revision=<revision> -n <namespace>
   ```

### Application Rollback

#### Container Image Rollback

1. **Identify previous working image:**

   ```bash
   az acr repository show-tags --name mysacr --repository <image-name> \
     --orderby time_desc --output table
   ```

2. **Update deployment to use previous tag:**
   ```bash
   kubectl set image deployment/<deployment-name> \
     <container-name>=mysacr.azurecr.io/<image-name>:<previous-tag> -n <namespace>
   ```

#### Static Web App Rollback

1. **Redeploy from previous commit:**
   - Navigate to the Static Web App in Azure Portal
   - Go to Deployment History
   - Select the previous successful deployment
   - Click "Redeploy"

### Post-Rollback Actions

1. **Verify rollback success:**
   - Check application health endpoints
   - Monitor error rates in Application Insights
   - Verify critical functionality

2. **Document the incident:**
   - Create an incident report
   - Note the root cause of the failure
   - Document the rollback steps taken

3. **Fix forward:**
   - Address the root cause in a new branch
   - Test thoroughly before re-deploying
   - Consider adding additional validation gates

### 4. Observability Integration

**Recommendations:**

- Integrate deployment events with monitoring (e.g., Azure Monitor, Datadog)
- Add deployment markers to dashboards
- Correlate deployments with error rate changes
- Implement deployment health scoring

### 5. Cost Governance

**Current State**: Infracost for PR cost estimation

**Recommendations:**

- Set budget thresholds per environment
- Block deployments exceeding budget
- Implement resource tagging for cost allocation
- Regular cost optimization reviews

## Security Best Practices

### Implemented

1. **OIDC Authentication** - No long-lived Azure secrets
2. **Least Privilege** - Jobs request only needed permissions
3. **Secret Masking** - Automatic masking in logs
4. **Branch Protection** - Required reviews for main branch
5. **Environment Protection** - Approval required for production
6. **Concurrency Control** - Prevent parallel conflicting deployments

### Recommended Additions

1. **Signed Commits** - Require GPG-signed commits
2. **SLSA Compliance** - Implement SLSA build provenance
3. **Dependency Scanning** - Add Dependabot or similar
4. **Container Scanning** - Scan images before deployment
5. **Network Policies** - Restrict workflow network access

## Workflow Patterns

### Reusable Workflows

Use `workflow_call` for common patterns:

```yaml
# _azure-login.yml - Reusable Azure authentication
# _infra-notify.yml - Reusable notifications
```

### Composite Actions

For complex step sequences, consider composite actions:

```yaml
# actions/setup-terraform/action.yml
name: Setup Terraform Environment
runs:
  using: composite
  steps:
    - uses: hashicorp/setup-terraform@v3
    - run: terragrunt --version
```

### Matrix Strategies

For multi-environment validation:

```yaml
strategy:
  fail-fast: false
  matrix:
    environment: [dev, staging, prod]
    product: [mystira-app, story-generator, admin]
```

## Testing Workflows

### Local Testing

Use [act](https://github.com/nektos/act) for local workflow testing:

```bash
act -j build --secret-file .secrets
```

### Validation

```bash
# Validate workflow syntax
yamllint .github/workflows/

# Check GitHub Actions syntax
actionlint .github/workflows/
```

## Related Documentation

- [OIDC Migration Guide](OIDC_MIGRATION.md)
- [Secret Rotation Runbook](SECRET_ROTATION.md)
- [Terraform Migration Guide](../infra/terraform/docs/MIGRATION_GUIDE.md)
- [ADR-0017: Resource Group Strategy](../docs/adr/ADR-0017-resource-group-strategy.md)
- [ADR-0019: CI/CD Architecture](../docs/adr/ADR-0019-cicd-architecture.md)
