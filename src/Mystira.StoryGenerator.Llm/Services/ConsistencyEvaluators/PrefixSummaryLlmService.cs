using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Newtonsoft.Json;

namespace Mystira.StoryGenerator.Llm.Services.ConsistencyEvaluators;

/// <summary>
/// Consistency evaluator that summarizes a prefix of a story path into a compact world-state summary.
/// </summary>
public class PrefixSummaryLlmService : IPrefixSummaryLlmService
{
    private readonly PrefixSummarySettings _settings;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<PrefixSummaryLlmService> _logger;

    public PrefixSummaryLlmService(
        IOptions<AiSettings> aiOptions,
        ILlmServiceFactory llmServiceFactory,
        ILogger<PrefixSummaryLlmService> logger)
    {
        _settings = aiOptions.Value.PrefixSummary;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<ScenarioPathPrefixSummary?> SummarizeAsync(IEnumerable<Scene> scenePath, CancellationToken cancellationToken = default)
    {
        var path = scenePath.ToArray();

        if (!path.Any())
        {
            _logger.LogWarning("Prefix summary requested with empty scene path");
            return null;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Prefix summary engine is not configured, skipping classification");
            return null;
        }

        try
        {
            var service = !string.IsNullOrEmpty(_settings.DeploymentName) && !string.IsNullOrEmpty(_settings.Provider)
                ? _llmServiceFactory.GetService(_settings.Provider, _settings.DeploymentName)
                : _llmServiceFactory.GetDefaultService();

            if (service == null)
            {
                _logger.LogDebug("Entity classifier is not configured, skipping classification");
                return null;
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
                        Content = GetUserPrompt(path),
                        Timestamp = DateTime.UtcNow
                    }
                ],
                JsonSchemaFormat = new JsonSchemaResponseFormat {SchemaJson = GetJsonSchemaFormat()}
            };

            var response = await service.CompleteAsync(request, cancellationToken);
            var content = response.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Entity classification failed: empty response");
                return null;
            }

