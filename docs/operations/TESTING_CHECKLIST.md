# Testing Checklist - Azure v2.2 Naming Convention Migration

**Status**: Draft
**Version**: 1.0
**Last Updated**: 2025-12-21
**Author**: DevOps Team

## Overview

This checklist ensures comprehensive testing of the Azure v2.2 naming convention migration across all environments. Each section must be completed and signed off before proceeding to the next phase.

## Pre-Deployment Testing

### Terraform Validation

- [ ] **Terraform Init**: All environments initialize successfully
  ```bash
  for env in dev staging prod; do
    cd infra/terraform/environments/$env
    terraform init
  done
  ```

- [ ] **Terraform Validate**: No syntax or validation errors
  ```bash
  for env in dev staging prod; do
    cd infra/terraform/environments/$env
    terraform validate
  done
  ```

- [ ] **Terraform Plan**: Review execution plans for all environments
  ```bash
  for env in dev staging prod; do
    cd infra/terraform/environments/$env
    terraform plan -out=tfplan-$env
    terraform show tfplan-$env > tfplan-$env.txt
  done
  ```

- [ ] **Resource Count**: Verify expected number of resources to be created/modified/destroyed
  - Dev: Expected creates: ~25, destroys: ~0 (fresh deployment)
  - Staging: Expected creates: ~25, destroys: ~0
  - Prod: Expected creates: ~25, destroys: ~0

- [ ] **Naming Validation**: All resource names follow v2.2 pattern
  ```bash
  # Pattern: [org]-[env]-[project]-[type]-[region]
  grep -r "mys-.*-core-.*-san" infra/terraform/
  ```

- [ ] **Region Validation**: All resources deploying to South Africa North (san)
  ```bash
  grep -r "location.*=.*southafricanorth" infra/terraform/
  ```

### Kubernetes Manifest Validation

- [ ] **Kustomize Build**: All overlays build successfully
  ```bash
  for env in dev staging prod; do
    kubectl kustomize infra/kubernetes/overlays/$env/ > /dev/null
    echo "✓ $env overlay builds successfully"
  done
  ```

- [ ] **Key Vault URLs**: All ConfigMaps reference correct Key Vault URLs
  ```bash
  kubectl kustomize infra/kubernetes/overlays/dev/ | grep "key_vault_url"
  # Expected: https://mys-dev-{service}-kv-san.vault.azure.net/

  kubectl kustomize infra/kubernetes/overlays/staging/ | grep "key_vault_url"
  # Expected: https://mys-stag-{service}-kv-san.vault.azure.net/

  kubectl kustomize infra/kubernetes/overlays/prod/ | grep "key_vault_url"
  # Expected: https://mys-prod-{service}-kv-san.vault.azure.net/
  ```

- [ ] **ACR References**: All images reference correct ACR
  ```bash
  kubectl kustomize infra/kubernetes/overlays/dev/ | grep "image:"
  # Expected: myssharedacr.azurecr.io/
  ```

- [ ] **Namespace Validation**: Correct namespaces configured
  ```bash
  kubectl kustomize infra/kubernetes/overlays/dev/ | grep "namespace:"
  # Expected: mys-dev
  ```

### Workflow Validation

- [ ] **All workflows reference new naming**:
  ```bash
  grep -r "mys-.*-core-.*-san" .github/workflows/
  ```

- [ ] **No old naming patterns remaining**:
  ```bash
  # Should return no results
  grep -r "mys-.*-mystira-.*-eus" .github/workflows/ || echo "✓ No old patterns found"
  ```

- [ ] **ACR name updated everywhere**:
  ```bash
  grep -r "myssharedacr" .github/workflows/
  grep -r "mysprodacr" .github/workflows/ && echo "✗ Old ACR name found" || echo "✓ No old ACR names"
  ```

### Documentation Review

- [ ] All markdown files updated with new naming
- [ ] ADR 0008 accurately reflects new convention
- [ ] Migration guide complete and accurate
- [ ] Deployment strategy documented
- [ ] Data migration plan reviewed
- [ ] Rollback procedure documented

## Dev Environment Testing

### Infrastructure Deployment

- [ ] **Deploy Infrastructure**:
  ```bash
  cd infra/terraform/environments/dev
  terraform apply
  ```

