# Azure Front Door Module

Terraform module for deploying Azure Front Door with WAF protection for Mystira services.

## Features

- **Global CDN** - Edge caching at 100+ locations worldwide
- **Web Application Firewall (WAF)** - OWASP 3.2 protection with custom rules
- **DDoS Protection** - Built-in Azure DDoS protection
- **SSL/TLS** - Managed certificates with automatic renewal
- **Health Probes** - Automatic backend health monitoring
- **Rate Limiting** - Configurable rate limiting rules
- **Bot Protection** - Microsoft Bot Manager ruleset

## Architecture

```
User
  ↓
Azure Front Door (Global Edge)
  ├─ WAF Rules (OWASP, Bot Protection, Custom)
  ├─ SSL Termination (Managed Certificates)
  └─ Edge Caching (Static Content)
      ↓
Backend Services (Publisher, Chain, Admin API/UI, Story Generator, Mystira.App)
  ↓
Kubernetes Pods (Publisher, Chain, Admin API)
App Service (Mystira.App API)
Static Web Apps (Admin UI, Story Generator SWA, Mystira.App SWA)
```

## Supported Services

Front Door can be configured to route traffic to:

- **Publisher** - Content publishing service (Kubernetes)
- **Chain** - Blockchain integration service (Kubernetes)
- **Admin API** - Admin backend API (Kubernetes)
- **Admin UI** - Admin dashboard frontend (Static Web App)
- **Story Generator API** - Story generation backend (Kubernetes)
- **Story Generator SWA** - Story generation frontend (Static Web App)
- **Mystira.App API** - Main application API (Azure App Service) - **Supports WebSockets/SignalR**
- **Mystira.App SWA** - Main application PWA (Static Web App)

## Usage

### Basic Example

```terraform
module "front_door" {
  source = "../../modules/front-door"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.main.name
  location            = "eastus"
  project_name        = "mystira"

  publisher_backend_address = "dev.publisher.mystira.app"
  chain_backend_address     = "dev.chain.mystira.app"
  custom_domain_publisher   = "dev.publisher.mystira.app"
  custom_domain_chain       = "dev.chain.mystira.app"

  enable_waf              = true
  waf_mode                = "Prevention"
  enable_caching          = true
  rate_limit_threshold    = 100

  tags = {
    Project     = "Mystira"
    Environment = "dev"
  }
}
```

### Production Example

```terraform
module "front_door" {
  source = "../../modules/front-door"

  environment         = "prod"
  resource_group_name = azurerm_resource_group.main.name
  location            = "eastus"
  project_name        = "mystira"

  publisher_backend_address = "publisher.mystira.app"
  chain_backend_address     = "chain.mystira.app"
  custom_domain_publisher   = "publisher.mystira.app"
  custom_domain_chain       = "chain.mystira.app"

  enable_waf              = true
  waf_mode                = "Prevention"
  enable_caching          = true
  cache_duration_seconds  = 7200  # 2 hours
  rate_limit_threshold    = 500   # Higher for production
  health_probe_interval   = 30
  session_affinity_enabled = false

  tags = {
    Project     = "Mystira"
    Environment = "prod"
    CostCenter  = "Infrastructure"
  }
}
```

### Full Example with All Services

```terraform
module "front_door" {
  source = "../../modules/front-door"

  environment         = "prod"
  resource_group_name = azurerm_resource_group.main.name
  location            = "eastus"
  project_name        = "mystira"

  # Core services (always required)
  publisher_backend_address = "publisher.mystira.app"
  chain_backend_address     = "chain.mystira.app"
  custom_domain_publisher   = "publisher.mystira.app"
  custom_domain_chain       = "chain.mystira.app"

  # Enable Admin services
  enable_admin_services       = true
  admin_api_backend_address   = "admin-api.mystira.app"
  admin_ui_backend_address    = "admin.mystira.app"
  custom_domain_admin_api     = "admin-api.mystira.app"
  custom_domain_admin_ui      = "admin.mystira.app"

  # Enable Story Generator services
  enable_story_generator                = true
  story_generator_api_backend_address   = "story-api.mystira.app"
  story_generator_swa_backend_address   = "story.mystira.app"
  custom_domain_story_generator_api     = "story-api.mystira.app"
  custom_domain_story_generator_swa     = "story.mystira.app"

  # Enable Mystira.App services (API + PWA)
  enable_mystira_app              = true
  mystira_app_api_backend_address = "mys-prod-app-api-san.azurewebsites.net"  # App Service hostname
  mystira_app_swa_backend_address = "mys-prod-app-swa-eus2.azurestaticapps.net"  # Static Web App hostname
  custom_domain_mystira_app_api   = "api.mystira.app"
  custom_domain_mystira_app_swa   = "app.mystira.app"

  enable_waf              = true
  waf_mode                = "Prevention"
  enable_caching          = true
  cache_duration_seconds  = 7200  # 2 hours
  rate_limit_threshold    = 500   # Higher for production
  health_probe_interval   = 30
  session_affinity_enabled = false

  tags = {
    Project     = "Mystira"
    Environment = "prod"
    CostCenter  = "Infrastructure"
  }
}
```

