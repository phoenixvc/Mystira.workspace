# Admin UI Infrastructure Module

Terraform module for deploying Mystira Admin UI as an Azure Static Web App.

## Overview

The Admin UI is a React/Vite frontend application that provides administrative functionality for Mystira. It connects to the Admin API for backend operations and uses Microsoft Entra ID for authentication.

## Resources Created

| Resource | Purpose |
|----------|---------|
| Azure Static Web App | Hosts the React/Vite frontend |
| Custom Domain (optional) | Custom domain configuration for the SWA |

## Usage

```hcl
module "admin_ui" {
  source = "../../modules/admin-ui"

  environment         = "dev"
  location            = var.location
  fallback_location   = "eastus2"  # SWA not available in all regions
  region_code         = "san"
  resource_group_name = azurerm_resource_group.admin.name

  static_web_app_sku   = "Free"
  enable_custom_domain = false

  tags = {
    CostCenter = "development"
  }
}
```

## Static Web App Notes

Azure Static Web Apps are not available in all Azure regions. This module uses a `fallback_location` variable to deploy the SWA to a supported region (default: eastus2) while other resources remain in the primary region.

## Custom Domain Setup

To enable a custom domain:

```hcl
module "admin_ui" {
  source = "../../modules/admin-ui"
  # ... other variables ...

  enable_custom_domain = true
  custom_domain        = "dev.admin.mystira.app"
}
```

When using Azure Front Door, the custom domain is typically managed there instead of on the SWA directly.

## GitHub Actions Deployment

The SWA API key is used to deploy from GitHub Actions:

```yaml
- name: Deploy Admin UI
  uses: Azure/static-web-apps-deploy@v1
  with:
    azure_static_web_apps_api_token: ${{ secrets.ADMIN_UI_SWA_TOKEN }}
    repo_token: ${{ secrets.GITHUB_TOKEN }}
    action: "upload"
    app_location: "/"
    output_location: "dist"
```

## Outputs

| Output | Description |
|--------|-------------|
| `static_web_app_id` | Static Web App resource ID |
| `static_web_app_name` | Static Web App name |
| `static_web_app_default_hostname` | Default hostname (for Front Door backend) |
| `static_web_app_url` | Full URL to the Static Web App |
| `static_web_app_api_key` | API key for deployments (sensitive) |
| `custom_domain` | Custom domain if enabled |
