# Mystira App API

Main backend API for the Mystira platform. Handles user accounts, game sessions, scenarios, badges, media, and COPPA compliance.

## Base URL

```
Production: https://api.mystira.app/api
Staging:    https://dev.api.mystira.app/api
Development: http://localhost:5001/api
```

## Authentication

All endpoints require JWT authentication unless otherwise noted.

```
Authorization: Bearer <jwt-token>
```

## Endpoints

### Accounts

#### Get Current Account

```http
GET /api/accounts/me
```

Returns the authenticated user's account details.

#### Update Account

```http
PATCH /api/accounts/me
```

#### Get Account By ID

```http
GET /api/accounts/{id}
```

---

### Auth

#### Magic Link Authentication

```http
POST /api/auth/magic/request
```

Request email for magic link login.

```http
POST /api/auth/magic/verify
```

Verify magic link token and get JWT.

#### Refresh Token

```http
POST /api/auth/refresh
```

Refresh an expired JWT token.

---

### User Profiles

#### Get Profile

```http
GET /api/userprofiles/{userId}
```

#### Update Profile

```http
PATCH /api/userprofiles/{userId}
```

---

### Game Sessions

#### List Sessions

```http
GET /api/gamesessions
```

Query parameters:

- `page` - Page number
- `pageSize` - Items per page
- `status` - Filter by status

#### Create Session

```http
POST /api/gamesessions
```

#### Get Session

```http
GET /api/gamesessions/{id}
```

#### Update Session

```http
PATCH /api/gamesessions/{id}
```

#### Delete Session

```http
DELETE /api/gamesessions/{id}
```

---

### Scenarios

#### List Scenarios

```http
GET /api/scenarios
```

Query parameters:

- `page` - Page number
- `pageSize` - Items per page
- `search` - Search text
- `ageGroup` - Filter by age group
- `genre` - Filter by genre

#### Get Scenario

```http
GET /api/scenarios/{id}
```

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

#### Award Badge

```http
POST /api/badges/{id}/award
```

---

### User Badges

#### Get User Badges

```http
GET /api/userbadges
```

#### Get Badges for User

```http
GET /api/userbadges/user/{userId}
```

---

### Profile Axis Scores

#### Get Axis Scores

```http
GET /api/profileaxisscores
```

#### Update Axis Scores

```http
PATCH /api/profileaxisscores
```

---

### Royalties

#### Get Royalty Info

```http
GET /api/royalties/{userId}
```

---

### Media

#### Upload Media

```http
POST /api/media/upload
```

#### Get Media

```http
GET /api/media/{id}
```

#### Delete Media

```http
DELETE /api/media/{id}
```

---

### Compass Axes

#### List Axes

```http
GET /api/compassaxes
```

#### Get Axis

```http
GET /api/compassaxes/{id}
```

---

### Age Groups

#### List Age Groups

```http
GET /api/agegroups
```

#### Get Age Group

```http
GET /api/agegroups/{id}
```

---

### Archetypes

#### List Archetypes

```http
GET /api/archetypes
```

#### Get Archetype

```http
GET /api/archetypes/{id}
```

---

### Avatars

#### List Avatars

```http
GET /api/avatars
```

#### Get Avatar

```http
GET /api/avatars/{id}
```

---

### Bundles

#### List Bundles

```http
GET /api/bundles
```

#### Get Bundle

```http
GET /api/bundles/{id}
```

---

### COPPA Compliance

#### Age Check

```http
POST /api/coppa/age-check
```

Submit age for COPPA classification.

#### Request Parental Consent

```http
POST /api/coppa/consent/request
```

Request parental consent for a user.

#### Verify Parental Consent

```http
POST /api/coppa/consent/verify
```

Verify parental consent token.

#### Revoke Consent

```http
POST /api/coppa/consent/revoke
```

Revoke parental consent.

#### Get Consent Status

```http
GET /api/coppa/consent/status/{userId}
```

---

### Health

```http
GET /api/health
```

Returns health status of the API and its dependencies.

---

### Discord

#### Link Discord Account

```http
POST /api/discord/link
```

#### Get Discord Status

```http
GET /api/discord/status
```

---

## Authorization

Default: All endpoints require authentication.

| Policy         | Roles         | Description             |
| -------------- | ------------- | ----------------------- |
| Default        | Authenticated | All authenticated users |
| ReadOnly       | Viewer+       | Read access             |
| CanModerate    | Moderator+    | Content moderation      |
| AdminOnly      | Admin+        | Admin operations        |
| SuperAdminOnly | SuperAdmin    | Dangerous operations    |

## Error Responses

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request",
    "details": [...]
  }
}
```

## Rate Limits

- Auth endpoints: 10 requests/minute
- General endpoints: 60 requests/minute
