# Azure Front Door Deployment Checklist

Use this checklist to track your Front Door deployment progress.

## Pre-Deployment Checklist

### Requirements Validation
- [ ] Backend services (Publisher, Chain) are deployed and healthy
- [ ] Current NGINX Ingress is working correctly
- [ ] DNS is managed via Azure DNS with Terraform
- [ ] Budget approved: ~$60-100/month (dev), ~$200-300/month (prod)
- [ ] Team has reviewed [FRONT_DOOR_DEPLOYMENT_GUIDE.md](./FRONT_DOOR_DEPLOYMENT_GUIDE.md)
- [ ] Tested in dev/staging before production (for prod deployments)

### Azure Prerequisites
- [ ] Azure subscription has sufficient quota
- [ ] Resource group exists
- [ ] Permissions: Contributor role on subscription
- [ ] Azure CLI installed and configured
- [ ] Terraform >= 1.5.0 installed

### Documentation Review
- [ ] Read [AZURE_FRONT_DOOR_PLAN.md](./AZURE_FRONT_DOOR_PLAN.md)
- [ ] Read [FRONT_DOOR_DEPLOYMENT_GUIDE.md](./FRONT_DOOR_DEPLOYMENT_GUIDE.md)
- [ ] Review [terraform/modules/front-door/README.md](./terraform/modules/front-door/README.md)

## Phase 1: Dev Environment Deployment

### Step 1: Module Configuration
- [ ] Navigate to `infra/terraform/environments/dev`
- [ ] Rename `front-door-example.tf.disabled` to `front-door.tf`
- [ ] Review and adjust configuration:
  - [ ] Backend addresses are correct
  - [ ] WAF mode is "Detection" (non-blocking for dev)
  - [ ] Rate limits are appropriate
  - [ ] Caching settings match requirements
- [ ] Run `terraform init`

### Step 2: Terraform Deployment
- [ ] Run `terraform plan -out=front-door.plan`
- [ ] Review plan carefully:
  - [ ] Front Door profile creation
  - [ ] 2 endpoints (Publisher, Chain)
  - [ ] 2 origin groups and origins
  - [ ] 2 custom domains
  - [ ] WAF policy and security policies
  - [ ] 2 routes
- [ ] Run `terraform apply front-door.plan`
- [ ] Wait for deployment (10-15 minutes)
- [ ] Verify no errors in output

### Step 3: Get Outputs
- [ ] Run `terraform output front_door_publisher_endpoint`
- [ ] Run `terraform output front_door_chain_endpoint`
- [ ] Run `terraform output -raw publisher_custom_domain_validation_token`
- [ ] Run `terraform output -raw chain_custom_domain_validation_token`
- [ ] Save these values for DNS configuration

### Step 4: DNS Configuration
- [ ] Update `infra/terraform/modules/dns/main.tf` (if not already done)
- [ ] Update dev environment DNS module:
  - [ ] Set `use_front_door = true`
  - [ ] Set `front_door_publisher_endpoint` from output
  - [ ] Set `front_door_chain_endpoint` from output
  - [ ] Set validation tokens
- [ ] Run `terraform plan` in dev environment
- [ ] Run `terraform apply`
- [ ] Verify CNAME records created

### Step 5: Domain Validation
- [ ] Wait 5-10 minutes for DNS propagation
- [ ] Check validation status:
  ```bash
  az afd custom-domain list \
    --profile-name mystira-dev-fd \
    --resource-group <rg-name>
  ```
- [ ] Verify `validationState: "Approved"`
- [ ] Verify `certificateState: "Succeeded"`
- [ ] Wait additional 5-10 minutes if still pending

### Step 6: Testing
- [ ] Test direct Front Door endpoint:
  ```bash
  curl https://mystira-dev-publisher.z01.azurefd.net/health
  ```
- [ ] Test custom domain:
  ```bash
  curl https://dev.publisher.mystira.app/health
  ```
- [ ] Test WAF blocking:
  ```bash
  curl -A "sqlmap" https://dev.publisher.mystira.app/health
  # Should return 403
  ```
- [ ] Verify in browser (check SSL certificate)
- [ ] Test from different geographic locations

