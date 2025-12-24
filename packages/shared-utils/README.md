# @mystira/shared-utils

Shared utilities for Mystira platform packages.

## Overview

This package provides common utilities used across the Mystira platform, including:

- **Retry logic** with exponential backoff
- **Structured logging** utilities
- **Validation** helpers

> **Note**: This package was previously located in `packages/publisher`. It has been moved to workspace level as part of the [Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md).

## Installation

```bash
npm install @mystira/shared-utils
# or
pnpm add @mystira/shared-utils
# or
yarn add @mystira/shared-utils
```

## Usage

### Retry with Exponential Backoff

```typescript
import { withRetry, createRetryable } from '@mystira/shared-utils';

// One-time retry
const result = await withRetry(
  () => fetchData(),
  {
    maxAttempts: 3,
    initialDelayMs: 1000,
    isRetryable: (error) => error instanceof NetworkError,
    onRetry: (attempt, error, delay) => {
      console.log(`Retry ${attempt} after ${delay}ms`);
    },
  }
);

// Create a retryable function
const retryableFetch = createRetryable(fetchData, { maxAttempts: 3 });
const data = await retryableFetch();
```

### Structured Logging

```typescript
import { createLogger } from '@mystira/shared-utils';

const logger = createLogger({ level: 'info' });

logger.info('Request received', { requestId: '123', path: '/api/stories' });
logger.error('Failed to process', { error: 'timeout', requestId: '123' });

// Create child logger with base context
const childLogger = logger.child({ service: 'story-generator' });
childLogger.info('Processing story'); // Includes service context automatically
```

### Validation Helpers

```typescript
import {
  isDefined,
  isNonEmptyString,
  isValidEmail,
  validateRequired,
} from '@mystira/shared-utils';

// Type guards
if (isDefined(value)) {
  // value is not null or undefined
}

if (isNonEmptyString(name)) {
  // name is a non-empty string
}

// Validate required fields
const result = validateRequired(data, ['name', 'email', 'content']);
if (!result.valid) {
  console.log(result.errors);
}
```

## API Reference

### Retry Utilities

| Function | Description |
|----------|-------------|
| `withRetry(fn, options)` | Execute a function with retry logic |
| `createRetryable(fn, options)` | Create a retryable wrapper function |
| `sleep(ms)` | Promise-based sleep |
| `calculateBackoffDelay(...)` | Calculate exponential backoff delay |

### Logger Utilities

| Function | Description |
|----------|-------------|
| `createLogger(options)` | Create a structured logger |

### Validation Utilities

| Function | Description |
|----------|-------------|
| `isDefined(value)` | Check if value is not null/undefined |
| `isNonEmptyString(value)` | Check if value is a non-empty string |
| `isPositiveNumber(value)` | Check if value is a positive number |
| `isValidEmail(value)` | Check if value is a valid email format |
| `isValidUrl(value)` | Check if value is a valid URL |
| `validateRequired(obj, fields)` | Validate required fields on an object |

## Related Documentation

- [ADR-0020: Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md)
- [Package Releases Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/package-releases.md)

## License

MIT
