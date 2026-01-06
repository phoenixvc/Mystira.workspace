using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

public static class AgentResponseParser
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

    public static (bool Success, T? Result, string? Error) TryParseJsonResponse<T>(
        string rawResponse) where T : class
    {
        return TryParseJsonResponse<T>(rawResponse, DefaultOptions);
    }

    public static (bool Success, T? Result, string? Error) TryParseJsonResponse<T>(
        string rawResponse,
        JsonSerializerOptions options) where T : class
    {
        try
        {
            var cleaned = CleanJsonResponse(rawResponse);
            var result = JsonSerializer.Deserialize<T>(cleaned, options);

            if (result == null)
                return (false, null, "JSON deserialized to null.");

            return (true, result, null);
        }
        catch (JsonException ex)
        {
            return (false, null, $"Invalid JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Failed to parse response: {ex.Message}");
        }
    }

    private static string CleanJsonResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return string.Empty;

        response = Regex.Replace(response, @"```json\s*|\s*```", string.Empty, RegexOptions.IgnoreCase);
        response = response.Trim();

        var firstBrace = response.IndexOf('{');
        var lastBrace = response.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            response = response.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return response.Trim();
    }
}
