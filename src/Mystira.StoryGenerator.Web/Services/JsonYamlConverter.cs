using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.EventEmitters;

namespace Mystira.StoryGenerator.Web.Services;

public static class JsonYamlConverter
{
    private static readonly Regex NewlinesRegex = new Regex("[\r\n]+", RegexOptions.Compiled);

    public static string ToYaml(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;

        try
        {
            // Sanitize JSON: collapse newline runs inside string values only.
            json = SanitizeJsonStringValues(json);
            // Parse JSON into native .NET types (Dictionary/List/primitives) rather than JsonElement
            // because YamlDotNet doesn't know how to serialize JsonElement (it emits value_kind: Object).
            object? obj;
            using (var doc = JsonDocument.Parse(json))
            {
                obj = ConvertElement(doc.RootElement);
            }
            if (obj is null) return string.Empty;

            // First attempt: use our custom emitter that prefers plain scalars (nicer display)
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithEventEmitter(next => new PreferPlainScalarEmitter(next))
                .Build();

            try
            {
                return serializer.Serialize(obj);
            }
            catch
            {
                // Fallback: build a safe serializer without the custom emitter to guarantee valid YAML
                var safeSerializer = new SerializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance)
                    .Build();
                return safeSerializer.Serialize(obj);
            }
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

    public static string ToJson(string? yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml)) return string.Empty;
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize<object?>(yaml);
            var jsonReady = ConvertYamlObject(yamlObject);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = null
            };
            var rawJson = JsonSerializer.Serialize(jsonReady, options);
            // As a safety net, sanitize JSON string values to prevent multiline descriptions.
            return SanitizeJsonStringValues(rawJson);
        }
        catch
        {
            // On failure, return the original string to avoid breaking the caller
            return yaml!;
        }
    }

    private static object? ConvertYamlObject(object? obj)
    {
        switch (obj)
        {
            case null:
                return null;
            case IDictionary<object, object> dict:
            {
                var result = new Dictionary<string, object?>();
                foreach (var kvp in dict)
                {
                    var key = kvp.Key?.ToString() ?? string.Empty;
                    result[key] = ConvertYamlObject(kvp.Value);
                }
                return result;
            }
            case IDictionary<string, object?> sdict:
            {
                var result = new Dictionary<string, object?>();
                foreach (var kvp in sdict)
                {
                    result[kvp.Key] = ConvertYamlObject(kvp.Value);
                }
                return result;
            }
            case IEnumerable<object?> list:
            {
                var res = new List<object?>();
                foreach (var item in list)
                {
                    res.Add(ConvertYamlObject(item));
                }
                return res;
            }
            default:
                return obj;
        }
    }

    private static string SanitizeJsonStringValues(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return string.Empty;
        var trimmed = json.Trim();
        if (!(trimmed.StartsWith("{") || trimmed.StartsWith("["))) return trimmed; // not JSON

        try
        {
            var node = JsonNode.Parse(trimmed);
            if (node == null) return trimmed;
            var sanitized = SanitizeJsonNodeStrings(node);
            return sanitized.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }
        catch
        {
            // If parsing fails, fall back to collapsing newlines globally (worst case)
            return NewlinesRegex.Replace(trimmed, " ");
        }
    }

    private static JsonNode? SanitizeJsonNodeStrings(JsonNode? node)
    {
        if (node is null) return null;

        switch (node)
        {
            case JsonValue v:
                if (v.TryGetValue<string>(out var s))
                {
                    var replaced = NewlinesRegex.Replace(s, " ");
                    return JsonValue.Create(replaced);
                }
                return node; // keep non-string primitives

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

    /// <summary>
    /// YamlDotNet event emitter that prefers plain scalars for safe strings, so that
    /// values like: Lina steps "forward" render without surrounding quotes.
    /// </summary>
    private sealed class PreferPlainScalarEmitter : ChainedEventEmitter
    {
        public PreferPlainScalarEmitter(IEventEmitter next) : base(next) { }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source?.Value is string s)
            {
                // Only consider changing the style if not already explicitly set to single quoted or literal
                if (eventInfo.Style == ScalarStyle.Any || eventInfo.Style == ScalarStyle.DoubleQuoted || eventInfo.Style == ScalarStyle.Plain)
                {
                    if (CanBePlain(s))
                    {
                        eventInfo.Style = ScalarStyle.Plain;
                    }
                    else
                    {
                        // Ensure escaped characters render correctly: use double-quoted for others
                        eventInfo.Style = ScalarStyle.DoubleQuoted;
                    }
                }
            }

            base.Emit(eventInfo, emitter);
        }

        private static bool CanBePlain(string s)
        {
            if (string.IsNullOrEmpty(s)) return false; // empty should be quoted

            // Disallow leading/trailing whitespace
            if (char.IsWhiteSpace(s[0]) || char.IsWhiteSpace(s[^1])) return false;

            // YAML plain scalars cannot start with these indicators in many contexts
            char first = s[0];
            switch (first)
            {
                case '-':
                case '?':
                case ':':
                case ',':
                case '[':
                case ']':
                case '{':
                case '}':
                case '#':
                case '&':
                case '*':
                case '!':
                case '|':
                case '>':
                case '\'':
                case '"':
                case '%':
                case '@':
                case '`':
                    return false;
            }

            // Newlines and tabs should be avoided (we already sanitize newlines earlier)
            if (s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0 || s.IndexOf('\t') >= 0) return false;

            // Note: YAML 1.2 allows ':' inside plain scalars for values; ambiguity mainly arises
            // when ':' starts a key or appears at the very beginning. Since we already restrict
            // leading indicators above and this method is used for values, we allow ': ' inside.
            // Similarly, allow internal sequences like ' - ' or ' ? ' which are common in prose.

            // Be conservative to avoid emitter/parse edge cases in plain scalars:
            // - Disallow any '#' to avoid comment confusion
            if (s.IndexOf('#') >= 0) return false;
            // - Disallow ": " anywhere; though YAML 1.2 allows it in values, many parsers
            //   or contexts can get tripped. We'll quote such strings instead.
            if (s.Contains(": ")) return false;

            // Control characters are not allowed
            foreach (char c in s)
            {
                if (char.IsControl(c)) return false;
            }

            // Otherwise it's safe to emit as plain; embedded double quotes are fine in plain scalars
            return true;
        }
    }
}
