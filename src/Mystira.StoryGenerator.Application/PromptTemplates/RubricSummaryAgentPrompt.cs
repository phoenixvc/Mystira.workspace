using System.Text.Json;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.PromptTemplates;

public static class RubricSummaryAgentPrompt
{
    public static string Build(
        string storyJson,
        EvaluationReport evaluationReport,
        int iterationNumber,
        string ageGroup)
    {
        return $@"
You are a UX-friendly summary writer. Your task is to create a concise, user-facing progress report on the story's quality.

## Story & Evaluation
Story (iteration {iterationNumber}):
{storyJson}

Evaluation Report:
{JsonSerializer.Serialize(evaluationReport)}

## Your Task
Write a brief, non-technical summary of:
1. **What's Working**: Highlight strengths (2-3 sentences)
2. **What Needs Work**: Summarize the key issues (2-3 sentences)
3. **Next Steps**: Recommend refinement direction (1-2 sentences)

Then provide a JSON scorecard:
{{
  ""summary"": ""Combined narrative summary (150 words max)"",
  ""strengths"": [""strength1"", ""strength2""],
  ""concerns"": [""concern1"", ""concern2""],
  ""suggested_focus"": [""area1"", ""area2""],
  ""ready_for_publish"": boolean
}}

## Tone
- Encouraging and constructive
- Use simple, non-technical language suitable for a {ageGroup} age group audience
- Avoid jargon

Output ONLY the JSON scorecard. No additional text.
";
    }
}
