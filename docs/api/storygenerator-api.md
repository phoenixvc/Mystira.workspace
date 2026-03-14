# Mystira Story Generator API

Backend API for AI-powered story generation. Handles story creation, validation, chat interactions, continuity analysis, and schema management.

## Base URL

```
Production: https://storygen.mystira.app/api
Staging:    https://dev.storygen.mystira.app/api
Development: http://localhost:5003/api
```

## Authentication

All endpoints require JWT authentication.

```
Authorization: Bearer <jwt-token>
```

---

## Endpoints

### Stories

#### Validate Story

```http
POST /api/stories/validate
```

Validates story content against schema and quality criteria.

Request:

```json
{
  "storyContent": "The story text...",
  "genre": "fantasy",
  "ageGroup": "children"
}
```

Response:

```json
{
  "isValid": true,
  "issues": [],
  "score": 85
}
```

#### Generate Story

```http
POST /api/stories/generate
```

Generates a new story using AI.

Request:

```json
{
  "prompt": "A young wizard discovers...",
  "genre": "fantasy",
  "ageGroup": "children",
  "maxLength": 2000,
  "temperature": 0.7
}
```

Response:

```json
{
  "storyId": "story-123",
  "content": "Generated story...",
  "title": "The Young Wizard",
  "metadata": {
    "genre": "fantasy",
    "wordCount": 1850
  }
}
```

#### Get Story

```http
GET /api/stories/{id}
```

#### Update Story

```http
PATCH /api/stories/{id}
```

#### Delete Story

```http
DELETE /api/stories/{id}
```

#### Get Story Analysis

```http
GET /api/stories/{id}/analysis
```

Returns continuity and consistency analysis.

#### Preview Story

```http
POST /api/stories/preview
```

Generates a preview without saving.

---

### Chat

#### Send Message

```http
POST /api/chat
```

Interactive chat for continuing a story.

Request:

```json
{
  "storyId": "story-123",
  "message": "What happens next?",
  "context": {
    "currentPosition": 1500
  }
}
```

Response:

```json
{
  "response": "The wizard stepped forward...",
  "suggestions": [
    "Continue the adventure",
    "Add a new character",
    "Change the tone"
  ]
}
```

#### Get Chat History

```http
GET /api/chat/{storyId}
```

Returns chat history for a story.

---

### Story Continuity

#### Analyze Continuity

```http
POST /api/storycontinuity/analyze
```

Analyzes story for continuity issues.

Request:

```json
{
  "storyId": "story-123"
}
```

Response:

```json
{
  "issues": [
    {
      "type": "character_inconsistency",
      "description": "Character 'John' was described as having brown hair in chapter 1, but black hair in chapter 3",
      "severity": "warning",
      "location": "chapter 3"
    }
  ],
  "overallScore": 92
}
```

#### Get Character Timeline

```http
GET /api/storycontinuity/{storyId}/timeline/{characterId}
```

Returns character appearance timeline.

#### Fix Continuity Issues

```http
POST /api/storycontinuity/fix
```

AI-powered fix for continuity issues.

---

### Story Agent

#### Analyze Story

```http
POST /api/storyagent/analyze
```

Request:

```json
{
  "storyId": "story-123",
  "analysisType": "full"
}
```

Response:

```json
{
  "themes": ["friendship", "courage"],
  "sentiment": "positive",
  "complexity": "medium",
  "targetAudience": "children",
  "suggestions": [...]
}
```

#### Suggest Improvements

```http
POST /api/storyagent/suggest
```

AI suggestions for story improvement.

---

### Schema

#### Get Schemas

```http
GET /api/schema
```

List available story schemas.

#### Get Schema

```http
GET /api/schema/{id}
```

Get specific schema definition.

#### Validate Against Schema

```http
POST /api/schema/validate
```

Validate story content against a schema.

---

### Scenario Dominator Path Analysis

#### Analyze Paths

```http
POST /api/scenariodominatorpath/analyze
```

Analyzes story paths for dominance patterns.

---

## Error Responses

```json
{
  "error": {
    "code": "VALIDATION_FAILED",
    "message": "Story validation failed",
    "details": [...]
  }
}
```

### Error Codes

| Code                | Description                            |
| ------------------- | -------------------------------------- |
| `VALIDATION_FAILED` | Story doesn't meet schema requirements |
| `GENERATION_FAILED` | AI generation failed                   |
| `CONTINUITY_ERROR`  | Critical continuity issues             |
| `QUOTA_EXCEEDED`    | User story quota exceeded              |

## Rate Limits

- Story generation: 5 requests/minute
- Chat: 20 requests/minute
- Analysis: 10 requests/minute
- General: 60 requests/minute

## Webhooks

Configure webhooks for story events:

```json
{
  "url": "https://your-service.com/webhook",
  "events": ["story.generated", "story.analyzed", "continuity.issues"]
}
```
