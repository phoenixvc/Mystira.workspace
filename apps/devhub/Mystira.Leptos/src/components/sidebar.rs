//! Sidebar navigation component

use leptos::prelude::*;
use leptos_router::components::A;

/// Sidebar navigation
#[component]
pub fn Sidebar(open: ReadSignal<bool>) -> impl IntoView {
    view! {
        <aside
            class=move || format!(
                "flex flex-col bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 transition-all duration-300 {}",
                if open.get() { "w-64" } else { "w-16" }
            )
        >
            // Logo
            <div class="flex items-center justify-center h-16 border-b border-gray-200 dark:border-gray-700">
                <span class="text-2xl font-bold text-blue-600 dark:text-blue-400">
                    {move || if open.get() { "Mystira" } else { "M" }}
                </span>
            </div>

            // Navigation links
            <nav class="flex-1 px-2 py-4 space-y-1">
                <NavLink href="/" icon=IconDashboard label="Dashboard" open=open />
                <NavLink href="/infrastructure" icon=IconInfra label="Infrastructure" open=open />
                <NavLink href="/services" icon=IconServices label="Services" open=open />
                <NavLink href="/settings" icon=IconSettings label="Settings" open=open />
            </nav>

            // Version info
            <div class="p-4 border-t border-gray-200 dark:border-gray-700">
                <span class="text-xs text-gray-500 dark:text-gray-400">
                    {move || if open.get() { "v0.1.0" } else { "" }}
                </span>
            </div>
        </aside>
    }
}

/// Navigation link component
#[component]
fn NavLink(
    href: &'static str,
    icon: fn() -> impl IntoView,
    label: &'static str,
    open: ReadSignal<bool>,
) -> impl IntoView {
    view! {
        <A
            href=href
            class="flex items-center px-3 py-2 text-gray-700 dark:text-gray-200 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
        >
            <span class="w-6 h-6">{icon()}</span>
            <span
                class=move || format!(
                    "ml-3 transition-opacity {}",
                    if open.get() { "opacity-100" } else { "opacity-0 w-0 overflow-hidden" }
                )
            >
                {label}
            </span>
        </A>
    }
}

// Icon components
fn IconDashboard() -> impl IntoView {
    view! {
        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
        </svg>
    }
}

fn IconInfra() -> impl IntoView {
    view! {
        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
        </svg>
    }
}

fn IconServices() -> impl IntoView {
    view! {
        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M5 12h14M5 12a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v4a2 2 0 01-2 2M5 12a2 2 0 00-2 2v4a2 2 0 002 2h14a2 2 0 002-2v-4a2 2 0 00-2-2m-2-4h.01M17 16h.01"/>
        </svg>
    }
}

fn IconSettings() -> impl IntoView {
    view! {
        <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
        </svg>
    }
}