- [ ] **Verify Resource Group Created**:
  ```bash
  az group show --name mys-dev-core-rg-san
  ```

- [ ] **Verify VNet Created**:
  ```bash
  az network vnet show \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-vnet-san
  ```

- [ ] **Verify AKS Cluster Created**:
  ```bash
  az aks show \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-aks-san
  ```

- [ ] **Verify PostgreSQL Server Created**:
  ```bash
  az postgres flexible-server show \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-psql-san
  ```

- [ ] **Verify Redis Cache Created**:
  ```bash
  az redis show \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-redis-san
  ```

- [ ] **Verify Key Vaults Created**:
  ```bash
  az keyvault list \
    --resource-group mys-dev-core-rg-san \
    --query "[].name" -o tsv
  # Expected: mys-dev-shared-kv-san, mys-dev-pub-kv-san, mys-dev-story-kv-san
  ```

- [ ] **Verify Log Analytics Workspace**:
  ```bash
  az monitor log-analytics workspace show \
    --resource-group mys-dev-core-rg-san \
    --workspace-name mys-dev-core-law-san
  ```

- [ ] **Verify Storage Account**:
  ```bash
  az storage account show \
    --resource-group mys-dev-core-rg-san \
    --name mysdevcorestoragesa
  ```

### AKS Cluster Validation

- [ ] **Get Credentials**:
  ```bash
  az aks get-credentials \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-aks-san \
    --overwrite-existing
  ```

- [ ] **Cluster Info**:
  ```bash
  kubectl cluster-info
  ```

- [ ] **Verify Nodes Running**:
  ```bash
  kubectl get nodes
  # Expected: 2-3 nodes in Ready state
  ```

- [ ] **Verify System Pods Running**:
  ```bash
  kubectl get pods -n kube-system
  # All pods should be Running
  ```

- [ ] **Verify Ingress Controller**:
  ```bash
  kubectl get pods -n ingress-nginx
  kubectl get svc -n ingress-nginx
  # Ingress controller should have external IP
  ```

- [ ] **Verify Cert-Manager**:
  ```bash
  kubectl get pods -n cert-manager
  ```

### Application Deployment

- [ ] **Deploy Applications**:
  ```bash
  kubectl apply -k infra/kubernetes/overlays/dev/
  ```

- [ ] **Verify Namespace Created**:
  ```bash
  kubectl get namespace mys-dev
  ```

- [ ] **Verify Pods Running**:
  ```bash
  kubectl get pods -n mys-dev
  # All pods should reach Running state within 5 minutes
  ```

- [ ] **Verify ConfigMaps**:
  ```bash
  kubectl get configmaps -n mys-dev
  kubectl describe configmap mys-publisher-config -n mys-dev
  kubectl describe configmap mys-story-generator-config -n mys-dev
  ```

- [ ] **Verify Secrets**:
  ```bash
  kubectl get secrets -n mys-dev
  # Secrets should be populated by CSI driver
  ```

- [ ] **Verify Services**:
  ```bash
  kubectl get services -n mys-dev
  ```

- [ ] **Verify Ingress**:
  ```bash
  kubectl get ingress -n mys-dev
  kubectl describe ingress -n mys-dev
  # Should have external IP assigned
  ```

### Database Validation

- [ ] **Database Connectivity**:
  ```bash
  # Test from pod
  kubectl exec -n mys-dev deploy/mys-publisher -- \
    sh -c 'psql $DATABASE_URL -c "SELECT 1"'
  ```

- [ ] **Databases Exist**:
  ```bash
  az postgres flexible-server db list \
    --resource-group mys-dev-core-rg-san \
    --server-name mys-dev-core-psql-san \
    --query "[].name" -o tsv
  # Expected: chain_db, publisher_db, story_generator_db
  ```

- [ ] **Test Data Seeded** (if applicable):
  ```bash
  kubectl exec -n mys-dev deploy/mys-publisher -- \
    sh -c 'psql $DATABASE_URL -c "SELECT COUNT(*) FROM users"'
  ```

### Redis Validation

- [ ] **Redis Connectivity**:
  ```bash
  kubectl exec -n mys-dev deploy/mys-publisher -- \
    sh -c 'redis-cli -u $REDIS_URL ping'
  # Expected: PONG
  ```

