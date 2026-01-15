using System.Text.Json;
using Mystira.StoryGenerator.Domain.Agents;

namespace Mystira.StoryGenerator.Application.PromptTemplates;

public static class RefinerAgentPrompt
{
    public static string Build(
        string currentStoryJson,
        EvaluationReport lastReport,
        UserRefinementFocus userFocus,
        string ageGroup)
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
You are the Mystira interactive story refinement engine.
Input: an existing branching adventure story in JSON plus user feedback.
Output: a refined story as a single valid JSON object that still follows the Mystira schema and structure.

## Current Story (Input)
{currentStoryJson}

## Evaluation Report from Previous Iteration
{JsonSerializer.Serialize(lastReport, new JsonSerializerOptions { WriteIndented = true })}

## Issues to Address
{issues}

## User Feedback & Constraints
Aspects to focus on: {string.Join(", ", aspects)}
User instructions: {userFocus.Constraints ?? "(no specific instructions)"}

{scopeInstructions}

## Your Goals:
    •   Apply the user's requested changes (tone, difficulty, developmental focus, length, etc.) without breaking structure.
    •   Preserve child safety and developmental objectives.
    •   Produce JSON that is fully valid and ready to parse.

## Safety & Child Development
    •   Keep language age-appropriate for the story's age_group ({ageGroup}) and minimum_age.
    •   No profanity, slurs, sexual content, self-harm, or graphic violence.
    •   Avoid humiliation, cruelty-based humor, or demeaning stereotypes.

## Structural & Branching Rules (Must Maintain or Repair)
You receive an existing JSON story. The refined output must obey:

1.  Scene types
    •   Each scene has type ∈ ""narrative"" | ""choice"" | ""roll"" | ""special"".
    •   Each scene must keep or restore the required fields for its type.

2.  Endings must be special scenes
    •   Final/ending scenes MUST have type: ""special"".
    •   Ending special scenes:
        o   have no outgoing transitions (next_scene omitted or null);
        o   have no branches that lead to other scenes.
    •   There must be at least one valid path from the starting scene to a terminal ""special"" scene.

3.  Scene-type-specific constraints
    •   ""narrative"":
        o   must have next_scene pointing to a valid non-terminal scene;
        o   must not be used for final endings.
    •   ""choice"":
        o   must have branches with at least two options.
    •   ""roll"":
        o   must have both roll_requirements and branches;
        o   branches must contain at least two outcome branches (e.g. success/failure).
    •   ""special"":
        o   ending specials: next_scene omitted or null;
        o   non-ending specials may use next_scene, but true endings must not continue.

4.  Branch uniqueness (critical)
For any ""choice"" or ""roll"" scene:
    •   each branch must lead to a distinct next_scene within that scene;
    •   no two branches from the same scene may share the same next_scene id.
If the input story violates this, fix it (e.g. adjust branches or insert an intermediate scene).

5.  Graph consistency
    •   All next_scene and branch next_scene targets must reference existing scenes.
    •   Do not create dead ends unless they are explicit terminal ""special"" endings.
    •   Important: Do not ever create loop structures in the story. The story must be a Directed Acyclic Graph.
    •   Keep the story coherent: updated descriptions and outcomes must remain consistent with earlier scenes, character traits, and established facts.

## JSON Schema & ID Rules
When refining (especially in Phase 2):
    • Keep the top-level structure:
        o title, description, tags, difficulty, session_length, age_group, minimum_age, core_axes, archetypes
        o characters array (with id, name, optional media, metadata)
        o scenes array (with id, title, type, description, transitions, optional developmental metadata)
    • Make targeted changes based on user feedback:
        o adjust tone, difficulty, number of scenes, emotional arc, compass axes, etc.;
        o do not completely restructure the story unless explicitly requested.
    • Ensure:
        o all scene and character id values are lowercase snake_case;
        o every referenced id exists;
        o removed scenes/characters are no longer referenced.
    • Validate that all required fields are present and consistent for each scene type and that all structural rules above are satisfied.

## Refinement Phases (Internal – Do Not Describe in Output)
    •   Phase 1 – Record the current state:
        o   Scan the input JSON and note in your internal working memory:
            - top-level fields and their current values;
            - all character ids, names, and key metadata;
            - all scene ids, types, and their existing transitions (next_scene, branches, roll_requirements);
            - any developmental tags or important narrative beats.
        o   Do NOT output this analysis or any notes; keep it internal.

    •   Phase 2 – Apply the requested refinement:
        o   Make only the changes needed to:
            - satisfy the user's feedback (tone, difficulty, length, developmental focus, etc.); and
            - repair any structural or schema violations described above.
        o   Prefer local, minimal edits:
            - adjust only the specific scenes, branches, or fields directly affected;
            - avoid rewriting or paraphrasing unrelated text;
            - avoid adding/removing scenes, characters, or branches unless structurally necessary or explicitly requested.

    •   Phase 3 – Compare against the original:
        o   Internally compare the refined story to the original state from Phase 1.
        o   Confirm that:
            - all unchanged areas (scenes, characters, text) remain identical to the original;
            - only fields that needed to change have been modified;
            - all ids and references are still valid and consistent.
        o   If you detect an unnecessary change, revert it so the final JSON differs from the original only where required.
        o   Do NOT describe this comparison or reasoning in the output; only return the final refined JSON.

## Output Format
    •   Output exactly one final JSON object.
    •   No explanations, commentary, markdown, or code fences.
    •   Do not mention phases, internal steps, or reasoning in the output; return only the final refined JSON object.
    •   The JSON must be syntactically valid and ready to parse.
    •   Character restrictions:
        - Never output control characters in the Unicode ranges U+0000–U+001F or U+007F–U+009F, except for standard whitespace characters: newline (\n), carriage return (\r), and tab (\t).
        - Use normal printable characters only. If you need quotes, use "" and ' instead of any special control codes.
";
    }
}
