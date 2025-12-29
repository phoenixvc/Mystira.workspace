/**
 * Story Generator API Types
 *
 * TypeScript type definitions matching the C# contracts in Mystira.Contracts.StoryGenerator.
 * These types are auto-synced from the .NET contracts package.
 *
 * @see packages/contracts/dotnet/Mystira.Contracts/StoryGenerator/
 */

// =============================================================================
// Enums
// =============================================================================

/**
 * Type of message in a chat conversation
 */
export enum ChatMessageType {
  User = 'User',
  AI = 'AI',
  System = 'System',
}

/**
 * Type of entity in a scene: character, location, item, or concept
 */
export enum SceneEntityType {
  /** Places, rooms, villages, forests, magical realms */
  Location = 'location',
  /** Any sentient individual (child, adult, animal with personality, monster, etc.) */
  Character = 'character',
  /** Physical objects, tools, weapons, artifacts, etc. */
  Item = 'item',
  /** Abstract ideas (fear, honesty, courage, chaos, magic, time) */
  Concept = 'concept',
}

/**
 * Confidence level for entity recognition
 */
export enum Confidence {
  Unknown = 'unknown',
  Low = 'low',
  Medium = 'medium',
  High = 'high',
}

/**
 * Intent category for routing requests
 */
export enum IntentCategory {
  StoryGeneration = 'story_generation',
  Validation = 'validation',
  Autofix = 'autofix',
  Summarization = 'summarization',
  Config = 'config',
  Safety = 'safety',
  Meta = 'meta',
}

/**
 * Specific instruction type for intent routing
 */
export enum IntentInstructionType {
  StoryGenerateInitial = 'story_generate_initial',
  StoryGenerateRefine = 'story_generate_refine',
  StoryValidate = 'story_validate',
  StoryAutofix = 'story_autofix',
  StorySummarize = 'story_summarize',
  ConfigView = 'config_view',
  ConfigUpdate = 'config_update',
  Help = 'help',
  SchemaDocs = 'schema_docs',
  SafetyPolicy = 'safety_policy',
  Requirements = 'requirements',
  Guidelines = 'guidelines',
}

// =============================================================================
// Chat Types
// =============================================================================

/**
 * A chat message in the Mystira conversation format
 */
export interface MystiraChatMessage {
  /** Unique identifier for the message */
  id: string;
  /** Type of message (User, AI, System) */
  messageType: ChatMessageType;
  /** Message content */
  content: string;
  /** Timestamp of the message (ISO 8601) */
  timestamp: string;
  /** Optional metadata */
  metadata?: Record<string, unknown>;
}

/**
 * Snapshot of the current story being worked on
 */
export interface StorySnapshot {
  /** Unique identifier for the story */
  storyId: string;
  /** Version number of the story */
  storyVersion: number;
  /** Story content (typically JSON) */
  content: string;
}

/**
 * JSON Schema response format for structured output
 */
export interface JsonSchemaResponseFormat {
  /** A short, descriptive name for the schema format */
  formatName: string;
  /** The JSON Schema as a raw JSON string */
  schemaJson: string;
  /** Whether the schema should be enforced strictly by the provider */
  isStrict: boolean;
}

/**
 * Request model for chat completion API calls
 */
export interface ChatCompletionRequest {
  /** Optional AI provider override (e.g., "azure-openai", "google-gemini") */
  provider?: string;
  /** Optional logical model identifier defined in configuration (e.g., "story-advanced") */
  modelId?: string;
  /** Provider-specific model name or deployment (e.g., "gpt-4o", "gemini-pro") */
  model?: string;
  /** Array of messages in the conversation */
  messages: MystiraChatMessage[];
  /** Controls randomness in responses (0.0 to 2.0), default 0.7 */
  temperature?: number;
  /** Maximum number of tokens to generate (up to 4096), default 1000 */
  maxTokens?: number;
  /** Optional system prompt to set behavior */
  systemPrompt?: string;
  /** Optional JSON Schema format for structured output */
  jsonSchemaFormat?: JsonSchemaResponseFormat;
  /** Whether the schema should be enforced strictly by the provider */
  isSchemaValidationStrict?: boolean;
  /** Optional snapshot of the current story being worked on */
  currentStory?: StorySnapshot;
}

