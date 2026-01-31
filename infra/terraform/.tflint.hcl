# =============================================================================
# TFLint Configuration
# =============================================================================
# https://github.com/terraform-linters/tflint

config {
  # Enable module inspection
  call_module_type = "local"

  # Force rules even if disabled by default
  force = false

  # Disable rules that require credentials
  disabled_by_default = false
}

# =============================================================================
# Plugins
# =============================================================================

plugin "terraform" {
  enabled = true
  preset  = "recommended"
}

plugin "azurerm" {
  enabled = true
  version = "0.27.0"
  source  = "github.com/terraform-linters/tflint-ruleset-azurerm"
}

# =============================================================================
# Rules
# =============================================================================

# Naming conventions
rule "terraform_naming_convention" {
  enabled = true
  format  = "snake_case"
}

# Require descriptions for variables
rule "terraform_documented_variables" {
  enabled = true
}

# Require descriptions for outputs
rule "terraform_documented_outputs" {
  enabled = true
}

# Standard module structure
rule "terraform_standard_module_structure" {
  enabled = true
}

# Unused declarations
rule "terraform_unused_declarations" {
  enabled = true
}

# Required version constraint
rule "terraform_required_version" {
  enabled = true
}

# Required providers
rule "terraform_required_providers" {
  enabled = true
}

# Workspace remote (deprecated)
rule "terraform_workspace_remote" {
  enabled = true
}

# Deprecated index usage
rule "terraform_deprecated_index" {
  enabled = true
}

# Deprecated interpolation
rule "terraform_deprecated_interpolation" {
  enabled = true
}

# Comment syntax
rule "terraform_comment_syntax" {
  enabled = true
}

# Empty list equality
rule "terraform_empty_list_equality" {
  enabled = true
}
