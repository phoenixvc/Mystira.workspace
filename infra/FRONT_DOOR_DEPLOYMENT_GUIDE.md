# Azure Front Door Deployment Guide

Step-by-step guide to deploy Azure Front Door for Mystira services.

## Prerequisites

Before starting, ensure:
- [ ] Backend services (Publisher, Chain) are deployed and healthy
- [ ] Current NGINX Ingress is working correctly
- [ ] DNS is managed via Azure DNS (Terraform)
- [ ] Budget is approved (~$150-300/month depending on environment)
- [ ] You have tested in dev environment first (if deploying to prod)

## Architecture Overview

### Before Front Door
```
User → DNS → Load Balancer → NGINX Ingress → Services
```

### After Front Door
```
User → DNS → Front Door (WAF + CDN) → Load Balancer → NGINX Ingress → Services
```

## Phase 1: Deploy to Dev Environment

### Step 1: Enable Front Door Module

```bash
cd infra/terraform/environments/dev

# Rename the example file to enable it
mv front-door-example.tf.disabled front-door.tf
```

### Step 2: Review Configuration

Edit `front-door.tf` and verify settings:
- Backend addresses point to current ingress
- WAF mode is "Detection" for dev (won't block traffic)
- Rate limits are appropriate

### Step 3: Plan Deployment

```bash
terraform init
terraform plan -out=front-door.plan

# Review the plan carefully:
# - New Front Door profile
# - 2 endpoints (Publisher, Chain)
# - 2 origin groups and origins
# - 2 custom domains
# - 1 WAF policy
# - 2 security policies
# - 2 routes
```

### Step 4: Deploy Front Door

```bash
terraform apply front-door.plan

# This will take 10-15 minutes
# Wait for "Apply complete!" message
```

### Step 5: Get Front Door Endpoints

```bash
terraform output front_door_publisher_endpoint
terraform output front_door_chain_endpoint
terraform output front_door_custom_domain_verification
```

Output will look like:
```
front_door_publisher_endpoint = "mystira-dev-publisher.z01.azurefd.net"
front_door_chain_endpoint = "mystira-dev-chain.z01.azurefd.net"
```

### Step 6: Update DNS Configuration

You have two options:

#### Option A: Update DNS Module (Recommended)

Update `infra/terraform/modules/dns/main.tf` to support CNAME records for Front Door:

```terraform
# Add variable for Front Door mode
variable "use_front_door" {
  description = "Use Front Door (CNAME) instead of direct Load Balancer (A record)"
  type        = bool
  default     = false
}

variable "front_door_publisher_endpoint" {
  description = "Front Door publisher endpoint (for CNAME)"
  type        = string
  default     = ""
}

variable "front_door_chain_endpoint" {
  description = "Front Door chain endpoint (for CNAME)"
  type        = string
  default     = ""
}

# Update A records to be conditional
resource "azurerm_dns_a_record" "publisher" {
  count               = !var.use_front_door && var.publisher_ip != "" ? 1 : 0
  # ... existing config
}

# Add CNAME records for Front Door
resource "azurerm_dns_cname_record" "publisher_fd" {
  count               = var.use_front_door ? 1 : 0
  name                = local.publisher_subdomain
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = var.front_door_publisher_endpoint

  tags = local.common_tags
}
```

Then update dev environment:

```bash
cd infra/terraform/environments/dev

# Edit main.tf to enable Front Door in DNS module
module "dns" {
  source = "../../modules/dns"
  # ... existing config
  
  use_front_door                = true
  front_door_publisher_endpoint = module.front_door.publisher_endpoint_hostname
  front_door_chain_endpoint     = module.front_door.chain_endpoint_hostname
}

terraform plan
terraform apply
```

#### Option B: Manual DNS Update via Azure Portal

1. Go to Azure Portal → DNS Zones → mystira.app
2. Delete A records for `dev.publisher` and `dev.chain`
3. Add CNAME records:
   - Name: `dev.publisher`, Value: `<front_door_publisher_endpoint>`
   - Name: `dev.chain`, Value: `<front_door_chain_endpoint>`

### Step 7: Add Domain Validation Records

Front Door needs to verify domain ownership:

```bash
# Get validation tokens
terraform output -raw publisher_custom_domain_validation_token
terraform output -raw chain_custom_domain_validation_token
```

Add TXT records to DNS:
```
_dnsauth.dev.publisher.mystira.app -> <validation_token_1>
_dnsauth.dev.chain.mystira.app -> <validation_token_2>
```

**Via Terraform (recommended):**

Update `infra/terraform/modules/dns/main.tf`:

```terraform
variable "front_door_publisher_validation_token" {
  description = "Front Door publisher domain validation token"
  type        = string
  default     = ""
}

resource "azurerm_dns_txt_record" "publisher_validation" {
  count               = var.front_door_publisher_validation_token != "" ? 1 : 0
  name                = "_dnsauth.${local.publisher_subdomain}"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300

  record {
    value = var.front_door_publisher_validation_token
  }

  tags = local.common_tags
}
```

### Step 8: Wait for Certificate Provisioning

```bash
# Check certificate status
az afd custom-domain list \
  --profile-name mystira-dev-fd \
  --resource-group <rg-name> \
  --query "[].{name:name, validationState:validationState, certificateState:domainValidationState}"
```

Wait for:
- `validationState: "Approved"`
- `certificateState: "Succeeded"`

This takes **5-15 minutes**.

### Step 9: Test Front Door Endpoints

```bash
# Test direct Front Door endpoint
curl https://mystira-dev-publisher.z01.azurefd.net/health

# Test custom domain (after DNS propagates)
curl https://dev.publisher.mystira.app/health

# Verify WAF is working
curl -A "sqlmap" https://dev.publisher.mystira.app/health
# Should be blocked by WAF (403 Forbidden)
```

### Step 10: Verify Traffic Flow

```bash
# Check Front Door metrics in Azure Portal
# Monitor > Metrics > Select Front Door resource

# Key metrics to watch:
# - Request Count (should be > 0)
# - Backend Health (should be 100%)
# - Total Latency (should be < 500ms)
# - WAF Blocked Requests (monitor security events)
# - Cache Hit Ratio (if caching enabled)
```

### Step 11: Monitor for Issues

Watch for 24-48 hours:

1. **Check Backend Health:**
   ```bash
   az afd origin show \
     --profile-name mystira-dev-fd \
     --origin-group-name publisher-origin-group \
     --origin-name publisher-origin \
     --resource-group <rg-name> \
     --query enabledState
   ```

2. **Check WAF Logs:**
   ```bash
   az monitor activity-log list \
     --resource-group <rg-name> \
     --query "[?contains(operationName.value, 'Microsoft.Cdn/profiles')]"
   ```

3. **Test from Different Locations:**
   - Use https://www.whatsmydns.net/ to check DNS propagation
   - Test from different geographic locations
   - Verify caching is working (check `x-cache` header)

## Phase 2: Deploy to Staging

Repeat Phase 1 steps for staging environment:

```bash
cd infra/terraform/environments/staging
mv front-door-example.tf.disabled front-door.tf
# Update configuration
terraform init && terraform plan && terraform apply
```

## Phase 3: Deploy to Production

**IMPORTANT:** Only proceed after successful testing in dev and staging!

### Step 1: Schedule Maintenance Window

- Notify users of potential brief disruption
- Schedule during low-traffic period
- Have rollback plan ready

### Step 2: Enable Front Door in Production

```bash
cd infra/terraform/environments/prod
mv front-door-example.tf.disabled front-door.tf

# Review configuration carefully
# Ensure waf_mode = "Prevention" for production
# Adjust rate_limit_threshold based on prod traffic patterns

terraform init
terraform plan -out=prod-front-door.plan

# Review plan thoroughly!
terraform apply prod-front-door.plan
```

### Step 3: Update DNS with Gradual Migration

**Option 1: Canary Deployment (Recommended)**

1. Keep A records pointing to current load balancer
2. Add CNAME for a test subdomain: `test.publisher.mystira.app`
3. Test thoroughly with test subdomain
4. Switch main domain to CNAME after validation

**Option 2: Direct Cutover**

1. Change A records to CNAMEs pointing to Front Door
2. Monitor closely for first hour
3. Be ready to rollback if issues arise

### Step 4: Monitor Production Traffic

```bash
# Watch real-time metrics
az monitor metrics list \
  --resource /subscriptions/<sub-id>/resourceGroups/<rg>/providers/Microsoft.Cdn/profiles/mystira-prod-fd \
  --metric "RequestCount" \
  --interval PT1M \
  --start-time $(date -u -d '5 minutes ago' +%Y-%m-%dT%H:%M:%SZ) \
  --end-time $(date -u +%Y-%m-%dT%H:%M:%SZ)
```

### Step 5: Validate Production

- [ ] All services accessible
- [ ] SSL certificates valid
- [ ] WAF blocking malicious traffic (check logs)
- [ ] Caching working (check cache hit ratio)
- [ ] Latency acceptable (< 500ms P95)
- [ ] No increase in errors

## Rollback Procedure

If issues arise, rollback immediately:

### Quick Rollback (DNS Change)

```bash
# Switch DNS back to A records pointing to load balancer
cd infra/terraform/environments/<env>

# Edit main.tf
module "dns" {
  source = "../../modules/dns"
  # ... config
  use_front_door = false  # Disable Front Door
  publisher_ip   = "<LB_IP>"
  chain_ip       = "<LB_IP>"
}

terraform apply -auto-approve

# Wait 5-10 minutes for DNS propagation
```

### Full Rollback (Remove Front Door)

```bash
# Disable Front Door module
mv front-door.tf front-door.tf.disabled

terraform plan
terraform apply

# This will destroy Front Door resources
# Traffic will flow directly to load balancer
```

## Troubleshooting

### Issue: Custom Domain Validation Fails

**Symptoms:** Domain shows "Pending" validation status

**Solution:**
```bash
# 1. Verify TXT record exists
nslookup -type=TXT _dnsauth.dev.publisher.mystira.app

# 2. Check validation token matches
terraform output -raw publisher_custom_domain_validation_token

# 3. Wait 10-15 minutes for DNS propagation
# 4. Front Door will auto-retry validation
```

### Issue: 502 Bad Gateway Errors

**Symptoms:** Front Door returns 502 errors

**Solution:**
```bash
# 1. Check backend health
az afd origin show \
  --profile-name mystira-<env>-fd \
  --origin-group-name publisher-origin-group \
  --origin-name publisher-origin \
  --resource-group <rg> \
  --query "{enabled:enabledState, health:privateLinkApprovalMessage}"

# 2. Verify backend is accessible
curl -I https://dev.publisher.mystira.app/health

# 3. Check origin address is correct
# Should match NGINX ingress hostname

# 4. Verify health probe path exists and returns 200
```

### Issue: WAF Blocking Legitimate Traffic

**Symptoms:** Users report 403 errors, legitimate requests blocked

**Solution:**
```bash
# 1. Switch WAF to Detection mode temporarily
# Edit front-door.tf:
waf_mode = "Detection"

terraform apply

# 2. Review WAF logs to identify false positives
az monitor activity-log list \
  --resource-group <rg> \
  --query "[?contains(operationName.value, 'firewallPolicy')]"

# 3. Add exclusions or adjust rules
# 4. Switch back to Prevention mode after fixing
```

### Issue: High Latency

**Symptoms:** Requests slower than before Front Door

**Solution:**
```bash
# 1. Check if caching is causing issues
# Disable caching temporarily:
enable_caching = false

# 2. Verify origin health and latency
# Check if backend itself is slow

# 3. Review Front Door metrics
# Total Latency = Origin Latency + Front Door Processing

# 4. Consider enabling compression
# Already enabled by default for static content
```

## Cost Monitoring

Monitor Front Door costs:

```bash
# Get cost analysis
az consumption usage list \
  --start-date $(date -d '30 days ago' +%Y-%m-%d) \
  --end-date $(date +%Y-%m-%d) \
  --query "[?contains(instanceName, 'front-door')]"

# Expected costs per environment:
# Dev: ~$60-100/month
# Staging: ~$100-150/month
# Production: ~$200-300/month (depends on traffic)
```

## Post-Deployment Tasks

After successful deployment:

1. **Update Documentation**
   - [ ] Update architecture diagrams
   - [ ] Document new DNS configuration
   - [ ] Update runbooks

2. **Set Up Monitoring**
   - [ ] Create Azure Monitor alerts
   - [ ] Set up dashboard for Front Door metrics
   - [ ] Configure WAF log analysis

3. **Security Review**
   - [ ] Review WAF logs weekly
   - [ ] Tune rate limiting based on traffic
   - [ ] Add custom rules as needed

4. **Performance Optimization**
   - [ ] Review cache hit ratio
   - [ ] Adjust cache duration
   - [ ] Optimize backend response times

## Success Criteria

Front Door deployment is successful when:

- ✅ All services accessible via Front Door
- ✅ SSL certificates valid and auto-renewing
- ✅ WAF blocking malicious traffic
- ✅ Cache hit ratio > 50% (if caching enabled)
- ✅ Latency P95 < 500ms
- ✅ Backend health 100%
- ✅ No increase in errors
- ✅ Cost within budget

## References

- [Azure Front Door Overview](https://docs.microsoft.com/en-us/azure/frontdoor/)
- [Custom Domain Configuration](https://docs.microsoft.com/en-us/azure/frontdoor/front-door-custom-domain)
- [WAF Configuration](https://docs.microsoft.com/en-us/azure/web-application-firewall/afds/afds-overview)
- [Troubleshooting Guide](https://docs.microsoft.com/en-us/azure/frontdoor/troubleshoot-issues)
