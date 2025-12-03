using System.Text;
using System.Text.Json;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Service for calling the story continuity API from the web app.
/// </summary>
public class WebStoryContinuityService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebStoryContinuityService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WebStoryContinuityService(HttpClient httpClient, ILogger<WebStoryContinuityService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Evaluates story continuity for a scenario.
    /// </summary>
    public async Task<EvaluateStoryContinuityResponse> EvaluateAsync(
        EvaluateStoryContinuityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "api/storycontinuity/evaluate",
                content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<EvaluateStoryContinuityResponse>(responseJson, _jsonOptions);
                return result ?? new EvaluateStoryContinuityResponse { Success = false, Error = "Empty response" };
            }

            _logger.LogWarning("API returned {StatusCode} for continuity evaluation", response.StatusCode);
            return new EvaluateStoryContinuityResponse
            {
                Success = false,
                Error = $"API returned status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling continuity evaluation API");
            return new EvaluateStoryContinuityResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    // ---- Async (202 + polling) helpers ----

    public async Task<string?> StartAsyncEvaluation(
        EvaluateStoryContinuityRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/storycontinuity/evaluate-async", content, cancellationToken);
            if (!response.IsSuccessStatusCode && (int)response.StatusCode != 202)
            {
                var bodyText = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("StartAsyncEvaluation returned status {Status}. Body: {Body}", response.StatusCode, bodyText);
                return null;
            }

            // Try to read operationId from JSON body first
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("operationId", out var idProp))
                    {
                        var id = idProp.GetString();
                        if (!string.IsNullOrWhiteSpace(id))
                            return id;
                    }
                }
                catch (Exception ex)
                {
                    // Non-JSON or malformed body. We'll try Location header next.
                    _logger.LogDebug(ex, "Unexpected response body when starting async evaluation");
                }
            }

            // Fall back to the Location header (from AcceptedAtAction)
            var location = response.Headers?.Location?.ToString();
            if (string.IsNullOrWhiteSpace(location) && response.Headers.TryGetValues("Location", out var values))
            {
                location = values.FirstOrDefault();
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                // Expecting something like .../api/storycontinuity/operations/{id}
                var lastSegment = location.TrimEnd('/').Split('/').LastOrDefault();
                if (!string.IsNullOrWhiteSpace(lastSegment))
                {
                    return lastSegment;
                }
            }

            _logger.LogWarning("StartAsyncEvaluation did not return an operationId (empty body and no Location header)");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting async continuity evaluation");
            return null;
        }
    }

    public async Task<ContinuityOperationInfo?> GetOperationStatusAsync(string operationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/storycontinuity/operations/{operationId}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetOperationStatusAsync returned status {Status}", response.StatusCode);
                return null;
            }
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var info = JsonSerializer.Deserialize<ContinuityOperationInfo>(json, _jsonOptions);
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting async continuity operation status");
            return null;
        }
    }

    public async Task<EvaluateStoryContinuityResponse> EvaluateWithPollingAsync(
        EvaluateStoryContinuityRequest request,
        TimeSpan? pollInterval = null,
        TimeSpan? overallTimeout = null,
        CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? TimeSpan.FromSeconds(2);
        var timeout = overallTimeout ?? TimeSpan.FromMinutes(15);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        var opId = await StartAsyncEvaluation(request, cts.Token);
        if (string.IsNullOrWhiteSpace(opId))
        {
            return new EvaluateStoryContinuityResponse { Success = false, Error = "Failed to start async continuity evaluation" };
        }

        while (!cts.IsCancellationRequested)
        {
            await Task.Delay(interval, cts.Token);
            var info = await GetOperationStatusAsync(opId, cts.Token);
            if (info == null) continue;

            switch (info.Status)
            {
                case ContinuityOperationStatus.Succeeded:
                    return info.Result ?? new EvaluateStoryContinuityResponse { Success = true, Issues = new List<StoryContinuityIssue>() };
                case ContinuityOperationStatus.Failed:
                    return new EvaluateStoryContinuityResponse { Success = false, Error = info.Error ?? "Async evaluation failed" };
                case ContinuityOperationStatus.Queued:
                case ContinuityOperationStatus.Running:
                default:
                    break; // keep polling
            }
        }

        return new EvaluateStoryContinuityResponse { Success = false, Error = "Async evaluation timed out" };
    }

    /// <summary>
    /// Applies filters to continuity issues.
    /// </summary>
    public List<StoryContinuityIssue> ApplyFilters(
        List<StoryContinuityIssue> issues,
        StoryContinuityIssueFilter filter)
    {
        if (filter == null)
            return issues;

        var filtered = issues;

        // Filter by confidence (case-insensitive, null-safe)
        if (filter.IncludedConfidences.Length > 0)
        {
            var confidences = new HashSet<string>(filter.IncludedConfidences, StringComparer.OrdinalIgnoreCase);
            filtered = filtered
                .Where(i => confidences.Contains(i.Confidence ?? string.Empty))
                .ToList();
        }

        // Filter by entity type (case-insensitive, null-safe)
        if (filter.IncludedEntityTypes.Length > 0)
        {
            var types = new HashSet<string>(filter.IncludedEntityTypes, StringComparer.OrdinalIgnoreCase);
            filtered = filtered
                .Where(i => types.Contains(i.EntityType ?? string.Empty))
                .ToList();
        }

        // Filter by proper nouns
         if (filter.ProperNounsOnly)
         {
             filtered = filtered.Where(i => i.IsProperNoun).ToList();
         }

        return filtered;
    }
}
