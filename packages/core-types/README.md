# @mystira/core-types

Core TypeScript types for the Mystira platform. Mirrors patterns from `Mystira.Shared` C# package.

## Installation

```bash
pnpm add @mystira/core-types
```

## Usage

### Result Pattern

```typescript
import { ok, err, isOk, unwrap, type Result } from '@mystira/core-types';

function divide(a: number, b: number): Result<number> {
  if (b === 0) {
    return err(validationError('Cannot divide by zero'));
  }
  return ok(a / b);
}

const result = divide(10, 2);
if (isOk(result)) {
  console.log(result.value); // 5
}
```

### Error Types

```typescript
import {
  validationError,
  notFoundError,
  toErrorResponse,
  type ErrorResponse,
} from '@mystira/core-types';

// Create typed errors
const error = notFoundError('Account', '123');

// Convert to API response
const response = toErrorResponse(error);
// { status: 404, code: 'NOT_FOUND', ... }
```

### Entity Types

```typescript
import type { AuditableEntity, DatabaseTarget } from '@mystira/core-types';

interface Account extends AuditableEntity {
  email: string;
  displayName: string;
}
```

### Pagination

```typescript
import { paginate, normalizePagination, type PaginatedResponse } from '@mystira/core-types';

const items = await fetchAccounts(page, pageSize);
const response: PaginatedResponse<Account> = paginate(items, page, pageSize, totalCount);
```

## Types

| Type | Description |
|------|-------------|
| `Result<T, E>` | Success/failure result type |
| `MystiraError` | Base error interface |
| `ErrorResponse` | RFC 7807 Problem Details format |
| `Entity` | Base entity with ID |
| `AuditableEntity` | Entity with audit fields |
| `PaginatedResponse<T>` | Paginated response wrapper |

## Related

- [Mystira.Shared](../shared/Mystira.Shared/) - C# shared infrastructure
- [OpenAPI Specs](../api-spec/) - API specifications
