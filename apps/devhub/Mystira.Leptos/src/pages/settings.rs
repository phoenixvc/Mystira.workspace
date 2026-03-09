//! Settings page

use leptos::prelude::*;

use crate::components::button::{Button, ButtonVariant};
use crate::components::card::Card;

/// Settings page component
#[component]
pub fn SettingsPage() -> impl IntoView {
    let (dark_mode, set_dark_mode) = signal(false);
    let (notifications, set_notifications) = signal(true);

    view! {
        <div class="space-y-6">
            // Page header
            <div>
                <h1 class="text-2xl font-bold text-gray-900 dark:text-white">"Settings"</h1>
                <p class="mt-1 text-gray-500 dark:text-gray-400">
                    "Configure DevHub preferences"
                </p>
            </div>

            // Appearance settings
            <Card title="Appearance".to_string()>
                <div class="space-y-4">
                    <SettingRow
                        title="Dark Mode"
                        description="Use dark theme for the interface"
                    >
                        <Toggle checked=dark_mode on_change=move |v| set_dark_mode.set(v) />
                    </SettingRow>
                </div>
            </Card>

            // Notification settings
            <Card title="Notifications".to_string()>
                <div class="space-y-4">
                    <SettingRow
                        title="Desktop Notifications"
                        description="Show notifications for deployment status changes"
                    >
                        <Toggle checked=notifications on_change=move |v| set_notifications.set(v) />
                    </SettingRow>
                </div>
            </Card>

            // Connection settings
            <Card title="Connections".to_string()>
                <div class="space-y-4">
                    <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                        <div>
                            <p class="font-medium text-gray-900 dark:text-white">"Azure CLI"</p>
                            <p class="text-sm text-gray-500 dark:text-gray-400">"Logged in as user@example.com"</p>
                        </div>
                        <Button
                            variant=ButtonVariant::Secondary
                            on_click=move |_| {
                                // TODO: Re-authenticate Azure CLI
                            }
                        >
                            "Re-authenticate"
                        </Button>
                    </div>
                    <div class="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-700 rounded-lg">
                        <div>
                            <p class="font-medium text-gray-900 dark:text-white">"GitHub CLI"</p>
                            <p class="text-sm text-gray-500 dark:text-gray-400">"Logged in as @username"</p>
                        </div>
                        <Button
                            variant=ButtonVariant::Secondary
                            on_click=move |_| {
                                // TODO: Re-authenticate GitHub CLI
                            }
                        >
                            "Re-authenticate"
                        </Button>
                    </div>
                </div>
            </Card>

            // About section
            <Card title="About".to_string()>
                <div class="space-y-2 text-gray-600 dark:text-gray-300">
                    <p>"Mystira DevHub v0.1.0"</p>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                        "Built with Leptos + Tauri"
                    </p>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                        "© 2025 PhoenixVC"
                    </p>
                </div>
            </Card>
        </div>
    }
}

/// Setting row component
#[component]
fn SettingRow(
    #[prop(into)] title: String,
    #[prop(into)] description: String,
    children: Children,
) -> impl IntoView {
    view! {
        <div class="flex items-center justify-between">
            <div>
                <p class="font-medium text-gray-900 dark:text-white">{title}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">{description}</p>
            </div>
            {children()}
        </div>
    }
}

/// Toggle switch component
#[component]
fn Toggle(
    checked: ReadSignal<bool>,
    #[prop(into)] on_change: Callback<bool>,
) -> impl IntoView {
    view! {
        <button
            type="button"
            role="switch"
            aria-checked=move || checked.get().to_string()
            class=move || format!(
                "relative inline-flex h-6 w-11 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 {}",
                if checked.get() { "bg-blue-600" } else { "bg-gray-200 dark:bg-gray-700" }
            )
            on:click=move |_| on_change.run(!checked.get())
        >
            <span
                class=move || format!(
                    "pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out {}",
                    if checked.get() { "translate-x-5" } else { "translate-x-0" }
                )
            />
        </button>
    }
}
