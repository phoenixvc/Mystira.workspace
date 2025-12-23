# Entra ID Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "admin_ui_redirect_uris" {
  description = "Redirect URIs for Admin UI application"
  type        = list(string)
  default     = []
}

variable "enable_external_id" {
  description = "Enable Entra External ID configuration (consumer authentication)"
  type        = bool
  default     = false
}

variable "external_id_tenant_domain" {
  description = "Entra External ID tenant domain (e.g., mystira.ciamlogin.com)"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