- [ ] **Redis Info**:
  ```bash
  az redis show \
    --resource-group mys-dev-core-rg-san \
    --name mys-dev-core-redis-san \
    --query "[hostName,sslPort,provisioningState]" -o table
  ```

### Key Vault Validation

- [ ] **Key Vault Access**:
  ```bash
  # Test from AKS pod identity
  kubectl exec -n mys-dev deploy/mys-publisher -- \
    sh -c 'az keyvault secret list --vault-name mys-dev-pub-kv-san --query "[].name" -o tsv'
  ```

- [ ] **CSI Driver Mounted Secrets**:
  ```bash
  kubectl exec -n mys-dev deploy/mys-publisher -- \
    ls -la /mnt/secrets-store/
  # Should see secret files
  ```

### Certificate Validation

- [ ] **Certificates Issued**:
  ```bash
  kubectl get certificates -n mys-dev
  # All should show Ready=True
  ```

- [ ] **Certificate Details**:
  ```bash
  kubectl describe certificate -n mys-dev
  # Should show Let's Encrypt issuer
  ```

- [ ] **TLS Secrets Created**:
  ```bash
  kubectl get secrets -n mys-dev -l "cert-manager.io/certificate-name"
  ```

### Endpoint Testing

- [ ] **Health Endpoints**:
  ```bash
  curl -f https://dev.publisher.mystira.app/health
  curl -f https://dev.chain.mystira.app/health
  curl -f https://dev.story-generator.mystira.app/health
  ```

- [ ] **API Endpoints**:
  ```bash
  # Test basic API functionality
  curl -X POST https://dev.publisher.mystira.app/api/v1/test
  ```

- [ ] **WebSocket Connections** (if applicable):
  ```bash
  wscat -c wss://dev.chain.mystira.app/ws
  ```

### Application Logs

- [ ] **No Critical Errors in Logs**:
  ```bash
  kubectl logs -n mys-dev -l app.kubernetes.io/name=mys-publisher --tail=100
  kubectl logs -n mys-dev -l app.kubernetes.io/name=mys-chain --tail=100
  kubectl logs -n mys-dev -l app.kubernetes.io/name=mys-story-generator --tail=100
  ```

- [ ] **Log Aggregation Working**:
  ```bash
  # Verify logs flowing to Log Analytics
  az monitor log-analytics query \
    --workspace mys-dev-core-law-san \
    --analytics-query "ContainerLog | where TimeGenerated > ago(1h) | take 10"
  ```

### Monitoring

- [ ] **Metrics Available in Azure Monitor**
- [ ] **Application Insights Connected** (if configured)
- [ ] **Alerts Configured** (if applicable)

### Dev Environment Sign-Off

**Tester**: _________________
**Date**: _________________
**Signature**: _________________

---

## Staging Environment Testing

### Pre-Deployment

- [ ] Dev environment fully validated and stable for 24 hours
- [ ] All dev issues resolved
- [ ] Stakeholders notified of staging deployment

### Data Migration

- [ ] **Backup Staging Databases**:
  ```bash
  ./scripts/backup-postgres.sh staging
  ```

- [ ] **Verify Backup Integrity**:
  ```bash
  ls -lh backups/staging_*/
  # Verify file sizes are reasonable
  ```

- [ ] **Upload Backup to Azure Storage**:
  ```bash
  az storage blob upload-batch \
    --account-name mysstagingstorageeus \
    --destination database-backups \
    --source backups/staging_*/
  ```

### Infrastructure Deployment

- [ ] Deploy staging infrastructure (same checklist as dev)
- [ ] All resources created successfully
- [ ] AKS cluster operational

### Data Restore

- [ ] **Restore Databases**:
  ```bash
  ./scripts/restore-postgres.sh staging
  ```

- [ ] **Verify Data Integrity**:
  ```bash
  ./scripts/verify-data.sh staging
  ```

- [ ] **Row Count Comparison**:
  - [ ] chain_db: Old ___ rows, New ___ rows ✓ Match
  - [ ] publisher_db: Old ___ rows, New ___ rows ✓ Match
  - [ ] story_generator_db: Old ___ rows, New ___ rows ✓ Match

### Application Deployment

- [ ] Deploy applications (same checklist as dev)
- [ ] All pods running
- [ ] All endpoints responding

