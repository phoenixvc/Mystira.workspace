//! Top navigation bar component

use leptos::prelude::*;

/// Navigation bar properties
#[component]
pub fn Navbar(
    sidebar_open: ReadSignal<bool>,
    #[prop(into)] on_toggle: Callback<()>,
) -> impl IntoView {
    view! {
        <header class="flex items-center justify-between h-16 px-6 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
            // Left section - Menu toggle
            <div class="flex items-center">
                <button
                    type="button"
                    class="p-2 rounded-md text-gray-500 hover:text-gray-700 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-gray-200 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-blue-500"
                    on:click=move |_| on_toggle.run(())
                >
                    <span class="sr-only">Toggle sidebar</span>
                    <svg
                        class="w-6 h-6"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        {move || if sidebar_open.get() {
                            view! {
                                <path
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    stroke-width="2"
                                    d="M11 19l-7-7 7-7m8 14l-7-7 7-7"
                                />
                            }.into_any()
                        } else {
                            view! {
                                <path
                                    stroke-linecap="round"
                                    stroke-linejoin="round"
                                    stroke-width="2"
                                    d="M4 6h16M4 12h16M4 18h16"
                                />
                            }.into_any()
                        }}
                    </svg>
                </button>

                <h1 class="ml-4 text-xl font-semibold text-gray-800 dark:text-white">
                    "Mystira DevHub"
                </h1>
            </div>

            // Right section - Status and actions
            <div class="flex items-center space-x-4">
                <ConnectionIndicator />
                <ThemeToggle />
            </div>
        </header>
    }
}

/// Connection status indicator
#[component]
fn ConnectionIndicator() -> impl IntoView {
    // TODO: Connect to actual status from Tauri
    let connected = signal(true);

    view! {
        <div class="flex items-center space-x-2">
            <span
                class=move || format!(
                    "w-2 h-2 rounded-full {}",
                    if connected.0.get() { "bg-green-500" } else { "bg-red-500" }
                )
            />
            <span class="text-sm text-gray-600 dark:text-gray-300">
                {move || if connected.0.get() { "Connected" } else { "Disconnected" }}
            </span>
        </div>
    }
}

/// Theme toggle button
#[component]
fn ThemeToggle() -> impl IntoView {
    let (dark_mode, set_dark_mode) = signal(false);

    view! {
        <button
            type="button"
            class="p-2 rounded-md text-gray-500 hover:text-gray-700 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-gray-200 dark:hover:bg-gray-700"
            on:click=move |_| set_dark_mode.update(|v| *v = !*v)
        >
            <span class="sr-only">Toggle theme</span>
            {move || if dark_mode.get() {
                view! {
                    <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z"/>
                    </svg>
                }.into_any()
            } else {
                view! {
                    <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                        <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z"/>
                    </svg>
                }.into_any()
            }}
        </button>
    }
}
