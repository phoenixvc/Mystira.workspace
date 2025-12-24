/**
 * Mystira Story Generator API Contracts
 *
 * This module contains TypeScript type definitions for the Mystira Story Generator API.
 *
 * @module @mystira/contracts/story-generator
 * @see {@link https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md | ADR-0020}
 *
 * @example
 * ```typescript
 * import { GeneratorConfig, GeneratorResult } from '@mystira/contracts/story-generator';
 *
 * const config: GeneratorConfig = {
 *   model: 'gpt-4',
 *   maxTokens: 2000,
 * };
 * ```
 */

// Re-export all Story Generator contract types
// TODO: Migrate types from @mystira/story-generator-contracts
export * from './types';
