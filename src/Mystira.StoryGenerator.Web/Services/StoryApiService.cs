using System.Text;
using System.Text.Json;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Web.Services;

public class StoryApiService : IStoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public StoryApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ValidationResponse> ValidateStoryAsync(string storyContent, string format = "json")
    {
        try
        {
            var request = new ValidateStoryRequest
            {
                StoryContent = storyContent,
                Format = format
            };

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/stories/validate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var validationResponse = JsonSerializer.Deserialize<ValidationResponse>(responseJson, _jsonOptions);
                return validationResponse ?? new ValidationResponse();
            }
            else
            {
                return new ValidationResponse
                {
                    IsValid = false,
                    Errors = new List<ValidationIssue>
                    {
                        new ValidationIssue
                        {
                            Path = "api",
                            Message = $"API request failed: {response.StatusCode}"
                        }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            return new ValidationResponse
            {
                IsValid = false,
                Errors = new List<ValidationIssue>
                {
                    new ValidationIssue
                    {
                        Path = "connection",
                        Message = $"Failed to connect to validation service: {ex.Message}"
                    }
                }
            };
        }
    }

    public async Task<GenerateJsonStoryResponse> GenerateJsonStoryAsync(GenerateJsonStoryRequest request)
    {
        try
        {
            // New flow: send user command to chat/complete. Package the GenerateJsonStoryRequest
            // as JSON in the user message to allow intent routing to detect parameters.
            var userMessage = new MystiraChatMessage
            {
                MessageType = ChatMessageType.User,
                Content = $"Please generate a story with these parameters: {JsonSerializer.Serialize(request, _jsonOptions)}"
            };
            var chatRequest = new ChatCompletionRequest
            {
                Provider = request.Provider,
                Model = request.Model,
                ModelId = request.ModelId,
                Messages = new List<MystiraChatMessage> { userMessage }
            };

            var json = JsonSerializer.Serialize(chatRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));

            var response = await _httpClient.PostAsync("api/chat/complete", content, cts.Token);
            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return new GenerateJsonStoryResponse
                {
                    Success = false,
                    Error = $"API request failed: {response.StatusCode} - {responseJson}"
                };
            }

            var orchestration = JsonSerializer.Deserialize<ChatOrchestrationResponse>(responseJson, _jsonOptions);
            if (orchestration == null)
            {
                return new GenerateJsonStoryResponse { Success = false, Error = "Empty response" };
            }

            if (orchestration.RequiresClarification)
            {
                return new GenerateJsonStoryResponse { Success = false, Error = orchestration.Prompt ?? "More information is required." };
            }

            // The handler result should be a GenerateJsonStoryResponse payload
            if (orchestration.Result is not null)
            {
                try
                {
                    // Re-serialize and deserialize into the expected type
                    var reJson = JsonSerializer.Serialize(orchestration.Result, _jsonOptions);
                    var mapped = JsonSerializer.Deserialize<GenerateJsonStoryResponse>(reJson, _jsonOptions);
                    if (mapped != null) return mapped;
                }
                catch
                {
                    // fallthrough to generic mapping
                }
            }

            // Fallback: if orchestration returned a raw chat response, map its content into Json field.
            return new GenerateJsonStoryResponse
            {
                Success = orchestration.Success,
                Error = orchestration.Success ? null : orchestration.Error ?? "Unknown error",
                Json = orchestration.Success ? (orchestration.Result?.ToString() ?? string.Empty) : string.Empty
            };
        }
        catch (Exception ex)
        {
            return new GenerateJsonStoryResponse
            {
                Success = false,
                Error = $"Failed to connect to chat service: {ex.Message}"
            };
        }
    }

    public async Task<ChatCompletionResponse> CompleteChatAsync(ChatCompletionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            var response = await _httpClient.PostAsync("api/chat/complete", content, cts.Token);
            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return new ChatCompletionResponse { Success = false, Error = $"API request failed: {response.StatusCode} - {responseJson}" };
            }

            // Map orchestration response to simple chat completion response used by UI
            var orchestration = JsonSerializer.Deserialize<ChatOrchestrationResponse>(responseJson, _jsonOptions);
            if (orchestration == null)
            {
                return new ChatCompletionResponse { Success = false, Error = "Empty response" };
            }

            if (orchestration.RequiresClarification)
            {
                return new ChatCompletionResponse { Success = true, Content = orchestration.Prompt ?? string.Empty };
            }

            // If handler provided a result try to extract the assistant text content from it
            // Never surface the raw JSON blob to the chat UI.
            string? contentText = null;
            if (orchestration.Result is not null)
            {
                try
                {
                    // If it's already a string, use it
                    if (orchestration.Result is string s)
                    {
                        contentText = s;
                    }
                    else
                    {
                        // Serialize to JSON and try to extract common fields like "content" or a messages[].content
                        var resultJson = JsonSerializer.Serialize(orchestration.Result, _jsonOptions);
                        using var doc = JsonDocument.Parse(resultJson);
                        var root = doc.RootElement;
                        if (root.ValueKind == JsonValueKind.String)
                        {
                            contentText = root.GetString();
                        }
                        else if (root.ValueKind == JsonValueKind.Object)
                        {
                            if (root.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                            {
                                contentText = contentProp.GetString();
                            }
                            else if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                            {
                                contentText = messageProp.GetString();
                            }
                            // Handle orchestration.Result objects that look like: { success: true, json: "{...}" }
                            else if (root.TryGetProperty("json", out var jsonProp) && jsonProp.ValueKind == JsonValueKind.String)
                            {
                                contentText = jsonProp.GetString();
                            }
                            else if (root.TryGetProperty("messages", out var messagesProp) && messagesProp.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in messagesProp.EnumerateArray())
                                {
                                    if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("content", out var innerContent) && innerContent.ValueKind == JsonValueKind.String)
                                    {
                                        contentText = innerContent.GetString();
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // fall through with null contentText
                }
            }

            return new ChatCompletionResponse
            {
                Success = orchestration.Success,
                Content = orchestration.Success ? (contentText ?? orchestration.Prompt ?? string.Empty) : null,
                Error = orchestration.Success ? null : orchestration.Error
            };
        }
        catch (Exception ex)
        {
            return new ChatCompletionResponse { Success = false, Error = $"Failed to connect to chat service: {ex.Message}" };
        }
    }

    public async Task<ChatCompletionResponse> SetupStoryAsync(ChatCompletionRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/stories/setup", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(responseJson, _jsonOptions);
            return result ?? new ChatCompletionResponse { Success = false, Error = "Empty response" };
        }
        catch (Exception ex)
        {
            return new ChatCompletionResponse
            {
                Success = false,
                Error = $"Failed to connect to setup service: {ex.Message}"
            };
        }
    }

    public async Task<RandomStoryParametersResponse> RandomizeStoryParametersAsync(RandomStoryParametersRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/stories/randomize", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RandomStoryParametersResponse>(responseJson, _jsonOptions);
                return result ?? new RandomStoryParametersResponse { Success = false, Error = "Empty response" };
            }

            return new RandomStoryParametersResponse
            {
                Success = false,
                Error = $"API request failed: {response.StatusCode} - {responseJson}"
            };
        }
        catch (Exception ex)
        {
            return new RandomStoryParametersResponse
            {
                Success = false,
                Error = $"Failed to connect to randomization service: {ex.Message}"
            };
        }
    }
}
