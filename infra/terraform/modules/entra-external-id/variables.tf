# Microsoft Entra External ID Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "tenant_name" {
  description = "External tenant subdomain (e.g., 'mystira' for mystira.ciamlogin.com)"
  type        = string
  default     = "mystira"
}

variable "tenant_id" {
  description = "Microsoft Entra External ID tenant ID (GUID) - must be created manually first"
  type        = string
}

variable "pwa_redirect_uris" {
  description = "Redirect URIs for PWA/SPA application"
  type        = list(string)
  default     = []
}

variable "mobile_redirect_uris" {
  description = "Redirect URIs for mobile application (e.g., mystira://auth/callback)"
  type        = list(string)
  default     = []
}
