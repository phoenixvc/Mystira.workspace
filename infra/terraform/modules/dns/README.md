# DNS Infrastructure Module

Terraform module for managing DNS zones and records for all Mystira services.

## Overview

This module manages the `mystira.app` DNS zone and creates appropriate records for all services. It supports both direct Load Balancer (A records) and Azure Front Door (CNAME records) configurations.

## Resources Created

| Resource      | Purpose                                       |
| ------------- | --------------------------------------------- |
| DNS Zone      | Primary zone for mystira.app                  |
| A Records     | Direct IP routing (when not using Front Door) |
| CNAME Records | Front Door routing (when using Front Door)    |
| TXT Records   | Domain validation tokens for Front Door       |

## Managed Subdomains

The module manages DNS records for all Mystira services:

| Service               | Dev                       | Staging                       | Production            |
| --------------------- | ------------------------- | ----------------------------- | --------------------- |
| Mystira.App (SWA)     | dev.mystira.app           | staging.mystira.app           | mystira.app           |
| Mystira.App (API)     | dev.api.mystira.app       | staging.api.mystira.app       | api.mystira.app       |
| Publisher             | dev.publisher.mystira.app | staging.publisher.mystira.app | publisher.mystira.app |
| Chain                 | dev.chain.mystira.app     | staging.chain.mystira.app     | chain.mystira.app     |
| Admin UI              | dev.admin.mystira.app     | staging.admin.mystira.app     | admin.mystira.app     |
| Admin API             | dev.admin-api.mystira.app | staging.admin-api.mystira.app | admin-api.mystira.app |
| Story Generator (SWA) | dev.story.mystira.app     | staging.story.mystira.app     | story.mystira.app     |
| Story Generator (API) | dev.story-api.mystira.app | staging.story-api.mystira.app | story-api.mystira.app |

## Usage

### Direct Load Balancer (A Records)

```hcl
module "dns" {
  source = "../../modules/dns"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.dns.name
  domain_name         = "mystira.app"

  use_front_door = false
  publisher_ip   = module.aks.publisher_ingress_ip
  chain_ip       = module.aks.chain_ingress_ip

  tags = var.common_tags
}
```

### Azure Front Door (CNAME Records)

```hcl
module "dns" {
  source = "../../modules/dns"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.dns.name
  domain_name         = "mystira.app"

  use_front_door = true

  # Core services
  front_door_publisher_endpoint         = module.front_door.publisher_endpoint_hostname
  front_door_chain_endpoint             = module.front_door.chain_endpoint_hostname
  front_door_publisher_validation_token = module.front_door.publisher_validation_token
  front_door_chain_validation_token     = module.front_door.chain_validation_token

  # Mystira.App
  front_door_mystira_app_swa_endpoint         = module.front_door.mystira_app_swa_hostname
  front_door_mystira_app_api_endpoint         = module.front_door.mystira_app_api_hostname
  front_door_mystira_app_swa_validation_token = module.front_door.mystira_app_swa_validation_token
  front_door_mystira_app_api_validation_token = module.front_door.mystira_app_api_validation_token

  # Admin services
  front_door_admin_api_endpoint         = module.front_door.admin_api_hostname
  front_door_admin_ui_endpoint          = module.front_door.admin_ui_hostname
  front_door_admin_api_validation_token = module.front_door.admin_api_validation_token
  front_door_admin_ui_validation_token  = module.front_door.admin_ui_validation_token

  # Story Generator
  front_door_story_api_endpoint         = module.front_door.story_api_hostname
  front_door_story_swa_endpoint         = module.front_door.story_swa_hostname
  front_door_story_api_validation_token = module.front_door.story_api_validation_token
  front_door_story_swa_validation_token = module.front_door.story_swa_validation_token

  tags = var.common_tags
}
```

## Name Server Configuration

After deploying the DNS zone, configure your domain registrar with the Azure name servers:

```hcl
output "name_servers" {
  value = module.dns.name_servers
}
```

## Outputs

| Output                 | Description                         |
| ---------------------- | ----------------------------------- |
| `dns_zone_id`          | DNS Zone resource ID                |
| `dns_zone_name`        | DNS Zone name                       |
| `name_servers`         | Azure name servers for the zone     |
| `publisher_fqdn`       | Full domain for publisher service   |
| `chain_fqdn`           | Full domain for chain service       |
| `mystira_app_swa_fqdn` | Full domain for Mystira.App SWA     |
| `mystira_app_api_fqdn` | Full domain for Mystira.App API     |
| `admin_api_fqdn`       | Full domain for Admin API           |
| `admin_ui_fqdn`        | Full domain for Admin UI            |
| `story_api_fqdn`       | Full domain for Story Generator API |
| `story_swa_fqdn`       | Full domain for Story Generator SWA |
