# Mystira.StoryGenerator

AI-powered story generation engine for creating dynamic, interactive narratives.

## Overview

Mystira.StoryGenerator is the creative heart of the Mystira platform, responsible for:

- Dynamic narrative generation
- Character development and dialogue
- World-building and lore creation
- Multi-model AI orchestration
- Context-aware story continuation
- Player choice integration

## Structure

```
story-generator/
├── core/              # Core generation logic
│   ├── engine/       # Story engine
│   ├── context/      # Context management
│   ├── memory/       # Story memory systems
│   └── prompts/      # Prompt templates
├── models/            # AI model integrations
│   ├── anthropic/    # Claude integration
│   ├── openai/       # OpenAI integration
│   └── local/        # Local model support
├── api/               # Story generation API
│   ├── routes/       # API endpoints
│   ├── middleware/   # Request handling
│   └── validators/   # Input validation
└── tests/            # Test suites
```

## Getting Started

```bash
# Install dependencies
pnpm install

# Set up environment
cp .env.example .env

# Start development server
pnpm dev

# Run tests
pnpm test
```

## API Usage

### Generate Story Segment

```typescript
POST /api/generate
{
  "context": {
    "storyId": "story_123",
    "previousEvents": [...],
    "characterState": {...}
  },
  "playerChoice": "explore the cave",
  "options": {
    "length": "medium",
    "tone": "mysterious"
  }
}
```

### Continue Story

```typescript
POST /api/continue
{
  "storyId": "story_123",
  "action": "examine the ancient artifact"
}
```

## Configuration

### Environment Variables

```env
# AI Providers
ANTHROPIC_API_KEY=sk-ant-...
OPENAI_API_KEY=sk-...

# Generation Settings
DEFAULT_MODEL=claude-3-opus
MAX_TOKENS=4096
TEMPERATURE=0.8

# Database
DATABASE_URL=postgres://...
REDIS_URL=redis://...
```

## Architecture

```
┌──────────────────────────────────────────────────┐
│              Story Generator API                  │
├──────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐   │
│  │ Context  │  │  Memory  │  │   Prompt     │   │
│  │ Manager  │  │  System  │  │   Builder    │   │
│  └────┬─────┘  └────┬─────┘  └──────┬───────┘   │
│       └─────────────┼────────────────┘           │
│                     │                            │
│              ┌──────▼──────┐                     │
│              │   Engine    │                     │
│              └──────┬──────┘                     │
│                     │                            │
│  ┌──────────────────┼──────────────────────┐    │
│  │           Model Orchestrator             │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐   │    │
│  │  │ Claude  │ │  GPT-4  │ │  Local  │   │    │
│  │  └─────────┘ └─────────┘ └─────────┘   │    │
│  └──────────────────────────────────────────┘   │
└──────────────────────────────────────────────────┘
```

## Features

### Narrative Generation

- Branching storylines
- Dynamic character interactions
- Procedural world events
- Emotional tone adaptation

### Context Management

- Long-term story memory
- Character relationship tracking
- World state persistence
- Event consequence chains

### Quality Controls

- Content moderation
- Consistency checking
- Style adherence
- Lore compliance

## Development

```bash
# Run in development
pnpm dev

# Run tests
pnpm test

# Generate types
pnpm generate:types

# Lint
pnpm lint
```

## Testing

```bash
# Unit tests
pnpm test:unit

# Integration tests
pnpm test:integration

# End-to-end tests
pnpm test:e2e
```

## License

Proprietary - All rights reserved

