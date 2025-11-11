namespace Mystira.StoryGenerator.Contracts.Stories;

public class ValidateStoryRequest
{
    public string StoryContent { get; set; } = string.Empty;
    public string Format { get; set; } = "yaml"; // "yaml" or "json"
}

public class ValidationResponse
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Errors { get; set; } = new();
    public List<ValidationIssue> Warnings { get; set; } = new();
    public List<ValidationSuggestion> Suggestions { get; set; } = new();
}

public class ValidationIssue
{
    public string Path { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LineNumber { get; set; }
}

public class ValidationSuggestion : ValidationIssue
{
    public object? AutoFixValue { get; set; }
    public string AutoFixDescription { get; set; } = string.Empty;
}