# Shared Identity Module Outputs

output "aks_acr_role_assignment_id" {
  description = "Role assignment ID for AKS to ACR access"
  value       = length(azurerm_role_assignment.aks_acr_pull) > 0 ? azurerm_role_assignment.aks_acr_pull[0].id : null
}

output "service_key_vault_role_assignments" {
  description = "Map of service to Key Vault role assignment IDs"
  value       = { for k, v in azurerm_role_assignment.service_key_vault_reader : k => v.id }
}

output "service_postgres_role_assignments" {
  description = "Map of service to PostgreSQL role assignment IDs"
  value       = { for k, v in azurerm_role_assignment.service_postgres_reader : k => v.id }
}

output "service_redis_role_assignments" {
  description = "Map of service to Redis role assignment IDs"
  value       = { for k, v in azurerm_role_assignment.service_redis_contributor : k => v.id }
}

output "workload_identity_federation_ids" {
  description = "Map of workload identity federated credential IDs"
  value       = { for k, v in azurerm_federated_identity_credential.workload_identity : k => v.id }
}
