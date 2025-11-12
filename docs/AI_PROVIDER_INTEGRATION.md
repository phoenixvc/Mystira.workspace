# AI Provider Integration Layer for Mystira Story Generator

This document describes the comprehensive AI provider integration layer that enables secure chat completion functionality with multiple LLM providers.

## Architecture Overview

The integration layer follows a clean architecture pattern with:

- **Abstraction Layer**: `ILLMService` interface for provider-agnostic implementation
- **Provider Implementations**: Specific services for Azure AI Foundry and Google Gemini
- **Factory Pattern**: `LLMServiceFactory` for dynamic provider selection
- **Secure Configuration**: Server-side API key management
- **Frontend Integration**: Enhanced chat interface with provider settings

## Backend Implementation (Mystira.StoryGenerator.Api)

### Core Components

#### 1. ILLMService Interface
Located in `Services/LLM/ILLMService.cs`

```csharp
public interface ILLMService
{
    string ProviderName { get; }
    Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
    bool IsAvailable();
}
```

#### 2. Provider Implementations

**Azure AI Foundry Service** (`Services/LLM/AzureAIFoundryService.cs`):
- Supports Azure AI Foundry chat completion endpoints
- Uses deployment identifiers resolved via model configuration
- Full error handling and logging
- Token usage tracking

**Google Gemini Service** (`Services/LLM/GoogleGeminiService.cs`):
- Supports Google Generative AI API
- Gemini Pro model integration
- Content safety and filtering
- Usage statistics

#### 3. LLM Service Factory
`Services/LLM/LLMServiceFactory.cs` provides:
- Dynamic provider resolution based on request
- Fallback to available providers
- Configuration-driven default provider selection
- Health checking for provider availability

#### 4. Chat Controller
`Controllers/ChatController.cs` exposes:

**POST** `/api/chat/complete` - Generate chat completions
- Request validation
- Provider selection
- Error handling
- Security (API keys never exposed)

**GET** `/api/chat/providers` - Get available providers
- Provider availability status
- Configuration validation

### Configuration

Update `appsettings.json`:

```json
{
  "Ai": {
    "DefaultModelId": "story-gpt4o",
    "DefaultProvider": "azure-ai-foundry",
    "DefaultTemperature": 0.7,
    "DefaultMaxTokens": 1000,
    "FeatureModels": {
      "story-generation": "story-gpt4o",
      "story-setup": "assistant-gpt4o-mini",
      "story-randomization": "assistant-gpt4o-mini"
    },
    "Models": [
      {
        "Id": "story-gpt4o",
        "Provider": "azure-ai-foundry",
        "Deployment": "gpt-4o",
        "Model": "gpt-4o",
        "Description": "Primary story generation model",
        "MaxTokens": 1800,
        "Temperature": 0.75
      },
      {
        "Id": "assistant-gpt4o-mini",
        "Provider": "azure-ai-foundry",
        "Deployment": "gpt-4o-mini",
        "Model": "gpt-4o-mini",
        "Description": "Lightweight orchestration model",
        "MaxTokens": 1200,
        "Temperature": 0.6
      }
    ],
    "AzureAIFoundry": {
      "Endpoint": "https://your-resource.openai.azure.com",
      "ApiKey": "YOUR_AZURE_AI_FOUNDRY_API_KEY",
      "ApiVersion": "2024-10-21"
    },
    "GoogleGemini": {
      "ApiKey": "YOUR_GOOGLE_API_KEY",
      "Model": "gemini-pro"
    }
  }
}
```

### Dependency Injection Setup

In `Program.cs`:

```csharp
// Register HttpClient for LLM services
builder.Services.AddHttpClient<AzureAIFoundryService>();
builder.Services.AddHttpClient<GoogleGeminiService>();

// Register LLM services
builder.Services.AddScoped<ILLMService, AzureAIFoundryService>();
builder.Services.AddScoped<ILLMService, GoogleGeminiService>();
builder.Services.AddScoped<ILLMServiceFactory, LLMServiceFactory>();
```

## Frontend Implementation (Mystira.StoryGenerator.Web)

### Core Components

#### 1. Chat Service
`Services/ChatService.cs` provides:
- API client for chat completion endpoints
- Provider information retrieval
- Error handling and timeout management
- Response validation

#### 2. Enhanced Chat Container
`Components/Chat/EnhancedChatContainer.razor`:
- Integrated provider settings panel
- Real-time AI chat completion
- Loading states and error handling
- Message history management
- Token usage display

#### 3. Provider Settings Component
`Components/Chat/ProviderSettings.razor`:
- Dynamic provider selection dropdown
- Temperature and max tokens controls
- System prompt configuration
- Real-time provider availability status
- Visual feedback for settings changes

### Features

#### User Interface
- **Provider Selection**: Dropdown with available providers and their status
- **Model Configuration**: Optional model id and deployment override fields
- **Temperature Control**: Slider for creativity adjustment (0.0 - 2.0)
- **Token Limit**: Input for maximum response tokens (1 - 4096)
- **System Prompt**: Optional instructions for AI behavior
- **Real-time Status**: Visual indicators for provider availability

