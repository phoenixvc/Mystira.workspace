# API Documentation

This document describes the API client architecture and how to interact with the Mystira Admin API from the frontend.

## Overview

The Mystira Admin UI communicates with the Mystira Admin API via RESTful HTTP requests. All API clients are located in the `src/api/` directory and use Axios for HTTP communication.

## API Client Architecture

### Base Configuration

All API clients share a common Axios instance configured in `src/api/axios.ts`:

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  withCredentials: true, // Include cookies for authentication
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor for adding auth tokens
api.interceptors.request.use((config) => {
  // Add any custom headers here
  return config;
});

// Response interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // Handle errors globally
    return Promise.reject(error);
  }
);

export default api;
```

### API Client Structure

Each domain has its own API client module:

```
src/api/
├── auth.ts          # Authentication
├── scenarios.ts     # Scenarios CRUD
├── media.ts         # Media management
├── badges.ts        # Badges CRUD
├── bundles.ts       # Bundles CRUD
├── avatars.ts       # Avatars management
├── masterData.ts    # Master data CRUD
└── ...
```

## Authentication API

### Login

Authenticate a user and receive a session cookie.

```typescript
import { login } from './api/auth';

const credentials = {
  username: 'admin',
  password: 'password123',
};

try {
  const user = await login(credentials);
  console.log('Logged in as:', user.username);
} catch (error) {
  console.error('Login failed:', error);
}
```

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "username": "string",
  "password": "string"
}
```

**Response**:
```json
{
  "id": "string",
  "username": "string",
  "email": "string",
  "role": "string"
}
```

### Logout

End the current session.

```typescript
import { logout } from './api/auth';

await logout();
```

**Endpoint**: `POST /api/auth/logout`

### Get Current User

Retrieve information about the currently authenticated user.

```typescript
import { getCurrentUser } from './api/auth';

const user = await getCurrentUser();
```

**Endpoint**: `GET /api/auth/me`

## Scenarios API

### List Scenarios

Retrieve a paginated list of scenarios with optional search.

```typescript
import { getScenarios } from './api/scenarios';

const params = {
  page: 1,
  pageSize: 20,
  search: 'adventure',
};

const result = await getScenarios(params);
console.log('Total:', result.total);
console.log('Scenarios:', result.items);
```

**Endpoint**: `GET /api/admin/scenarios`

**Query Parameters**:
- `page` (number): Page number (1-indexed)
- `pageSize` (number): Items per page
- `search` (string, optional): Search query

**Response**:
```json
{
  "items": [
    {
      "id": "string",
      "name": "string",
      "description": "string",
      "version": "string",
      "createdAt": "string",
      "updatedAt": "string"
    }
  ],
  "total": 100,
  "page": 1,
  "pageSize": 20
}
```

### Get Scenario by ID

Retrieve a single scenario by its ID.

```typescript
import { getScenarioById } from './api/scenarios';

const scenario = await getScenarioById('scenario-id');
```

**Endpoint**: `GET /api/admin/scenarios/:id`

### Create Scenario

Create a new scenario.

```typescript
import { createScenario } from './api/scenarios';

const data = {
  name: 'New Adventure',
  description: 'An exciting adventure',
  version: '1.0.0',
  // ... other scenario fields
};

const scenario = await createScenario(data);
```

**Endpoint**: `POST /api/admin/scenarios`

### Update Scenario

Update an existing scenario.

```typescript
import { updateScenario } from './api/scenarios';

const updates = {
  name: 'Updated Adventure',
  description: 'An even more exciting adventure',
};

const scenario = await updateScenario('scenario-id', updates);
```

**Endpoint**: `PUT /api/admin/scenarios/:id`

### Delete Scenario

Delete a scenario.

```typescript
import { deleteScenario } from './api/scenarios';

await deleteScenario('scenario-id');
```

**Endpoint**: `DELETE /api/admin/scenarios/:id`

### Import Scenario

Import a scenario from a YAML or JSON file.

```typescript
import { importScenario } from './api/scenarios';

const file = new File([content], 'scenario.yaml', { type: 'text/yaml' });
const scenario = await importScenario(file);
```

**Endpoint**: `POST /api/admin/scenarios/import`

**Content-Type**: `multipart/form-data`

