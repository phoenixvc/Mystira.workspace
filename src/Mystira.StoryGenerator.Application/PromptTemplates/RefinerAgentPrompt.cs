using System.Text.Json;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.PromptTemplates;

public static class RefinerAgentPrompt
{
    public static string Build(
        string currentStoryJson,
        EvaluationReport lastReport,
        UserRefinementFocus userFocus,
        string ageGroup,
        string knowledgeContext)
    {
        var scopeInstructions = userFocus.TargetSceneIds.Any()
            ? $@"
## TARGETING CONSTRAINT
You MUST only edit these scenes: {string.Join(", ", userFocus.TargetSceneIds)}
You MUST NOT modify any other scenes.
Preserve the exact JSON for all other scenes and characters.
Preserve all narrative and structural elements outside your target scope.
"
            : @"
## FULL REWRITE MODE
You may rewrite the entire story while preserving the core plot and characters.
";

        var issues = lastReport.Findings.Any()
            ? string.Join("\n", lastReport.Findings.SelectMany(kvp => kvp.Value).Select(f => $"- {f}"))
            : "- (no findings provided)";

        var aspects = userFocus.Aspects.Any() ? userFocus.Aspects : new List<string> { "overall" };

        return $@"
You are an expert story refiner. Your task is to fix issues in a branching narrative story.

## Current Story
{currentStoryJson}

## Evaluation Report from Previous Iteration
{JsonSerializer.Serialize(lastReport)}

## Issues to Address
{issues}

## User Feedback & Constraints
Aspects to focus on: {string.Join(", ", aspects)}
User instructions: {userFocus.Constraints ?? "(no specific instructions)"}

{scopeInstructions}

## Knowledge Base Context
{knowledgeContext}

## Refinement Instructions
1. Address the evaluation findings directly
2. Maintain {ageGroup} age-appropriateness
3. Keep character motivations and narrative arc intact
4. Ensure new choices have realistic axes impacts
5. Verify JSON schema compliance

## Output Format
Return ONLY a valid JSON story object. No explanations or additional text.
The output must be valid JSON that passes schema validation.
";
    }
}
