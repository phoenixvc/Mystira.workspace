/**
 * Pagination types for Mystira platform.
 * Mirrors common pagination patterns used across APIs.
 */

/**
 * Paginated request parameters.
 */
export interface PaginationRequest {
  /** Page number (1-indexed) */
  page?: number;
  /** Items per page */
  pageSize?: number;
  /** Sort field */
  sortBy?: string;
  /** Sort direction */
  sortOrder?: 'asc' | 'desc';
}

/**
 * Paginated response wrapper.
 */
export interface PaginatedResponse<T> {
  /** Array of items for current page */
  items: T[];
  /** Pagination metadata */
  pagination: PaginationMetadata;
}

/**
 * Pagination metadata.
 */
export interface PaginationMetadata {
  /** Current page number (1-indexed) */
  page: number;
  /** Items per page */
  pageSize: number;
  /** Total number of items */
  totalItems: number;
  /** Total number of pages */
  totalPages: number;
  /** Whether there is a next page */
  hasNextPage: boolean;
  /** Whether there is a previous page */
  hasPreviousPage: boolean;
}

/**
 * Create pagination metadata from request and total count.
 */
export function createPaginationMetadata(
  page: number,
  pageSize: number,
  totalItems: number
): PaginationMetadata {
  const totalPages = Math.ceil(totalItems / pageSize);
  return {
    page,
    pageSize,
    totalItems,
    totalPages,
    hasNextPage: page < totalPages,
    hasPreviousPage: page > 1,
  };
}

/**
 * Create a paginated response.
 */
export function paginate<T>(
  items: T[],
  page: number,
  pageSize: number,
  totalItems: number
): PaginatedResponse<T> {
  return {
    items,
    pagination: createPaginationMetadata(page, pageSize, totalItems),
  };
}

/**
 * Default pagination values.
 */
export const DEFAULT_PAGINATION = {
  page: 1,
  pageSize: 20,
  maxPageSize: 100,
} as const;

/**
 * Normalize pagination request with defaults and limits.
 */
export function normalizePagination(request: PaginationRequest): Required<Pick<PaginationRequest, 'page' | 'pageSize'>> {
  const page = Math.max(1, request.page ?? DEFAULT_PAGINATION.page);
  const pageSize = Math.min(
    DEFAULT_PAGINATION.maxPageSize,
    Math.max(1, request.pageSize ?? DEFAULT_PAGINATION.pageSize)
  );
  return { page, pageSize };
}
