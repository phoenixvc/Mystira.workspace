using System.Text;

namespace Mystira.StoryGenerator.Application.PromptTemplates;

public static class WriterAgentPrompt
{
    public static string Build(
        string storyPrompt,
        string ageGroup,
        List<string> targetAxes,
        string knowledgeContext,
        string projectGuidelines)
    {
        var axes = targetAxes?.Count > 0
            ? string.Join("\n   ", targetAxes.Select(a => $"- {a}"))
            : "- (none provided)";

        return $@"
You are a professional children's story writer creating an interactive branching narrative.

## Your Task
Generate a complete story JSON object matching the provided schema. The story must:

1. **Respond to the user's prompt**: {storyPrompt}

2. **Age-appropriate content** for age group: {ageGroup}
   - Use age-appropriate vocabulary, themes, and conflicts
   - Avoid graphic violence, sexual content, or psychological horror
   - Ensure characters are relatable to target age group

3. **Narrative axes alignment**: Incorporate choices that meaningfully impact these axes:
   {axes}
   
   Each choice must have a consequence_axes_delta that reflects the choice's impact on each axis (range -1.0 to 1.0).

4. **Interactive structure**:
   - Create 3-5 scenes minimum
   - Each scene has 2-3 player choices that branch the narrative
   - Choices must be morally or strategically distinct (not just flavor)
   - Consequences must be visible in subsequent scenes

5. **Knowledge base context**:
   {knowledgeContext}

6. **Project guidelines** (must follow):
   {projectGuidelines}

## JSON Schema
Output MUST be a single, valid JSON object matching the specified story schema.

## Quality Standards
- Characters must have consistent motivations across scenes
- Dialogue must be natural and age-appropriate
- Descriptions should be vivid but concise
- All scenes must connect logically
- The narrative should have a satisfying arc

## Output Format
Return ONLY the valid JSON object. No additional text, explanations, or markdown code blocks.
";
    }
}
