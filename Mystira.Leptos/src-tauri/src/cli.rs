//! CLI execution utilities
//!
//! Handles communication with the .NET CLI wrapper.

use mystira_contracts::devhub::{CommandRequest, CommandResponse};
use serde::de::DeserializeOwned;
use std::process::Stdio;
use tokio::io::{AsyncBufReadExt, AsyncWriteExt, BufReader};
use tokio::process::Command;

use crate::error::{AppError, AppResult};

/// Path to the DevHub CLI executable
const CLI_PATH: &str = "../Mystira.DevHub.CLI/bin/Debug/net9.0/Mystira.DevHub.CLI";

/// Execute a command through the DevHub CLI
pub async fn execute_cli<T: DeserializeOwned>(
    command: &str,
    args: serde_json::Value,
) -> AppResult<T> {
    let request = CommandRequest {
        command: command.to_string(),
        args,
    };

    let request_json = serde_json::to_string(&request)?;
    tracing::debug!("Executing CLI command: {} with args: {}", command, request_json);

    // Spawn the CLI process
    let mut child = Command::new(CLI_PATH)
        .stdin(Stdio::piped())
        .stdout(Stdio::piped())
        .stderr(Stdio::piped())
        .spawn()
        .map_err(|e| AppError::Cli(format!("Failed to spawn CLI: {}", e)))?;

    // Write request to stdin
    if let Some(mut stdin) = child.stdin.take() {
        stdin.write_all(request_json.as_bytes()).await?;
        stdin.write_all(b"\n").await?;
        stdin.flush().await?;
    }

    // Read response from stdout
    let stdout = child.stdout.take().ok_or_else(|| {
        AppError::Cli("Failed to capture stdout".to_string())
    })?;

    let mut reader = BufReader::new(stdout);
    let mut response_line = String::new();
    reader.read_line(&mut response_line).await?;

    // Wait for process to complete
    let status = child.wait().await?;
    if !status.success() {
        return Err(AppError::Cli(format!("CLI exited with status: {}", status)));
    }

    // Parse response
    let response: CommandResponse<T> = serde_json::from_str(&response_line)?;

    if response.success {
        response.result.ok_or_else(|| {
            AppError::Command("Command succeeded but returned no result".to_string())
        })
    } else {
        Err(AppError::Command(
            response.error.unwrap_or_else(|| "Unknown error".to_string()),
        ))
    }
}

/// Execute an Azure CLI command
pub async fn execute_az(args: &[&str]) -> AppResult<serde_json::Value> {
    let output = Command::new("az")
        .args(args)
        .args(["--output", "json"])
        .output()
        .await
        .map_err(|e| AppError::Azure(format!("Failed to execute az: {}", e)))?;

    if output.status.success() {
        let stdout = String::from_utf8_lossy(&output.stdout);
        if stdout.trim().is_empty() {
            Ok(serde_json::Value::Null)
        } else {
            serde_json::from_str(&stdout)
                .map_err(|e| AppError::Azure(format!("Failed to parse az output: {}", e)))
        }
    } else {
        let stderr = String::from_utf8_lossy(&output.stderr);
        Err(AppError::Azure(stderr.to_string()))
    }
}

/// Execute a GitHub CLI command
pub async fn execute_gh(args: &[&str]) -> AppResult<serde_json::Value> {
    let output = Command::new("gh")
        .args(args)
        .args(["--json"])
        .output()
        .await
        .map_err(|e| AppError::Command(format!("Failed to execute gh: {}", e)))?;

    if output.status.success() {
        let stdout = String::from_utf8_lossy(&output.stdout);
        if stdout.trim().is_empty() {
            Ok(serde_json::Value::Null)
        } else {
            serde_json::from_str(&stdout).map_err(|e| {
                AppError::Command(format!("Failed to parse gh output: {}", e))
            })
        }
    } else {
        let stderr = String::from_utf8_lossy(&output.stderr);
        Err(AppError::Command(stderr.to_string()))
    }
}

/// Check if a command is available
pub async fn command_exists(cmd: &str) -> bool {
    Command::new("which")
        .arg(cmd)
        .output()
        .await
        .map(|o| o.status.success())
        .unwrap_or(false)
}