### Automated Testing

- [ ] **Run E2E Test Suite**:
  ```bash
  npm run test:e2e -- --env=staging
  ```

- [ ] **Run Integration Tests**:
  ```bash
  npm run test:integration -- --env=staging
  ```

- [ ] **Run Performance Tests**:
  ```bash
  npm run test:performance -- --env=staging
  ```

### Manual Testing

- [ ] User registration flow
- [ ] Login/authentication
- [ ] Story generation workflow
- [ ] Publishing workflow
- [ ] Chain interaction
- [ ] File uploads
- [ ] Email notifications (if applicable)

### Comparison with Old Staging

- [ ] Response times comparable
- [ ] Error rates comparable
- [ ] Feature parity verified
- [ ] Data consistency verified

### Staging Environment Sign-Off

**Tester**: _________________
**Date**: _________________
**Signature**: _________________

---

## Production Environment Testing

### Pre-Deployment

- [ ] Staging environment stable for 48 hours
- [ ] All staging issues resolved
- [ ] Change Advisory Board approval (if applicable)
- [ ] Stakeholder sign-off
- [ ] Maintenance window scheduled and communicated
- [ ] On-call team briefed
- [ ] Rollback plan reviewed

### Data Migration

- [ ] **Final Production Backup**:
  ```bash
  ./scripts/backup-postgres.sh prod
  ```

- [ ] **Verify Backup Integrity**:
  ```bash
  # Test restore to temporary server
  az postgres flexible-server create \
    --resource-group mys-prod-core-rg-san \
    --name mys-prod-temp-psql-san \
    --location southafricanorth

  ./scripts/restore-postgres.sh prod --server mys-prod-temp-psql-san

  # Verify, then delete temp server
  az postgres flexible-server delete \
    --resource-group mys-prod-core-rg-san \
    --name mys-prod-temp-psql-san
  ```

- [ ] **Upload Backup to Multiple Locations**:
  ```bash
  # Primary backup
  az storage blob upload-batch \
    --account-name mysprodstorageeus \
    --destination database-backups \
    --source backups/prod_*/

  # Secondary backup (different region)
  az storage blob upload-batch \
    --account-name mysprodbackupwest \
    --destination database-backups \
    --source backups/prod_*/
  ```

### Infrastructure Deployment

- [ ] Deploy production infrastructure
- [ ] All resources created successfully
- [ ] AKS cluster operational
- [ ] Network connectivity verified

### Data Restore

- [ ] Restore databases to new PostgreSQL server
- [ ] Verify data integrity
- [ ] Row count comparison (document counts):
  - [ ] chain_db: Old _______ rows, New _______ rows ✓
  - [ ] publisher_db: Old _______ rows, New _______ rows ✓
  - [ ] story_generator_db: Old _______ rows, New _______ rows ✓

### Application Deployment

- [ ] Deploy applications to new AKS
- [ ] All pods running healthy
- [ ] No crash loops
- [ ] Logs showing normal startup

### Pre-Cutover Validation

- [ ] **Synthetic Transactions**:
  ```bash
  # Test against new infrastructure before cutover
  curl -H "Host: publisher.mystira.app" https://<NEW_INGRESS_IP>/health
  ```

- [ ] **Database Queries**:
  ```bash
  kubectl exec -n mys-prod deploy/mys-publisher -- \
    sh -c 'psql $DATABASE_URL -c "SELECT COUNT(*) FROM users"'
  ```

- [ ] **Cache Connectivity**:
  ```bash
  kubectl exec -n mys-prod deploy/mys-publisher -- \
    sh -c 'redis-cli -u $REDIS_URL ping'
  ```

### Cutover

- [ ] **DNS/Ingress Update** (cutover moment):
  ```bash
  # Update DNS records or ingress controller
  kubectl apply -k infra/kubernetes/overlays/prod/
  ```

- [ ] **Verify New IP Active**:
  ```bash
  kubectl get ingress -n mys-prod
  nslookup publisher.mystira.app
  ```

- [ ] **First Production Request**:
  ```bash
  curl -f https://publisher.mystira.app/health
  # Record timestamp: _______________
  ```

### Post-Cutover Monitoring (First Hour)

