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

# No dependencies - this is the foundation layer
# This file is included by environment configs via: include "product"
