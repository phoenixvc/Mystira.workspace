using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mystira.StoryGenerator.Application.Utilities;

/// <summary>
/// Utilities for cleaning/sanitizing story JSON text returned from LLMs.
/// </summary>
public static class StoryTextSanitizer
{
    private static readonly Regex NewlinesRegex = new Regex("[\r\n]+", RegexOptions.Compiled);

    /// <summary>
    /// Replaces any newline (\n) and carriage return (\r) characters – including multiple in a row – with a single space.
    /// Does not otherwise collapse spaces or tabs.
    /// </summary>
    public static string? CollapseNewlinesToSpace(string? input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // Trim leading/trailing whitespace to avoid accidental parse errors and keep tidy output.
        var trimmed = input.Trim();

        // If it's JSON, only collapse newlines INSIDE string values, not in the structural formatting.
        // This preserves valid JSON while normalizing user-facing text fields like "description".
        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
        {
            try
            {
                var node = JsonNode.Parse(trimmed);
                if (node != null)
                {
                    var sanitized = SanitizeJsonNodeStrings(node);
                    return sanitized.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
                }
            }
            catch
            {
                // As a fallback for JSON-like input that fails to parse, collapse escaped newline sequences
                // only inside JSON string literals without touching structure.
                var jsonHeuristic = CollapseEscapedNewlinesInJsonLiterals(trimmed);
                if (!ReferenceEquals(jsonHeuristic, trimmed))
                {
                    return jsonHeuristic;
                }
                // Fall through to plain-text handling if heuristic didn't change anything
            }
        }

        // Non-JSON text: collapse newline runs to a single space globally.
        return NewlinesRegex.Replace(trimmed, " ");
    }

    private static JsonNode? SanitizeJsonNodeStrings(JsonNode? node)
    {
        if (node is null) return null;

        switch (node)
        {
            case JsonValue v:
                // When parsing from text, JsonValue may internally hold a JsonElement rather than a raw string.
                if (v.TryGetValue<string>(out var s))
                {
                    var replaced = NewlinesRegex.Replace(s, " ");
                    return JsonValue.Create(replaced);
                }
                if (v.TryGetValue<JsonElement>(out var el) && el.ValueKind == JsonValueKind.String)
                {
                    var str = el.GetString() ?? string.Empty;
                    var replaced = NewlinesRegex.Replace(str, " ");
                    return JsonValue.Create(replaced);
                }
                return node; // numbers/bools/null remain unchanged

            case JsonArray arr:
            {
                var newArr = new JsonArray();
                foreach (var item in arr)
                {
                    newArr.Add(SanitizeJsonNodeStrings(item));
                }
                return newArr;
            }

            case JsonObject obj:
            {
                var newObj = new JsonObject();
                foreach (var kvp in obj)
                {
                    newObj[kvp.Key] = SanitizeJsonNodeStrings(kvp.Value);
                }
                return newObj;
            }

            default:
                return node;
        }
    }

    // Minimal JSON-string-literal aware fallback: replace escaped newline sequences (\\r\\n, \\n, \\r)
    // inside quotes without modifying JSON structure. This is used only when JSON parsing fails.
    private static string CollapseEscapedNewlinesInJsonLiterals(string text)
    {
        bool changed = false;
        // Regex to match JSON string literals, handling escaped quotes and backslashes.
        var pattern = "\"(?:\\\\.|[^\\\"])*\""; // " ( \\ . | [^\\ "] )* "
        var regex = new Regex(pattern);
        string result = regex.Replace(text, m =>
        {
            var s = m.Value;
            var replaced = s
                .Replace("\\r\\n", " ")
                .Replace("\\n", " ")
                .Replace("\\r", " ");
            // If multiple escapes were adjacent (e.g. \n\n), the above can introduce double spaces.
            // Normalize multiple consecutive spaces inside the string literal to a single space.
            replaced = Regex.Replace(replaced, " {2,}", " ");
            if (!ReferenceEquals(replaced, s)) changed = true;
            return replaced;
        });
        return changed ? result : text;
    }
}
