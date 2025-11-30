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
public class SceneEntityLlmClassifier : IEntityLlmClassificationService
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
Your job is to read a single scene description (1–5 sentences) and extract story-relevant entities plus meta-information into a structured JSON object.
You must only output JSON, no explanations.
________________________________________
1. Task
From the input text, identify named or narratively important entities and classify each into one of these types:
•	Character – people, creatures, or personified beings
•	Location – places, regions, buildings, landmarks
•	Item – concrete, manipulable objects with ongoing story relevance
•	Concept – organizations, factions, events, rituals, laws, abstract forces or ideas that matter for the plot
You must also:
•	Decide which entities are introduced in this scene.
•	Decide which entities are removed in this scene (e.g., destroyed, lost, given away, permanently leaving).
•	Estimate the time_delta between the previous scene and this scene based only on the textual cues in the input.
________________________________________
2. Inclusion Rules (What counts as an entity)
Include an entity only if:
•	It is clearly distinct and identifiable in the story (can be referenced later by name or a clear label), and
•	It is plausibly relevant to the ongoing narrative (not just generic background clutter).
Typical inclusions:
Characters:
•	Named people/creatures: Alice, Captain Reyes, The Shadow King
•	Title + name: Captain Reyes, Lord Harren, Professor Willow
•	Personified non-humans with names: Whiskers the Cat, Blaze the Dragon
Locations:
•	Proper places: Grand Market, Tower of Dawn, Rivermoor, Crystal Forest
•	Distinct in-world sites: Whispering Docks, Hall of Echoes
Items:
•	Named / special objects: Silver Key, Codex of Storms, Heartstone Amulet
•	Distinct magical or plot-critical items even if not capitalized, if clearly unique in context.
Concepts (abstract / institutional / event-like):
•	Factions / organizations: Shadow Guild, Order of the Dawn, Council of Nine
•	Events / rituals / festivals: Festival of Masks, Trial of Embers, Night of Falling Stars
•	Ideals / oaths / codified rules when story-relevant: Code of Storms, Oath of Silence
•	Mythic / systemic forces that matter to the plot: The Old Magic, The Great Silence, The Long Winter
•	Emotional or psychological forces when they are clearly acting as significant internal drivers in the scene, e.g.
    o	“Fear tightens Leo’s chest…”
    o	“Curiosity pulls her toward the Gate of Whispers.”
When strong emotions are described as active internal forces shaping a character’s decision, treat them as Concept entities with usually medium confidence.
________________________________________
3. Exclusion Rules (What NOT to make entities for)
Do NOT create entities for:
•	Generic, everyday objects: table, door, candles, bag, coins
•	Generic natural phenomena and weather: rain, wind, thunder, snow, waves
Exception: Treat them as Concept only if they are clearly named, capitalized, and used like a unique force or myth:
e.g., The Endless Rain, The Red Storm.
•	Generic emotions or vague ideas: fear, hope, courage when used as generic descriptors only and not as distinct, driving forces in the scene.
•	Verbs, actions, or descriptions: running, battle, music, glow
•	Purely descriptive phrases that are not stable entities: the old wooden bridge (unless it behaves like a named, recurring landmark such as Old Wooden Bridge on a map).
If no valid entities are found, introduced_entities and removed_entities must both be empty arrays.
________________________________________
4. Proper Nouns vs. Non-Proper Nouns
Set is_proper_noun based on how it appears and behaves in context:
IsProperNoun = true if:
•	It is capitalized like a name or title: Grand Market, Silver Key, Shadow Guild, Tower of Dawn, Festival of Masks.
•	It functions as a unique in-world name, even if not obviously multi-word.
IsProperNoun = false if:
•	It is a generic label: the guard, the merchant, the village, the sword
•	It is a unique instance but written as a common noun and not clearly used as a name.
•	You may include non-proper entities only if they are clearly story-relevant and distinct
(e.g., “the ancient dragon sealed below the city” in a later scene that references “the dragon” again).
________________________________________
5. Confidence Score
Use the following strict rules to assign a confidence level to each extracted entity:
Confidence levels:
•	""high"" — The entity is explicitly named, unambiguous, and fits cleanly into a single entity type.
    o	Example: “Captain Reyes”, “Silver Key”, “Rivermoor”.
•	""medium"" — The entity exists but has mild ambiguity in boundaries or type.
    o	Example: “The Hidden Circle”, “Ancient Ritual”, “River Guardians”, strong emotions clearly shaping actions.
•	""low"" — The entity is vague, generic, metaphorical, or only implicitly referenced.
    o	Example: “fear”, “magic”, “the storm” when it is not clearly a named force or institution.
Confidence output rules:
•	Always output one of: ""high"", ""medium"", ""low"".
•	Confidence must reflect:
    o	how certain you are that the text refers to a distinct entity, and
    o	that the entity type is correct.
