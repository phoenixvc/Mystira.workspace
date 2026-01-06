using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Mystira.StoryGenerator.Api.Models;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Contracts.Models;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Implementation of IAgentSessionService for Blazor WebAssembly.
/// </summary>
public class AgentSessionService : IAgentSessionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AgentSessionService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AgentSessionService(HttpClient httpClient, ILogger<AgentSessionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<SessionStartResponse> StartSessionAsync(StartSessionRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/story-agent/sessions/start", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SessionStartResponse>(_jsonOptions);
            return result ?? throw new InvalidOperationException("Failed to parse session start response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting session");
            throw;
        }
    }

    public async Task<SessionStateResponse> GetSessionAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/story-agent/sessions/{sessionId}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<SessionStateResponse>(_jsonOptions);
            return result ?? throw new InvalidOperationException("Failed to parse session state response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<EvaluateResponse> EvaluateAsync(string sessionId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/story-agent/sessions/{sessionId}/evaluate", null);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EvaluateResponse>(_jsonOptions);
            return result ?? throw new InvalidOperationException("Failed to parse evaluation response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<RefineResponse> RefineAsync(string sessionId, RefineRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/story-agent/sessions/{sessionId}/refine", request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RefineResponse>(_jsonOptions);
            return result ?? throw new InvalidOperationException("Failed to parse refinement response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refining session {SessionId}", sessionId);
            throw;
        }
    }

    public async IAsyncEnumerable<AgentStreamEvent> SubscribeToStreamAsync(
        string sessionId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Subscribing to SSE stream for session {SessionId}", sessionId);

        HttpResponseMessage? response = null;
        Stream? contentStream = null;
        StreamReader? reader = null;

        try
        {
            response = await _httpClient.GetAsync(
                $"/api/story-agent/sessions/{sessionId}/stream",
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();
            contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            reader = new StreamReader(contentStream);

            string? line;
            string? currentEventType = null;
            string? currentData = null;

            while ((line = await reader.ReadLineAsync()) != null && !cancellationToken.IsCancellationRequested)
            {
                if (line.StartsWith("event: "))
                {
                    currentEventType = line.Substring(7).Trim();
                }
                else if (line.StartsWith("data: "))
                {
                    currentData = line.Substring(6).Trim();

                    if (!string.IsNullOrEmpty(currentEventType) && !string.IsNullOrEmpty(currentData))
                    {
                        AgentStreamEvent? evt = null;
                        try
                        {
                            evt = JsonSerializer.Deserialize<AgentStreamEvent>(currentData, _jsonOptions);
                            if (evt != null)
                            {
                                // Parse the event type from the SSE event field
                                if (Enum.TryParse<AgentStreamEvent.EventType>(currentEventType, out var eventType))
                                {
                                    evt.Type = eventType;
                                }
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse SSE event data: {Data}", currentData);
                        }

                        if (evt != null)
                        {
                            _logger.LogDebug("Received SSE event: {EventType}", evt.Type);
                            yield return evt;
                        }

                        // Reset for next event
                        currentEventType = null;
                        currentData = null;
                    }
                }
                else if (string.IsNullOrWhiteSpace(line))
                {
                    // Empty line indicates end of event
                    currentEventType = null;
                    currentData = null;
                }
            }
        }
        finally
        {
            reader?.Dispose();
            contentStream?.Dispose();
            response?.Dispose();
            _logger.LogInformation("SSE stream ended for session {SessionId}", sessionId);
        }
    }
}
