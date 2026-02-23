//! DevHub-specific contracts
//!
//! These types are specific to the DevHub application and correspond
//! to the existing TypeScript types in `src/types/index.ts`.

use serde::{Deserialize, Serialize};

#[cfg(feature = "typescript")]
use ts_rs::TS;

/// Command request for CLI invocation
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct CommandRequest {
    /// Command name (e.g., "cosmos.stats", "infrastructure.validate")
    pub command: String,

    /// Command arguments
    #[serde(default)]
    pub args: serde_json::Value,
}

impl CommandRequest {
    /// Create a new command request
    pub fn new(command: impl Into<String>) -> Self {
        Self {
            command: command.into(),
            args: serde_json::Value::Null,
        }
    }

    /// Create a command request with arguments
    pub fn with_args(command: impl Into<String>, args: impl Serialize) -> Result<Self, serde_json::Error> {
        Ok(Self {
            command: command.into(),
            args: serde_json::to_value(args)?,
        })
    }
}

/// Command response from CLI
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct CommandResponse<T = serde_json::Value> {
    /// Whether the command succeeded
    pub success: bool,

    /// Result data (on success)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub result: Option<T>,

    /// Error message (on failure)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub error: Option<String>,

    /// Additional message
    #[serde(skip_serializing_if = "Option::is_none")]
    pub message: Option<String>,
}

impl<T> CommandResponse<T> {
    /// Create a successful response
    pub fn success(result: T) -> Self {
        Self {
            success: true,
            result: Some(result),
            error: None,
            message: None,
        }
    }

    /// Create a failure response
    pub fn failure(error: impl Into<String>) -> Self {
        Self {
            success: false,
            result: None,
            error: Some(error.into()),
            message: None,
        }
    }

    /// Add a message to the response
    pub fn with_message(mut self, message: impl Into<String>) -> Self {
        self.message = Some(message.into());
        self
    }
}

/// Azure resource representation
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct AzureResource {
    /// Azure resource ID
    pub id: String,

    /// Resource name
    pub name: String,

    /// Resource type (e.g., "Microsoft.Storage/storageAccounts")
    #[serde(rename = "type")]
    pub resource_type: String,

    /// Azure region
    pub location: String,

    /// Resource group name
    pub resource_group: String,

    /// Resource tags
    #[serde(default)]
    pub tags: std::collections::HashMap<String, String>,

    /// Provisioning state
    #[serde(skip_serializing_if = "Option::is_none")]
    pub provisioning_state: Option<String>,
}

/// What-if change from Azure deployment preview
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct WhatIfChange {
    /// Resource type
    pub resource_type: String,

    /// Resource name
    pub resource_name: String,

    /// Type of change
    pub change_type: ChangeType,

    /// Property changes (for modifications)
    #[serde(default)]
    pub changes: Vec<PropertyChange>,
}

/// Type of resource change
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
#[serde(rename_all = "camelCase")]
pub enum ChangeType {
    Create,
    Modify,
    Delete,
    NoChange,
    Ignore,
    Deploy,
}

/// Property change details
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct PropertyChange {
    /// Property path
    pub path: String,

    /// Previous value
    #[serde(skip_serializing_if = "Option::is_none")]
    pub before: Option<serde_json::Value>,

    /// New value
    #[serde(skip_serializing_if = "Option::is_none")]
    pub after: Option<serde_json::Value>,
}

/// Deployment record
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct Deployment {
    /// Unique deployment ID
    pub id: String,

    /// Deployment timestamp
    pub timestamp: chrono::DateTime<chrono::Utc>,

    /// Action performed
    pub action: DeploymentAction,

    /// Deployment status
    pub status: DeploymentStatus,

    /// Target environment
    #[serde(skip_serializing_if = "Option::is_none")]
    pub environment: Option<String>,

    /// Resources affected
    #[serde(default)]
    pub resources: Vec<String>,

    /// Error message (if failed)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub error: Option<String>,
}

/// Deployment action type
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
#[serde(rename_all = "snake_case")]
pub enum DeploymentAction {
    Deploy,
    Validate,
    Preview,
    Destroy,
}

/// Deployment status
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
#[serde(rename_all = "snake_case")]
pub enum DeploymentStatus {
    Pending,
    InProgress,
    Success,
    Failed,
    Cancelled,
}

/// Connection status for services
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ConnectionStatus {
    /// Cosmos DB connection status
    pub cosmos: ServiceStatus,

    /// Azure Storage connection status
    pub storage: ServiceStatus,

    /// Azure CLI login status
    pub azure_cli: ServiceStatus,

    /// GitHub CLI login status
    pub github_cli: ServiceStatus,
}

/// Individual service status
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ServiceStatus {
    /// Whether the service is connected
    pub connected: bool,

    /// Status message
    #[serde(skip_serializing_if = "Option::is_none")]
    pub message: Option<String>,

    /// Last checked timestamp
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_checked: Option<chrono::DateTime<chrono::Utc>>,

    /// Account/endpoint information
    #[serde(skip_serializing_if = "Option::is_none")]
    pub account: Option<String>,
}

impl ServiceStatus {
    /// Create a connected status
    pub fn connected() -> Self {
        Self {
            connected: true,
            message: None,
            last_checked: Some(chrono::Utc::now()),
            account: None,
        }
    }

    /// Create a disconnected status
    pub fn disconnected(message: impl Into<String>) -> Self {
        Self {
            connected: false,
            message: Some(message.into()),
            last_checked: Some(chrono::Utc::now()),
            account: None,
        }
    }

    /// Add account information
    pub fn with_account(mut self, account: impl Into<String>) -> Self {
        self.account = Some(account.into());
        self
    }
}

/// Project information
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ProjectInfo {
    /// Project identifier
    pub id: String,

    /// Display name
    pub name: String,

    /// Project type
    pub project_type: ProjectType,

    /// Infrastructure requirements
    pub infrastructure: InfrastructureRequirements,
}

/// Project type
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
#[serde(rename_all = "kebab-case")]
pub enum ProjectType {
    Api,
    AdminApi,
    Pwa,
    Service,
    Worker,
    Function,
}

/// Infrastructure requirements for a project
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct InfrastructureRequirements {
    /// Requires Azure Storage
    #[serde(default)]
    pub storage: bool,

    /// Requires Cosmos DB
    #[serde(default)]
    pub cosmos: bool,

    /// Requires Redis cache
    #[serde(default)]
    pub redis: bool,

    /// Requires Service Bus
    #[serde(default)]
    pub service_bus: bool,

    /// Requires Key Vault
    #[serde(default)]
    pub key_vault: bool,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_command_response() {
        let response = CommandResponse::success(42);
        assert!(response.success);
        assert_eq!(response.result, Some(42));
    }

    #[test]
    fn test_service_status() {
        let status = ServiceStatus::connected().with_account("test@example.com");
        assert!(status.connected);
        assert_eq!(status.account, Some("test@example.com".to_string()));
    }
}
