//! Application state management

use leptos::prelude::*;
use mystira_contracts::devhub::{ConnectionStatus, ServiceStatus};

/// Global application state
#[derive(Clone, Debug)]
pub struct AppState {
    /// Current connection status
    pub connection_status: RwSignal<ConnectionStatus>,

    /// Dark mode enabled
    pub dark_mode: RwSignal<bool>,

    /// Current environment (dev, staging, prod)
    pub environment: RwSignal<String>,
}

impl Default for AppState {
    fn default() -> Self {
        Self::new()
    }
}

impl AppState {
    /// Create new application state
    pub fn new() -> Self {
        Self {
            connection_status: RwSignal::new(ConnectionStatus {
                cosmos: ServiceStatus::disconnected("Not checked"),
                storage: ServiceStatus::disconnected("Not checked"),
                azure_cli: ServiceStatus::disconnected("Not checked"),
                github_cli: ServiceStatus::disconnected("Not checked"),
            }),
            dark_mode: RwSignal::new(false),
            environment: RwSignal::new("dev".to_string()),
        }
    }

    /// Provide state context
    pub fn provide_context(self) {
        provide_context(self);
    }

    /// Use state from context
    pub fn use_context() -> Self {
        expect_context::<Self>()
    }
}

/// Service state for managing running services
#[derive(Clone, Debug, Default)]
pub struct ServicesState {
    /// Running services and their status
    pub services: RwSignal<Vec<ServiceInfo>>,
}

/// Information about a running service
#[derive(Clone, Debug)]
pub struct ServiceInfo {
    pub id: String,
    pub name: String,
    pub port: u16,
    pub status: ServiceRunStatus,
    pub pid: Option<u32>,
}

/// Service running status
#[derive(Clone, Debug, PartialEq, Eq)]
pub enum ServiceRunStatus {
    Stopped,
    Starting,
    Running,
    Stopping,
    Error(String),
}

/// Deployment state
#[derive(Clone, Debug, Default)]
pub struct DeploymentState {
    /// Recent deployments
    pub deployments: RwSignal<Vec<mystira_contracts::devhub::Deployment>>,

    /// Current deployment in progress
    pub current_deployment: RwSignal<Option<String>>,
}
