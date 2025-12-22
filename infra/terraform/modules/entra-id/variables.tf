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

variable "enable_b2c" {
  description = "Enable Azure AD B2C configuration (consumer authentication)"
  type        = bool
  default     = false
}

variable "b2c_tenant_domain" {
  description = "Azure AD B2C tenant domain (e.g., mystirab2c.onmicrosoft.com)"
  type        = string
  default     = ""
}

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