#### Chat Experience
- **Enhanced Messages**: Shows provider and model used for each response
- **Loading States**: Animated typing indicator during AI processing
- **Error Handling**: Clear error messages with retry options
- **Settings Toggle**: Collapsible settings panel to save space
- **Mobile Responsive**: Adaptive layout for different screen sizes

## Security Features

### API Key Protection
- API keys are stored server-side only
- No sensitive credentials exposed to client
- Environment-based configuration support

### Request Validation
- Server-side parameter validation
- Rate limiting capability
- Input sanitization
- Error message sanitization

### CORS Configuration
- Configurable allowed origins
- Secure header policies
- Development vs. production settings

## API Endpoints

### Chat Completion
**POST** `/api/chat/complete`

Request:
```json
{
  "provider": "azure-ai-foundry",
  "model_id": "story-gpt4o",
  "messages": [
    {
      "messageType": "User",
      "content": "Create a fantasy story about a magical forest"
    }
  ],
  "temperature": 0.7,
  "max_tokens": 1500,
  "system_prompt": "You are a creative storyteller..."
}
```

Response:
```json
{
  "content": "Once upon a time, in a mystical forest...",
  "model": "gpt-4o",
  "model_id": "story-gpt4o",
  "provider": "azure-ai-foundry",
  "usage": {
    "promptTokens": 25,
    "completionTokens": 150,
    "totalTokens": 175
  },
  "timestamp": "2024-01-15T10:30:00Z",
  "success": true
}
```

### Provider Information
**GET** `/api/chat/providers`

Response:
```json
{
  "providers": [
    {
      "name": "azure-ai-foundry",
      "available": true
    },
    {
      "name": "google-gemini",
      "available": true
    }
  ],
  "count": 2
}
```

## Testing

### Unit Tests
The solution includes comprehensive unit tests covering:

- **LLMServiceFactory**: Provider resolution logic
- **AzureAIFoundryService**: Configuration validation and availability checks
- **Provider Integration**: Mock HTTP responses and error scenarios

Run tests:
```bash
dotnet test
```

### Manual Testing
1. Configure API keys in `appsettings.json`
2. Start the API: `dotnet run --project src/Mystira.StoryGenerator.Api`
3. Start the Web app: `dotnet run --project src/Mystira.StoryGenerator.Web`
4. Open browser to test chat interface

## Development Commands

```bash
# Build the entire solution
dotnet build

# Run tests
dotnet test

# Start API (Backend)
dotnet run --project src/Mystira.StoryGenerator.Api --urls="https://localhost:5001"

# Start Web App (Frontend)
dotnet run --project src/Mystira.StoryGenerator.Web --urls="https://localhost:5074"
```

## Configuration Guide

### Azure AI Foundry Setup
1. Create an Azure AI Foundry project workspace
2. Deploy an OpenAI model (e.g., gpt-4o) within the workspace
3. Get the endpoint URL and API key from the project settings
4. Update configuration:
   ```json
   "AzureAIFoundry": {
     "Endpoint": "https://your-resource.openai.azure.com",
     "ApiKey": "your-api-key",
     "ApiVersion": "2024-10-21"
   }
   ```

### Google Gemini Setup
1. Get a Google AI API key from Google AI Studio
2. Update configuration:
   ```json
   "GoogleGemini": {
     "ApiKey": "your-google-api-key",
     "Model": "gemini-pro"
   }
   ```

### Environment Variables (Recommended)
```bash
export AI__AZUREAIFOUNDRY__APIKEY="your-azure-key"
export AI__GOOGLEGEMINI__APIKEY="your-google-key"
```

## Error Handling

The system provides comprehensive error handling:

- **Configuration Errors**: Clear messages for missing API keys or endpoints
- **Network Errors**: Timeout and connection error handling
- **API Errors**: Provider-specific error message translation
- **Validation Errors**: Input parameter validation with helpful messages
- **Fallback Logic**: Automatic provider switching when primary is unavailable

## Performance Considerations

- **HTTP Client Reuse**: Efficient connection pooling
- **Async Operations**: Non-blocking API calls
- **Error Recovery**: Graceful degradation
- **Logging**: Comprehensive logging for debugging and monitoring
- **Caching**: Provider availability caching to reduce API calls

## Future Enhancements

Potential improvements for the integration layer:

1. **Additional Providers**: OpenAI, Anthropic Claude, etc.
2. **Streaming Responses**: Real-time token streaming
3. **Response Caching**: Intelligent caching for similar requests
4. **Rate Limiting**: Built-in rate limiting and queue management
5. **Analytics**: Usage tracking and analytics dashboard
6. **A/B Testing**: Provider performance comparison
7. **Custom Models**: Support for fine-tuned models
8. **Cost Tracking**: Token usage and cost monitoring

## Troubleshooting

### Common Issues

1. **Provider Not Available**
   - Check API key configuration
   - Verify endpoint URLs
   - Check network connectivity

2. **Request Timeouts**
   - Adjust HttpClient timeout settings
   - Check provider service status

3. **Build Errors**
   - Ensure all NuGet packages are restored
   - Check .NET SDK version compatibility

4. **CORS Issues**
   - Update allowed origins in configuration
   - Check frontend URL configuration

For more detailed troubleshooting, check the application logs and ensure all configuration values are properly set.