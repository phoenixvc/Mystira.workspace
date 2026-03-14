# Mystira API Documentation

## Overview

Mystira is a platform for AI-powered interactive storytelling. This documentation covers all backend APIs.

## APIs

| API                                          | Description              | Base URL                |
| -------------------------------------------- | ------------------------ | ----------------------- |
| [App API](app-api.md)                        | Main platform API        | `api.mystira.app`       |
| [Admin API](admin-api.md)                    | Administrative functions | `admin.mystira.app`     |
| [Story Generator API](storygenerator-api.md) | AI story generation      | `storygen.mystira.app`  |
| [Publisher API](publisher-api.md)            | On-chain publishing      | `publisher.mystira.app` |

## Authentication

All APIs use JWT authentication. Include the token in the `Authorization` header:

```
Authorization: Bearer <jwt-token>
```

Tokens are issued by the Identity API.

### Obtaining Tokens

```http
POST /api/auth/magic/verify
```

See [Identity API](./identity-api.md) for details.

## Common Features

### Authorization Policies

| Policy           | Roles      | Description          |
| ---------------- | ---------- | -------------------- |
| `ReadOnly`       | Viewer+    | Read access          |
| `CanModerate`    | Moderator+ | Content moderation   |
| `AdminOnly`      | Admin+     | Admin operations     |
| `SuperAdminOnly` | SuperAdmin | Dangerous operations |

### Error Format

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": [...]
  }
}
```

### Rate Limiting

Rate limits vary by endpoint:

- Auth endpoints: 10 requests/minute
- Read endpoints: 60 requests/minute
- Write endpoints: 30 requests/minute

## Development

### Running Locally

```bash
# App API
dotnet run --project apps/app/src/Mystira.App.Api

# Admin API
dotnet run --project apps/admin/api/src/Mystira.Admin.Api

# Story Generator API
dotnet run --project apps/story-generator/src/Mystira.StoryGenerator.Api

# Publisher API
dotnet run --project apps/publisher/api/src/Mystira.Publisher.Api
```

### Configuration

See [Configuration Guide](../configuration.md) for environment variables.
