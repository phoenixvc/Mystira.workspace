/**
 * @mystira/contracts
 *
 * Unified API contracts for the Mystira platform.
 * This package consolidates type definitions from:
 * - @mystira/app-contracts (deprecated)
 * - @mystira/story-generator-contracts (deprecated)
 *
 * @see {@link https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md | ADR-0020}
 *
 * @example
 * ```typescript
 * // Import specific modules
 * import { StoryRequest } from '@mystira/contracts/app';
 * import { GeneratorConfig } from '@mystira/contracts/story-generator';
 *
 * // Or import everything via namespace
 * import { App, StoryGenerator } from '@mystira/contracts';
 * const request: App.StoryRequest = { ... };
 * ```
 *
 * @packageDocumentation
 */

// Re-export modules as namespaces
import * as App from './app';
import * as StoryGenerator from './story-generator';

export { App, StoryGenerator };

// Also re-export common types at root level for convenience
export type { ApiRequest, ApiResponse, ApiError } from './app/types';
