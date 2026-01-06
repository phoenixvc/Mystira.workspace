using System.ComponentModel.DataAnnotations;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Api.Models;

/// <summary>
/// Request for starting a new story generation session.
/// </summary>
public class StartSessionRequest
{
    /// <summary>
    /// The user's story prompt.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string StoryPrompt { get; set; } = string.Empty;

    /// <summary>
    /// The knowledge retrieval mode.
    /// </summary>
    [Required]
    public string KnowledgeMode { get; set; } = string.Empty;

    /// <summary>
    /// Target age group for the story.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(50)]
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Optional narrative axes to emphasize.
    /// </summary>
    public List<string> TargetAxes { get; set; } = new();

    /// <summary>
    /// Validates that the knowledge mode is one of the allowed values.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(KnowledgeMode))
        {
            yield return new ValidationResult("KnowledgeMode is required", new[] { nameof(KnowledgeMode) });
        }
        else if (!Enum.TryParse<KnowledgeMode>(KnowledgeMode, out var mode))
        {
            yield return new ValidationResult("KnowledgeMode must be either 'FileSearch' or 'AISearch'", new[] { nameof(KnowledgeMode) });
        }
    }
}