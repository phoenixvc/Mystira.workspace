//! Tauri command handlers
//!
//! These commands are exposed to the frontend via Tauri's IPC.

use mystira_contracts::devhub::{
    AzureResource, CommandResponse, ConnectionStatus, Deployment, DeploymentAction,
    DeploymentStatus, ServiceStatus, WhatIfChange,
};
use serde_json::json;

use crate::cli::{command_exists, execute_az, execute_cli};
use crate::error::AppResult;

// ============================================================================
// Infrastructure Commands
// ============================================================================

/// Validate infrastructure templates
#[tauri::command]
pub async fn infrastructure_validate(environment: String) -> Result<CommandResponse<bool>, String> {
    tracing::info!("Validating infrastructure for environment: {}", environment);

    match execute_cli::<bool>("infrastructure.validate", json!({ "environment": environment })).await {
        Ok(result) => Ok(CommandResponse::success(result)),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Preview infrastructure changes
#[tauri::command]
pub async fn infrastructure_preview(
    environment: String,
) -> Result<CommandResponse<Vec<WhatIfChange>>, String> {
    tracing::info!("Previewing infrastructure for environment: {}", environment);

    match execute_cli::<Vec<WhatIfChange>>(
        "infrastructure.preview",
        json!({ "environment": environment }),
    )
    .await
    {
        Ok(result) => Ok(CommandResponse::success(result)),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Deploy infrastructure
#[tauri::command]
pub async fn infrastructure_deploy(
    environment: String,
) -> Result<CommandResponse<Deployment>, String> {
    tracing::info!("Deploying infrastructure for environment: {}", environment);

    match execute_cli::<Deployment>("infrastructure.deploy", json!({ "environment": environment }))
        .await
    {
        Ok(result) => Ok(CommandResponse::success(result)),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Destroy infrastructure
#[tauri::command]
pub async fn infrastructure_destroy(environment: String) -> Result<CommandResponse<()>, String> {
    tracing::info!("Destroying infrastructure for environment: {}", environment);

    match execute_cli::<()>("infrastructure.destroy", json!({ "environment": environment })).await {
        Ok(_) => Ok(CommandResponse::success(())),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Get infrastructure status
#[tauri::command]
pub async fn infrastructure_status(
    environment: String,
) -> Result<CommandResponse<Deployment>, String> {
    tracing::info!("Getting infrastructure status for environment: {}", environment);

    match execute_cli::<Deployment>("infrastructure.status", json!({ "environment": environment }))
        .await
    {
        Ok(result) => Ok(CommandResponse::success(result)),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

// ============================================================================
// Azure Commands
// ============================================================================

/// Get Azure resources in a resource group
#[tauri::command]
pub async fn get_azure_resources(
    resource_group: String,
) -> Result<CommandResponse<Vec<AzureResource>>, String> {
    tracing::info!("Getting Azure resources for: {}", resource_group);

    match execute_az(&["resource", "list", "--resource-group", &resource_group]).await {
        Ok(value) => {
            let resources: Vec<AzureResource> = serde_json::from_value(value).unwrap_or_default();
            Ok(CommandResponse::success(resources))
        }
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Delete an Azure resource
#[tauri::command]
pub async fn delete_azure_resource(resource_id: String) -> Result<CommandResponse<()>, String> {
    tracing::info!("Deleting Azure resource: {}", resource_id);

    match execute_az(&["resource", "delete", "--ids", &resource_id]).await {
        Ok(_) => Ok(CommandResponse::success(())),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Check if Azure CLI is logged in
#[tauri::command]
pub async fn check_azure_cli_login() -> Result<CommandResponse<bool>, String> {
    match execute_az(&["account", "show"]).await {
        Ok(_) => Ok(CommandResponse::success(true)),
        Err(_) => Ok(CommandResponse::success(false)),
    }
}

// ============================================================================
// Service Commands
// ============================================================================

/// Start a development service
#[tauri::command]
pub async fn start_service(service_id: String) -> Result<CommandResponse<()>, String> {
    tracing::info!("Starting service: {}", service_id);

    match execute_cli::<()>("service.start", json!({ "serviceId": service_id })).await {
        Ok(_) => Ok(CommandResponse::success(())),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Stop a development service
#[tauri::command]
pub async fn stop_service(service_id: String) -> Result<CommandResponse<()>, String> {
    tracing::info!("Stopping service: {}", service_id);

    match execute_cli::<()>("service.stop", json!({ "serviceId": service_id })).await {
        Ok(_) => Ok(CommandResponse::success(())),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Pre-build a service
#[tauri::command]
pub async fn prebuild_service(service_id: String) -> Result<CommandResponse<()>, String> {
    tracing::info!("Building service: {}", service_id);

    match execute_cli::<()>("service.build", json!({ "serviceId": service_id })).await {
        Ok(_) => Ok(CommandResponse::success(())),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

/// Get service status
#[tauri::command]
pub async fn get_service_status(
    service_id: String,
) -> Result<CommandResponse<ServiceStatus>, String> {
    tracing::info!("Getting service status: {}", service_id);

    match execute_cli::<ServiceStatus>("service.status", json!({ "serviceId": service_id })).await {
        Ok(result) => Ok(CommandResponse::success(result)),
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}

// ============================================================================
// Connection Commands
// ============================================================================

/// Test all connections
#[tauri::command]
pub async fn test_connection() -> Result<CommandResponse<ConnectionStatus>, String> {
    tracing::info!("Testing connections");

    // Check Azure CLI
    let azure_cli = if command_exists("az").await {
        match execute_az(&["account", "show"]).await {
            Ok(value) => {
                let account = value
                    .get("user")
                    .and_then(|u| u.get("name"))
                    .and_then(|n| n.as_str())
                    .unwrap_or("unknown")
                    .to_string();
                ServiceStatus::connected().with_account(account)
            }
            Err(_) => ServiceStatus::disconnected("Not logged in"),
        }
    } else {
        ServiceStatus::disconnected("Azure CLI not installed")
    };

    // Check GitHub CLI
    let github_cli = if command_exists("gh").await {
        match std::process::Command::new("gh")
            .args(["auth", "status"])
            .output()
        {
            Ok(output) if output.status.success() => {
                ServiceStatus::connected().with_account("authenticated")
            }
            _ => ServiceStatus::disconnected("Not logged in"),
        }
    } else {
        ServiceStatus::disconnected("GitHub CLI not installed")
    };

    // For now, mark these as not checked (would need actual connection testing)
    let cosmos = ServiceStatus::disconnected("Not configured");
    let storage = ServiceStatus::disconnected("Not configured");

    Ok(CommandResponse::success(ConnectionStatus {
        cosmos,
        storage,
        azure_cli,
        github_cli,
    }))
}

// ============================================================================
// Utility Commands
// ============================================================================

/// Get the repository root directory
#[tauri::command]
pub async fn get_repo_root() -> Result<CommandResponse<String>, String> {
    match std::env::current_dir() {
        Ok(path) => {
            // Walk up to find .git directory
            let mut current = path.as_path();
            loop {
                if current.join(".git").exists() {
                    return Ok(CommandResponse::success(current.to_string_lossy().to_string()));
                }
                match current.parent() {
                    Some(parent) => current = parent,
                    None => break,
                }
            }
            Ok(CommandResponse::failure("Not in a git repository".to_string()))
        }
        Err(e) => Ok(CommandResponse::failure(e.to_string())),
    }
}