•	Confidence is never numeric; use only these three categories.
________________________________________
6. Time Delta Classification
You must infer how much time has passed between the previous scene and the current scene, based only on explicit or strongly implied textual cues in the input.
Set ""time_delta"" to one of:
•	""none""
    o	The scene appears to continue immediately from the previous moment.
    o	No explicit time jump phrases. Transitions like “and then”, “as”, “while” usually imply continuity.
•	""short""
    o	A brief time skip: minutes or hours, typically within the same day.
    o	Clues: “later that afternoon”, “after a while”, “an hour later”, “as night falls” (if previous was daytime).
•	""long""
    o	A large time jump: days, weeks, months, or more.
    o	Clues: “the next day”, “weeks later”, “months passed”, “years later”, “after a long winter”, “many seasons passed”.
If there is no clear evidence of a time jump, default to ""time_delta"": ""none"".
________________________________________
7. Introduced vs. Removed Entities
You are classifying entities relative to this scene alone, based on what the text explicitly does to them:
•	introduced_entities:
    Entities that are first clearly established or reintroduced as active in this scene.
    o	Examples:
        	A character appearing or being named for the first time.
        	A new location the characters arrive at.
        	An item that becomes important when it is found, revealed, or described.
        	A concept that is explicitly named or given importance (e.g., a new order, pact, ritual, or dominant emotion that clearly drives actions).
•	removed_entities:
    Entities that the scene explicitly destroys, discards, permanently loses, or decisively dismisses from the current narrative context.
    Only mark an entity as removed if the text very clearly indicates it will not remain with the characters or in the current context:
    o	An item is destroyed, lost beyond retrieval, or given away.
        	“The Silver Key shatters into dust.”
        	“She throws the Heartstone Amulet into the abyss.”
    o	A character dies or leaves in a way that feels final for the current storyline.
        	“The old guardian draws his last breath.”
        	“The merchant sails away, never to return.”
    o	A concept or magical effect ends decisively.
        	“With the last word spoken, the Binding Oath is broken forever.”
If the text is ambiguous about permanence, you may either:
    •	Omit the entity from removed_entities, or
    •	Include it with confidence: ""low"".
