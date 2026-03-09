//! Services management page

use leptos::prelude::*;
use mystira_contracts::devhub::ProjectType;

use crate::components::button::{Button, ButtonSize, ButtonVariant};
use crate::components::card::Card;
use crate::components::status_badge::{Status, StatusBadge};

/// Services page component
#[component]
pub fn ServicesPage() -> impl IntoView {
    view! {
        <div class="space-y-6">
            // Page header
            <div class="flex items-center justify-between">
                <div>
                    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">"Services"</h1>
                    <p class="mt-1 text-gray-500 dark:text-gray-400">
                        "Manage local development services"
                    </p>
                </div>

                <div class="flex space-x-2">
                    <Button
                        variant=ButtonVariant::Secondary
                        size=ButtonSize::Small
                        on_click=move |_| {
                            // TODO: Refresh all services
                        }
                    >
                        "Refresh All"
                    </Button>
                    <Button
                        variant=ButtonVariant::Primary
                        size=ButtonSize::Small
                        on_click=move |_| {
                            // TODO: Start all services
                        }
                    >
                        "Start All"
                    </Button>
                </div>
            </div>

            // Services grid
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <ServiceCard
                    name="Mystira API"
                    project_type=ProjectType::Api
                    port=5001
                    status=Status::Running
                />
                <ServiceCard
                    name="Mystira Admin API"
                    project_type=ProjectType::AdminApi
                    port=5002
                    status=Status::Idle
                />
                <ServiceCard
                    name="Mystira PWA"
                    project_type=ProjectType::Pwa
                    port=3000
                    status=Status::Running
                />
                <ServiceCard
                    name="Story Generator"
                    project_type=ProjectType::Service
                    port=5003
                    status=Status::Idle
                />
            </div>

            // Logs section
            <Card title="Service Logs".to_string()>
                <div class="h-64 bg-gray-900 rounded-lg p-4 font-mono text-sm text-gray-300 overflow-auto">
                    <p>"[2025-12-24 10:23:45] API: Server started on port 5001"</p>
                    <p>"[2025-12-24 10:23:46] API: Connected to Cosmos DB"</p>
                    <p>"[2025-12-24 10:23:47] PWA: Development server ready"</p>
                    <p class="text-gray-500">"Waiting for more logs..."</p>
                </div>
            </Card>
        </div>
    }
}

/// Service card component
#[component]
fn ServiceCard(
    #[prop(into)] name: String,
    project_type: ProjectType,
    port: u16,
    status: Status,
) -> impl IntoView {
    let is_running = status == Status::Running;

    let type_label = match project_type {
        ProjectType::Api => "API",
        ProjectType::AdminApi => "Admin API",
        ProjectType::Pwa => "PWA",
        ProjectType::Service => "Service",
        ProjectType::Worker => "Worker",
        ProjectType::Function => "Function",
    };

    view! {
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <div class="flex items-start justify-between mb-4">
                <div>
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{name}</h3>
                    <div class="flex items-center space-x-2 mt-1">
                        <span class="text-sm text-gray-500 dark:text-gray-400">{type_label}</span>
                        <span class="text-gray-300 dark:text-gray-600">"|"</span>
                        <span class="text-sm text-gray-500 dark:text-gray-400">{format!("Port {}", port)}</span>
                    </div>
                </div>
                <StatusBadge status=status />
            </div>

            // Action buttons
            <div class="flex space-x-2">
                {if is_running {
                    view! {
                        <Button
                            variant=ButtonVariant::Danger
                            size=ButtonSize::Small
                            on_click=move |_| {
                                // TODO: Stop service
                            }
                        >
                            "Stop"
                        </Button>
                        <Button
                            variant=ButtonVariant::Secondary
                            size=ButtonSize::Small
                            on_click=move |_| {
                                // TODO: Restart service
                            }
                        >
                            "Restart"
                        </Button>
                    }.into_any()
                } else {
                    view! {
                        <Button
                            variant=ButtonVariant::Primary
                            size=ButtonSize::Small
                            on_click=move |_| {
                                // TODO: Start service
                            }
                        >
                            "Start"
                        </Button>
                        <Button
                            variant=ButtonVariant::Secondary
                            size=ButtonSize::Small
                            on_click=move |_| {
                                // TODO: Build service
                            }
                        >
                            "Build"
                        </Button>
                    }.into_any()
                }}
                <Button
                    variant=ButtonVariant::Ghost
                    size=ButtonSize::Small
                    on_click=move |_| {
                        // TODO: View logs
                    }
                >
                    "Logs"
                </Button>
            </div>
        </div>
    }
}
