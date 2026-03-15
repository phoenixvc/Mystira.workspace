# Mystira Admin API

Administrative backend API for managing the Mystira platform. Handles user management, content moderation, data migration, and system configuration.

## Base URL

```
Production: https://admin.mystira.app/api
Staging:    https://dev.admin.mystira.app/api
Development: http://localhost:5002/api
```

## Authentication

All endpoints require JWT authentication with appropriate roles.

```
Authorization: Bearer <jwt-token>
```

## Authorization Policies

| Policy           | Roles                                | Description          |
| ---------------- | ------------------------------------ | -------------------- |
| `ReadOnly`       | Viewer, Moderator, Admin, SuperAdmin | Read access          |
| `CanModerate`    | Moderator, Admin, SuperAdmin         | Content moderation   |
| `AdminOnly`      | Admin, SuperAdmin                    | Admin operations     |
| `SuperAdminOnly` | SuperAdmin                           | Dangerous operations |

---

## Endpoints

### User Management

#### List Users

```http
GET /api/users
```

Query parameters:

- `page` - Page number
- `pageSize` - Items per page
- `search` - Search by name/email
- `role` - Filter by role

Requires: `AdminOnly`

#### Get User

```http
GET /api/users/{id}
```

#### Update User

```http
PATCH /api/users/{id}
```

Requires: `AdminOnly`

#### Delete User

```http
DELETE /api/users/{id}
```

Requires: `SuperAdminOnly`

---

### User Profiles Admin

#### List Profiles

```http
GET /api/userprofilesadmin
```

#### Get Profile

```http
GET /api/userprofilesadmin/{userId}
```

#### Update Profile

```http
PATCH /api/userprofilesadmin/{userId}
```

---

### Scenarios

#### List Scenarios (Admin)

```http
GET /api/scenarios
```

Query parameters:

- `page`, `pageSize` - Pagination
- `status` - Filter by status
- `genre` - Filter by genre

#### Get Scenario

```http
GET /api/scenarios/{id}
```

#### Create Scenario

```http
POST /api/scenarios
```

Requires: `AdminOnly`

#### Update Scenario

```http
PATCH /api/scenarios/{id}
```

Requires: `AdminOnly`

#### Delete Scenario

```http
DELETE /api/scenarios/{id}
```

Requires: `SuperAdminOnly`

#### Import Scenarios

```http
POST /api/scenarios/import
```

Requires: `AdminOnly`

---

### Badges

#### List Badges

```http
GET /api/badges
```

#### Get Badge

```http
GET /api/badges/{id}
```

#### Create Badge

```http
POST /api/badges
```

Requires: `AdminOnly`

#### Update Badge

```http
PATCH /api/badges/{id}
```

Requires: `AdminOnly`

#### Delete Badge

```http
DELETE /api/badges/{id}
```

Requires: `SuperAdminOnly`

---

### Badge Images

#### Upload Badge Image

```http
POST /api/badgeimages/upload
```

Requires: `AdminOnly`

#### Get Badge Image

```http
GET /api/badgeimages/{id}
```

---

### Media Management

#### Upload Media

```http
POST /api/media/upload
```

Requires: `CanModerate`

#### List Media

```http
GET /api/media
```

Query parameters:

- `page`, `pageSize`
- `type` - Filter by media type

#### Get Media

```http
GET /api/media/{id}
```

#### Delete Media

```http
DELETE /api/media/{id}
```

Requires: `CanModerate`

---

### Character Maps

#### List Character Maps

```http
GET /api/characterMaps
```

#### Get Character Map

```http
GET /api/characterMaps/{id}
```

#### Create Character Map

```http
POST /api/characterMaps
```

Requires: `AdminOnly`

---

### Character Media Metadata

#### List Metadata

```http
GET /api/characterMediaMetadata
```

#### Update Metadata

```http
PATCH /api/characterMediaMetadata/{id}
```

Requires: `CanModerate`

---

### Avatars

#### List Avatars

```http
GET /api/avatars
```

#### Create Avatar

```http
POST /api/avatars
```

Requires: `AdminOnly`

---

### Archetypes

#### List Archetypes

```http
GET /api/archetypes
```

#### Create Archetype

```http
POST /api/archetypes
```

Requires: `AdminOnly`

---

### Compass Axes

#### List Axes

```http
GET /api/compassaxes
```

#### Create Axis

```http
POST /api/compassaxes
```

Requires: `AdminOnly`

---

### Age Groups

#### List Age Groups

```http
GET /api/agegroups
```

#### Create Age Group

```http
POST /api/agegroups
```

Requires: `AdminOnly`

---

### Fantasy Themes

#### List Themes

```http
GET /api/fantasyThemes
```

#### Create Theme

```http
POST /api/fantasyThemes
```

Requires: `AdminOnly`

---

### Echo Types

#### List Types

```http
GET /api/echoTypes
```

#### Create Type

```http
POST /api/echoTypes
```

Requires: `AdminOnly`

---

### Content Bundles

#### List Bundles

```http
GET /api/bundles
```

#### Get Bundle

```http
GET /api/bundles/{id}
```

#### Create Bundle

```http
POST /api/bundles
```

Requires: `AdminOnly`

#### Update Bundle

```http
PATCH /api/bundles/{id}
```

Requires: `AdminOnly`

#### Import Bundle

```http
POST /api/bundles/import
```

Requires: `AdminOnly`

---

### Game Sessions (Admin)

#### List Sessions

```http
GET /api/gamesessions
```

#### Get Session

```http
GET /api/gamesessions/{id}
```

#### Delete Session

```http
DELETE /api/gamesessions/{id}
```

Requires: `CanModerate`

---

### Contributors (Admin)

#### List Contributors

```http
GET /api/contributors
```

#### Get Contributor

```http
GET /api/contributors/{id}
```

#### Update Contributor

```http
PATCH /api/contributors/{id}
```

Requires: `CanModerate`

---

### Migration Status

#### Get Migration Status

```http
GET /api/migration/status
```

#### Trigger Migration

```http
POST /api/migration/trigger
```

Requires: `SuperAdminOnly`

---

### API Info

```http
GET /api/info
```

Returns API version and status information.

---

### Health Check

```http
GET /api/health
```

Returns health status of API and dependencies (Cosmos DB, PostgreSQL, Redis, Discord).

---

## Error Responses

```json
{
  "error": {
    "code": "FORBIDDEN",
    "message": "Insufficient permissions"
  }
}
```

## Rate Limits

- General endpoints: 60 requests/minute
- Write operations: 30 requests/minute
- Admin-only operations: 10 requests/minute