### Step 7: Monitoring Setup
- [ ] Check Azure Portal → Front Door → Metrics
- [ ] Verify metrics appearing:
  - [ ] Request Count > 0
  - [ ] Backend Health = 100%
  - [ ] Total Latency < 500ms
- [ ] Check WAF logs for any blocked requests
- [ ] Set up alerts for:
  - [ ] Backend health < 100%
  - [ ] High error rate (>1%)
  - [ ] Certificate expiration

### Step 8: Documentation
- [ ] Document Front Door endpoints in team wiki
- [ ] Update architecture diagrams
- [ ] Add to runbooks
- [ ] Share with team

## Phase 2: Staging Environment Deployment

### Staging Prerequisites
- [ ] Dev deployment successful and stable for 24+ hours
- [ ] No issues observed in dev
- [ ] Team comfortable with Front Door configuration

### Staging Deployment
- [ ] Navigate to `infra/terraform/environments/staging`
- [ ] Rename `front-door-example.tf.disabled` to `front-door.tf`
- [ ] Adjust configuration for staging (if needed)
- [ ] Run `terraform init && terraform plan && terraform apply`
- [ ] Update DNS configuration
- [ ] Wait for domain validation
- [ ] Test endpoints
- [ ] Monitor for 24-48 hours

## Phase 3: Production Environment Deployment

### Production Prerequisites
- [ ] Dev AND staging deployments successful
- [ ] Both environments stable for 48+ hours
- [ ] Team trained on Front Door operations
- [ ] Rollback procedure tested in staging
- [ ] Maintenance window scheduled
- [ ] Users notified of potential brief disruption
- [ ] On-call engineer available

### Production Deployment
- [ ] Navigate to `infra/terraform/environments/prod`
- [ ] Rename `front-door-example.tf.disabled` to `front-door.tf`
- [ ] Review configuration thoroughly:
  - [ ] WAF mode is "Prevention" (blocking)
  - [ ] Rate limits appropriate for production traffic
  - [ ] Caching settings optimized
  - [ ] All values correct
- [ ] Run `terraform plan` and review carefully
- [ ] Get approval from team lead
- [ ] Run `terraform apply` during maintenance window
- [ ] Update DNS configuration
- [ ] Wait for domain validation (monitor closely)
- [ ] Gradually shift traffic (if using canary deployment)
- [ ] Monitor intensively for first hour

### Production Validation
- [ ] All services accessible
- [ ] SSL certificates valid (check in browser)
- [ ] No increase in error rate
- [ ] Latency acceptable (compare to baseline)
- [ ] WAF blocking malicious traffic (check logs)
- [ ] Cache hit ratio > 50% (if caching enabled)
- [ ] Backend health 100%
- [ ] User feedback positive (no complaints)

### Production Monitoring (First 48 Hours)
- [ ] Check metrics every hour for first 8 hours
- [ ] Review WAF logs for false positives
- [ ] Monitor costs in Azure Cost Management
- [ ] Track latency trends
- [ ] Check cache performance
- [ ] Verify SSL certificate auto-renewal scheduled

## Post-Deployment Tasks

### Documentation Updates
- [ ] Update production architecture diagrams
- [ ] Document new traffic flow
- [ ] Update DNS documentation
- [ ] Update monitoring runbooks
- [ ] Document rollback procedure
- [ ] Add to disaster recovery plan

### Monitoring & Alerts
- [ ] Configure Azure Monitor dashboards
- [ ] Set up alerts for:
  - [ ] Backend health drops
  - [ ] High error rate (>1%)
  - [ ] High latency (P95 > 500ms)
  - [ ] WAF blocking spike (potential attack)
  - [ ] Certificate expiration (30 days before)
  - [ ] Cost exceeds budget
- [ ] Configure log forwarding (if using SIEM)
- [ ] Set up weekly WAF log review

### Security Configuration
- [ ] Review WAF logs weekly
- [ ] Tune rate limiting based on real traffic
- [ ] Add custom WAF rules as needed
- [ ] Document security incidents
- [ ] Schedule quarterly security review

### Performance Optimization
- [ ] Review cache hit ratio weekly
- [ ] Adjust cache duration based on patterns
- [ ] Optimize backend response times
- [ ] Monitor and optimize compression
- [ ] Review and optimize routing rules

