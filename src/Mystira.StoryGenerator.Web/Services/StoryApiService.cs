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

    public async Task<ValidationResponse> ValidateStoryAsync(string storyContent, string format = "yaml")
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

    public async Task<GenerateYamlStoryResponse> GenerateYamlStoryAsync(GenerateYamlStoryRequest request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Set a timeout for the request (e.g., 180 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));

            var response = await _httpClient.PostAsync("api/stories/generate", content, cts.Token);
            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<GenerateYamlStoryResponse>(responseJson, _jsonOptions);
                return result ?? new GenerateYamlStoryResponse { Success = false, Error = "Empty response" };
            }

            return new GenerateYamlStoryResponse
            {
                Success = false,
                Error = $"API request failed: {response.StatusCode} - {responseJson}"
            };
        }
        catch (Exception ex)
        {
            return new GenerateYamlStoryResponse
            {
                Success = false,
                Error = $"Failed to connect to generation service: {ex.Message}"
            };
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
