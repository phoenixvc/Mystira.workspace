namespace Mystira.StoryGenerator.Contracts.Chat;

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
                var json = System.Text.Json.JsonDocument.Parse(Content);
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
