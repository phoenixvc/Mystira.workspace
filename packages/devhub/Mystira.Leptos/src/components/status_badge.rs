//! Status badge component

use leptos::prelude::*;

/// Status types for badges
#[derive(Default, Clone, Copy, PartialEq, Eq)]
pub enum Status {
    #[default]
    Idle,
    Pending,
    Running,
    Success,
    Error,
    Warning,
}

/// Status badge component
#[component]
pub fn StatusBadge(
    status: Status,
    #[prop(optional, into)] label: Option<String>,
) -> impl IntoView {
    let (bg_class, text_class, default_label) = match status {
        Status::Idle => ("bg-gray-100 dark:bg-gray-700", "text-gray-800 dark:text-gray-200", "Idle"),
        Status::Pending => ("bg-yellow-100 dark:bg-yellow-900", "text-yellow-800 dark:text-yellow-200", "Pending"),
        Status::Running => ("bg-blue-100 dark:bg-blue-900", "text-blue-800 dark:text-blue-200", "Running"),
        Status::Success => ("bg-green-100 dark:bg-green-900", "text-green-800 dark:text-green-200", "Success"),
        Status::Error => ("bg-red-100 dark:bg-red-900", "text-red-800 dark:text-red-200", "Error"),
        Status::Warning => ("bg-orange-100 dark:bg-orange-900", "text-orange-800 dark:text-orange-200", "Warning"),
    };

    let display_label = label.unwrap_or_else(|| default_label.to_string());

    view! {
        <span class=format!(
            "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium {} {}",
            bg_class, text_class
        )>
            // Animated dot for running status
            {(status == Status::Running).then(|| view! {
                <span class="w-2 h-2 mr-1.5 bg-blue-500 rounded-full animate-pulse"/>
            })}
            {display_label}
        </span>
    }
}
