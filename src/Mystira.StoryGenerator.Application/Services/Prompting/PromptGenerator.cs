using System.Text.Json;
using Mystira.StoryGenerator.Application.Infrastructure.Agents;
using Mystira.StoryGenerator.Application.PromptTemplates;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.Services.Prompting;

public class PromptGenerator : IPromptGenerator
{
    private readonly IProjectGuidelinesService _guidelines;
    private readonly IKnowledgeProvider _knowledge;

    public PromptGenerator(IProjectGuidelinesService guidelines, IKnowledgeProvider knowledge)
    {
        _guidelines = guidelines;
        _knowledge = knowledge;
    }

    public string GenerateWriterPrompt(string storyPrompt, string ageGroup, List<string> axes)
    {
        var guidelines = _guidelines.GetForAgeGroup(ageGroup);
        var knowledge = _knowledge.GetContextualGuidance(ageGroup);  // Pass age group
        return WriterAgentPrompt.Build(storyPrompt, ageGroup, axes, knowledge, guidelines);
    }

    public string GenerateJudgePrompt(string storyJson, string ageGroup, List<string> axes)
    {
        var developmentPrinciples = _guidelines.GetDevelopmentPrinciples(ageGroup);
        var safetyGuidelines = _guidelines.GetSafetyGuidelines(ageGroup);
        var knowledge = _knowledge.GetContextualGuidance(ageGroup);  // Pass age group
        return JudgeAgentPrompt.Build(storyJson, ageGroup, axes, knowledge, developmentPrinciples, safetyGuidelines);
    }

    public string GenerateRefinerPrompt(string storyJson, EvaluationReport report, UserRefinementFocus focus)
    {
        var ageGroup = TryExtractAgeGroup(storyJson) ?? "6-9";
        var knowledge = _knowledge.GetContextualGuidance(ageGroup);  // Pass age group
        return RefinerAgentPrompt.Build(storyJson, report, focus, ageGroup, knowledge);
    }

    public string GenerateRubricPrompt(string storyJson, EvaluationReport report, int iteration)
    {
        var ageGroup = TryExtractAgeGroup(storyJson) ?? "6-9";
        var knowledge = _knowledge.GetContextualGuidance(ageGroup);  // Pass age group
        return RubricSummaryAgentPrompt.Build(storyJson, report, iteration, ageGroup, knowledge);
    }

    private static string? TryExtractAgeGroup(string storyJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(storyJson);
            if (!doc.RootElement.TryGetProperty("metadata", out var metadata))
                return null;

            if (!metadata.TryGetProperty("age_group", out var ageGroupProp))
                return null;

            return ageGroupProp.GetString();
        }
        catch
        {
            return null;
        }
    }
}
