// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

// Module declarations
mod azure;
mod cache;
mod cli;
mod config;
mod cosmos;
mod github;
mod helpers;
mod infrastructure;
mod rate_limit;
mod retry;
mod services;
mod types;
mod utils;

// Re-export commonly used types
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use types::ServiceManager;

// Re-export command functions from modules
use azure::cli::{check_azure_cli, install_azure_cli};
use azure::deploy_now::{
    check_azure_login, check_github_pat, check_npm, check_swa_cli, disconnect_swa_cicd,
    get_git_status, get_swa_deployment_token, git_commit, git_commit_empty, git_push,
    git_stage_all, git_sync, restart_api_services, scan_existing_resources, update_cors_settings,
};
use azure::deployment::{
    azure_create_resource_group, azure_deploy_infrastructure, azure_preview_infrastructure,
    azure_validate_infrastructure, check_infrastructure_exists, check_infrastructure_status,
};
use azure::resources::{check_subscription_owner, delete_azure_resource, get_azure_resources};
use config::{get_app_config, reload_config, save_app_config};
use cosmos::{
    check_azure_cli_login, cosmos_export, cosmos_stats, fetch_environment_connections,
    migration_run,
};
use github::{
    get_github_deployments, github_dispatch_workflow, github_workflow_logs, github_workflow_status,
    list_github_workflows,
};
use infrastructure::{
    infrastructure_deploy, infrastructure_destroy, infrastructure_preview, infrastructure_status,
    infrastructure_validate,
};
use services::lifecycle::{prebuild_service, start_service, stop_service};
use services::ports::{
    check_port_available, find_available_port, get_service_port, update_service_port,
};
use services::status::{check_service_health, get_service_status};
use utils::{
    build_cli, check_resource_health_endpoint, create_webview_window, get_cli_build_time,
    get_current_branch, get_repo_root, read_bicep_file, test_connection,
};

fn main() {
    // Initialize logging
    tracing_subscriber::fmt()
        .with_env_filter(tracing_subscriber::EnvFilter::from_default_env())
        .init();

    tracing::info!("Mystira DevHub starting...");

    // Initialize service manager
    let services: ServiceManager = Arc::new(Mutex::new(HashMap::new()));

    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_fs::init())
        .manage(services)
        .invoke_handler(tauri::generate_handler![
            cosmos_export,
            cosmos_stats,
            migration_run,
            fetch_environment_connections,
            check_azure_cli_login,
            infrastructure_validate,
            infrastructure_preview,
            infrastructure_deploy,
            infrastructure_destroy,
            infrastructure_status,
            azure_deploy_infrastructure,
            azure_create_resource_group,
            azure_validate_infrastructure,
            azure_preview_infrastructure,
            check_infrastructure_exists,
            check_infrastructure_status,
            get_azure_resources,
            delete_azure_resource,
            check_subscription_owner,
            check_azure_cli,
            install_azure_cli,
            check_azure_login,
            check_github_pat,
            check_swa_cli,
            check_npm,
            scan_existing_resources,
            get_git_status,
            git_stage_all,
            git_commit,
            git_commit_empty,
            git_push,
            git_sync,
            update_cors_settings,
            restart_api_services,
            disconnect_swa_cicd,
            get_swa_deployment_token,
            get_github_deployments,
            github_dispatch_workflow,
            github_workflow_status,
            github_workflow_logs,
            test_connection,
            prebuild_service,
            start_service,
            stop_service,
            get_service_status,
            get_repo_root,
            read_bicep_file,
            build_cli,
            get_cli_build_time,
            get_current_branch,
            create_webview_window,
            check_port_available,
            check_service_health,
            get_service_port,
            update_service_port,
            find_available_port,
            list_github_workflows,
            check_resource_health_endpoint,
            get_app_config,
            save_app_config,
            reload_config,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