### Validate Scenario

Validate all media references in scenarios.

```typescript
import { validateScenarios } from './api/scenarios';

const scenarioIds = ['id1', 'id2', 'id3'];
const results = await validateScenarios(scenarioIds);

results.forEach(result => {
  console.log(`Scenario ${result.scenarioId}: ${result.valid ? 'Valid' : 'Invalid'}`);
  if (!result.valid) {
    console.log('Errors:', result.errors);
  }
});
```

**Endpoint**: `POST /api/admin/scenarios/validate`

**Request Body**:
```json
{
  "scenarioIds": ["string"]
}
```

## Media API

### List Media

Retrieve a paginated list of media items.

```typescript
import { getMedia } from './api/media';

const params = {
  page: 1,
  pageSize: 20,
  type: 'image', // 'image', 'audio', 'video'
};

const result = await getMedia(params);
```

**Endpoint**: `GET /api/admin/media`

### Upload Media

Upload a single media file.

```typescript
import { uploadMedia } from './api/media';

const file = new File([blob], 'image.png', { type: 'image/png' });
const metadata = {
  title: 'My Image',
  description: 'A beautiful image',
  tags: ['nature', 'landscape'],
};

const media = await uploadMedia(file, metadata);
```

**Endpoint**: `POST /api/admin/media/upload`

**Content-Type**: `multipart/form-data`

### Upload Media ZIP

Upload multiple media files with metadata from a ZIP file.

```typescript
import { uploadMediaZip } from './api/media';

const zipFile = new File([zipBlob], 'media.zip', { type: 'application/zip' });
const options = {
  overwriteMetadata: true,
  overwriteMedia: false,
};

const result = await uploadMediaZip(zipFile, options);
console.log('Uploaded:', result.uploaded);
console.log('Failed:', result.failed);
```

**Endpoint**: `POST /api/admin/media/upload-zip`

**Content-Type**: `multipart/form-data`

**ZIP Structure**:
```
media.zip
├── media-metadata.json
├── image1.png
├── image2.jpg
└── video1.mp4
```

**media-metadata.json Format**:
```json
{
  "media-id-1": {
    "filename": "image1.png",
    "title": "Image 1",
    "description": "Description",
    "tags": ["tag1", "tag2"]
  },
  "media-id-2": {
    "filename": "image2.jpg",
    "title": "Image 2"
  }
}
```

## Badges API

### List Badges

Retrieve a paginated list of badges.

```typescript
import { getBadges } from './api/badges';

const result = await getBadges({ page: 1, pageSize: 20 });
```

**Endpoint**: `GET /api/admin/badges`

### Create Badge

Create a new badge.

```typescript
import { createBadge } from './api/badges';

const data = {
  name: 'Achievement',
  description: 'Complete 10 scenarios',
  imageId: 'media-id',
};

const badge = await createBadge(data);
```

**Endpoint**: `POST /api/admin/badges`

### Update Badge

Update an existing badge.

```typescript
import { updateBadge } from './api/badges';

const updates = {
  name: 'Updated Achievement',
  description: 'Complete 20 scenarios',
};

const badge = await updateBadge('badge-id', updates);
```

**Endpoint**: `PUT /api/admin/badges/:id`

### Delete Badge

Delete a badge.

```typescript
import { deleteBadge } from './api/badges';

await deleteBadge('badge-id');
```

**Endpoint**: `DELETE /api/admin/badges/:id`

## Bundles API

### List Bundles

Retrieve a paginated list of bundles.

```typescript
import { getBundles } from './api/bundles';

const result = await getBundles({ page: 1, pageSize: 20 });
```

**Endpoint**: `GET /api/admin/bundles`

### Create Bundle

Create a new bundle.

```typescript
import { createBundle } from './api/bundles';

const data = {
  name: 'Adventure Pack',
  description: 'Collection of adventure scenarios',
  version: '1.0.0',
};

const bundle = await createBundle(data);
```

**Endpoint**: `POST /api/admin/bundles`

### Update Bundle

Update an existing bundle.

```typescript
import { updateBundle } from './api/bundles';

const updates = {
  name: 'Updated Adventure Pack',
  version: '1.1.0',
};

const bundle = await updateBundle('bundle-id', updates);
```

