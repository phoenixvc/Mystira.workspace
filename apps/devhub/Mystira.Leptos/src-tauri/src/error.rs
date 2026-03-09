//! Error types for the Tauri application

use serde::Serialize;

/// Application error type
#[derive(Debug, thiserror::Error)]
pub enum AppError {
    #[error("CLI error: {0}")]
    Cli(String),

    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("Serialization error: {0}")]
    Serialization(#[from] serde_json::Error),

    #[error("Command failed: {0}")]
    Command(String),

    #[error("Not found: {0}")]
    NotFound(String),

    #[error("Validation error: {0}")]
    Validation(String),

    #[error("Azure error: {0}")]
    Azure(String),
}

impl Serialize for AppError {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        serializer.serialize_str(&self.to_string())
    }
}

/// Result type alias for the application
pub type AppResult<T> = Result<T, AppError>;