### Cost Optimization
- [ ] Review costs weekly
- [ ] Compare to budget
- [ ] Identify optimization opportunities
- [ ] Document cost trends
- [ ] Forecast future costs

## Rollback Checklist

If issues arise, use this rollback procedure:

### Quick DNS Rollback
- [ ] Navigate to environment terraform directory
- [ ] Edit DNS module: set `use_front_door = false`
- [ ] Set `publisher_ip` and `chain_ip` to load balancer IPs
- [ ] Run `terraform apply -auto-approve`
- [ ] Wait 5-10 minutes for DNS propagation
- [ ] Verify services accessible
- [ ] Monitor for 30 minutes

### Full Front Door Removal
- [ ] Rename `front-door.tf` to `front-door.tf.disabled`
- [ ] Run `terraform plan` (should show destruction)
- [ ] Run `terraform apply` to destroy Front Door
- [ ] Verify resources removed in Azure Portal
- [ ] Document reason for rollback
- [ ] Schedule post-mortem meeting

## Troubleshooting Reference

Common issues and solutions:

### Issue: Custom Domain Validation Fails
- [ ] Check TXT record: `nslookup -type=TXT _dnsauth.dev.publisher.mystira.app`
- [ ] Verify validation token matches Terraform output
- [ ] Wait 10-15 minutes for DNS propagation
- [ ] Check Azure Portal for error messages

### Issue: 502 Bad Gateway
- [ ] Check backend health in Azure Portal
- [ ] Verify backend accessible: `curl -I https://dev.publisher.mystira.app/health`
- [ ] Check origin address in Front Door config
- [ ] Verify health probe path returns 200

### Issue: WAF Blocking Legitimate Traffic
- [ ] Switch to Detection mode temporarily
- [ ] Review WAF logs in Azure Portal
- [ ] Identify false positive rules
- [ ] Add exclusions or adjust rules
- [ ] Switch back to Prevention mode

### Issue: High Latency
- [ ] Check backend latency separately
- [ ] Review Front Door metrics (Total Latency breakdown)
- [ ] Consider disabling caching temporarily
- [ ] Check for network issues
- [ ] Review origin health

## Success Criteria

Deployment is successful when ALL criteria met:

- ✅ All services accessible via Front Door
- ✅ SSL certificates valid and auto-renewing
- ✅ WAF blocking malicious traffic (check logs)
- ✅ Cache hit ratio > 50% (if caching enabled)
- ✅ Latency P95 < 500ms
- ✅ Backend health 100%
- ✅ Error rate < 0.1%
- ✅ Cost within budget
- ✅ No user complaints
- ✅ Team comfortable with operations

## Timeline

Estimated timeline for full deployment:

| Phase                 | Duration     | Notes                     |
| --------------------- | ------------ | ------------------------- |
| Dev deployment        | 2-4 hours    | Including testing         |
| Dev monitoring        | 24-48 hours  | Stability check           |
| Staging deployment    | 2-4 hours    | Similar to dev            |
| Staging monitoring    | 24-48 hours  | Stability check           |
| Production planning   | 1-2 hours    | Review and approval       |
| Production deployment | 2-4 hours    | During maintenance window |
| Production monitoring | 48 hours     | Intensive monitoring      |
| **Total**             | **5-8 days** | End to end                |

## Support & Escalation

If you encounter issues:

1. **Check documentation:**
   - [FRONT_DOOR_DEPLOYMENT_GUIDE.md](./FRONT_DOOR_DEPLOYMENT_GUIDE.md)
   - [terraform/modules/front-door/README.md](./terraform/modules/front-door/README.md)

2. **Review Azure documentation:**
   - [Azure Front Door Docs](https://docs.microsoft.com/en-us/azure/frontdoor/)
   - [Troubleshooting Guide](https://docs.microsoft.com/en-us/azure/frontdoor/troubleshoot-issues)

3. **Escalation path:**
   - DevOps team lead
   - Azure Support (if needed)

## Notes

Use this section for environment-specific notes:

```
Date: ___________
Environment: [ ] Dev  [ ] Staging  [ ] Prod
Deployed by: ___________
Issues encountered: ___________
Lessons learned: ___________
```
