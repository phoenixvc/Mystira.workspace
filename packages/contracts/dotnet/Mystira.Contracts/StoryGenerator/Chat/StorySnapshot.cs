using System.Text;
using System.Text.Json;

namespace Mystira.Contracts.StoryGenerator.Chat;

public static class StoryJsonRepairHelper
{
    /// <summary>
    /// Attempts to "close" a partial JSON string by adding missing closing braces and brackets.
    /// This is a heuristic approach and may not always produce perfect JSON, but should
    /// make it "parseable enough" for previewing.
    /// </summary>
    public static string RepairPartialJson(string partialJson)
    {
        if (string.IsNullOrWhiteSpace(partialJson)) return "{}";

        var trimmed = partialJson.Trim();

        // If it already looks closed, just return it
        if (trimmed.EndsWith("}") || trimmed.EndsWith("]")) return trimmed;

        var stack = new Stack<char>();
        bool inString = false;
        bool isEscaped = false;

        foreach (var c in trimmed)
        {
            if (isEscaped)
            {
                isEscaped = false;
                continue;
            }

            if (c == '\\')
            {
                isEscaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (!inString)
            {
                if (c == '{' || c == '[')
                {
                    stack.Push(c);
                }
                else if (c == '}' || c == ']')
                {
                    if (stack.Count > 0)
                    {
                        var open = stack.Peek();
                        if ((c == '}' && open == '{') || (c == ']' && open == '['))
                        {
                            stack.Pop();
                        }
                    }
                }
            }
        }

        var sb = new StringBuilder(trimmed);

        // If we stopped inside a string, close it
        if (inString)
        {
            sb.Append('"');
        }

        // Close all open braces/brackets
        while (stack.Count > 0)
        {
            var open = stack.Pop();
            if (open == '{') sb.Append('}');
            else if (open == '[') sb.Append(']');
        }

        return sb.ToString();
    }
}

public class StorySnapshot
{
    public string StoryId { get; set; } = string.Empty;
    public int StoryVersion { get; set; }
    public string Content { get; set; } = string.Empty;

    public string? AgeGroup
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Content))
                return null;

            try
            {
                var json = JsonDocument.Parse(Content);
                if (json.RootElement.TryGetProperty("age_group", out var ageGroupElement))
                {
                    return ageGroupElement.GetString();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
