namespace Mystira.Contracts.StoryGenerator.Configuration;

public class InstructionSearchSettings
{
    public const string SectionName = "InstructionSearch";
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? IndexName { get; set; }
    public Dictionary<string, string> AgeGroupIndexMapping { get; set; } = new();
    public string EmbeddingFieldName { get; set; } = "embedding";
    public string IdFieldName { get; set; } = "id";
    public string ContentFieldName { get; set; } = "content";
    public string? TitleFieldName { get; set; } = "title";
    public string CategoryFieldName { get; set; } = "category";
    public string? SubcategoryFieldName { get; set; } = "subcategory";
    public bool IsCategoryFieldCollection { get; set; }
    public string InstructionTypeFieldName { get; set; } = "instructionType";
    public bool IsInstructionTypeFieldCollection { get; set; }
    public string MandatoryFieldName { get; set; } = "isMandatory";
    public string TagsFieldName { get; set; } = "tags";
    public string? PriorityFieldName { get; set; } = "priority";
    public string? SourceFieldName { get; set; } = "source";
    public string? VersionFieldName { get; set; } = "version";
    public string? CreatedAtFieldName { get; set; } = "createdAt";
    public string? UpdatedAtFieldName { get; set; } = "updatedAt";
    public string? SectionFieldName { get; set; } = "section";
    public string? DatasetFieldName { get; set; } = "dataset";
    public string? KeywordsFieldName { get; set; } = "keywords";
    public int DefaultTopK { get; set; } = 5;
    public int MandatoryChunkLimit { get; set; } = 15;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        (!string.IsNullOrWhiteSpace(IndexName) || AgeGroupIndexMapping.Count > 0) &&
        !string.IsNullOrWhiteSpace(EmbeddingFieldName);

    public string? ResolveIndexName(string? ageGroup)
    {
        if (string.IsNullOrWhiteSpace(ageGroup))
        {
            return IndexName;
        }

        if (AgeGroupIndexMapping.TryGetValue(ageGroup, out var indexName))
        {
            return indexName;
        }

        return IndexName;
    }
}
