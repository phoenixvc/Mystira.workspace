# Front Door DNS Setup Guide

This guide documents the DNS configuration required for Azure Front Door custom domains across all Mystira services and environments.

## Overview

Azure Front Door requires proper DNS configuration before it can provision SSL certificates for custom domains. Without correct DNS setup, you'll see `ERR_CERT_COMMON_NAME_INVALID` errors.

### Requirements for Each Custom Domain

1. **CNAME record** - Points your domain to the Front Door endpoint
2. **TXT record** - Contains the validation token for domain ownership verification (`_dnsauth.<subdomain>`)

## Domain Inventory by Environment

### Development (`dev`)

| Service | Domain | Purpose |
|---------|--------|---------|
| Mystira.App (PWA) | `dev.mystira.app` | Main web application |
| Mystira.App API | `dev.api.mystira.app` | Backend API |
| Admin UI | `dev.admin.mystira.app` | Admin portal |
| Admin API | `dev.admin-api.mystira.app` | Admin backend |
| Story Generator | `dev.story.mystira.app` | Story generator PWA |
| Story API | `dev.story-api.mystira.app` | Story generator backend |
| Publisher | `dev.publisher.mystira.app` | Publisher service (AKS) |
| Chain | `dev.chain.mystira.app` | Chain service (AKS) |

### Staging (`staging`)

| Service | Domain | Purpose |
|---------|--------|---------|
| Mystira.App (PWA) | `staging.app.mystira.app` | Main web application |
| Mystira.App API | `staging.api.mystira.app` | Backend API |
| Admin UI | `staging.admin.mystira.app` | Admin portal |
| Admin API | `staging.admin-api.mystira.app` | Admin backend |
| Story Generator | `staging.story.mystira.app` | Story generator PWA |
| Story API | `staging.story-api.mystira.app` | Story generator backend |
| Publisher | `staging.publisher.mystira.app` | Publisher service (AKS) |
| Chain | `staging.chain.mystira.app` | Chain service (AKS) |

### Production (`prod`)

| Service | Domain | Purpose |
|---------|--------|---------|
| Mystira.App (PWA) | `app.mystira.app` | Main web application |
| Mystira.App API | `api.mystira.app` | Backend API |
| Admin UI | `admin.mystira.app` | Admin portal |
| Admin API | `admin-api.mystira.app` | Admin backend |
| Story Generator | `story.mystira.app` | Story generator PWA |
| Story API | `story-api.mystira.app` | Story generator backend |
| Publisher | `publisher.mystira.app` | Publisher service (AKS) |
| Chain | `chain.mystira.app` | Chain service (AKS) |

## DNS Configuration Steps

### Step 1: Deploy Front Door Infrastructure

```bash
# Deploy infrastructure for your environment
cd infra/terraform/environments/<env>
terraform init
terraform apply
```

### Step 2: Get Front Door Endpoints and Validation Tokens

After deployment, retrieve the endpoints and validation tokens:

```bash
# Get all outputs
terraform output -json

# Or get specific values
terraform output front_door_mystira_app_swa_endpoint
terraform output front_door_custom_domain_verification
```

### Step 3: Create DNS Records

For each custom domain, create two records in your DNS provider:

#### Example: `dev.mystira.app`

```
# CNAME Record
Type:  CNAME
Name:  dev
Value: <front_door_mystira_app_swa_endpoint>  # e.g., mystira-dev-app-swa.azurefd.net

# TXT Record for domain validation
Type:  TXT
Name:  _dnsauth.dev
Value: <validation_token>  # e.g., abc123def456...
```

### Step 4: Wait for Certificate Provisioning

After DNS records are configured:

1. Azure Front Door will detect the DNS changes
2. Domain validation will occur (may take 5-15 minutes)
3. Managed SSL certificate will be provisioned automatically
4. Certificate propagation can take up to 1 hour

### Step 5: Verify Configuration

