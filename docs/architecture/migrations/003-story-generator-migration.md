# Migration 003: Story Generator API Migration Guide

## Overview

This document provides guidance for migrating to the new unified Story Generator API endpoints and domain structure.

## Domain Changes

### Before (Legacy)

```
# Various inconsistent domains
https://story-generator.mystira.io/v1
https://storygen.mystira.io/api
https://story.mystira.app/api
```

### After (Unified)

| Environment | Domain | Description |
|-------------|--------|-------------|
| Production | `https://story-api.mystira.app/v1` | Main production API |
| Staging | `https://staging.story-api.mystira.app/v1` | Staging environment |
| Development | `https://dev.story-api.mystira.app/v1` | Development environment |
| Local | `http://localhost:5001/v1` | Local development |

## API Endpoints

### Story Generation

```
POST /stories/generate
```

Request:
```json
{
  "prompt": "A brave knight discovers an ancient artifact...",
  "genre": "fantasy",
  "tone": "serious",
  "targetAgeGroup": "adult",
  "maxTokens": 1000,
  "temperature": 0.7
}
```

Response:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "content": "...",
  "title": "The Artifact's Guardian",
  "entities": [
    {
      "name": "Sir Aldric",
      "type": "character",
      "description": "A noble knight of the realm",
      "firstMentionedAt": 45
    }
  ],
  "metadata": {
    "wordCount": 850,
    "estimatedReadingTime": "4 min",
    "tokensUsed": 950,
    "modelUsed": "gpt-4o"
  },
  "createdAt": "2024-12-26T12:00:00Z"
}
```

### Chat Completion

```
POST /chat/complete
```

Request:
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440001",
  "message": "The knight decides to examine the artifact more closely",
  "context": {
    "previousStoryId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

Response:
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440001",
  "response": "Sir Aldric carefully lifted the artifact...",
  "storySnapshot": {
    "currentScene": "Ancient Temple - Inner Chamber",
    "activeCharacters": ["Sir Aldric", "The Guardian Spirit"],
    "mood": "tense",
    "plotProgress": 35
  },
  "suggestedActions": [
    "Examine the inscriptions",
    "Call out to the spirit",
    "Return to the temple entrance"
  ]
}
```

## Contract Types

### TypeScript

```typescript
import {
  GenerateStoryRequest,
  GenerateStoryResponse,
  ChatCompletionRequest,
  ChatCompletionResponse,
  StoryEntity,
  StorySnapshot,
} from '@mystira/contracts/story-generator';

// Types are generated from OpenAPI spec
const request: GenerateStoryRequest = {
  prompt: 'A brave knight...',
  genre: 'fantasy',
};
```

### C#

```csharp
using Mystira.Contracts.StoryGenerator;

// Use the generated types
var request = new GenerateStoryRequest
{
    Prompt = "A brave knight...",
    Genre = StoryGenre.Fantasy,
    Tone = StoryTone.Serious
};
```

## SDK Updates

### TypeScript/JavaScript

```typescript
// Update your Story Generator client
import { createStoryClient } from '@mystira/contracts/story-generator';

const storyClient = createStoryClient({
  baseUrl: process.env.NODE_ENV === 'production'
    ? 'https://story-api.mystira.app/v1'
    : 'https://dev.story-api.mystira.app/v1',
  apiKey: process.env.MYSTIRA_API_KEY,
});

// Generate a story
const story = await storyClient.generateStory({
  prompt: 'A mysterious forest...',
  genre: 'mystery',
});
```

### C# (.NET)

```csharp
services.AddHttpClient<IStoryGeneratorClient>(client =>
{
    var baseUrl = Environment.GetEnvironmentVariable("STORY_API_URL")
        ?? "https://story-api.mystira.app/v1";
    client.BaseAddress = new Uri(baseUrl);
});
```

## Environment Variable Updates

```bash
# .env.development
STORY_API_URL=https://dev.story-api.mystira.app/v1

# .env.staging
STORY_API_URL=https://staging.story-api.mystira.app/v1

# .env.production
STORY_API_URL=https://story-api.mystira.app/v1
```

## Static Web App (SWA)

The Story Generator also includes a Static Web App frontend:

| Environment | Domain |
|-------------|--------|
| Production | `https://story.mystira.app` |
| Staging | `https://staging.story.mystira.app` |
| Development | `https://dev.story.mystira.app` |

## Breaking Changes

1. **Domain Pattern**: API endpoints now use `story-api.mystira.app` (not `story-generator`)
2. **Entity Schema**: `StoryEntity.firstMentionedAt` is now an integer (character position) instead of string
3. **Rate Limiting**: Rate limit response (429) now returns standard `ErrorResponse` schema

## Rate Limits

| Tier | Requests/Minute | Tokens/Minute |
|------|-----------------|---------------|
| Free | 10 | 10,000 |
| Pro | 60 | 100,000 |
| Enterprise | 300 | 500,000 |

## Migration Checklist

- [ ] Update API base URLs in all clients
- [ ] Update environment variables
- [ ] Migrate to new request/response schemas
- [ ] Update error handling for new `ErrorResponse` format
- [ ] Test rate limiting behavior
- [ ] Update CORS configuration
- [ ] Update webhook URLs if using callbacks

## Error Handling

```typescript
try {
  const story = await storyClient.generateStory(request);
} catch (error) {
  if (error.code === 'RATE_LIMITED') {
    const retryAfter = error.metadata?.retryAfterSeconds;
    // Handle rate limiting
  } else if (error.code === 'VALIDATION') {
    // Handle validation errors
    console.error('Validation error:', error.details);
  }
}
```

## Support

For migration assistance:
- Technical: jurie@phoenixvc.tech
- Business/Admin: eben@phoenixvc.tech
- Issues: https://github.com/phoenixvc/Mystira.workspace/issues
