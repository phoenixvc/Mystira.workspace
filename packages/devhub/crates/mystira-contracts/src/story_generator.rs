//! Story Generator contracts
//!
//! These types correspond to `@mystira/contracts/story-generator` (TypeScript)
//! and `Mystira.Contracts.StoryGenerator` (C#).

use serde::{Deserialize, Serialize};

#[cfg(feature = "typescript")]
use ts_rs::TS;

/// Generator configuration
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct GeneratorConfig {
    /// Model to use for generation
    pub model: String,

    /// Maximum tokens to generate
    #[serde(default = "default_max_tokens")]
    pub max_tokens: u32,

    /// Temperature for generation (0.0 - 2.0)
    #[serde(default = "default_temperature")]
    pub temperature: f32,

    /// Top-p sampling parameter
    #[serde(default = "default_top_p")]
    pub top_p: f32,

    /// Stop sequences
    #[serde(default)]
    pub stop_sequences: Vec<String>,

    /// Additional generation parameters
    #[serde(default)]
    pub parameters: std::collections::HashMap<String, serde_json::Value>,
}

fn default_max_tokens() -> u32 {
    2048
}
fn default_temperature() -> f32 {
    0.7
}
fn default_top_p() -> f32 {
    0.9
}

impl Default for GeneratorConfig {
    fn default() -> Self {
        Self {
            model: "default".to_string(),
            max_tokens: default_max_tokens(),
            temperature: default_temperature(),
            top_p: default_top_p(),
            stop_sequences: Vec::new(),
            parameters: std::collections::HashMap::new(),
        }
    }
}

/// Story generation request
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct GeneratorRequest {
    /// The prompt or story beginning
    pub prompt: String,

    /// Generation configuration
    #[serde(default)]
    pub config: GeneratorConfig,

    /// Generation context
    #[serde(skip_serializing_if = "Option::is_none")]
    pub context: Option<GeneratorContext>,
}

/// Generation context for continuations
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct GeneratorContext {
    /// Previous story segments
    #[serde(default)]
    pub history: Vec<String>,

    /// Character definitions
    #[serde(default)]
    pub characters: Vec<Character>,

    /// World/setting information
    #[serde(skip_serializing_if = "Option::is_none")]
    pub world: Option<String>,

    /// Style instructions
    #[serde(skip_serializing_if = "Option::is_none")]
    pub style: Option<String>,
}

/// Character definition
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct Character {
    /// Character name
    pub name: String,

    /// Character description
    pub description: String,

    /// Character traits
    #[serde(default)]
    pub traits: Vec<String>,
}

/// Story generation result
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct GeneratorResult {
    /// Generated story content
    pub content: String,

    /// Generation metadata
    pub metadata: GeneratorMetadata,

    /// Whether generation completed successfully
    pub completed: bool,

    /// Finish reason
    #[serde(skip_serializing_if = "Option::is_none")]
    pub finish_reason: Option<FinishReason>,
}

/// Generation metadata
#[derive(Debug, Clone, Serialize, Deserialize)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
pub struct GeneratorMetadata {
    /// Model used for generation
    pub model: String,

    /// Tokens in the prompt
    pub prompt_tokens: u32,

    /// Tokens in the completion
    pub completion_tokens: u32,

    /// Total tokens used
    pub total_tokens: u32,

    /// Generation duration in milliseconds
    pub duration_ms: u64,

    /// Generation timestamp
    pub timestamp: chrono::DateTime<chrono::Utc>,
}

/// Reason generation finished
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[cfg_attr(feature = "typescript", derive(TS))]
#[cfg_attr(feature = "typescript", ts(export))]
#[serde(rename_all = "snake_case")]
pub enum FinishReason {
    /// Completed naturally
    Stop,
    /// Hit max token limit
    Length,
    /// Hit a stop sequence
    StopSequence,
    /// Content was filtered
    ContentFilter,
    /// Generation was cancelled
    Cancelled,
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_default_config() {
        let config = GeneratorConfig::default();
        assert_eq!(config.max_tokens, 2048);
        assert!((config.temperature - 0.7).abs() < f32::EPSILON);
        assert!((config.top_p - 0.9).abs() < f32::EPSILON);
    }

    #[test]
    fn test_generator_request_serialization() {
        let request = GeneratorRequest {
            prompt: "Once upon a time".to_string(),
            config: GeneratorConfig::default(),
            context: None,
        };

        let json = serde_json::to_string(&request).unwrap();
        let deserialized: GeneratorRequest = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized.prompt, request.prompt);
    }
}