## Important Notes

### WebSocket Support for Mystira.App API

Azure Front Door **fully supports WebSockets** for SignalR and other real-time connections. The Mystira.App API endpoint configuration:

- Uses `forwarding_protocol = "HttpsOnly"` which maintains WebSocket upgrades
- Has minimal caching (`query_string_caching_behavior = "UseQueryString"`) to avoid interfering with persistent connections
- The backend Azure App Service has `websockets_enabled = true` configured

No additional configuration is needed for WebSocket/SignalR to work through Front Door.

## Inputs

| Name                      | Description                                 | Type          | Default        | Required |
| ------------------------- | ------------------------------------------- | ------------- | -------------- | -------- |
| environment               | Deployment environment (dev, staging, prod) | `string`      | n/a            | yes      |
| resource_group_name       | Name of the resource group                  | `string`      | n/a            | yes      |
| location                  | Azure region for resources                  | `string`      | `"eastus"`     | no       |
| project_name              | Project name                                | `string`      | `"mystira"`    | no       |
| publisher_backend_address | Backend address for Publisher service       | `string`      | n/a            | yes      |
| chain_backend_address     | Backend address for Chain service           | `string`      | n/a            | yes      |
| custom_domain_publisher   | Custom domain for Publisher                 | `string`      | n/a            | yes      |
| custom_domain_chain       | Custom domain for Chain                     | `string`      | n/a            | yes      |
| enable_waf                | Enable Web Application Firewall             | `bool`        | `true`         | no       |
| waf_mode                  | WAF mode: Detection or Prevention           | `string`      | `"Prevention"` | no       |
| enable_caching            | Enable caching for static content           | `bool`        | `true`         | no       |
| cache_duration_seconds    | Cache duration in seconds                   | `number`      | `3600`         | no       |
| rate_limit_threshold      | Requests per minute before rate limiting    | `number`      | `100`          | no       |
| health_probe_path         | Health probe path                           | `string`      | `"/health"`    | no       |
| health_probe_interval     | Health probe interval in seconds            | `number`      | `30`           | no       |
| session_affinity_enabled  | Enable session affinity                     | `bool`        | `false`        | no       |
| tags                      | Tags to apply to all resources              | `map(string)` | `{}`           | no       |

## Outputs

| Name                        | Description                                 |
| --------------------------- | ------------------------------------------- |
| front_door_id               | ID of the Front Door profile                |
| front_door_name             | Name of the Front Door profile              |
| publisher_endpoint_hostname | Hostname of the Publisher endpoint          |
| chain_endpoint_hostname     | Hostname of the Chain endpoint              |
| waf_policy_id               | ID of the WAF policy                        |
| dns_cname_targets           | CNAME targets for DNS configuration         |
| custom_domain_verification  | Instructions for custom domain verification |

## WAF Rules

The module includes the following WAF rules:

### Managed Rules

1. **Microsoft Default Rule Set 2.1**
   - OWASP Top 10 protection
   - SQL injection prevention
   - Cross-site scripting (XSS) protection
   - Remote code execution prevention

2. **Microsoft Bot Manager Rule Set 1.0**
   - Bad bot protection
   - Known bot detection
   - Bot reputation scoring

### Custom Rules

1. **Rate Limiting**
   - Blocks IPs exceeding threshold (default: 100 req/min)
   - 1-minute sliding window
   - Configurable threshold

