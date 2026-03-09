//! Card component

use leptos::prelude::*;

/// Card component for content grouping
#[component]
pub fn Card(
    #[prop(optional, into)] title: Option<String>,
    #[prop(optional, into)] class: String,
    children: Children,
) -> impl IntoView {
    view! {
        <div class=format!(
            "bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 {}",
            class
        )>
            {title.map(|t| view! {
                <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700">
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white">{t}</h3>
                </div>
            })}
            <div class="p-6">
                {children()}
            </div>
        </div>
    }
}

/// Stat card for displaying metrics
#[component]
pub fn StatCard(
    #[prop(into)] title: String,
    #[prop(into)] value: String,
    #[prop(optional, into)] description: Option<String>,
    #[prop(optional, into)] icon: Option<View>,
    #[prop(optional, into)] trend: Option<Trend>,
) -> impl IntoView {
    view! {
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
            <div class="flex items-center justify-between">
                <div>
                    <p class="text-sm font-medium text-gray-500 dark:text-gray-400">{title}</p>
                    <p class="mt-1 text-3xl font-semibold text-gray-900 dark:text-white">{value}</p>
                    {description.map(|d| view! {
                        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">{d}</p>
                    })}
                    {trend.map(|t| view! {
                        <div class=format!(
                            "mt-2 flex items-center text-sm {}",
                            if t.positive { "text-green-600" } else { "text-red-600" }
                        )>
                            {if t.positive {
                                view! { <span>"↑"</span> }.into_any()
                            } else {
                                view! { <span>"↓"</span> }.into_any()
                            }}
                            <span class="ml-1">{t.value}</span>
                        </div>
                    })}
                </div>
                {icon.map(|i| view! {
                    <div class="p-3 bg-blue-100 dark:bg-blue-900 rounded-full">
                        {i}
                    </div>
                })}
            </div>
        </div>
    }
}

/// Trend indicator
#[derive(Clone)]
pub struct Trend {
    pub value: String,
    pub positive: bool,
}
