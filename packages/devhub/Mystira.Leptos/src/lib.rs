//! Mystira Leptos Frontend
//!
//! A reactive web frontend built with Leptos for the Mystira DevHub application.

pub mod app;
pub mod components;
pub mod pages;
pub mod state;
pub mod tauri;

use wasm_bindgen::prelude::*;

/// Initialize the Leptos application
#[wasm_bindgen(start)]
pub fn main() {
    // Set up panic hook for better error messages
    console_error_panic_hook::set_once();

    // Initialize tracing for logging
    tracing_wasm::set_as_global_default()
        .expect("Failed to initialize tracing");

    // Mount the application
    leptos::mount::mount_to_body(app::App);
}
