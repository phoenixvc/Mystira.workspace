//! Core application contracts
//!
//! These types correspond to `@mystira/contracts/app` (TypeScript)
//! and `Mystira.Contracts.App` (C#).

use serde::{Deserialize, Serialize};

#[cfg(feature = "typescript")]
use ts_rs::TS;

/// Story request payload
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct StoryRequest {
    /// Story title
    pub title: String,

    /// Story content or prompt
    pub content: String,

    /// Optional story metadata
    #[serde(skip_serializing_if = "Option::is_none")]
    pub metadata: Option<StoryMetadata>,
}

/// Story metadata
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct StoryMetadata {
    /// Story genre
    #[serde(skip_serializing_if = "Option::is_none")]
    pub genre: Option<String>,

    /// Story tags
    #[serde(default)]
    pub tags: Vec<String>,

    /// Author identifier
    #[serde(skip_serializing_if = "Option::is_none")]
    pub author_id: Option<String>,
}

/// Story response payload
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct StoryResponse {
    /// Unique story identifier
    pub id: String,

    /// Story title
    pub title: String,

    /// Story content
    pub content: String,

    /// Creation timestamp
    pub created_at: chrono::DateTime<chrono::Utc>,

    /// Story metadata
    #[serde(skip_serializing_if = "Option::is_none")]
    pub metadata: Option<StoryMetadata>,
}

/// Generic API request wrapper
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ApiRequest<T> {
    /// Request payload
    pub data: T,

    /// Optional request ID for tracing
    #[serde(skip_serializing_if = "Option::is_none")]
    pub request_id: Option<String>,

    /// Request timestamp
    #[serde(skip_serializing_if = "Option::is_none")]
    pub timestamp: Option<chrono::DateTime<chrono::Utc>>,
}

impl<T> ApiRequest<T> {
    /// Create a new API request with the given data
    pub fn new(data: T) -> Self {
        Self {
            data,
            request_id: None,
            timestamp: Some(chrono::Utc::now()),
        }
    }

    /// Create a new API request with a request ID
    pub fn with_id(data: T, request_id: impl Into<String>) -> Self {
        Self {
            data,
            request_id: Some(request_id.into()),
            timestamp: Some(chrono::Utc::now()),
        }
    }
}

/// Generic API response wrapper
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ApiResponse<T> {
    /// Whether the request was successful
    pub success: bool,

    /// Response data (present on success)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub data: Option<T>,

    /// Error information (present on failure)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub error: Option<ApiError>,

    /// Optional message
    #[serde(skip_serializing_if = "Option::is_none")]
    pub message: Option<String>,

    /// Response timestamp
    pub timestamp: chrono::DateTime<chrono::Utc>,
}

impl<T> ApiResponse<T> {
    /// Create a successful response
    pub fn success(data: T) -> Self {
        Self {
            success: true,
            data: Some(data),
            error: None,
            message: None,
            timestamp: chrono::Utc::now(),
        }
    }

    /// Create a successful response with a message
    pub fn success_with_message(data: T, message: impl Into<String>) -> Self {
        Self {
            success: true,
            data: Some(data),
            error: None,
            message: Some(message.into()),
            timestamp: chrono::Utc::now(),
        }
    }

    /// Create a failure response
    pub fn failure(error: ApiError) -> Self {
        Self {
            success: false,
            data: None,
            error: Some(error),
            message: None,
            timestamp: chrono::Utc::now(),
        }
    }

    /// Create a failure response with a message
    pub fn failure_with_message(error: ApiError, message: impl Into<String>) -> Self {
        Self {
            success: false,
            data: None,
            error: Some(error),
            message: Some(message.into()),
            timestamp: chrono::Utc::now(),
        }
    }
}

/// API error information
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct ApiError {
    /// Error code
    pub code: String,

    /// Human-readable error message
    pub message: String,

    /// Additional error details
    #[serde(skip_serializing_if = "Option::is_none")]
    pub details: Option<serde_json::Value>,

    /// Stack trace (debug builds only)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stack: Option<String>,
}

impl ApiError {
    /// Create a new API error
    pub fn new(code: impl Into<String>, message: impl Into<String>) -> Self {
        Self {
            code: code.into(),
            message: message.into(),
            details: None,
            stack: None,
        }
    }

    /// Create a validation error
    pub fn validation(message: impl Into<String>) -> Self {
        Self::new("VALIDATION_ERROR", message)
    }

    /// Create a not found error
    pub fn not_found(message: impl Into<String>) -> Self {
        Self::new("NOT_FOUND", message)
    }

    /// Create an internal error
    pub fn internal(message: impl Into<String>) -> Self {
        Self::new("INTERNAL_ERROR", message)
    }

    /// Create an unauthorized error
    pub fn unauthorized(message: impl Into<String>) -> Self {
        Self::new("UNAUTHORIZED", message)
    }

    /// Add details to the error
    pub fn with_details(mut self, details: serde_json::Value) -> Self {
        self.details = Some(details);
        self
    }
}

impl std::fmt::Display for ApiError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "[{}] {}", self.code, self.message)
    }
}

impl std::error::Error for ApiError {}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_api_response_success() {
        let response = ApiResponse::success("test data");
        assert!(response.success);
        assert_eq!(response.data, Some("test data"));
        assert!(response.error.is_none());
    }

    #[test]
    fn test_api_response_failure() {
        let error = ApiError::validation("Invalid input");
        let response: ApiResponse<()> = ApiResponse::failure(error);
        assert!(!response.success);
        assert!(response.data.is_none());
        assert!(response.error.is_some());
    }

    #[test]
    fn test_api_error_display() {
        let error = ApiError::new("TEST_ERROR", "Something went wrong");
        assert_eq!(error.to_string(), "[TEST_ERROR] Something went wrong");
    }
}