2. **Bad Bot Blocking**
   - Blocks known scanner user agents
   - Blocks: sqlmap, nikto, scanner, masscan
   - Case-insensitive matching

3. **HTTP Method Restriction**
   - Allows only standard HTTP methods
   - Blocks unusual methods (TRACE, CONNECT, etc.)

## DNS Configuration

After deploying Front Door, you need to update your DNS:

1. **Get CNAME targets from Terraform outputs:**
   ```bash
   terraform output dns_cname_targets
   ```

2. **Update DNS records:**
   ```bash
   # For dev environment
   # Add CNAME: dev.publisher -> <endpoint>.azurefd.net
   # Add CNAME: dev.chain -> <endpoint>.azurefd.net
   ```

3. **Verify custom domains:**
   - Front Door will validate domain ownership
   - SSL certificates will be automatically provisioned
   - Wait 5-10 minutes for validation

## Custom Domain Verification

The module outputs validation tokens for custom domains. Use these to verify domain ownership:

```bash
terraform output publisher_custom_domain_validation_token
terraform output chain_custom_domain_validation_token
```

Add these as TXT records in your DNS:

```
_dnsauth.dev.publisher.mystira.app -> <validation_token>
_dnsauth.dev.chain.mystira.app -> <validation_token>
```

## Caching Configuration

### Publisher Service
- **Caching:** Enabled (for static assets)
- **Compression:** Enabled
- **Cache Duration:** Configurable (default: 1 hour)
- **Content Types:** JS, CSS, HTML, JSON, XML

### Chain Service
- **Caching:** Disabled (blockchain RPC needs real-time data)
- **Compression:** Disabled
- **Forwarding:** All requests forwarded to backend

## Health Probes

Health probes monitor backend service health:

- **Path:** `/health` (configurable)
- **Protocol:** HTTPS
- **Method:** HEAD
- **Interval:** 30 seconds (configurable)
- **Threshold:** 3/4 successful samples required

## Monitoring

Monitor Front Door metrics in Azure Portal:

- **Request Count** - Total requests
- **Request Size** - Size of requests
- **Response Size** - Size of responses
- **Total Latency** - End-to-end latency
- **Backend Health** - Backend availability
- **WAF Blocked Requests** - Security events
- **Cache Hit Ratio** - Caching efficiency

## Cost Estimate

### Standard Tier Costs

| Component           | Monthly Cost    |
| ------------------- | --------------- |
| Base fee            | $35             |
| Outbound data (1TB) | ~$85            |
| Inbound data        | Free            |
| WAF policy          | $20             |
| Rules (7 custom)    | $2              |
| **Total**           | **~$142/month** |

*Costs scale with traffic volume*

## Limitations

1. **Certificate Provisioning:** Takes 5-10 minutes for initial setup
2. **DNS Propagation:** May take up to 48 hours globally
3. **Rate Limits:** Azure Front Door has internal rate limits
4. **Caching:** Cannot cache responses > 1MB

## Troubleshooting

### Custom Domain Validation Fails

```bash
# Check DNS records
nslookup _dnsauth.dev.publisher.mystira.app

# Verify CNAME is correct
nslookup dev.publisher.mystira.app
```

### WAF Blocking Legitimate Traffic

```bash
# Switch to Detection mode temporarily
waf_mode = "Detection"

# Review WAF logs in Azure Portal
# Adjust custom rules as needed
```

### Health Probe Failures

```bash
# Verify backend is responding
curl -I https://dev.publisher.mystira.app/health

# Check Front Door logs
az monitor activity-log list --resource-id <front-door-id>
```

## Security Best Practices

1. **Use Prevention Mode** - Set `waf_mode = "Prevention"` in production
2. **Monitor WAF Logs** - Review blocked requests regularly
3. **Adjust Rate Limits** - Tune based on legitimate traffic patterns
4. **Enable HTTPS Only** - Module enforces HTTPS by default
5. **Regular Updates** - Keep managed rules up to date
6. **Custom Rules** - Add environment-specific rules as needed

## References

- [Azure Front Door Documentation](https://docs.microsoft.com/en-us/azure/frontdoor/)
- [WAF on Front Door](https://docs.microsoft.com/en-us/azure/web-application-firewall/afds/afds-overview)
- [Terraform azurerm_cdn_frontdoor_profile](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/resources/cdn_frontdoor_profile)
