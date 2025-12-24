/**
 * App API Types
 *
 * These types will be migrated from @mystira/app-contracts.
 * During migration, types will be added here and deprecated in the old package.
 *
 * Migration status: PENDING
 * See: docs/architecture/adr/0020-package-consolidation-strategy.md
 */

// =============================================================================
// Placeholder types - to be replaced with actual types during migration
// =============================================================================

/**
 * Base request interface for API calls
 */
export interface ApiRequest {
  /** Request correlation ID for tracing */
  correlationId?: string;
}

/**
 * Base response interface for API calls
 */
export interface ApiResponse<T = unknown> {
  /** Whether the request was successful */
  success: boolean;
  /** Response data */
  data?: T;
  /** Error details if not successful */
  error?: ApiError;
}

/**
 * API error details
 */
export interface ApiError {
  /** Error code */
  code: string;
  /** Human-readable error message */
  message: string;
  /** Additional error details */
  details?: Record<string, unknown>;
}

// =============================================================================
// Story Types - Placeholder for migration
// =============================================================================

/**
 * Story creation request
 * @deprecated Placeholder - will be replaced during migration
 */
export interface StoryRequest extends ApiRequest {
  title: string;
  content: string;
  metadata?: Record<string, unknown>;
}

/**
 * Story response
 * @deprecated Placeholder - will be replaced during migration
 */
export interface StoryResponse {
  id: string;
  title: string;
  content: string;
  createdAt: string;
  updatedAt: string;
}
