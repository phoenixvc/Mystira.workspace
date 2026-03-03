using System.Text.Json;
using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;

namespace Mystira.Domain.Serialization;

/// <summary>
/// JSON converter for StringEnum types.
/// </summary>
/// <typeparam name="T">The StringEnum type.</typeparam>
public class StringEnumJsonConverter<T> : JsonConverter<T> where T : StringEnum<T>
{
    /// <inheritdoc />
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Expected string for {typeof(T).Name}, got {reader.TokenType}");

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return null;

        if (StringEnum<T>.TryParse(value, out var result))
            return result;

        throw new JsonException($"Unknown {typeof(T).Name} value: '{value}'");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Value);
    }
}

/// <summary>
/// JSON converter factory for StringEnum types.
/// Automatically creates converters for any StringEnum-derived type.
/// </summary>
public class StringEnumJsonConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsClass || typeToConvert.IsAbstract)
            return false;

        // Check if the type inherits from StringEnum<T>
        var baseType = typeToConvert.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(StringEnum<>))
                return true;

            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <inheritdoc />
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(StringEnumJsonConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}