/**
 * Usage statistics for a chat completion
 */
export interface ChatCompletionUsage {
  /** Number of tokens in the prompt */
  promptTokens: number;
  /** Number of tokens in the completion */
  completionTokens: number;
  /** Total number of tokens used */
  totalTokens: number;
}

/**
 * Response model for chat completion API calls
 */
export interface ChatCompletionResponse {
  /** The generated message content */
  content: string;
  /** The model used for generation */
  model: string;
  /** Logical model identifier resolved from configuration */
  modelId?: string;
  /** The provider used for generation */
  provider: string;
  /** Usage statistics for the request */
  usage?: ChatCompletionUsage;
  /** Timestamp of the response (ISO 8601) */
  timestamp: string;
  /** Whether the response was successful */
  success: boolean;
  /** Error message if the request failed */
  error?: string;
  /** The reason the model stopped generating (e.g., "stop", "length", "content_filter") */
  finishReason?: string;
  /** Whether the generation was incomplete (e.g., due to length limits) */
  isIncomplete: boolean;
}

// =============================================================================
// Entity Types
// =============================================================================

/**
 * Entity that was introduced in a specific scene
 */
export interface SceneEntity {
  /** Type of entity (location, character, item, concept) */
  type: SceneEntityType;
  /** Name of the entity */
  name: string;
  /** Whether the entity name is a proper noun */
  isProperNoun: boolean;
  /** Confidence level of the entity recognition */
  confidence: Confidence;
}

// =============================================================================
// Intent Types
// =============================================================================

/**
 * Classification result for intent routing
 */
export interface IntentClassification {
  /** Categories the request belongs to */
  categories: string[];
  /** Specific instruction types identified */
  instructionTypes: string[];
}

// =============================================================================
// Story Generation Types
// =============================================================================

/**
 * Request for story generation
 */
export interface GenerateStoryRequest {
  /** The prompt or starting text for the story */
  prompt: string;
  /** The tone of the story (e.g., "adventurous", "mysterious") */
  tone: string;
  /** Target length in words, default 500 */
  targetLength?: number;
}

/**
 * Response from story generation
 */
export interface GenerateStoryResponse {
  /** The generated story content */
  story: string;
  /** The model used for generation */
  model: string;
}

// =============================================================================
// Configuration Types
// =============================================================================

/**
 * Configuration for story generation
 */
export interface GeneratorConfig {
  /** AI model to use for generation */
  model: string;
  /** Maximum tokens for generation */
  maxTokens?: number;
  /** Temperature for generation creativity (0-2) */
  temperature?: number;
  /** Additional model parameters */
  parameters?: Record<string, unknown>;
}

/**
 * Story generation request with full configuration
 */
export interface GeneratorRequest {
  /** Prompt or starting text */
  prompt: string;
  /** Generator configuration */
  config: GeneratorConfig;
  /** Context from previous generations */
  context?: GeneratorContext;
}

/**
 * Context for multi-turn generation
 */
export interface GeneratorContext {
  /** Previous generation IDs */
  previousGenerations?: string[];
  /** Character definitions */
  characters?: Record<string, unknown>;
  /** World/setting context */
  worldContext?: string;
}

/**
 * Result of story generation
 */
export interface GeneratorResult {
  /** Unique generation ID */
  id: string;
  /** Generated content */
  content: string;
  /** Generation metadata */
  metadata: GeneratorMetadata;
  /** Whether generation completed successfully */
  success: boolean;
}

/**
 * Metadata about a generation
 */
export interface GeneratorMetadata {
  /** Tokens used in generation */
  tokensUsed: number;
  /** Model used */
  model: string;
  /** Generation timestamp (ISO 8601) */
  generatedAt: string;
  /** Processing duration in milliseconds */
  durationMs: number;
}
