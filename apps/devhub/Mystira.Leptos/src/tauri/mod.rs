//! Tauri integration layer
//!
//! This module provides bindings to call Tauri commands from the Leptos frontend.

use mystira_contracts::devhub::{
    AzureResource, CommandResponse, ConnectionStatus, Deployment, WhatIfChange,
};
use serde::{de::DeserializeOwned, Serialize};
use wasm_bindgen::prelude::*;

/// JavaScript bindings for Tauri invoke
#[wasm_bindgen]
extern "C" {
    #[wasm_bindgen(js_namespace = ["window", "__TAURI__", "core"], catch)]
    async fn invoke(cmd: &str, args: JsValue) -> Result<JsValue, JsValue>;
}

/// Error type for Tauri calls
#[derive(Debug, thiserror::Error)]
pub enum TauriError {
    #[error("Failed to serialize arguments: {0}")]
    Serialization(String),

    #[error("Failed to deserialize response: {0}")]
    Deserialization(String),

    #[error("Tauri invoke failed: {0}")]
    Invoke(String),

    #[error("Command failed: {0}")]
    Command(String),
}

/// Call a Tauri command
pub async fn call<A, R>(command: &str, args: &A) -> Result<R, TauriError>
where
    A: Serialize,
    R: DeserializeOwned,
{
    let args_js = serde_wasm_bindgen::to_value(args)
        .map_err(|e| TauriError::Serialization(e.to_string()))?;

    let result_js = invoke(command, args_js)
        .await
        .map_err(|e| TauriError::Invoke(format!("{:?}", e)))?;

    serde_wasm_bindgen::from_value(result_js)
        .map_err(|e| TauriError::Deserialization(e.to_string()))
}

/// Call a Tauri command that returns a CommandResponse
pub async fn call_command<A, R>(command: &str, args: &A) -> Result<R, TauriError>
where
    A: Serialize,
    R: DeserializeOwned,
{
    let response: CommandResponse<R> = call(command, args).await?;

    if response.success {
        response.result.ok_or_else(|| TauriError::Command("No result returned".to_string()))
    } else {
        Err(TauriError::Command(
            response.error.unwrap_or_else(|| "Unknown error".to_string()),
        ))
    }
}

// ============================================================================
// Infrastructure Commands
// ============================================================================

/// Validate infrastructure templates
pub async fn infrastructure_validate(environment: &str) -> Result<bool, TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        environment: &'a str,
    }
    call_command("infrastructure_validate", &Args { environment }).await
}

/// Preview infrastructure changes (what-if)
pub async fn infrastructure_preview(environment: &str) -> Result<Vec<WhatIfChange>, TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        environment: &'a str,
    }
    call_command("infrastructure_preview", &Args { environment }).await
}

/// Deploy infrastructure
pub async fn infrastructure_deploy(environment: &str) -> Result<Deployment, TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        environment: &'a str,
    }
    call_command("infrastructure_deploy", &Args { environment }).await
}

/// Destroy infrastructure
pub async fn infrastructure_destroy(environment: &str) -> Result<(), TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        environment: &'a str,
    }
    call_command("infrastructure_destroy", &Args { environment }).await
}

// ============================================================================
// Azure Resource Commands
// ============================================================================

/// Get Azure resources
pub async fn get_azure_resources(resource_group: &str) -> Result<Vec<AzureResource>, TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        resource_group: &'a str,
    }
    call_command("get_azure_resources", &Args { resource_group }).await
}

/// Delete an Azure resource
pub async fn delete_azure_resource(resource_id: &str) -> Result<(), TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        resource_id: &'a str,
    }
    call_command("delete_azure_resource", &Args { resource_id }).await
}

// ============================================================================
// Service Commands
// ============================================================================

/// Start a service
pub async fn start_service(service_id: &str) -> Result<(), TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        service_id: &'a str,
    }
    call_command("start_service", &Args { service_id }).await
}

/// Stop a service
pub async fn stop_service(service_id: &str) -> Result<(), TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        service_id: &'a str,
    }
    call_command("stop_service", &Args { service_id }).await
}

/// Build a service
pub async fn prebuild_service(service_id: &str) -> Result<(), TauriError> {
    #[derive(Serialize)]
    struct Args<'a> {
        service_id: &'a str,
    }
    call_command("prebuild_service", &Args { service_id }).await
}

// ============================================================================
// Connection Commands
// ============================================================================

/// Test all connections
pub async fn test_connections() -> Result<ConnectionStatus, TauriError> {
    call_command("test_connection", &()).await
}

/// Check Azure CLI login
pub async fn check_azure_cli() -> Result<bool, TauriError> {
    call_command("check_azure_cli_login", &()).await
}
