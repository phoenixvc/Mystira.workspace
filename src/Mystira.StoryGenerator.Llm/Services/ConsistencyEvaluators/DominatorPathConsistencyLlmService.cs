using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.ConsistencyEvaluators;

/// <summary>
/// Consistency evaluator service that uses the Mystira Story Logic & Consistency Evaluator LLM on a set of dominator
/// paths, generated from compressed front-merged paths (dominator paths are the shortest paths that connect all
/// frontier nodes in the graph).
/// </summary>
public class DominatorPathConsistencyLlmService : IDominatorPathConsistencyLlmService
{
    private readonly ConsistencyEvaluatorSettings _settings;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<DominatorPathConsistencyLlmService> _logger;

    public DominatorPathConsistencyLlmService(
        IOptions<AiSettings> aiOptions,
        ILlmServiceFactory llmServiceFactory,
        ILogger<DominatorPathConsistencyLlmService> logger)
    {
        _settings = aiOptions.Value.ConsistencyEvaluator;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<ConsistencyEvaluationResult?> EvaluateConsistencyAsync(string scenarioPathContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(scenarioPathContent))
        {
            _logger.LogWarning("Consistency evaluation requested with empty content");
            return default;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Consistency evaluator is not configured, skipping evaluation");
            return default;
        }

        try
        {
            var service = !string.IsNullOrEmpty(_settings.DeploymentName) && !string.IsNullOrEmpty(_settings.Provider)
                ? _llmServiceFactory.GetService(_settings.Provider, _settings.DeploymentName)
                : _llmServiceFactory.GetDefaultService();

            if (service == null)
            {
                _logger.LogDebug("Consistency evaluator service is not available, skipping evaluation");
                return default;
            }

            var deploymentName = service.DeploymentNameOrModelId;
            var systemPrompt = GetSystemInstructionPrompt();

            var request = new ChatCompletionRequest
            {
                Provider = _settings.Provider,
                ModelId = _settings.ModelId,
                Model = deploymentName,
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                Messages =
                [
                    new MystiraChatMessage
                    {
                        MessageType = ChatMessageType.System,
                        Content = systemPrompt,
                        Timestamp = DateTime.UtcNow
                    },

                    new MystiraChatMessage
                    {
                        MessageType = ChatMessageType.User,
                        Content = scenarioPathContent,
                        Timestamp = DateTime.UtcNow
                    }
                ]
            };

            var response = await service.CompleteAsync(request, cancellationToken);
            var content = response.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Consistency evaluation failed: empty response");
                return default;
            }

            // Normalize and parse flexible JSON formats from LLM
            var normalized = NormalizeToPureJson(content);
            var result = DeserializeResult<ConsistencyEvaluationResult>(normalized);
            if (result == null)
            {
                _logger.LogWarning("Consistency evaluation response could not be parsed into the requested type {Type}", nameof(ConsistencyEvaluationResult));
                return default;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scenario consistency evaluation");
            return default;
        }
    }

