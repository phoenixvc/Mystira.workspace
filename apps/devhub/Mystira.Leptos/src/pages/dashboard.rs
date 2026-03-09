//! Dashboard page - main overview

use leptos::prelude::*;

use crate::components::card::{Card, StatCard};
use crate::components::status_badge::{Status, StatusBadge};

/// Dashboard page component
#[component]
pub fn DashboardPage() -> impl IntoView {
    view! {
        <div class="space-y-6">
            // Page header
            <div>
                <h1 class="text-2xl font-bold text-gray-900 dark:text-white">"Dashboard"</h1>
                <p class="mt-1 text-gray-500 dark:text-gray-400">
                    "Overview of your Mystira development environment"
                </p>
            </div>

            // Stats grid
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <StatCard
                    title="Active Services"
                    value="4"
                    description="2 running locally"
                />
                <StatCard
                    title="Deployments"
                    value="12"
                    description="Last 30 days"
                />
                <StatCard
                    title="Azure Resources"
                    value="23"
                    description="Across 3 environments"
                />
                <StatCard
                    title="Build Status"
                    value="Passing"
                    description="All checks green"
                />
            </div>

            // Quick actions and recent activity
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <Card title="Quick Actions".to_string()>
                    <div class="space-y-3">
                        <QuickActionItem
                            title="Deploy to Dev"
                            description="Push current changes to development environment"
                            status=Status::Idle
                        />
                        <QuickActionItem
                            title="Run Tests"
                            description="Execute full test suite"
                            status=Status::Idle
                        />
                        <QuickActionItem
                            title="Sync Cosmos Data"
                            description="Migrate data between environments"
                            status=Status::Idle
                        />
                    </div>
                </Card>

                <Card title="Recent Activity".to_string()>
                    <div class="space-y-3">
                        <ActivityItem
                            title="Deployed to staging"
                            time="2 hours ago"
                            status=Status::Success
                        />
                        <ActivityItem
                            title="Infrastructure validation"
                            time="5 hours ago"
                            status=Status::Success
                        />
                        <ActivityItem
                            title="Database migration"
                            time="1 day ago"
                            status=Status::Success
                        />
                    </div>
                </Card>
            </div>

            // Connection status
            <Card title="Connection Status".to_string()>
                <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <ConnectionItem name="Azure CLI" connected=true />
                    <ConnectionItem name="GitHub CLI" connected=true />
                    <ConnectionItem name="Cosmos DB" connected=true />
                    <ConnectionItem name="Storage" connected=false />
                </div>
            </Card>
        </div>
    }
}

/// Quick action item component
#[component]
fn QuickActionItem(
    #[prop(into)] title: String,
    #[prop(into)] description: String,
    status: Status,
) -> impl IntoView {
    view! {
        <div class="flex items-center justify-between p-3 bg-gray-50 dark:bg-gray-700 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-600 cursor-pointer transition-colors">
            <div>
                <p class="font-medium text-gray-900 dark:text-white">{title}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">{description}</p>
            </div>
            <StatusBadge status=status />
        </div>
    }
}

/// Activity item component
#[component]
fn ActivityItem(
    #[prop(into)] title: String,
    #[prop(into)] time: String,
    status: Status,
) -> impl IntoView {
    view! {
        <div class="flex items-center justify-between py-2">
            <div class="flex items-center space-x-3">
                <StatusBadge status=status />
                <span class="text-gray-900 dark:text-white">{title}</span>
            </div>
            <span class="text-sm text-gray-500 dark:text-gray-400">{time}</span>
        </div>
    }
}

/// Connection status item
#[component]
fn ConnectionItem(
    #[prop(into)] name: String,
    connected: bool,
) -> impl IntoView {
    view! {
        <div class="flex items-center space-x-2">
            <span class=format!(
                "w-2 h-2 rounded-full {}",
                if connected { "bg-green-500" } else { "bg-red-500" }
            )/>
            <span class="text-gray-700 dark:text-gray-300">{name}</span>
        </div>
    }
}
