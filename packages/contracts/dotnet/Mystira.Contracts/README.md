# Mystira.Contracts

Unified API contracts for the Mystira platform. This package consolidates all DTOs, requests, and responses for both the App API and StoryGenerator API.

## Installation

```bash
dotnet add package Mystira.Contracts
```

## Namespaces

### App API (`Mystira.Contracts.App`)

| Namespace | Description |
|-----------|-------------|
| `Requests.Scenarios` | Scenario creation and query requests |
| `Requests.GameSessions` | Game session management |
| `Requests.Accounts` | Account and profile management |
| `Requests.Badges` | Badge and achievement requests |
| `Responses.Scenarios` | Scenario responses and summaries |
| `Responses.GameSessions` | Game session state responses |
| `Responses.Badges` | Badge and achievement responses |
| `Enums` | Shared enumerations (AgeGroup, ScenarioStatus, etc.) |
| `Models` | Shared models (AccountSettings, SubscriptionDetails) |

### StoryGenerator API (`Mystira.Contracts.StoryGenerator`)

| Namespace | Description |
|-----------|-------------|
| `Chat` | Chat completion requests/responses |
| `Stories` | Story generation requests/responses |
| `Entities` | Entity recognition (SceneEntity, Confidence) |
| `StoryConsistency` | Story consistency evaluation |
| `Intent` | Intent classification |
| `Configuration` | AI and LLM settings |

### Generated Contracts (`Mystira.Contracts.Generated`)

Auto-generated from OpenAPI specs:
- `AppApiContracts` - Generated App API client
- `StoryGeneratorApiContracts` - Generated StoryGenerator API client

## Usage Examples

### Creating a Scenario Request

```csharp
using Mystira.Contracts.App.Requests.Scenarios;

var request = new CreateScenarioRequest
{
    Title = "The Lost Kingdom",
    Description = "An adventure in a forgotten realm",
    AgeGroup = "Teen",
    Scenes = new List<SceneRequest>
    {
        new SceneRequest
        {
            Title = "The Beginning",
            Description = "Your journey starts here..."
        }
    }
};
```

### Using StoryGenerator Contracts

```csharp
using Mystira.Contracts.StoryGenerator.Chat;
using Mystira.Contracts.StoryGenerator.Stories;

var storyRequest = new GenerateStoryRequest
{
    Prompt = "A brave knight discovers a hidden cave",
    Style = "fantasy",
    Length = "short"
};
```

## Migration from Legacy Packages

This package replaces:
- `Mystira.App.Contracts` → `Mystira.Contracts.App`
- `Mystira.StoryGenerator.Contracts` → `Mystira.Contracts.StoryGenerator`

Update your imports:
```csharp
// Before
using Mystira.App.Contracts.Requests;

// After
using Mystira.Contracts.App.Requests.Scenarios;
```

## Related Packages

- **Mystira.Shared** - Infrastructure (CQRS, validation, mapping)
- **@mystira/contracts** - TypeScript/NPM version of these contracts
