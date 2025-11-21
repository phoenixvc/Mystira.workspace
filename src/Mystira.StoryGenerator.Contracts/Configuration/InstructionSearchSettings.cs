namespace Mystira.StoryGenerator.Contracts.Configuration;

public class InstructionSearchSettings
{
    public const string SectionName = "InstructionSearch";

    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? IndexName { get; set; }
    public string VectorFieldName { get; set; } = "contentVector";
    public string IdFieldName { get; set; } = "id";
    public string ContentFieldName { get; set; } = "chunk";
    public string CategoryFieldName { get; set; } = "category";
    public bool IsCategoryFieldCollection { get; set; }
    public string InstructionTypeFieldName { get; set; } = "instruction_type";
    public bool IsInstructionTypeFieldCollection { get; set; }
    public string MandatoryFieldName { get; set; } = "is_mandatory";
    public string OrderFieldName { get; set; } = "order";
    public string TagsFieldName { get; set; } = "tags";
    public int DefaultTopK { get; set; } = 6;
    public int MandatoryChunkLimit { get; set; } = 12;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Endpoint) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(IndexName) &&
        !string.IsNullOrWhiteSpace(VectorFieldName);
}
