# Azure AD B2C Module Variables

variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "b2c_tenant_name" {
  description = "Azure AD B2C tenant name (e.g., mystirab2c)"
  type        = string
  default     = "mystirab2c"
}

variable "b2c_tenant_id" {
  description = "Azure AD B2C tenant ID (GUID)"
  type        = string
  default     = ""
}

variable "pwa_redirect_uris" {
  description = "Redirect URIs for PWA/SPA application"
  type        = list(string)
  default     = []
}

variable "sign_up_sign_in_policy" {
  description = "Name of the sign-up/sign-in user flow policy"
  type        = string
  default     = "B2C_1_SignUpSignIn"
}

variable "password_reset_policy" {
  description = "Name of the password reset user flow policy"
  type        = string
  default     = "B2C_1_PasswordReset"
}

variable "profile_edit_policy" {
  description = "Name of the profile edit user flow policy"
  type        = string
  default     = "B2C_1_ProfileEdit"
}