**Endpoint**: `PUT /api/admin/bundles/:id`

### Delete Bundle

Delete a bundle.

```typescript
import { deleteBundle } from './api/bundles';

await deleteBundle('bundle-id');
```

**Endpoint**: `DELETE /api/admin/bundles/:id`

## Avatars API

### Get Avatars by Age Group

Retrieve avatars for a specific age group.

```typescript
import { getAvatarsByAgeGroup } from './api/avatars';

const avatars = await getAvatarsByAgeGroup('teen');
// Returns array of media IDs
```

**Endpoint**: `GET /api/admin/avatars/:ageGroup`

### Update Avatars for Age Group

Update the list of avatars for an age group.

```typescript
import { updateAvatarsForAgeGroup } from './api/avatars';

const mediaIds = ['media-id-1', 'media-id-2', 'media-id-3'];
await updateAvatarsForAgeGroup('teen', mediaIds);
```

**Endpoint**: `PUT /api/admin/avatars/:ageGroup`

**Request Body**:
```json
{
  "mediaIds": ["string"]
}
```

## Error Handling

### Error Response Format

All API errors follow a consistent format:

```json
{
  "message": "Error message",
  "code": "ERROR_CODE",
  "status": 400,
  "details": {
    "field": "Additional error details"
  }
}
```

### Handling Errors

Use the error handler utility for consistent error handling:

```typescript
import { handleApiError } from '../utils/errorHandler';

try {
  await createScenario(data);
  showToast.success('Scenario created successfully');
} catch (error) {
  handleApiError(error, 'Failed to create scenario');
}
```

### Common Error Codes

| Status | Code | Description |
|--------|------|-------------|
| 400 | BAD_REQUEST | Invalid request data |
| 401 | UNAUTHORIZED | Authentication required |
| 403 | FORBIDDEN | Insufficient permissions |
| 404 | NOT_FOUND | Resource not found |
| 409 | CONFLICT | Resource already exists |
| 422 | VALIDATION_ERROR | Validation failed |
| 500 | INTERNAL_ERROR | Server error |

## Best Practices

### Use React Query

Wrap API calls in React Query hooks for automatic caching and state management:

```typescript
import { useQuery } from '@tanstack/react-query';
import { getScenarios } from '../api/scenarios';

function useScenarios(params) {
  return useQuery({
    queryKey: ['scenarios', params],
    queryFn: () => getScenarios(params),
  });
}
```

### Handle Loading States

Always handle loading and error states:

```typescript
const { data, isLoading, error } = useScenarios({ page: 1 });

if (isLoading) return <LoadingSpinner />;
if (error) return <ErrorAlert error={error} />;

return <ScenarioList scenarios={data.items} />;
```

### Invalidate Cache

Invalidate queries after mutations:

```typescript
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createScenario } from '../api/scenarios';

function useCreateScenario() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createScenario,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['scenarios'] });
    },
  });
}
```

### Type Safety

Always define TypeScript types for API responses:

```typescript
interface Scenario {
  id: string;
  name: string;
  description: string;
  version: string;
  createdAt: string;
  updatedAt: string;
}

interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

async function getScenarios(params): Promise<PaginatedResponse<Scenario>> {
  const response = await api.get('/api/admin/scenarios', { params });
  return response.data;
}
```

## Testing API Clients

### Mock API Responses

Use MSW (Mock Service Worker) for testing:

```typescript
import { rest } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer(
  rest.get('/api/admin/scenarios', (req, res, ctx) => {
    return res(
      ctx.json({
        items: [{ id: '1', name: 'Test Scenario' }],
        total: 1,
        page: 1,
        pageSize: 20,
      })
    );
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### Test Error Handling

Verify that errors are handled correctly:

```typescript
it('handles API errors', async () => {
  server.use(
    rest.get('/api/admin/scenarios', (req, res, ctx) => {
      return res(ctx.status(500), ctx.json({ message: 'Server error' }));
    })
  );

  await expect(getScenarios({ page: 1 })).rejects.toThrow();
});
```

## Conclusion

The API client architecture provides a clean, type-safe interface for communicating with the backend. By following these patterns and best practices, you can build robust features that handle errors gracefully and provide excellent user experience.
