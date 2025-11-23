using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mystira.StoryGenerator.Api.Services;

public class StoryValidationService : IStoryValidationService
{
    private readonly JsonSchema? _schema;
    private readonly AiSettings _settings;
    private readonly IStorySchemaProvider _schemaProvider;
    private readonly ILogger<StoryValidationService> _logger;

    public StoryValidationService(ILogger<StoryValidationService> logger, IOptions<AiSettings> aiOptions, IStorySchemaProvider schemaProvider)
    {
        _logger = logger;
        _settings = aiOptions.Value;
        _schemaProvider = schemaProvider;
        _schema = LoadSchema();
    }

    public async Task<ValidationResponse> ValidateStoryAsync(ValidateStoryRequest request)
    {
        var response = new ValidationResponse();

        try
        {
            if (_schema == null)
            {
                response.Errors.Add(new ValidationIssue
                {
                    Path = "schema",
                    Message = "Story schema could not be loaded"
                });
                return response;
            }

            // Convert YAML to JSON if needed
            var jsonContent = await ConvertToJsonAsync(request.StoryContent, request.Format);
            if (string.IsNullOrEmpty(jsonContent))
            {
                response.Errors.Add(new ValidationIssue
                {
                    Path = "content",
                    Message = "Story content could not be parsed"
                });
                return response;
            }

            // Parse JSON
            JObject? storyObject = null;
            try
            {
                storyObject = JObject.Parse(jsonContent);
            }
            catch (JsonReaderException ex)
            {
                response.Errors.Add(new ValidationIssue
                {
                    Path = "json",
                    Message = $"Invalid JSON format: {ex.Message}"
                });
                return response;
            }

            if (storyObject == null)
            {
                response.Errors.Add(new ValidationIssue
                {
                    Path = "content",
                    Message = "Story content is empty"
                });
                return response;
            }

            // Validate against schema
            var validationErrors = _schema.Validate(storyObject);

            // Process validation errors
            foreach (var error in validationErrors)
            {
                response.Errors.Add(new ValidationIssue
                {
                    Path = error.Path,
                    Message = error.ToString()
                });
            }

            // Add warnings and suggestions
            await AddWarningsAndSuggestions(storyObject, response);

            response.IsValid = response.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating story");
            response.Errors.Add(new ValidationIssue
            {
                Path = "validation",
                Message = $"Validation error: {ex.Message}"
            });
        }

        return response;
    }

    private Dictionary<object, object> EnforceSchemaTypes(Dictionary<object, object> dictionary, JsonSchema schema)
    {
        if (schema == null || dictionary == null)
            return dictionary;

        var result = new Dictionary<object, object>(dictionary);

        if (schema.Properties != null)
        {
            foreach (var property in schema.Properties)
            {
                var propertyName = property.Key;
                var propertySchema = property.Value;

                if (result.TryGetValue(propertyName, out var value))
                {
                    if (propertySchema.Type.HasFlag(JsonObjectType.Integer))
                    {
                        if (value is string stringValue && int.TryParse(stringValue, out var intValue))
                        {
                            result[propertyName] = intValue;
                        }
                        else if (value is double doubleValue)
                        {
                            result[propertyName] = (int)doubleValue;
                        }
                        else if (value is float floatValue)
                        {
                            result[propertyName] = (int)floatValue;
                        }
                    }
                    else if (propertySchema.Type.HasFlag(JsonObjectType.Number))
                    {
                        if (value is string stringValue &&
                            double.TryParse(stringValue, out var numValue))
                        {
                            result[propertyName] = numValue;
                        }
                    }
                    else if (propertySchema.Type.HasFlag(JsonObjectType.Boolean))
                    {
                        if (value is string stringValue)
                        {
                            var strValue = stringValue.ToLowerInvariant();
                            if (strValue == "true" || strValue == "yes" || strValue == "1")
                                result[propertyName] = true;
                            else if (strValue == "false" || strValue == "no" || strValue == "0")
                                result[propertyName] = false;
                        }
                    }

                    if (value is Dictionary<object, object> nestedDict && propertySchema.Properties != null)
                    {
                        result[propertyName] = EnforceSchemaTypes(nestedDict, propertySchema);
                    }

                    if (value is List<object> list)
                    {
                        var newList = new List<object>();

                        var hasItemSchema = propertySchema.Item != null &&
                                           (propertySchema.Item.Properties?.Count > 0 ||
                                            propertySchema.Item.Type != JsonObjectType.None);

                        foreach (var item in list)
                        {
                            if (item is Dictionary<object, object> dictItem && hasItemSchema)
                            {
                                newList.Add(EnforceSchemaTypes(dictItem, propertySchema.Item));
                            }
                            else if (propertySchema.Item != null && item is string strItem)
                            {
                                if (propertySchema.Item.Type.HasFlag(JsonObjectType.Integer) &&
                                    int.TryParse(strItem, out var intValue))
                                {
                                    newList.Add(intValue);
                                }
                                else if (propertySchema.Item.Type.HasFlag(JsonObjectType.Number) &&
                                         double.TryParse(strItem, out var doubleValue))
                                {
                                    newList.Add(doubleValue);
                                }
                                else if (propertySchema.Item.Type.HasFlag(JsonObjectType.Boolean))
                                {
                                    var lowerStr = strItem.ToLowerInvariant();
                                    if (lowerStr == "true" || lowerStr == "yes" || lowerStr == "1")
                                        newList.Add(true);
                                    else if (lowerStr == "false" || lowerStr == "no" || lowerStr == "0")
                                        newList.Add(false);
                                    else
                                        newList.Add(item);
                                }
                                else
                                {
                                    newList.Add(item);
                                }
                            }
                            else
                            {
                                newList.Add(item);
                            }
                        }
                        result[propertyName] = newList;
                    }
                }
            }
        }

        return result;
    }