Entities that are neither clearly introduced nor clearly removed in the current scene should be omitted entirely from both lists.
________________________________________
8. Output Format
Always return a single JSON object with exactly these top-level fields:
•	""time_delta"" – a string, one of: ""none"", ""short"", ""long""
•	""introduced_entities"" – an array of entity objects
•	""removed_entities"" – an array of entity objects
Each entity object must have exactly these fields:
•	""name"" (string) – the text form of the entity as it appears (or the clearest canonical form)
•	""type"" (string) – one of: ""character"", ""location"", ""item"", ""concept""
•	""is_proper_noun"" (boolean) – true or false
•	""confidence"" (string) – ""high"", ""medium"", or ""low""
No extra fields. No comments. No surrounding text.
If no valid entities are introduced or removed, use:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [],
  ""removed_entities"": []
}
________________________________________
9. Examples
Example 1 — Characters, Location, Item
Input scene text:
Alice steps into the Grand Market, clutching the Silver Key.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
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
  ],
  ""removed_entities"": []
}
________________________________________
Example 2 — Faction / Abstract Group as Concept
Input scene text:
The Shadow Guild extends its reach into Rivermoor.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
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
  ],
  ""removed_entities"": []
}
________________________________________
Example 3 — Named Event and Tradition as Concepts
Input scene text:
During the Festival of Blossoms, the people of Dawnridge renew the Pact of Crows.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
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
  ],
  ""removed_entities"": []
}
________________________________________
Example 4 — Abstract Ideals as Concepts (Internal Emotional Drivers)
Input scene text:
Fear tightens Leo’s chest, but his courage and hope push him to step forward.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
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
  ],
  ""removed_entities"": []
}
________________________________________
Example 5 — Ambient Description, No Story-Relevant Entities
Input scene text:
Rain patters softly against the window until the candles finally go out.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [],
  ""removed_entities"": []
}
(No durable characters, locations, items, or important concepts are introduced or removed.)
________________________________________
Example 6 — Mixed Proper/Common Nouns, Items and Location
Input scene text:
At the edge of the Whispering Woods, the village healer wraps the Moonstone Shard in a strip of cloth.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
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
  ],
  ""removed_entities"": []
}
________________________________________
Example 7 — Emotions as Internal Drivers (like your failing test)
Input scene text:
Fear and curiosity wrestle inside Liora as she approaches the Gate of Whispers.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
    {
      ""name"": ""Liora"",
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
      ""name"": ""curiosity"",
      ""type"": ""concept"",
      ""is_proper_noun"": false,
      ""confidence"": ""medium""
    },
    {
      ""name"": ""Gate of Whispers"",
      ""type"": ""location"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    }
  ],
  ""removed_entities"": []
}
Here the emotions act as strong internal forces influencing Liora’s behavior, so they are included as Concept entities.
________________________________________
Example 8 — Long Time Delta, No Removals
Input scene text:
Years later, Mira returns to the Ruined Observatory, its broken dome now framing a field of stars.
Expected output JSON:
{
  ""time_delta"": ""long"",
  ""introduced_entities"": [
    {
      ""name"": ""Mira"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    },
    {
      ""name"": ""Ruined Observatory"",
      ""type"": ""location"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    }
  ],
  ""removed_entities"": []
}
The phrase “Years later” clearly indicates a long time delta.
________________________________________
Example 9 — Short Time Delta, No New Entities
Input scene text:
A few hours later, the campfire has burned low and the forest around the travelers is quiet.
Expected output JSON:
{
  ""time_delta"": ""short"",
  ""introduced_entities"": [],
  ""removed_entities"": []
}
No new named entities are introduced; the phrase “A few hours later” indicates a short time delta.
________________________________________
Example 10 — Item and Concept Removed
Input scene text:
As the final word of the spell is spoken, the Crimson Sigil cracks and crumbles to dust, and the Binding Oath it carried dissolves into the night.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [],
  ""removed_entities"": [
    {
      ""name"": ""Crimson Sigil"",
      ""type"": ""item"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    },
    {
      ""name"": ""Binding Oath"",
      ""type"": ""concept"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    }
  ]
}
Both the item (Crimson Sigil) and the concept (Binding Oath) are explicitly destroyed/ended, so they belong in removed_entities.
________________________________________
Example 11 — Character Removed, New Location Introduced
Input scene text:
At the Docks of Emberfall, Old Rurik presses the map into Nia’s hands, boards the last ship, and sails away, never to return.
Expected output JSON:
{
  ""time_delta"": ""none"",
  ""introduced_entities"": [
    {
      ""name"": ""Docks of Emberfall"",
      ""type"": ""location"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    },
    {
      ""name"": ""Old Rurik"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    },
    {
      ""name"": ""Nia"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""confidence"": ""high""
    },
    {
      ""name"": ""map"",
      ""type"": ""item"",
      ""is_proper_noun"": false,
      ""confidence"": ""medium""
    }
  ],
  ""removed_entities"": [
    {
      ""name"": ""Old Rurik"",
      ""type"": ""character"",
      ""is_proper_noun"": true,
      ""confidence"": ""medium""
    }
  ]
}
Here, Old Rurik is both introduced and effectively removed in the same scene because the text explicitly states he sails away “never to return.” The classification of removal uses ""confidence"": ""medium"" because “never to return” could be metaphorical but is strongly suggestive of permanence.
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
                return new EntityClassification { Entities = entities, IntroducedEntities = entities, RemovedEntities = [], TimeDelta = "none" };
            }

            var root = JObject.Parse(json);
            // New structured format support
            var timeDelta = root.Value<string>("time_delta") ?? root.Value<string>("TimeDelta") ?? "none";
            var introducedToken = root["introduced_entities"] ?? root["IntroducedEntities"];
            var removedToken = root["removed_entities"] ?? root["RemovedEntities"];
            var introArr = introducedToken as JArray;
            var remArr = removedToken as JArray;

            if (introArr != null || remArr != null)
            {
                var introduced = introArr != null
                    ? introArr.Select(ParseSceneEntityFromToken).Where(e => e != null).Cast<SceneEntity>().ToArray()
                    : Array.Empty<SceneEntity>();
                var removed = remArr != null
                    ? remArr.Select(ParseSceneEntityFromToken).Where(e => e != null).Cast<SceneEntity>().ToArray()
                    : Array.Empty<SceneEntity>();

                return new EntityClassification
                {
                    TimeDelta = string.IsNullOrWhiteSpace(timeDelta) ? "none" : timeDelta,
                    IntroducedEntities = introduced,
                    RemovedEntities = removed,
                    // Back-compat: Entities mirrors IntroducedEntities
                    Entities = introduced
                };
            }

            // Try common keys: entities / Entities
            var entitiesToken = root["entities"] ?? root["Entities"];
            if (entitiesToken is JArray jArr)
            {
                var entities = jArr.Select(ParseSceneEntityFromToken)
                                   .Where(e => e != null)
                                   .Cast<SceneEntity>()
                                   .ToArray();
                return new EntityClassification { Entities = entities, IntroducedEntities = entities, RemovedEntities = [], TimeDelta = "none" };
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
            var confidenceStr = o.Value<string>("confidence") ?? o.Value<string>("Confidence");

            var type = ParseEntityType(typeStr);
            var confidence = ParseConfidence(confidenceStr);

            return new SceneEntity
            {
                Name = name,
                Type = type,
                IsProperNoun = isProper,
                Confidence = confidence
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

    private static Confidence ParseConfidence(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Confidence.Medium;
        switch (value.Trim().ToLowerInvariant())
        {
            case "high":
                return Confidence.High;
            case "low":
                return Confidence.Low;
            case "medium":
                return Confidence.Medium;
            default:
                return Confidence.Medium;
        }
    }
}
