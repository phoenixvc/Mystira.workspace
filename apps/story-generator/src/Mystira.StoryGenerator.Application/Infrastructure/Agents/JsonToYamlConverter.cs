using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Utility for converting JSON to YAML format.
/// </summary>
public static class JsonToYamlConverter
{
    /// <summary>
    /// Converts JSON string to YAML format.
    /// </summary>
    /// <param name="json">The JSON string to convert.</param>
    /// <returns>YAML string representation.</returns>
    public static string ToYaml(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return string.Empty;

        try
        {
            // Parse JSON into native .NET types
            object? obj;
            using (var doc = JsonDocument.Parse(json))
            {
                obj = ConvertElement(doc.RootElement);
            }

            if (obj is null)
                return string.Empty;

            // Serialize to YAML
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            return serializer.Serialize(obj);
        }
        catch
        {
            // If conversion fails, return empty string
            return string.Empty;
        }
    }

    private static object? ConvertElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
            {
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ConvertElement(prop.Value);
                }
                return dict;
            }
            case JsonValueKind.Array:
            {
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertElement(item));
                }
                return list;
            }
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Number:
            {
                if (element.TryGetInt64(out var l)) return l;
                if (element.TryGetDouble(out var d)) return d;
                return element.GetRawText();
            }
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                return null;
        }
    }
}
