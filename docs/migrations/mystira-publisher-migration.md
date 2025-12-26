# Mystira.Publisher Migration Guide

**Target**: Migrate Publisher to use `@mystira/core-types` and subscribe to platform events
**Prerequisites**: `@mystira/core-types` v0.2.0-alpha published to NPM
**Estimated Effort**: 0.5-1 day
**Last Updated**: December 2025
**Status**: 📋 Planned

---

## Overview

Publisher is a React/TypeScript frontend. Migration focuses on:

1. Adopting `@mystira/core-types` for shared type definitions
2. Subscribing to platform events via WebSocket/SignalR
3. Proper error handling with shared error types
4. **Dockerfile migration** to submodule repo (ADR-0019)

---

## Phase 1: Install Core Types

### 1.1 Add Package

```bash
cd packages/publisher
pnpm add @mystira/core-types@0.2.0-alpha
```

### 1.2 Update package.json

```json
{
  "dependencies": {
    "@mystira/core-types": "^0.2.0-alpha"
  }
}
```

---

## Phase 2: Use Shared Types

### 2.1 Result Pattern

```typescript
import { Result, ok, err, isOk } from '@mystira/core-types';

async function fetchScenario(id: string): Promise<Result<Scenario>> {
  try {
    const response = await api.get(`/scenarios/${id}`);
    return ok(response.data);
  } catch (error) {
    return err({ code: 'NOT_FOUND', message: 'Scenario not found' });
  }
}

// Usage
const result = await fetchScenario('123');
if (isOk(result)) {
  console.log(result.value);
} else {
  console.error(result.error);
}
```

### 2.2 Error Types

```typescript
import { ErrorResponse, MystiraError, ErrorCode } from '@mystira/core-types';

// Handle API errors
function handleApiError(response: ErrorResponse) {
  switch (response.code) {
    case 'VALIDATION':
      showValidationErrors(response.errors);
      break;
    case 'NOT_FOUND':
      navigate('/404');
      break;
    case 'UNAUTHORIZED':
      redirectToLogin();
      break;
    default:
      showGenericError(response.detail);
  }
}
```

### 2.3 Pagination

```typescript
import { PaginatedResponse, PaginationParams } from '@mystira/core-types';

async function fetchScenarios(params: PaginationParams): Promise<PaginatedResponse<Scenario>> {
  const response = await api.get('/scenarios', { params });
  return response.data;
}
```

---

## Phase 3: Event Subscriptions

### 3.1 Event Types

```typescript
import {
  MystiraEvent,
  SessionStarted,
  SessionCompleted,
  ScenarioPublished,
} from '@mystira/core-types';

// Type-safe event handling
function handleEvent(event: MystiraEvent) {
  switch (event.type) {
    case 'SessionStarted':
      handleSessionStarted(event);
      break;
    case 'SessionCompleted':
      handleSessionCompleted(event);
      break;
    case 'ScenarioPublished':
      refreshScenarioList();
      break;
  }
}
```

### 3.2 WebSocket Connection (Future)

```typescript
// Connect to real-time events (when backend implements SignalR hub)
import { MystiraEvent } from '@mystira/core-types';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/events')
  .build();

connection.on('ReceiveEvent', (event: MystiraEvent) => {
  handleEvent(event);
});
```

---

## Phase 4: Dockerfile Migration (ADR-0019)

Move Dockerfile from workspace to submodule repo:

### 4.1 Create Dockerfile in Submodule

```dockerfile
# packages/publisher/Dockerfile (new location)
FROM node:22-alpine AS build

WORKDIR /app

COPY package.json pnpm-lock.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY . .
RUN pnpm build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

---

## Migration Checklist

### Pre-Migration
- [ ] Ensure @mystira/core-types is published
- [ ] Create feature branch

### Phase 1: Package Setup
- [ ] Install @mystira/core-types
- [ ] Verify build succeeds

### Phase 2: Type Adoption
- [ ] Replace local error types with shared
- [ ] Use Result pattern for async operations
- [ ] Update pagination types

### Phase 3: Events
- [ ] Add event type imports
- [ ] Prepare for real-time updates

### Phase 4: Dockerfile
- [ ] Move Dockerfile to submodule
- [ ] Update CI/CD workflow

---

## Notes

Publisher's migration is simpler than backend services since it primarily:
1. Consumes types (not produces)
2. Handles events (not publishes)
3. Uses REST APIs (already compatible)

---

## Related Documentation

- [ADR-0019: Dockerfile Location Standardization](../architecture/adr/ADR-0019-dockerfile-location-standardization.md)
- [@mystira/core-types](../../packages/core-types/README.md)
- [Mystira.App Migration Guide](./mystira-app-migration.md)
