using System.Text;
using System.Text.Json;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Web.Services;

/// <summary>
/// Service for calling the scenario dominator path analysis API from the web app.
/// </summary>
public class WebScenarioDominatorPathAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebScenarioDominatorPathAnalysisService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public WebScenarioDominatorPathAnalysisService(HttpClient httpClient, ILogger<WebScenarioDominatorPathAnalysisService> logger)
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
    /// Evaluates dominator path consistency for a scenario.
    /// </summary>
    public async Task<EvaluateDominatorPathConsistencyResponse> EvaluateAsync(
        EvaluateDominatorPathConsistencyRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "api/scenariodominatorpathanalysis/evaluate",
                content,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<EvaluateDominatorPathConsistencyResponse>(responseJson, _jsonOptions);
                return result ?? new EvaluateDominatorPathConsistencyResponse { Success = false, Error = "Empty response" };
            }

            _logger.LogWarning("API returned {StatusCode} for path consistency evaluation", response.StatusCode);
            return new EvaluateDominatorPathConsistencyResponse
            {
                Success = false,
                Error = $"API returned status {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling path consistency evaluation API");
            return new EvaluateDominatorPathConsistencyResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}
