# =============================================================================
# Shared Infrastructure - Product Configuration
# =============================================================================
# This layer contains all shared resources that multiple products depend on:
#   - PostgreSQL (shared database server)
#   - Redis (shared cache)
#   - Cosmos DB (shared NoSQL database)
#   - Storage (shared blob storage)
#   - Azure AI Foundry (AI/ML services)
#   - Service Bus (messaging)
#   - Container Registry (shared ACR)
#   - DNS Zone (mystira.app)
#   - Front Door (CDN/WAF)
#   - Monitoring (Log Analytics, App Insights)
#
# IMPORTANT: This layer must be deployed BEFORE any product layers.
# =============================================================================

# Include the root terragrunt.hcl
include "root" {
  path = find_in_parent_folders()
}

# No dependencies - this is the foundation layer
