# Shared Identity Module Variables

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "aks_principal_id" {
  description = "Principal ID of the AKS cluster managed identity"
  type        = string
  default     = ""
}

variable "cicd_principal_id" {
  description = "Principal ID of the CI/CD service principal (for ACR push access)"
  type        = string
  default     = ""
}

variable "acr_id" {
  description = "Resource ID of the container registry"
  type        = string
  default     = ""
}

variable "storage_role" {
  description = "Role to assign for storage account access"
  type        = string
  default     = "Storage Blob Data Contributor"
}

variable "service_identities" {
  description = "Map of service identities and their resource access requirements"
  type = map(object({
    principal_id               = string
    key_vault_id               = optional(string, "")
    postgres_server_id         = optional(string, "")
    postgres_role              = optional(string, "reader")
    redis_cache_id             = optional(string, "")
    log_analytics_workspace_id = optional(string, "")
    storage_account_id         = optional(string, "")
  }))
  default = {}
}

variable "workload_identities" {
  description = "Map of workload identities for AKS federation"
  type = map(object({
    identity_id         = string
    aks_oidc_issuer_url = string
    namespace           = string
    service_account     = string
  }))
  default = {}
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
