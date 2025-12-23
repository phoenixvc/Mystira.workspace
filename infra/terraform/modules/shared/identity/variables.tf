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

# Static boolean flags to control role assignments (must be known at plan time)
variable "enable_aks_acr_pull" {
  description = "Enable AKS to ACR pull role assignment. Set to true when AKS and ACR are configured."
  type        = bool
  default     = false
}

variable "enable_cicd_acr_push" {
  description = "Enable CI/CD to ACR push role assignment. Set to true when CI/CD principal and ACR are configured."
  type        = bool
  default     = false
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
    # Static boolean flags to determine which role assignments to create
    # These must be known at plan time (not derived from resource attributes)
    enable_key_vault_access    = optional(bool, false)
    enable_postgres_access     = optional(bool, false)
    enable_redis_access        = optional(bool, false)
    enable_log_analytics       = optional(bool, false)
    enable_storage_access      = optional(bool, false)
    enable_servicebus_sender   = optional(bool, false)
    enable_servicebus_receiver = optional(bool, false)
    # Resource IDs (can be unknown at plan time)
    key_vault_id               = optional(string, "")
    postgres_server_id         = optional(string, "")
    postgres_role              = optional(string, "reader")
    redis_cache_id             = optional(string, "")
    log_analytics_workspace_id = optional(string, "")
    storage_account_id         = optional(string, "")
    servicebus_namespace_id    = optional(string, "")
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