```bash
# Check DNS propagation
dig CNAME dev.mystira.app
dig TXT _dnsauth.dev.mystira.app

# Test HTTPS access
curl -I https://dev.mystira.app
```

## Complete DNS Records Reference

### Dev Environment DNS Records

```
# Mystira.App
CNAME  dev                    -> mystira-dev-app-swa.azurefd.net
TXT    _dnsauth.dev           -> <validation_token>
CNAME  dev.api                -> mystira-dev-app-api.azurefd.net
TXT    _dnsauth.dev.api       -> <validation_token>

# Admin Services
CNAME  dev.admin              -> mystira-dev-admin-ui.azurefd.net
TXT    _dnsauth.dev.admin     -> <validation_token>
CNAME  dev.admin-api          -> mystira-dev-admin-api.azurefd.net
TXT    _dnsauth.dev.admin-api -> <validation_token>

# Story Generator
CNAME  dev.story              -> mystira-dev-story-swa.azurefd.net
TXT    _dnsauth.dev.story     -> <validation_token>
CNAME  dev.story-api          -> mystira-dev-story-api.azurefd.net
TXT    _dnsauth.dev.story-api -> <validation_token>

# Publisher/Chain
CNAME  dev.publisher          -> mystira-dev-publisher.azurefd.net
TXT    _dnsauth.dev.publisher -> <validation_token>
CNAME  dev.chain              -> mystira-dev-chain.azurefd.net
TXT    _dnsauth.dev.chain     -> <validation_token>
```

### Staging Environment DNS Records

```
# Mystira.App
CNAME  staging.app                -> mystira-staging-app-swa.azurefd.net
TXT    _dnsauth.staging.app       -> <validation_token>
CNAME  staging.api                -> mystira-staging-app-api.azurefd.net
TXT    _dnsauth.staging.api       -> <validation_token>

# Admin Services
CNAME  staging.admin              -> mystira-staging-admin-ui.azurefd.net
TXT    _dnsauth.staging.admin     -> <validation_token>
CNAME  staging.admin-api          -> mystira-staging-admin-api.azurefd.net
TXT    _dnsauth.staging.admin-api -> <validation_token>

# Story Generator
CNAME  staging.story              -> mystira-staging-story-swa.azurefd.net
TXT    _dnsauth.staging.story     -> <validation_token>
CNAME  staging.story-api          -> mystira-staging-story-api.azurefd.net
TXT    _dnsauth.staging.story-api -> <validation_token>

# Publisher/Chain
CNAME  staging.publisher          -> mystira-staging-publisher.azurefd.net
TXT    _dnsauth.staging.publisher -> <validation_token>
CNAME  staging.chain              -> mystira-staging-chain.azurefd.net
TXT    _dnsauth.staging.chain     -> <validation_token>
```

### Production Environment DNS Records

```
# Mystira.App
CNAME  app                -> mystira-prod-app-swa.azurefd.net
TXT    _dnsauth.app       -> <validation_token>
CNAME  api                -> mystira-prod-app-api.azurefd.net
TXT    _dnsauth.api       -> <validation_token>

# Admin Services
CNAME  admin              -> mystira-prod-admin-ui.azurefd.net
TXT    _dnsauth.admin     -> <validation_token>
CNAME  admin-api          -> mystira-prod-admin-api.azurefd.net
TXT    _dnsauth.admin-api -> <validation_token>

# Story Generator
CNAME  story              -> mystira-prod-story-swa.azurefd.net
TXT    _dnsauth.story     -> <validation_token>
CNAME  story-api          -> mystira-prod-story-api.azurefd.net
TXT    _dnsauth.story-api -> <validation_token>

# Publisher/Chain
CNAME  publisher          -> mystira-prod-publisher.azurefd.net
TXT    _dnsauth.publisher -> <validation_token>
CNAME  chain              -> mystira-prod-chain.azurefd.net
TXT    _dnsauth.chain     -> <validation_token>
```

