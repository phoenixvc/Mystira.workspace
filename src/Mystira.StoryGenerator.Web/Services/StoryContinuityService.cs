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

        // Filter by pronouns
        if (filter.PronounsOnly)
        {
            filtered = filtered.Where(i => i.IsPronoun).ToList();
        }

        return filtered;
    }
}
