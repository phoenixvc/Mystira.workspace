//! Main layout component with sidebar and navbar

use leptos::prelude::*;

use super::navbar::Navbar;
use super::sidebar::Sidebar;

/// Main layout wrapper component
#[component]
pub fn Layout(children: Children) -> impl IntoView {
    let (sidebar_open, set_sidebar_open) = signal(true);

    view! {
        <div class="flex h-screen bg-gray-100 dark:bg-gray-900">
            // Sidebar
            <Sidebar open=sidebar_open />

            // Main content area
            <div class="flex flex-col flex-1 overflow-hidden">
                // Top navbar
                <Navbar
                    sidebar_open=sidebar_open
                    on_toggle=move |_| set_sidebar_open.update(|v| *v = !*v)
                />

                // Page content
                <main class="flex-1 overflow-y-auto p-6">
                    {children()}
                </main>
            </div>
        </div>
    }
}
