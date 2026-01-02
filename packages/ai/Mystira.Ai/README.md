# Mystira.Ai

LLM/AI infrastructure for Azure OpenAI and Anthropic providers.

## Features

- Multi-provider support (Azure OpenAI, Anthropic Claude)
- Rate limiting with per-minute request throttling
- Streaming and non-streaming completions
- Configurable via `IOptions<AiSettings>`

## Usage

```csharp
services.AddMystiraAi(configuration);
```