    private JsonSchema? LoadSchema()
    {
        try
        {
            var schemaContent = _schemaProvider.GetSchemaJsonAsync().Result;
            if (string.IsNullOrWhiteSpace(schemaContent))
            {
                _logger.LogError("Story schema content is empty or missing");
                return null;
            }
            return JsonSchema.FromJsonAsync(schemaContent).Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load story schema");
            return null;
        }
    }

    private async Task<string> ConvertToJsonAsync(string content, string format)
    {
        if (format.ToLower() == "json")
        {
            return content;
        }

        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(content);
            if (yamlObject == null)
            {
                return string.Empty;
            }

            yamlObject = EnforceSchemaTypes(yamlObject, _schema);

            return JsonConvert.SerializeObject(yamlObject, Formatting.Indented);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert YAML to JSON");
            return string.Empty;
        }
    }

    private async Task AddWarningsAndSuggestions(JObject storyObject, ValidationResponse response)
    {
        if (storyObject["difficulty"] == null)
        {
            response.Suggestions.Add(new ValidationSuggestion
            {
                Path = "difficulty",
                Message = "Difficulty is missing. Consider adding a difficulty level.",
                AutoFixValue = "Medium",
                AutoFixDescription = "Add default difficulty: Medium"
            });
        }

        if (storyObject["session_length"] == null)
        {
            response.Suggestions.Add(new ValidationSuggestion
            {
                Path = "session_length",
                Message = "Session length is missing. Consider adding an expected duration.",
                AutoFixValue = "Medium",
                AutoFixDescription = "Add default session length: Medium"
            });
        }

        var scenes = storyObject["scenes"] as JArray;
        if (scenes != null)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                var scene = scenes[i] as JObject;
                if (scene != null)
                {
                    var sceneType = scene["type"]?.ToString();
                    if (sceneType == "roll")
                    {
                        if (scene["difficulty"] == null)
                        {
                            response.Warnings.Add(new ValidationIssue
                            {
                                Path = $"scenes[{i}].difficulty",
                                Message = "Roll type scenes should have a difficulty specified"
                            });
                        }
                        if (scene["branches"] == null)
                        {
                            response.Warnings.Add(new ValidationIssue
                            {
                                Path = $"scenes[{i}].branches",
                                Message = "Roll type scenes should have branches for different outcomes"
                            });
                        }
                    }

                    if (sceneType == "choice" && scene["branches"] == null)
                    {
                        response.Warnings.Add(new ValidationIssue
                        {
                            Path = $"scenes[{i}].branches",
                            Message = "Choice type scenes should have branches for player options"
                        });
                    }

                    var description = scene["description"]?.ToString();
                    if (!string.IsNullOrEmpty(description) && description.Length < 10)
                    {
                        response.Warnings.Add(new ValidationIssue
                        {
                            Path = $"scenes[{i}].description",
                            Message = "Scene description is very short. Consider adding more detail."
                        });
                    }
                }
            }
        }

        var characters = storyObject["characters"] as JArray;
        if (characters != null)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                var character = characters[i] as JObject;
                if (character != null)
                {
                    var backstory = character["metadata"]?["backstory"]?.ToString();
                    if (!string.IsNullOrEmpty(backstory) && backstory.Length < 20)
                    {
                        response.Warnings.Add(new ValidationIssue
                        {
                            Path = $"characters[{i}].metadata.backstory",
                            Message = "Character backstory is very brief. Consider adding more depth."
                        });
                    }

                    var traits = character["metadata"]?["traits"] as JArray;
                    if (traits == null || traits.Count == 0)
                    {
                        response.Suggestions.Add(new ValidationSuggestion
                        {
                            Path = $"characters[{i}].metadata.traits",
                            Message = "Character has no traits. Consider adding personality traits.",
                            AutoFixValue = new[] { "brave", "curious" },
                            AutoFixDescription = "Add default traits: brave, curious"
                        });
                    }
                }
            }
        }

        var tags = storyObject["tags"] as JArray;
        if (tags != null && tags.Count < 2)
        {
            response.Suggestions.Add(new ValidationSuggestion
            {
                Path = "tags",
                Message = "Consider adding more tags to improve discoverability",
                AutoFixValue = null,
                AutoFixDescription = "Add 2-3 relevant tags describing the story theme, setting, or mechanics"
            });
        }
    }
}
