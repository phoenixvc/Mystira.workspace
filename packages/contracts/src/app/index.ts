/**
 * Mystira App API Contracts
 *
 * This module contains TypeScript type definitions for the Mystira App API.
 *
 * @module @mystira/contracts/app
 * @see {@link https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md | ADR-0020}
 *
 * @example
 * ```typescript
 * import { StoryRequest, StoryResponse } from '@mystira/contracts/app';
 *
 * const request: StoryRequest = {
 *   title: 'My Story',
 *   content: 'Once upon a time...',
 * };
 * ```
 */

// Re-export all App contract types
// TODO: Migrate types from @mystira/app-contracts
export * from './types';
