using System.Net.Http.Json;
using System.Text.Json;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Service for making chat completion API calls
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Get available AI providers
    /// </summary>
    /// <returns>List of available providers</returns>
    Task<List<ProviderInfo>> GetProvidersAsync();

    /// <summary>
    /// Generate a chat completion
    /// </summary>
    /// <param name="request">The chat completion request</param>
    /// <returns>The chat completion response</returns>
    Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request);
}

/// <summary>
/// Provider information model
/// </summary>
public class ProviderInfo
{
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
}

/// <summary>
/// Chat service implementation
/// </summary>
public class ChatService : IChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChatService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatService(HttpClient httpClient, ILogger<ChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<ProviderInfo>> GetProvidersAsync()
    {
        try
        {
            _logger.LogDebug("Fetching available providers");

            var response = await _httpClient.GetAsync("/api/chat/providers");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProvidersResponse>(content, _jsonOptions);

            return result?.Providers ?? new List<ProviderInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching providers");
            return new List<ProviderInfo>();
        }
    }

    public async Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request)
    {
        try
        {
            _logger.LogDebug("Sending chat completion request to provider: {Provider}", request.Provider);

            var response = await _httpClient.PostAsJsonAsync("/api/chat/complete", request, _jsonOptions);

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ChatCompletionResponse>(content, _jsonOptions);

            if (result == null)
            {
                return new ChatCompletionResponse
                {
                    Success = false,
                    Error = "Failed to deserialize response"
                };
            }

            if (!response.IsSuccessStatusCode && result.Success)
            {
                // Override success if HTTP status indicates failure
                result.Success = false;
                if (string.IsNullOrEmpty(result.Error))
                {
                    result.Error = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                }
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during chat completion");
            return new ChatCompletionResponse
            {
                Success = false,
                Error = $"Network error: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Chat completion request timed out");
            return new ChatCompletionResponse
            {
                Success = false,
                Error = "Request timed out"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during chat completion");
            return new ChatCompletionResponse
            {
                Success = false,
                Error = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private class ProvidersResponse
    {
        public List<ProviderInfo> Providers { get; set; } = new();
        public int Count { get; set; }
    }
}