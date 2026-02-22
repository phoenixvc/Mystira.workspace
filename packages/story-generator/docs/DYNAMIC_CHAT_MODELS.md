# Dynamic Chat Models API

This document describes the new dynamic chat models functionality added to the Mystira Story Generator.

## Overview

The system now supports dynamically fetching available AI models from providers instead of using static configuration. This allows for more flexible model management and automatic discovery of available models.

## API Changes

### New Endpoint: `/api/chat/models`

Returns a list of available models grouped by provider.

**Response Format:**
```json
{
  "providers": [
    {
      "provider": "azure-openai",
      "available": true,
      "models": [
        {
          "id": "gpt-4.1",
          "displayName": "GPT-4",
          "description": "Azure OpenAI GPT model deployment",
          "maxTokens": 4096,
          "defaultTemperature": 0.7,
          "minTemperature": 0.0,
          "maxTemperature": 2.0,
          "supportsJsonSchema": true,
          "capabilities": ["chat", "json-schema", "story-generation"]
        }
      ]
    },
    {
      "provider": "google-gemini",
      "available": false,
      "models": []
    }
  ],
  "totalModels": 1
}
```

## Implementation Details

### Backend Changes

1. **New Contract Models**: `ChatModelsResponse`, `ProviderModels`, and `ChatModelInfo` classes
2. **Extended ILLMService**: Added `GetAvailableModels()` method to expose provider models
3. **Azure OpenAI Service**: Returns configured deployment with model metadata
4. **Google Gemini Service**: New implementation with model discovery
5. **Factory Updates**: `LLMServiceFactory` now aggregates models from all providers
6. **ChatController**: New `/api/chat/models` endpoint

### Frontend Changes

1. **DynamicModelService**: New service for fetching and caching models from API
2. **ProviderSettings Component**: Enhanced with dynamic model selection dropdown
3. **Auto-population**: Model selection automatically sets temperature and token limits
4. **Caching**: Models are cached for 30 minutes to reduce API calls

## Configuration

### Azure OpenAI Configuration

```json
{
  "Ai": {
    "AzureOpenAI": {
      "Endpoint": "https://your-resource.openai.azure.com/",
      "ApiKey": "your-api-key",
      "DeploymentName": "gpt-4.1"
    }
  }
}
```

### Google Gemini Configuration

```json
{
  "Ai": {
    "GoogleGemini": {
      "ApiKey": "your-api-key",
      "Model": "gemini-pro"
    }
  }
}
```

## Usage

1. **Provider Selection**: Users select an AI provider from available options
2. **Model Selection**: Based on provider, available models are displayed
3. **Auto-configuration**: Selecting a model auto-populates recommended settings
4. **Fallback**: If API is unavailable, system falls back to cached models

## Benefits

- **Dynamic Discovery**: No need to update configuration files when adding new models
- **Provider Agnostic**: Supports multiple AI providers with different model formats
- **Automatic Updates**: Models are fetched fresh periodically
- **Better UX**: Users see actual available models with descriptions
- **Validation**: Models are validated to be available before being displayed

## Testing

Unit tests have been added for:
- `LLMServiceFactory.GetAvailableModels()`
- `ChatController.GetModels()` endpoint
- Error handling and fallback scenarios

## Migration

The existing static model configuration (`ai-models.json`) remains for backward compatibility. The new dynamic service operates alongside the existing service, allowing gradual migration.