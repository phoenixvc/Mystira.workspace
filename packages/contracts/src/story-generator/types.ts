/**
 * Story Generator API Types
 *
 * These types will be migrated from @mystira/story-generator-contracts.
 * During migration, types will be added here and deprecated in the old package.
 *
 * Migration status: PENDING
 * See: docs/architecture/adr/0020-package-consolidation-strategy.md
 */

// =============================================================================
// Generator Configuration Types
// =============================================================================

/**
 * Configuration for story generation
 * @deprecated Placeholder - will be replaced during migration
 */
export interface GeneratorConfig {
  /** AI model to use for generation */
  model: string;
  /** Maximum tokens for generation */
  maxTokens?: number;
  /** Temperature for generation creativity (0-1) */
  temperature?: number;
  /** Additional model parameters */
  parameters?: Record<string, unknown>;
}

/**
 * Story generation request
 * @deprecated Placeholder - will be replaced during migration
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

// =============================================================================
// Generator Result Types
// =============================================================================

/**
 * Result of story generation
 * @deprecated Placeholder - will be replaced during migration
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
  /** Generation timestamp */
  generatedAt: string;
  /** Processing duration in milliseconds */
  durationMs: number;
}
