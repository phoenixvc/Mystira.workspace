using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Domain.Services;
using Newtonsoft.Json.Linq;

namespace Mystira.StoryGenerator.Llm.Services.DominatorBasedConsistency;

/// <summary>
/// LLM-based scene entity classifier. Uses Azure OpenAI to perform entity classification, i.e.,
/// extract and classify all entities introduced within a scene of an interactive branching story.
/// </summary>
public class SceneEntityLlmClassifier : IEntityClassificationService
{
    private readonly EntityClassifierSettings _settings;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<SceneEntityLlmClassifier> _logger;

    public SceneEntityLlmClassifier(
        IOptions<AiSettings> aiOptions,
        ILlmServiceFactory llmServiceFactory,
        ILogger<SceneEntityLlmClassifier> logger)
    {
        _settings = aiOptions.Value.EntityClassifier;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<EntityClassification?> ClassifyAsync(string sceneContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sceneContent))
        {
            _logger.LogWarning("Intent classification requested with empty query");
            return null;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("Intent classifier is not configured, skipping classification");
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
                        Content = sceneContent,
                        Timestamp = DateTime.UtcNow
                    }
                ]
            };

            var response = await service.CompleteAsync(request, cancellationToken);
            var content = response.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Entity classification failed: empty response");
                return null;
            }

            // Normalize and parse flexible JSON formats from LLM
            var normalized = NormalizeToPureJson(content);
            var classification = ParseEntityClassification(normalized);
            if (classification == null || classification.Entities.Length == 0)
            {
                _logger.LogWarning("Entity classification response did not contain recognizable entities payload");
                return null;
            }

            return classification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scene entity classification");
            return null;
        }
    }

    private string GetSystemInstructionPrompt()
    {
        return
            @"
You are the Mystira Entity Classification Engine.
Your job is to read a single scene description (1–5 sentences) and extract all story-relevant entities into a structured JSON array.

1. Task
From the input text, identify named or narratively important entities and classify each into one of these types:
* Character – people, creatures, or personified beings
* Location – places, regions, buildings, landmarks
* Item – concrete, manipulable objects with ongoing story relevance
* Concept – organizations, factions, events, rituals, laws, abstract forces or ideas that matter for the plot

You must only output JSON, no explanations.

2. Inclusion Rules
Include an entity only if:
* It is clearly distinct and identifiable in the story (can be referenced later by name or a clear label), and
* It is plausibly relevant to the ongoing narrative (not just generic background clutter).
Typically include:
*Characters:
** Named people/creatures: Alice, Captain Reyes, The Shadow King
** Title + name: Captain Reyes, Lord Harren, Professor Willow
**Personified non-humans with names: Whiskers the Cat, Blaze the Dragon
*Locations:
**Proper places: Grand Market, Tower of Dawn, Rivermoor, Crystal Forest
**Distinct in-world sites: Whispering Docks, Hall of Echoes
*Items:
**Named / special objects: Silver Key, Codex of Storms, Heartstone Amulet
**Distinct magical or plot-critical items even if not capitalized, if clearly unique in context.
*Concepts (abstract / institutional / event-like):
**Factions / organizations: Shadow Guild, Order of the Dawn, Council of Nine
**Events / rituals / festivals: Festival of Masks, Trial of Embers, Night of Falling Stars
**Ideals / oaths / codified rules when story-relevant: Code of Storms, Oath of Silence
**Mythic / systemic forces that matter to the plot: The Old Magic, The Great Silence, The Long Winter
**Emotional or psychological forces when they are clearly acting as significant internal drivers in the scene
3. Exclusion Rules
Do NOT create entities for:
*Generic, everyday objects: table, door, candles, bag, coins
*Generic natural phenomena and weather: rain, wind, thunder, snow, waves
**Exception: treat them as Concept only if they are clearly named, capitalized, and used like a unique force or myth (The Endless Rain, The Red Storm).
*Generic emotions or vague ideas: fear, hope, courage (unless clearly formalized as a named code/order: The Courage Pact)
*Verbs, actions, or descriptions: running, battle, music, glow
*Purely descriptive phrases that are not stable entities: the old wooden bridge (unless it behaves like a named, recurring landmark such as Old Wooden Bridge in a map-like sense)

If no valid entities are found, return an empty array [].

4. Proper Nouns vs. Non-Proper Nouns
*Set IsProperNoun based on how it appears and behaves in context:
*IsProperNoun = true if:
**It is capitalized like a name or title: Grand Market, Silver Key, Shadow Guild, Tower of Dawn, Festival of Masks.
**It functions as a unique in-world name, even if not obviously multi-word.
*IsProperNoun = false if:
**It is a generic label: the guard, the merchant, the village, the sword
**It is a unique instance but written as a common noun and not clearly used as a name.
**You may include non-proper entities only if they are clearly story-relevant and distinct (e.g., “the ancient dragon sealed below the city” in a later scene referencing “the dragon” again).

5. Confidence Score
Use the following strict rules to assign confidence to each extracted entity:
Confidence Levels
High — The entity is explicitly named, unambiguous, and fits cleanly into a single entity type. Example: “Captain Reyes,” “Silver Key,” “Rivermoor.”
Medium — The entity exists but has mild ambiguity in boundaries or type. Example: “The Hidden Circle,” “Ancient Ritual,” “River Guardians.”
Low — The entity is vague, generic, metaphorical, or only implicitly referenced. Example: “fear,” “magic,” “the storm” (when not a proper title).
Confidence Output Rules
*Always output one of: ""high"", ""medium"", ""low"".
*Confidence must reflect how certain the model is that the text refers to a distinct entity and that the entity type is correct.
*Confidence is never numerical; these three categories only.

6. Output Format
Always return a JSON array of entities.
Each entity must have exactly these fields:
*Name (string) – the text form of the entity as it appears (or the clearest canonical form)
*Type (string) – one of: ""Character"", ""Location"", ""Item"", ""Concept""
*IsProperNoun (boolean) – true or false
*Confidence (string) – ""high"", ""medium"", or ""low""

No extra fields. No comments. No surrounding text.

Examples

Example 1 — Characters, Location, Item
Input scene text:
Alice steps into the Grand Market, clutching the Silver Key.

Expected output JSON:

[
  {
    ""name"": ""Alice"",
    ""type"": ""character"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""Grand Market"",
    ""type"": ""location"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""Silver Key"",
    ""type"": ""item"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  }
]


Example 2 — Faction / Abstract Group as Concept
Input scene text:
The Shadow Guild extends its reach into Rivermoor.

Expected output JSON:

[
  {
    ""name"": ""Shadow Guild"",
    ""type"": ""concept"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""Rivermoor"",
    ""type"": ""location"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  }
]

Example 3 — Named Event and Tradition as Concepts
Input scene text:
During the Festival of Blossoms, the people of Dawnridge renew the Pact of Crows.

Expected output JSON:

[
  {
    ""name"": ""Festival of Blossoms"",
    ""type"": ""concept"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""Dawnridge"",
    ""type"": ""location"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""Pact of Crows"",
    ""type"": ""concept"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  }
]

Example 4 — Abstract Ideals as Concepts
Input scene text:
Fear tightens Leo’s chest, but his courage and hope push him to step forward.

Expected output JSON:

[
  {
    ""name"": ""Leo"",
    ""type"": ""character"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""fear"",
    ""type"": ""concept"",
    ""is_proper_noun"": false,
    ""confidence"": ""medium""
  },
  {
    ""name"": ""courage"",
    ""type"": ""concept"",
    ""is_proper_noun"": false,
    ""confidence"": ""medium""
  },
  {
    ""name"": ""hope"",
    ""type"": ""concept"",
    ""is_proper_noun"": false,
    ""confidence"": ""medium""
  }
]

Example 5 — Ambient Description, No Story-Relevant Entities
Input scene text:
Rain patters softly against the window until the candles finally go out.
Expected output JSON:

[]

(No durable characters, locations, items, or important concepts are introduced.)

Example 6 — Mixed Proper/Common Nouns, Items and Location
Input scene text:
At the edge of the Whispering Woods, the village healer wraps the Moonstone Shard in a strip of cloth.

Expected output JSON:

[
  {
    ""name"": ""Whispering Woods"",
    ""type"": ""location"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  },
  {
    ""name"": ""village healer"",
    ""type"": ""character"",
    ""is_proper_noun"": false,
    ""confidence"": ""medium""
  },
  {
    ""name"": ""Moonstone Shard"",
    ""type"": ""item"",
    ""is_proper_noun"": true,
    ""confidence"": ""high""
  }
]
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

    private static EntityClassification? ParseEntityClassification(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            var first = json.TrimStart();
            if (first.StartsWith("["))
            {
                var arr = JArray.Parse(json);
                var entities = arr.Select(ParseSceneEntityFromToken)
                                  .Where(e => e != null)
                                  .Cast<SceneEntity>()
                                  .ToArray();
                return new EntityClassification { Entities = entities };
            }

            var root = JObject.Parse(json);
            // Try common keys: entities / Entities
            var entitiesToken = root["entities"] ?? root["Entities"];
            if (entitiesToken is JArray jArr)
            {
                var entities = jArr.Select(ParseSceneEntityFromToken)
                                   .Where(e => e != null)
                                   .Cast<SceneEntity>()
                                   .ToArray();
                return new EntityClassification { Entities = entities };
            }

            // As a fallback, attempt direct mapping
            var fallback = root.ToObject<EntityClassification>();
            return fallback;
        }
        catch
        {
            return null;
        }
    }

    private static SceneEntity? ParseSceneEntityFromToken(JToken token)
    {
        try
        {
            if (token is not JObject o) return null;

            var name = o.Value<string>("name") ?? o.Value<string>("Name") ?? string.Empty;
            var typeStr = o.Value<string>("type") ?? o.Value<string>("Type");
            var isProper = o.Value<bool?>("is_proper_noun")
                           ?? o.Value<bool?>("is_proper")
                           ?? o.Value<bool?>("IsProperNoun")
                           ?? false;

            var type = ParseEntityType(typeStr);

            return new SceneEntity
            {
                Name = name,
                Type = type,
                IsProperNoun = isProper
            };
        }
        catch
        {
            return null;
        }
    }

    private static SceneEntityType ParseEntityType(string? type)
    {
        if (string.IsNullOrWhiteSpace(type)) return SceneEntityType.Concept;
        switch (type.Trim().ToLowerInvariant())
        {
            case "character":
                return SceneEntityType.Character;
            case "location":
                return SceneEntityType.Location;
            case "item":
                return SceneEntityType.Item;
            case "concept":
                return SceneEntityType.Concept;
            // Map several synonyms/fallbacks
            case "organization":
            case "org":
            case "event":
            case "ability":
            case "skill":
            case "condition":
            case "state":
            case "creature":
                return SceneEntityType.Concept;
            default:
                return SceneEntityType.Concept;
        }
    }
}
