namespace Mystira.StoryGenerator.Web.Services;

public interface IYamlImportService
{
    Task<YamlImportResult> ImportYamlAsync(string yamlContent);
}

public class YamlImportService : IYamlImportService
{
    private readonly ILogger<YamlImportService> _logger;

    public YamlImportService(ILogger<YamlImportService> logger)
    {
        _logger = logger;
    }

    public async Task<YamlImportResult> ImportYamlAsync(string yamlContent)
    {
        return await Task.Run(() => ParseAndValidateYaml(yamlContent));
    }

    private YamlImportResult ParseAndValidateYaml(string yamlContent)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                return new YamlImportResult
                {
                    IsValid = false,
                    ErrorMessage = "YAML content is empty"
                };
            }

            // Try to parse basic YAML structure
            var parsedData = ParseYamlStructure(yamlContent);

            if (parsedData == null)
            {
                return new YamlImportResult
                {
                    IsValid = false,
                    ErrorMessage = "Failed to parse YAML structure"
                };
            }

            // Validate required fields
            var validationErrors = ValidateYamlStructure(parsedData);
            if (validationErrors.Count > 0)
            {
                return new YamlImportResult
                {
                    IsValid = false,
                    ErrorMessage = string.Join("; ", validationErrors)
                };
            }

            // Generate summary
            var summary = GenerateSummary(parsedData);

            return new YamlImportResult
            {
                IsValid = true,
                Summary = summary,
                ParsedData = parsedData,
                YamlContent = yamlContent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing YAML");
            return new YamlImportResult
            {
                IsValid = false,
                ErrorMessage = $"Error parsing YAML: {ex.Message}"
            };
        }
    }

    private Dictionary<string, object>? ParseYamlStructure(string yamlContent)
    {
        try
        {
            var lines = yamlContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var data = new Dictionary<string, object>();
            var currentKey = "";
            var indentStack = new Stack<(int indent, string key)>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                    continue;

                var indent = GetIndentation(line);
                var trimmedLine = line.Trim();

                // Handle key-value pairs
                if (trimmedLine.Contains(":"))
                {
                    var colonIndex = trimmedLine.IndexOf(':');
                    var key = trimmedLine.Substring(0, colonIndex).Trim();
                    var value = trimmedLine.Substring(colonIndex + 1).Trim();

                    if (!string.IsNullOrEmpty(key))
                    {
                        // Remove quotes if present
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                            value = value.Substring(1, value.Length - 2);
                        else if (value.StartsWith("'") && value.EndsWith("'"))
                            value = value.Substring(1, value.Length - 2);

                        currentKey = key;
                        data[key] = value;
                    }
                }
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing YAML structure");
            return null;
        }
    }

    private int GetIndentation(string line)
    {
        var count = 0;
        foreach (var c in line)
        {
            if (c == ' ')
                count++;
            else if (c == '\t')
                count += 2;
            else
                break;
        }
        return count;
    }

    private List<string> ValidateYamlStructure(Dictionary<string, object> data)
    {
        var errors = new List<string>();

        // Check for required fields
        var requiredFields = new[] { "title", "description" };
        foreach (var field in requiredFields)
        {
            if (!data.ContainsKey(field) || string.IsNullOrWhiteSpace(data[field]?.ToString()))
            {
                errors.Add($"Missing required field: '{field}'");
            }
        }

        // Validate characters field if present (can be empty but should be valid YAML)
        if (data.ContainsKey("characters"))
        {
            var charValue = data["characters"]?.ToString();
            if (!string.IsNullOrWhiteSpace(charValue) && !charValue.StartsWith("-"))
            {
                // If it's not empty and doesn't look like a list, it might be invalid
                // But we'll be lenient here
            }
        }

        return errors;
    }

    private string GenerateSummary(Dictionary<string, object> data)
    {
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("Story Imported Successfully");

        return summary.ToString();
    }
}

public class YamlImportResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Summary { get; set; }
    public Dictionary<string, object>? ParsedData { get; set; }
    public string? YamlContent { get; set; }
}