            var summary = JsonConvert.DeserializeObject<ScenarioPathPrefixSummary>(content);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scene entity classification");
            return null;
        }
    }

    private string GetUserPrompt(IEnumerable<Scene> scenePath)
    {
        var input = new PrefixSummaryInput
        {
            PrefixScenes = scenePath.Select(s => new PrefixScene {Id = s.Id, Description = s.Description}).ToArray()
        };

        var ret = JsonConvert.SerializeObject(input);
        return ret;
    }

    private string GetSystemInstructionPrompt()
    {
        return @"
You are the Mystira Prefix Summary Engine.
Your job is to read a sequence of scenes that form a prefix of a branching story path and produce a compact world-state summary for that prefix.
You must only output JSON, with no explanations or extra text.
________________________________________
1. Input
You will receive a JSON object with this shape:
    •	prefix_scenes: an array of scenes, in chronological order, each with:
        o	id (string) – the scene id (e.g., ""scene_start"", ""scene_03_river"")
        o	description (string) – the player-facing scene text
Example input (schema only):
{
  ""prefix_scenes"": [
    {
      ""id"": ""scene_start"",
      ""description"": ""...""
    },
    {
      ""id"": ""scene_forest_path"",
      ""description"": ""...""
    }
  ]
}
You must treat this as the entire story so far along one path (a front-merged prefix).
________________________________________
2. Task
From all scenes in prefix_scenes, you must:
    1.	Identify story-relevant entities accumulated over the prefix.
    2.	For each entity, summarize:
        o	what it is,
        o	when it was first introduced,
        o	whether it should still “exist” or be relevant at the end of the prefix,
        o	which characters know about it (if clear).
    3.	Provide a light time summary for the prefix.
You are not judging style or quality; you are building a state snapshot that later checks can use to detect inconsistencies.
________________________________________
3. What counts as an entity
Use the same entity notions as other Mystira tools:
    •	character – people, creatures, named personified beings
    •	location – places, regions, buildings, landmarks
    •	item – concrete objects that matter to the story (kept, carried, used, sought)
    •	concept – organizations, factions, events, rituals, laws, oaths, abstract forces/ideals that are clearly important
Follow the same inclusion/exclusion rules as usual:
    •	Include named / clearly distinct / story-relevant entities that can be referenced later.
    •	Do not include generic background clutter (tables, doors, generic rain, generic fear, etc.).
    •	When a named character is followed by a generic type/species/title (e.g., “Patty the panther”), treat as one character entity with name ""Patty"" (not ""Patty the panther"" and not ""panther"" as a separate entity).
________________________________________
4. Entity Status & Knowledge
For each entity, you must estimate:
    •	status_at_end – one of:
        o	""active"" – still present/relevant in the story context at the end of the prefix.
        o	""absent_but_known"" – not physically present in the final scene, but clearly still exists and is known/remembered.
        o	""removed"" – dead, destroyed, lost forever, or explicitly gone in a final way.
        o	""unclear"" – unclear whether it remains relevant or not.
    •	first_scene_id – the id of the first scene in which this entity is clearly introduced.
    •	last_mention_scene_id – the id of the last scene in the prefix where the entity is clearly mentioned or implied.
You should also track who knows about what, when clearly implied:
    •	known_by – array of character names that clearly know of or have interacted with this entity by the end of the prefix.
        o	For characters themselves, known_by usually includes the character and any other characters who have clearly met or interacted with them.
________________________________________
5. Time Summary for the Prefix
You must estimate the overall time span covered by the entire prefix:
    •	prefix_time_span – one of:
        o	""none"" – essentially continuous; no clear jumps.
        o	""short"" – within roughly the same day; hours at most.
        o	""long"" – days, weeks, months, or more.
Base this on explicit or strongly implied cues across all scenes (“a few hours later”, ""the next day"", ""years later"", etc.).
If uncertain, default to ""none"".
________________________________________
6. Output Format
You must always output a single JSON object with exactly these top-level fields:
    •	prefix_time_span – ""none"", ""short"", or ""long"".
    •	entities – array of entity objects.
Each entity object must have exactly these fields:
    •	name (string) – canonical name of the entity (e.g., ""Alex"", ""Grand Market"", ""Shadow Guild"").
    •	type (string) – ""character"", ""location"", ""item"", or ""concept"".
    •	is_proper_noun (boolean) – whether it functions like a proper name.
    •	first_scene_id (string) – id of the scene where this entity is first clearly introduced.
    •	last_mention_scene_id (string) – id of the last scene in the prefix where this entity is clearly mentioned or implied.
    •	status_at_end (string) – ""active"", ""absent_but_known"", ""removed"", or ""unclear"".
    •	known_by (array of strings) – names of characters that clearly know about this entity by the end of the prefix (can be empty).
    •	notes (string) – brief 1–2 sentence summary of the entity’s current narrative role/state, suitable for downstream consistency checks.
If there are no valid entities, return:
{
  ""prefix_time_span"": ""none"",
  ""entities"": []
}
No extra fields. No comments. No surrounding text.
________________________________________
Example
Input to the Prefix Summary Engine
{
  ""prefix_scenes"": [
    {
      ""id"": ""scene_start"",
      ""description"": ""Alex studies the worn map in his cottage. Tomorrow, he will finally leave for the Crystal Pass.""
    },
    {
      ""id"": ""scene_village_gate"",
      ""description"": ""At dawn, Alex meets Bob at the village gate of Brindlebrook. Bob adjusts his pack and grins. \""Ready for the Crystal Pass?\"" he asks.""
    },
    {
      ""id"": ""scene_forest_path"",
      ""description"": ""A few hours later, Alex and Bob follow the forest path toward the mountains. The trees thin as the first snowy peaks of the Crystal Pass appear in the distance.""
    }
  ]
}
Expected Output JSON
{
  ""prefix_time_span"": ""short"",
  ""entities"": [
    {
      ""name"": ""Alex"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""first_scene_id"": ""scene_start"",
      ""last_mention_scene_id"": ""scene_forest_path"",
      ""status_at_end"": ""active"",
      ""known_by"": [""Alex"", ""Bob""],
      ""notes"": ""Protagonist preparing to travel through the Crystal Pass; currently journeying with Bob along the forest path toward the mountains.""
    },
    {
      ""name"": ""Bob"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""first_scene_id"": ""scene_village_gate"",
      ""last_mention_scene_id"": ""scene_forest_path"",
      ""status_at_end"": ""active"",
      ""known_by"": [""Alex"", ""Bob""],
      ""notes"": ""Alex's companion who meets him at the village gate and travels with him toward the Crystal Pass.""
    },
    {
      ""name"": ""Brindlebrook"",
      ""type"": ""location"",
      ""is_proper_noun"": true,
      ""first_scene_id"": ""scene_village_gate"",
      ""last_mention_scene_id"": ""scene_village_gate"",
      ""status_at_end"": ""absent_but_known"",
      ""known_by"": [""Alex"", ""Bob""],
      ""notes"": ""The village where Alex and Bob begin their journey; they have already departed by the end of the prefix.""
    },
    {
      ""name"": ""Crystal Pass"",
      ""type"": ""location"",
      ""is_proper_noun"": true,
      ""first_scene_id"": ""scene_start"",
      ""last_mention_scene_id"": ""scene_forest_path"",
      ""status_at_end"": ""absent_but_known"",
      ""known_by"": [""Alex"", ""Bob""],
      ""notes"": ""The mountain pass that is the goal of Alex and Bob's journey; seen in the distance by the end of the prefix but not yet reached.""
    }
  ]
}
";
    }

    public string GetJsonSchemaFormat()
    {
        return @"
{
  ""type"": ""object"",
  ""title"": ""MystiraPrefixSummary"",
  ""description"": ""Canonical entity + state summary for a front-merged prefix of scenes."",
  ""properties"": {
    ""prefix_scene_ids"": {
      ""type"": ""array"",
      ""description"": ""Ordered list of scene ids that form this prefix (from start to current join node)."",
      ""items"": { ""type"": ""string"" },
      ""minItems"": 1
    },

    ""prefix_summary"": {
      ""type"": ""string"",
      ""description"": ""Very short natural-language summary (2–5 sentences) of what has definitely happened and what the party knows by the end of this prefix.""
    },

    ""time_span"": {
      ""type"": ""string"",
      ""description"": ""Overall qualitative time range covered by this prefix."",
      ""enum"": [""none"", ""short"", ""long"", ""mixed"", ""uncertain""]
    },

    ""entities"": {
      ""type"": ""array"",
      ""description"": ""Canonical entities that are definitely established by the end of this prefix."",
      ""items"": {
        ""type"": ""object"",
        ""properties"": {
          ""canonical_name"": {
            ""type"": ""string"",
            ""description"": ""Canonical label for this entity (used consistently across scenes/branches).""
          },
          ""type"": {
            ""type"": ""string"",
            ""description"": ""Entity category."",
            ""enum"": [""character"", ""location"", ""item"", ""concept""]
          },
          ""is_proper_noun"": {
            ""type"": ""boolean"",
            ""description"": ""True if this entity behaves like a proper name/title in the story.""
          },

          ""first_introduced_scene_id"": {
            ""type"": ""string"",
            ""description"": ""Scene id where this entity is first clearly introduced in this prefix.""
          },
          ""introduction_evidence"": {
            ""type"": ""string"",
            ""description"": ""Short quote or paraphrase justifying that introduction scene."",
            ""maxLength"": 500
          },
          ""introduction_confidence"": {
            ""type"": ""string"",
            ""description"": ""How confident we are that this is the true first introduction."",
            ""enum"": [""high"", ""medium"", ""low""]
          },

          ""status_at_end"": {
            ""type"": ""string"",
            ""description"": ""Entity’s narrative status at the end of the prefix."",
            ""enum"": [
              ""active"",          // present and relevant
              ""removed"",         // destroyed / gone / dead / permanently left
              ""unknown"",         // unclear or not specified
              ""background_only""  // exists but not central
            ]
          },
          ""status_confidence"": {
            ""type"": ""string"",
            ""description"": ""Confidence in the status_at_end classification."",
            ""enum"": [""high"", ""medium"", ""low""]
          },

          ""known_to_player_party"": {
            ""type"": ""boolean"",
            ""description"": ""True if the player party definitely knows about this entity by the end of the prefix.""
          },
          ""knowledge_confidence"": {
            ""type"": ""string"",
            ""description"": ""Confidence that the party knows this entity (vs only narrator/other NPCs)."",
            ""enum"": [""high"", ""medium"", ""low""]
          },

          ""role_tags"": {
            ""type"": ""array"",
            ""description"": ""Optional lightweight tags for downstream checking."",
            ""items"": {
              ""type"": ""string"",
              ""enum"": [
                ""protagonist"",
                ""supporting"",
                ""antagonist"",
                ""quest_giver"",
                ""major_location"",
                ""minor_location"",
                ""key_item"",
                ""flavor_item"",
                ""core_concept"",
                ""emotional_driver"",
                ""other""
              ]
            }
          },

          ""notes"": {
            ""type"": ""string"",
            ""description"": ""Optional compact notes / disambiguation useful for suffix checking."",
            ""maxLength"": 500
          }
        },
        ""required"": [
          ""canonical_name"",
          ""type"",
          ""is_proper_noun"",
          ""first_introduced_scene_id"",
          ""introduction_confidence"",
          ""status_at_end"",
          ""status_confidence"",
          ""known_to_player_party"",
          ""knowledge_confidence""
        ],
        ""additionalProperties"": false
      }
    }
  },
  ""required"": [""prefix_scene_ids"", ""entities""],
  ""additionalProperties"": false
}
";
    }

    private class PrefixSummaryInput
    {
        [JsonProperty("prefix_scenes")]
        public PrefixScene[] PrefixScenes { get; set; }
    }

    private class PrefixScene
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