- [ ] **0-5 min**: Monitor pod health
  ```bash
  watch kubectl get pods -n mys-prod
  ```

- [ ] **0-5 min**: Monitor application logs
  ```bash
  kubectl logs -n mys-prod -l app.kubernetes.io/name=mys-publisher -f
  ```

- [ ] **5-10 min**: Check error rates
  - HTTP 2xx: ____%
  - HTTP 4xx: ____%
  - HTTP 5xx: ____%

- [ ] **10-15 min**: Check response times
  - p50: _____ ms
  - p95: _____ ms
  - p99: _____ ms

- [ ] **15-30 min**: Database performance
  - Active connections: _____
  - Query performance: Normal / Degraded
  - No deadlocks: ✓

- [ ] **30-60 min**: End-to-end functionality
  - [ ] User login
  - [ ] Story creation
  - [ ] Publishing
  - [ ] File upload
  - [ ] All critical paths working

### Post-Cutover Monitoring (24 Hours)

- [ ] **Hour 2**: No critical issues
- [ ] **Hour 4**: Response times stable
- [ ] **Hour 8**: Error rates normal
- [ ] **Hour 12**: Database performance normal
- [ ] **Hour 24**: All metrics normal

### User Acceptance Testing

- [ ] Power users notified to test
- [ ] Critical workflows tested by users
- [ ] No user-reported issues
- [ ] Performance acceptable to users

### Comparison with Old Production

- [ ] Response time comparison:
  - Old p50: _____ ms
  - New p50: _____ ms
  - Difference: _____%

- [ ] Error rate comparison:
  - Old error rate: _____%
  - New error rate: _____%

- [ ] Throughput comparison:
  - Old requests/sec: _____
  - New requests/sec: _____

### Cleanup

- [ ] **48 hours post-cutover**: Final approval to destroy old infrastructure
- [ ] Destroy old production infrastructure
- [ ] Verify old resources deleted
- [ ] Archive backups for compliance (retain for _____ days)

### Production Sign-Off

**Deployment Lead**: _________________
**Date**: _________________
**Time**: _________________

**Infrastructure Lead**: _________________
**Date**: _________________

**Application Lead**: _________________
**Date**: _________________

**Product Owner**: _________________
**Date**: _________________

---

## Post-Migration Tasks

### Documentation

- [ ] Update architecture diagrams
- [ ] Update runbooks
- [ ] Update incident response procedures
- [ ] Update monitoring dashboard links
- [ ] Archive old documentation

### Terraform Cleanup

- [ ] Remove old terraform states
- [ ] Archive old state files
- [ ] Update terraform backend configurations
- [ ] Clean up old workspaces

### Access Control

- [ ] Update IAM policies for new resource groups
- [ ] Remove access to old resource groups
- [ ] Update service principal permissions
- [ ] Rotate credentials if needed

### Monitoring & Alerts

- [ ] Update alert thresholds if needed
- [ ] Verify all alerts firing correctly
- [ ] Update on-call runbooks
- [ ] Update monitoring dashboards

### Cost Optimization

- [ ] Review actual costs vs. estimates
- [ ] Identify any unexpected costs
- [ ] Optimize resource sizing if needed
- [ ] Document cost savings

### Post-Mortem

- [ ] Schedule post-mortem meeting
- [ ] Document lessons learned
- [ ] Identify process improvements
- [ ] Update migration playbook

### Final Checklist

- [ ] All environments migrated successfully
- [ ] All tests passed
- [ ] No outstanding issues
- [ ] Documentation complete
- [ ] Stakeholders notified of completion
- [ ] Celebration! 🎉

---

## Rollback Criteria

Initiate rollback if ANY of these conditions occur:

- [ ] HTTP 5xx error rate > 5% for more than 5 minutes
- [ ] Application pods crash looping
- [ ] Database connectivity failures
- [ ] Data loss or corruption detected
- [ ] Critical functionality broken
- [ ] Performance degradation > 50%
- [ ] Security incident related to migration

See [ROLLBACK_PROCEDURE.md](./ROLLBACK_PROCEDURE.md) for detailed rollback steps.

---

## Notes Section

Use this section to document any deviations, issues, or observations during testing:

```
Date: ___________
Environment: ___________
Issue/Observation:




Resolution:




Signed: ___________
```
