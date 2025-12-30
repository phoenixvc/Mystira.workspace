//! Mystira Contracts - Shared API types for Mystira applications
//!
//! This crate provides unified contract types that mirror the TypeScript
//! `@mystira/contracts` and C# `Mystira.Contracts` packages.
//!
//! # Modules
//!
//! - [`app`] - Core application contracts (API requests/responses)
//! - [`story_generator`] - Story generator service contracts
//! - [`devhub`] - DevHub-specific contracts
//!
//! # Feature Flags
//!
//! - `typescript` - Enable TypeScript bindings generation via ts-rs

pub mod app;
pub mod devhub;
pub mod story_generator;

// Re-export common types at crate root for convenience
pub use app::{ApiError, ApiRequest, ApiResponse};
pub use devhub::{CommandRequest, CommandResponse};
