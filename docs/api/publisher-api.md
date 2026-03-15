# Mystira Publisher API

Backend API for the Mystira.Publisher frontend application. Handles story management, contributor attribution, royalty splits, and on-chain publishing via Story Protocol.

## Base URL

```
Production: https://publisher.mystira.app/api
Staging:    https://dev.publisher.mystira.app/api
Development: http://localhost:5005/api
```

## Authentication

All endpoints require JWT authentication. Include the token in the `Authorization` header:

```
Authorization: Bearer <jwt-token>
```

### Obtaining a Token

Tokens are obtained from the Identity API. See [Identity API Documentation](./identity-api.md).

## Endpoints

### Stories

#### List Stories

```http
GET /api/stories
```

Query Parameters:
| Parameter | Type | Description |
|-----------|------|-------------|
| `status` | string | Filter by status (draft, pending_approval, approved, published) |
| `page` | integer | Page number (default: 1) |
| `limit` | integer | Items per page (default: 20) |

Response:

```json
{
  "data": [
    {
      "id": "story-123",
      "title": "The Enchanted Forest",
      "description": "A magical adventure...",
      "status": "draft",
      "createdAt": "2026-01-15T10:30:00Z",
      "updatedAt": "2026-01-15T10:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 100
  }
}
```

#### Get Story

```http
GET /api/stories/{id}
```

Response:

```json
{
  "id": "story-123",
  "title": "The Enchanted Forest",
  "description": "A magical adventure...",
  "status": "draft",
  "content": "...",
  "ipfsCid": null,
  "transactionHash": null,
  "contributors": [
    {
      "userId": "user-1",
      "role": "author",
      "royaltyShare": 50,
      "approved": true
    }
  ],
  "createdAt": "2026-01-15T10:30:00Z",
  "updatedAt": "2026-01-15T10:30:00Z"
}
```

#### Create Story

```http
POST /api/stories
```

Request:

```json
{
  "title": "The Enchanted Forest",
  "description": "A magical adventure...",
  "content": "Full story content here..."
}
```

#### Update Story

```http
PATCH /api/stories/{id}
```

Request:

```json
{
  "title": "Updated Title",
  "description": "Updated description..."
}
```

#### Delete Story

```http
DELETE /api/stories/{id}
```

---

### Contributors

#### Get Contributors by Story

```http
GET /api/contributors/story/{storyId}
```

Response:

```json
[
  {
    "id": "attr-123",
    "userId": "user-1",
    "userName": "John Author",
    "userEmail": "john@example.com",
    "role": "author",
    "royaltyShare": 50,
    "approvalStatus": "approved",
    "approvedAt": "2026-01-15T10:30:00Z",
    "addedAt": "2026-01-10T08:00:00Z"
  }
]
```

#### Add Contributor

```http
POST /api/contributors
```

Request:

```json
{
  "storyId": "story-123",
  "userId": "user-2",
  "role": "illustrator",
  "royaltyShare": 30
}
```

#### Update Contributor

```http
PATCH /api/contributors/{id}
```

Request:

```json
{
  "role": "author",
  "royaltyShare": 60
}
```

#### Remove Contributor

```http
DELETE /api/contributors/{id}
```

#### Submit Approval

```http
POST /api/contributors/approve
```

Request:

```json
{
  "storyId": "story-123",
  "approved": true,
  "comment": "Great story!"
}
```

Response:

```json
{
  "id": "attr-123",
  "approvalStatus": "approved",
  "approvedAt": "2026-01-15T10:30:00Z"
}
```

#### Override Contributor

Used when a contributor is non-responsive. Requires Admin or SuperAdmin role.

```http
POST /api/contributors/override
```

Request:

```json
{
  "storyId": "story-123",
  "contributorId": "attr-456",
  "reason": "No response after 3 attempts"
}
```

#### Validate Royalty Splits

```http
GET /api/contributors/validate/{storyId}
```

Response:

```json
{
  "valid": true,
  "totalShare": 100,
  "message": "Royalty splits sum to 100%"
}
```

If invalid:

```json
{
  "valid": false,
  "totalShare": 130,
  "message": "Royalty splits exceed 100%"
}
```

---

### Users

#### Search Users

```http
GET /api/users/search
```

Query Parameters:
| Parameter | Type | Description |
|-----------|------|-------------|
| `query` | string | Search by name or email |
| `limit` | integer | Max results (default: 10) |

Response:

```json
[
  {
    "id": "user-123",
    "name": "John Author",
    "email": "john@example.com",
    "avatarUrl": "https://..."
  }
]
```

---

## Authorization Policies

| Policy        | Required Role                | Endpoints            |
| ------------- | ---------------------------- | -------------------- |
| Default       | Authenticated                | All above            |
| `AdminOnly`   | Admin, SuperAdmin            | Override contributor |
| `CanModerate` | Moderator, Admin, SuperAdmin | (future)             |

## Error Responses

All errors follow this format:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request",
    "details": [
      {
        "field": "title",
        "message": "Title is required"
      }
    ]
  }
}
```

### Common Error Codes

| Code               | Description                      |
| ------------------ | -------------------------------- |
| `UNAUTHORIZED`     | Missing or invalid token         |
| `FORBIDDEN`        | Insufficient permissions         |
| `NOT_FOUND`        | Resource not found               |
| `VALIDATION_ERROR` | Invalid request data             |
| `CONFLICT`         | Resource already exists          |
| `ROYALTY_INVALID`  | Royalty splits don't sum to 100% |

## Rate Limiting

- Auth endpoints: 10 requests/minute
- General endpoints: 60 requests/minute

## Webhooks

When a story is published to the blockchain, webhooks are sent to configured endpoints.

```json
{
  "event": "story.published",
  "data": {
    "storyId": "story-123",
    "ipfsCid": "Qm...",
    "transactionHash": "0x...",
    "publishedAt": "2026-01-15T10:30:00Z"
  }
}
```
