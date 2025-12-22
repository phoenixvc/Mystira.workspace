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

variable "tags" {
  description = "Tags to apply to all resources"
  type        = map(string)
  default     = {}
}