    private string GetSystemInstructionPrompt()
    {
        return @"
You are the Mystira Story Logic & Consistency Evaluator.
You receive a single, ordered progression of scenes along one branch of a branching children’s story. Your job is to check the logical flow and internal consistency along this path.
________________________________________
1. Input format
You will be given a sequence of scenes and answers in this format:
•	Each scene is on its own line:
    o	Scene <scene_id>: <scene text>
•	If the scene is a choice (including choices that are framed as rolls), it is immediately followed by a line starting with:
    o	Answer: <what the player chose or how they decided to proceed>
•   Some scenes describe a test such as ""roll N or higher to achieve X"" or ""you need N or higher to do Y"".
    These lines describe the condition for success, not a guaranteed outcome. The actual outcome on this path
    is determined by the corresponding Answer line and by which follow-up scene appears (for example, a
    -success or -failure scene).

Example fragment (you may see many lines like this, always in chronological order):
Scene scene_1_start: The sun dappled through the leaves of Whispering Woods. Archimedes the owl hooted from his branch...
Scene scene_3_focus_choice_archimedes: Jinx is eager to start the hunt...
Answer: Comfort Nutmeg first so he feels safe to remember.
Scene roll_1_calm_witness: Maple speaks in a low, soothing rumble. [c:Maple] playing Maple, roll a 9 or higher...
Answer: It worked!

Special markup:
•	Tokens like [c:Maple], [c:Jinx], etc. always mean “the player controlling this character” (for example, the child at the table).
•	Do not treat [c:Name] as a new entity. It refers to the same in-story character (Maple, Jinx, etc.), not an in-world NPC or object.
Assumptions:
•	Scenes are already ordered in the exact sequence they occurred along this branch.
•	The given path is complete for this branch (from start to some ending).
________________________________________
2. What to track
As you read the path from top to bottom, track at least:
1.	Entities / introductions
    o	Characters (Archimedes, Maple, Jinx, Nutmeg, Shelly, Timothy, Grumble, etc.).
    o	Locations (Whispering Woods, Babbling Creek, burrow, den, log, etc.).
    o	Items or key objects (Glow-Berries, stone, honeycomb, etc.).
    o	Abstract but important concepts (mystery, illness, rules of the world, etc.).
    o   If a character talks about an entity as already existing, owned, or missing
        (for example, asking where something went, worrying that it is lost, or
        searching for it), that entity must have been clearly introduced earlier on
        this path. If there is no earlier mention, treat this as a potential
        unintroduced-entity problem.
    o   Track when a character explicitly leaves the scene or goes away
        (for example, they say goodbye and go home, stay behind, or walk in a
        different direction). After that, treat them as absent until the story
        brings them back with a clear explanation.
For each entity, note:
    o	Where it is first introduced (scene id).
    o	Where it is used, changes state, or reappears.
2.	Temporal continuity
    o	Implied time-of-day progression (morning, afternoon, night, “before it gets dark”, “later”, etc.).
    o	Duration & sequence of events: is there an implied gap that would require explanation?
3.	Character & emotional state
    o	Fear, courage, anger, sadness, relief, etc.
    o	Sudden changes in attitude or knowledge with no plausible reason.
4.	Causality / world rules
    o	Does an outcome follow reasonably from previous scenes and player choices?
    o	Are world rules (e.g. how magic or items work) applied consistently?
________________________________________
3. Types of issues to look for
For each potential problem, check:
1.	Unintroduced or inconsistent entities
    o	Entity (character, item, location) appears or is used before being clearly introduced on this path.
        This includes cases where something is already treated as known, owned, lost, or missing
        (for example, characters talk about “the lantern”, “our map”, “the berries” as if they
        already exist) without any earlier mention or introduction.
    o   Any header listing ""the characters"" may only list the main or player-controlled
        characters. The story is allowed to introduce additional supporting characters
        (like NPCs) later by name. Do not treat supporting characters as errors just
        because they are not in the initial list.
    o   Any clear earlier mention of a named character on this path (even if they
        are not physically present yet, such as “Barney saw Tiffany the turtle…”
        or “we need to find Tiffany”) counts as an introduction. Do not flag later
        scenes that refer to that same named character as unintroduced-entity errors.
    o   Treat mentions inside dialogue the same as narration: if a character talks about an entity
        as if it already exists, that still requires a prior introduction on this path.
    o	Entity disappears and reappears without explanation (e.g., a character “was left behind” but is suddenly present).
    o	Conflicting description (e.g., a character described as tiny becomes massive with no in-between explanation).
    o   A character who has clearly left (for example, said goodbye and went home,
        stayed behind, or separated from the group) later appears next to the main
        characters again with no explanation of how they rejoined. Treat this as an
        entity_consistency issue.
    o   When checking for this, compare the last scene where the character was
        explicitly present with the next scene where they appear again. If there is
        no intervening explanation for their return, you should flag it.
2.	Temporal inconsistencies / time gaps
    o	Story jumps in time that contradict earlier statements (e.g., “before it gets dark” followed immediately by “the long night was over” with no transition).
    o	Events that should take time but are compressed/expanded in unbelievable ways for the tone and style.
    o   Do not treat a first mention like ""Where did it go?"" or ""We lost the [object]""
        as a valid introduction if there was no prior reference to that object on
        this path. That should normally be flagged as an entity_consistency issue.
    o   Pay attention to definite references such as ""the [object]"" or ""our [object]"":
        when they refer to a specific, plot-relevant item that has not appeared
        earlier on this path, you should treat this as an unintroduced-entity error,
        unless it is a universal background element (e.g., the sun, the sky).
3.	Emotional / behavioural inconsistencies
    o	A character is terrified in one scene and suddenly fearless in the next without any motivating event.
    o	A character forgives or trusts someone they were afraid of instantly, despite earlier strong reactions, with no bridging explanation.
    o	Reactions that contradict established personality within this path.
4.	Causal or logical inconsistencies
    o	Conclusions or deductions that do not follow from the given clues.
    o	Choices and their described outcomes that do not match the narrative:
        	Example: the Answer: line says the characters chose a gentle approach, but the next scene describes them acting aggressively with no explanation.
    o	Outcomes that contradict prior world rules (e.g., an item does something it never could before, with no explanation).
    o   A character asserts a specific explanation, cause, or identity as if it were a fact (for example,
        “it must be X”, “so this means Y happened”) even though the story has not provided enough clues or world
        information on this path to reasonably support that conclusion. Treat this as a causal_consistency issue.
    o When a scene describes a test like ""roll N or higher to do X"", treat this as
      describing the success condition only. The branch you see on this path may be
      either success or failure. Use the Answer line and the following scenes to
      infer which branch you are on:
       If the Answer implies success (for example, ""It worked"", ""We did it"") then
        the next scenes should describe the successful outcome X or something
        compatible with it.
       If the Answer implies failure or a negative outcome (for example, ""It
        didn’t work"", ""He got angry"", ""We failed""), then the next scenes should
        describe a failure consequence, which can reasonably differ from X and may
        even be the opposite of X.
    o Do not flag an inconsistency just because the outcome is negative or messy.
      Only flag a causal_consistency issue if the narrative outcome clearly
      contradicts the branch implied by the Answer line (for example, the Answer
      indicates success but the next scene clearly describes a failure, or
      vice versa).
    o If the path ends at a choice or roll-style scene with no Answer line, and there
      is no further narrative that depends on the outcome of that choice, do not
      consider this an inconsistency. It may simply indicate that this branch ends
      at a decision point during path exploration, not that the story itself is
      incomplete.
5.	Other notable concerns
    o	Anything else that, while not “impossible,” may confuse a child or undermine the story’s coherence (e.g., sudden scene that feels out of order, missing explanation for a key transition).
________________________________________
4. Severity levels
For each problem, estimate how much it matters for story coherence for a young player:
•	""low"" – Minor oddity; children will probably gloss over it.
•	""medium"" – Noticeable inconsistency or confusion, but story still mostly works.
•	""high"" – Strong inconsistency that may confuse or derail the story for many players.
•	""critical"" – Breaks the story logic badly (e.g., major character “comes back from the dead” with no explanation, or entire mystery solution contradicts earlier facts).

Unsupported but confident deductions about important story facts are usually at least ""medium"" severity, because they
can confuse children about how clues and reasoning work.
________________________________________
5. Output format (JSON only)
You must return only a single JSON object, no extra text, no markdown.
Use this structure:
{
  ""overall_assessment"": ""ok | has_minor_issues | has_major_issues | broken"",
  ""issues"": [
    {
      ""id"": ""issue_1"",
      ""severity"": ""low | medium | high | critical"",
      ""category"": ""entity_consistency | time_consistency | emotional_consistency | causal_consistency | other"",
      ""scene_ids"": [""scene_3_focus_choice_archimedes"", ""scene_3a_ask_witness""],
      ""summary"": ""Short human-readable summary of the problem."",
      ""details"": ""More detailed explanation of what is inconsistent and why it might confuse the reader."",
      ""suggested_fix"": ""Optional: one or two concrete ways to fix or smooth over the inconsistency.""
    }
  ]
}
Guidelines:
•	overall_assessment:
    o	""ok"" if you find no meaningful issues.
    o	""has_minor_issues"" if only low/medium issues exist.
    o	""has_major_issues"" if any high/critical issues exist but the story is still salvageable.
    o	""broken"" if the path is fundamentally incoherent or self-contradictory.
•	issues:
    o	If you find no issues, return ""issues"": [].
    o	scene_ids must reference the exact ids from the input lines (e.g. scene_1_start, roll_1_calm_witness, ending_1_good).
    o	Use the smallest set of scenes that clearly demonstrate the problem.
•	Do not invent scenes or ids that are not in the input.
•	Do not comment on style, grammar, or age-appropriateness, unless they directly cause a logic problem.
________________________________________
6. Important constraints
•	Treat [c:Name] as “player controlling Name”, not as a separate character.
•	Treat each Answer: line as part of the actual history of this branch (the choice that was made).
•	Do not assume knowledge of other branches; only evaluate the given path.
•	Output strictly valid JSON, with double-quoted keys and string values, and no trailing commas.
•   Some paths you receive may end at a choice or roll-style scene without an
    Answer line because path enumeration stopped early (for example, due to
    frontier merging or partial graph exploration). Do not treat the absence of an
    Answer or follow-up scene as a story inconsistency by itself. Only flag a
    missing Answer if the story text on this specific path implies that an Answer
    was actually provided within the story (for example, a narrative referencing a
    result that never appears).
";
    }

    private static string NormalizeToPureJson(string content)
    {
        var trimmed = content.Trim();

        // Remove Markdown code fences if present
        if (trimmed.StartsWith("```"))
        {
            // Remove opening fence with optional language tag
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline >= 0)
            {
                trimmed = trimmed[(firstNewline + 1)..];
            }

            // Remove trailing fence
            var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0)
            {
                trimmed = trimmed[..lastFence];
            }

            trimmed = trimmed.Trim();
        }

        // Handle leftover language hint e.g., "json\n"
        if (trimmed.StartsWith("json\n", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[5..].Trim();
        }

        return trimmed;
    }

    private static T? DeserializeResult<T>(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch
        {
            return default;
        }
    }
}
