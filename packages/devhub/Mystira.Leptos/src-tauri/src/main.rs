//! Mystira Leptos Tauri Application
//!
//! Main entry point for the Tauri desktop application.

#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod commands;
mod cli;
mod error;

use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt};

fn main() {
    // Initialize tracing
    tracing_subscriber::registry()
        .with(tracing_subscriber::fmt::layer())
        .with(tracing_subscriber::EnvFilter::from_default_env())
        .init();

    tracing::info!("Starting Mystira DevHub");

    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_dialog::init())
        .invoke_handler(tauri::generate_handler![
            // Infrastructure commands
            commands::infrastructure_validate,
            commands::infrastructure_preview,
            commands::infrastructure_deploy,
            commands::infrastructure_destroy,
            commands::infrastructure_status,
            // Azure commands
            commands::get_azure_resources,
            commands::delete_azure_resource,
            commands::check_azure_cli_login,
            // Service commands
            commands::start_service,
            commands::stop_service,
            commands::prebuild_service,
            commands::get_service_status,
            // Connection commands
            commands::test_connection,
            // Utility commands
            commands::get_repo_root,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
