// Application constants

// Query configuration
export const QUERY_STALE_TIME = 1000 * 60 * 5; // 5 minutes
export const QUERY_GC_TIME = 1000 * 60 * 10; // 10 minutes (garbage collection)
export const QUERY_RETRY = 1;

// Notification polling
export const NOTIFICATION_POLL_INTERVAL = 30000; // 30 seconds

// Royalty splits
export const MAX_ROYALTY_SPLIT = 100;
export const MIN_ROYALTY_SPLIT = 0;

// Pagination
export const DEFAULT_PAGE_SIZE = 20;
export const MAX_PAGE_SIZE = 100;

// API timeouts
export const API_TIMEOUT = 30000; // 30 seconds

// Token expiration buffer (refresh before actual expiration)
export const TOKEN_REFRESH_BUFFER = 60000; // 1 minute

// Debounce delays
export const SEARCH_DEBOUNCE_DELAY = 300;
export const INPUT_DEBOUNCE_DELAY = 500;

// File upload limits
export const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB
export const ALLOWED_FILE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'application/pdf'];

// Story limits
export const MAX_STORY_TITLE_LENGTH = 200;
export const MAX_STORY_SUMMARY_LENGTH = 2000;
export const MIN_STORY_SUMMARY_LENGTH = 10;

// Role request limits
export const MAX_MESSAGE_LENGTH = 1000;
export const MAX_PORTFOLIO_URL_LENGTH = 500;

