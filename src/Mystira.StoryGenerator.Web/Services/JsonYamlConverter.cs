using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mystira.StoryGenerator.Web.Services;

public static class JsonYamlConverter
{
    public static string ToYaml(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;

        try
        {
            // Parse JSON into native .NET types (Dictionary/List/primitives) rather than JsonElement
            // because YamlDotNet doesn't know how to serialize JsonElement (it emits value_kind: Object).
            object? obj;
            using (var doc = JsonDocument.Parse(json))
            {
                obj = ConvertElement(doc.RootElement);
            }
            if (obj is null) return string.Empty;

            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            return serializer.Serialize(obj);
        }
        catch
        {
            // If conversion fails, just return the original JSON as a YAML code block-ish string
            return json!;
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
                // Prefer integral types when possible, otherwise fall back to double
                if (element.TryGetInt64(out var l)) return l;
                if (element.TryGetDouble(out var d)) return d;
                // As a last resort, keep it as string to avoid precision loss
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
