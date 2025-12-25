# GitHub OIDC Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "github_org" {
  description = "GitHub organization name"
  type        = string
  default     = "phoenixvc"
}

variable "repositories" {
  description = "Map of repositories to configure federated credentials for"
  type = map(object({
    name                 = string                       # GitHub repository name
    branches             = list(string)                 # Branches to create credentials for
    environments         = optional(list(string), [])   # GitHub environments
    enable_tags          = optional(bool, false)        # Enable tag-based auth (for releases)
    enable_pull_requests = optional(bool, false)        # Enable PR-based auth
  }))
  default = {}
}

# Role assignment options
variable "enable_subscription_contributor" {
  description = "Grant Contributor role on the subscription (required for most deployments)"
  type        = bool
  default     = true
}

variable "acr_id" {
  description = "Azure Container Registry resource ID (for AcrPush role). Empty string to skip."
  type        = string
  default     = ""
}

variable "aks_id" {
  description = "AKS cluster resource ID (for Cluster Admin role). Empty string to skip."
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
  default     = {}
}