## Automated DNS Configuration (via Terraform)

If you're using the DNS module with Azure DNS:

```hcl
module "dns" {
  source = "../../modules/dns"

  environment         = var.environment
  resource_group_name = azurerm_resource_group.dns.name
  domain_name         = "mystira.app"

  # Enable Front Door integration
  use_front_door = true

  # Front Door endpoints (from front-door module outputs)
  front_door_publisher_endpoint          = module.front_door.publisher_endpoint_hostname
  front_door_chain_endpoint              = module.front_door.chain_endpoint_hostname
  front_door_mystira_app_swa_endpoint    = module.front_door.mystira_app_swa_endpoint_hostname
  front_door_mystira_app_api_endpoint    = module.front_door.mystira_app_api_endpoint_hostname
  front_door_admin_api_endpoint          = module.front_door.admin_api_endpoint_hostname
  front_door_admin_ui_endpoint           = module.front_door.admin_ui_endpoint_hostname
  front_door_story_api_endpoint          = module.front_door.story_generator_api_endpoint_hostname
  front_door_story_swa_endpoint          = module.front_door.story_generator_swa_endpoint_hostname

  # Validation tokens (from front-door module outputs)
  front_door_publisher_validation_token          = module.front_door.publisher_custom_domain_validation_token
  front_door_chain_validation_token              = module.front_door.chain_custom_domain_validation_token
  front_door_mystira_app_swa_validation_token    = module.front_door.mystira_app_swa_custom_domain_validation_token
  front_door_mystira_app_api_validation_token    = module.front_door.mystira_app_api_custom_domain_validation_token
  front_door_admin_api_validation_token          = module.front_door.admin_api_custom_domain_validation_token
  front_door_admin_ui_validation_token           = module.front_door.admin_ui_custom_domain_validation_token
  front_door_story_api_validation_token          = module.front_door.story_generator_api_custom_domain_validation_token
  front_door_story_swa_validation_token          = module.front_door.story_generator_swa_custom_domain_validation_token
}
```

## Troubleshooting

### ERR_CERT_COMMON_NAME_INVALID

**Cause:** DNS is not pointing to Front Door or validation hasn't completed.

**Solution:**
1. Verify CNAME record points to correct Front Door endpoint
2. Verify TXT validation record exists and has correct token
3. Wait for Azure to complete domain validation (5-15 minutes)
4. Check Front Door custom domain status in Azure Portal

### Domain Validation Stuck

**Cause:** TXT record not found or incorrect.

**Solution:**
```bash
# Check if TXT record is resolvable
dig TXT _dnsauth.<subdomain>.mystira.app

# Verify the token matches terraform output
terraform output front_door_custom_domain_verification
```

### Certificate Not Provisioning

**Cause:** Domain validation incomplete or DNS propagation delay.

**Solution:**
1. Wait up to 1 hour for DNS propagation
2. Check Azure Portal → Front Door → Custom Domains for status
3. Ensure no conflicting records (e.g., existing A records)

## Submodule Changes Required

The following submodules may need service worker or PWA manifest updates to handle offline scenarios gracefully:

### packages/app (Mystira.App)

The service worker has been updated to show a friendly offline page when network errors occur (including certificate errors). Changes are in:
- `src/Mystira.App.PWA/wwwroot/service-worker.js`
- `src/Mystira.App.PWA/wwwroot/service-worker.published.js`

### Other PWA Submodules

Similar service worker improvements should be applied to:
- `Mystira.StoryGenerator` - Story Generator PWA
- `Mystira.Admin.UI` - Admin portal (if using service worker)

## Related Documentation

- [ADR-0012: Azure Front Door WAF Strategy](../adr/ADR-0012-front-door-waf-strategy.md)
- [Infrastructure Deployment Guide](./infra-deployment.md)
- [Azure Front Door Documentation](https://docs.microsoft.com/azure/frontdoor/)
