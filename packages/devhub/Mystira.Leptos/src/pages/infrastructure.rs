//! Infrastructure management page

use leptos::prelude::*;
use mystira_contracts::devhub::{ChangeType, WhatIfChange};

use crate::components::button::{Button, ButtonVariant};
use crate::components::card::Card;
use crate::components::status_badge::{Status, StatusBadge};

/// Infrastructure page component
#[component]
pub fn InfrastructurePage() -> impl IntoView {
    let (selected_env, set_selected_env) = signal("dev".to_string());
    let (is_validating, set_is_validating) = signal(false);
    let (is_deploying, set_is_deploying) = signal(false);

    // Mock what-if changes for demonstration
    let what_if_changes = signal(vec![
        WhatIfChange {
            resource_type: "Microsoft.Storage/storageAccounts".to_string(),
            resource_name: "mystiradev001".to_string(),
            change_type: ChangeType::NoChange,
            changes: vec![],
        },
        WhatIfChange {
            resource_type: "Microsoft.DocumentDB/databaseAccounts".to_string(),
            resource_name: "mystira-cosmos-dev".to_string(),
            change_type: ChangeType::Modify,
            changes: vec![],
        },
    ]);

    view! {
        <div class="space-y-6">
            // Page header
            <div class="flex items-center justify-between">
                <div>
                    <h1 class="text-2xl font-bold text-gray-900 dark:text-white">"Infrastructure"</h1>
                    <p class="mt-1 text-gray-500 dark:text-gray-400">
                        "Manage Azure infrastructure deployments"
                    </p>
                </div>

                // Environment selector
                <div class="flex items-center space-x-4">
                    <select
                        class="px-4 py-2 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-lg text-gray-900 dark:text-white focus:ring-2 focus:ring-blue-500"
                        on:change=move |ev| {
                            set_selected_env.set(event_target_value(&ev));
                        }
                    >
                        <option value="dev" selected=move || selected_env.get() == "dev">"Development"</option>
                        <option value="staging" selected=move || selected_env.get() == "staging">"Staging"</option>
                        <option value="prod" selected=move || selected_env.get() == "prod">"Production"</option>
                    </select>
                </div>
            </div>

            // Action buttons
            <div class="flex space-x-4">
                <Button
                    variant=ButtonVariant::Secondary
                    loading=is_validating.get()
                    on_click=move |_| {
                        set_is_validating.set(true);
                        // TODO: Call Tauri validate command
                    }
                >
                    "Validate"
                </Button>
                <Button
                    variant=ButtonVariant::Secondary
                    on_click=move |_| {
                        // TODO: Call Tauri preview command
                    }
                >
                    "Preview"
                </Button>
                <Button
                    variant=ButtonVariant::Primary
                    loading=is_deploying.get()
                    on_click=move |_| {
                        set_is_deploying.set(true);
                        // TODO: Call Tauri deploy command
                    }
                >
                    "Deploy"
                </Button>
                <Button
                    variant=ButtonVariant::Danger
                    on_click=move |_| {
                        // TODO: Confirm and destroy
                    }
                >
                    "Destroy"
                </Button>
            </div>

            // What-If Preview
            <Card title="Deployment Preview".to_string()>
                <div class="space-y-2">
                    <For
                        each=move || what_if_changes.0.get()
                        key=|change| change.resource_name.clone()
                        children=move |change| {
                            view! { <WhatIfChangeRow change=change /> }
                        }
                    />
                </div>
            </Card>

            // Resource groups
            <Card title="Resource Groups".to_string()>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <ResourceGroupCard
                        name="mystira-dev-rg"
                        location="East US"
                        resource_count=8
                        status=Status::Success
                    />
                    <ResourceGroupCard
                        name="mystira-staging-rg"
                        location="East US"
                        resource_count=8
                        status=Status::Success
                    />
                    <ResourceGroupCard
                        name="mystira-prod-rg"
                        location="East US"
                        resource_count=12
                        status=Status::Success
                    />
                </div>
            </Card>
        </div>
    }
}

/// What-if change row component
#[component]
fn WhatIfChangeRow(change: WhatIfChange) -> impl IntoView {
    let (icon, color) = match change.change_type {
        ChangeType::Create => ("+", "text-green-600"),
        ChangeType::Modify => ("~", "text-yellow-600"),
        ChangeType::Delete => ("-", "text-red-600"),
        ChangeType::NoChange => ("=", "text-gray-400"),
        ChangeType::Ignore => ("!", "text-gray-400"),
        ChangeType::Deploy => ("→", "text-blue-600"),
    };

    view! {
        <div class="flex items-center space-x-3 p-2 hover:bg-gray-50 dark:hover:bg-gray-700 rounded">
            <span class=format!("font-mono font-bold {}", color)>{icon}</span>
            <div class="flex-1">
                <p class="font-medium text-gray-900 dark:text-white">{change.resource_name}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">{change.resource_type}</p>
            </div>
            <span class="text-sm text-gray-500 dark:text-gray-400">
                {format!("{:?}", change.change_type)}
            </span>
        </div>
    }
}

/// Resource group card component
#[component]
fn ResourceGroupCard(
    #[prop(into)] name: String,
    #[prop(into)] location: String,
    resource_count: u32,
    status: Status,
) -> impl IntoView {
    view! {
        <div class="p-4 border border-gray-200 dark:border-gray-700 rounded-lg">
            <div class="flex items-center justify-between mb-2">
                <h4 class="font-medium text-gray-900 dark:text-white">{name}</h4>
                <StatusBadge status=status />
            </div>
            <div class="text-sm text-gray-500 dark:text-gray-400">
                <p>{format!("Location: {}", location)}</p>
                <p>{format!("Resources: {}", resource_count)}</p>
            </div>
        </div>
    }
}
