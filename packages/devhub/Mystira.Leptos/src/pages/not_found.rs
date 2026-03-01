//! 404 Not Found page

use leptos::prelude::*;
use leptos_router::components::A;

use crate::components::button::ButtonVariant;

/// Not found page component
#[component]
pub fn NotFoundPage() -> impl IntoView {
    view! {
        <div class="flex flex-col items-center justify-center min-h-[60vh] text-center">
            <h1 class="text-6xl font-bold text-gray-300 dark:text-gray-700">"404"</h1>
            <h2 class="mt-4 text-2xl font-semibold text-gray-900 dark:text-white">
                "Page Not Found"
            </h2>
            <p class="mt-2 text-gray-500 dark:text-gray-400">
                "The page you're looking for doesn't exist or has been moved."
            </p>
            <A
                href="/"
                class="mt-6 inline-flex items-center px-4 py-2 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 transition-colors"
            >
                "Go to Dashboard"
            </A>
        </div>
    }
}
