using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Newtonsoft.Json;

namespace Mystira.StoryGenerator.Llm.Services.ConsistencyEvaluators;

/// <summary>
/// Consistency evaluator that summarizes a prefix of a story path into a compact world-state summary.
/// </summary>
public class SemanticRoleLabellingLlmService : ISemanticRoleLabellingLlmService
{
    private readonly SemanticRoleLabellingSettings _settings;
    private readonly ILlmServiceFactory _llmServiceFactory;
    private readonly ILogger<SemanticRoleLabellingLlmService> _logger;

    public SemanticRoleLabellingLlmService(
        IOptions<AiSettings> aiOptions,
        ILlmServiceFactory llmServiceFactory,
        ILogger<SemanticRoleLabellingLlmService> logger)
    {
        _settings = aiOptions.Value.SemanticRoleLabelling;
        _llmServiceFactory = llmServiceFactory;
        _logger = logger;
    }

    public async Task<SemanticRoleLabellingClassification?> ClassifyAsync(
        Scene scene,
        IEnumerable<SceneEntity> candidateEntities,
        IEnumerable<SceneEntity> knownActiveEntities,
        IEnumerable<SceneEntity> knownRemovedEntities,
        CancellationToken cancellationToken = default)
    {
        if (!candidateEntities.Any())
        {
            _logger.LogWarning("SRL service requested with no candidate entities");
            return null;
        }

        if (!_settings.IsConfigured)
        {
            _logger.LogDebug("SRL service is not configured, skipping classification");
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
                        Content = GetUserPrompt(scene, candidateEntities, knownActiveEntities, knownRemovedEntities),
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

            var summary = JsonConvert.DeserializeObject<SemanticRoleLabellingClassification>(content);
            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scene entity classification");
            return null;
        }
    }

    private string GetUserPrompt(Scene scene, IEnumerable<SceneEntity> candidateEntities, IEnumerable<SceneEntity> knownActiveEntities, IEnumerable<SceneEntity> knownRemovedEntities)
    {
        var input = new SrlInput
        {
            CandidateEntities = GetSrlEntities(candidateEntities),
            KnownActiveEntities = GetSrlEntities(knownActiveEntities),
            KnownRemovedEntities = GetSrlEntities(knownRemovedEntities),
            SceneId = scene.Id,
            SceneText = scene.Description
        };

        SrlEntityRef[] GetSrlEntities(IEnumerable<SceneEntity> entities)
        {
            return entities.Select(x => new SrlEntityRef {Name = x.Name, Type = x.Type}).ToArray();
        }

        var ret = JsonConvert.SerializeObject(input);
        return ret;
    }

    private string GetSystemInstructionPrompt()
    {
        return @"
You are the Mystira Local SRL Role Classifier.
Your job is to read a single scene (1–5 sentences) plus light context and classify how each entity is used locally in this scene, using semantic-role–style labels and introduction/removal status.
You must only output JSON, no explanations.
________________________________________
1. Inputs
You will receive a single JSON object with this structure:
{
  ""scene_id"": ""scene_12_market_square"",
  ""scene_text"": ""Alice steps into the Grand Market and spots Bob arguing with a merchant near the fountain."",
  ""known_active_entities"": [
    { ""name"": ""Alice"", ""type"": ""character"" },
    { ""name"": ""Grand Market"", ""type"": ""location"" }
  ],
  ""known_removed_entities"": [
    { ""name"": ""Old Tower"", ""type"": ""location"" }
  ],
    ""candidate_entities"": [
    { ""name"": ""Alice"", ""type"": ""character"" },
    { ""name"": ""Bob"", ""type"": ""character"" },
    { ""name"": ""Grand Market"", ""type"": ""location"" },
    { ""name"": ""Old Tower"", ""type"": ""location"" }
  ]
}
    •	scene_text: the raw prose of the current scene.
    •	known_active_entities (optional): entities that are already known/active before this scene (from prefix summaries).
    •	known_removed_entities (optional): entities that have been removed before this scene.
    •	candidate_entities: all entities we care about in this story (names + coarse type).
You must treat known_active_entities and known_removed_entities as truth about prior context: do not re-decide their past status, only how they behave in this scene.
________________________________________
2. Classification Goals
For each entity in candidate_entities, decide:
    1.	Whether it appears in this scene (any textual mention or clear reference).
    2.	What semantic roles it plays in the scene (SRL-style).
    3.	Whether the scene locally introduces it (first clear appearance or re-entry into the story).
    4.	Whether the scene locally removes it (dies, destroyed, given away, departs “for good”, etc.).
    5. How the scene's wording treats the entity (local_usage_style).
    6. Overall confidence and a short evidence span.
    7. Whether the entity is used as a proper noun (is_proper_noun) in the local wording of this scene.
You are doing local classification: only this scene’s text plus the provided known_active / known_removed context.
________________________________________
3. Local Introduction / Removal Status
Use the following rules:
present_in_scene = false → introduction_status = ""not_present""
If the entity does not appear in the scene text (no mention, no pronoun, no implied reference), then:
    •	introduction_status = ""not_present""
    •	removal_status = ""not_removed""
This applies even if the entity appears in known_active_entities.

Hard constraints for introduction_status:
    • If an entity appears in the scene AND its (name, type) pair is present in
      known_active_entities, you MUST set introduction_status = ""already_known"".
      You are NOT allowed to label such an entity as ""new"" or ""reintroduced"".
    • If an entity appears in the scene AND its (name, type) pair is present in
      known_removed_entities, you MUST choose between:
        o ""reintroduced"" if the scene clearly shows it coming back, OR
        o ""not_present"" if the name is only mentioned indirectly without the entity actually being back.
 You are NOT allowed to label a known_removed entity as ""new"".
________________________________________
If the entity appears in the scene text:
introduction_status = ""new""
Use this when:
    •	The entity is not in known_active_entities
and
    •	The scene clearly brings it into the story for the first time:
        o	meeting for the first time
        o	arriving
        o	discovering
        o	being described as newly noticed
        o	seeing someone/something unexpectedly
This includes cases like:
    •	“He sees a lynx resting on a branch.” → new (if Larry wasn’t previously active)
    •	“A turtle watches quietly from the log.” → new (for “turtle”)
________________________________________
introduction_status = ""already_known""
Use this when:
    •	The entity appears
and
    •	The entity is listed in known_active_entities
This means the story already considers the entity known/relevant.
Examples:
    •	“Alice steps into the Grand Market.”
    •	“Bob nods at Alice.”
________________________________________
introduction_status = ""reintroduced""
Use this only when:
    •	The entity appears in the scene
and
    •	It is present in known_removed_entities
and
    •	The scene clearly indicates a return:
        o	comes back
        o	returns to the group
        o	rebuilt/restored
        o	revived
        o	reappears
________________________________________
Removal Status
removal_status = ""removed""
Use when the scene makes the entity permanently or decisively gone, such as:
    •	Death
    •	Object destroyed
    •	Lost for good
    •	Leaves permanently (“goes home and won’t join again”, “sent away for good”)
removal_status = ""not_removed""
Use in all other cases.
________________________________________
4. Local Usage Style (Very Important)
This describes how the wording treats the entity in this scene.
Use exactly one of:
""clear_introduction""
Choose this when:
    •	The wording presents the entity as if new to the reader/player.
    •	There is descriptive scaffolding, e.g.:
        o	“a lynx named Larry”
        o	“a sad turtle sitting by the rocks”
        o	“a large bear emerges from the cave”
        o	“the old tower rises before them”
This can be used even if the entity is in fact already-known, because this field captures style, not truth.
________________________________________
""already_known_style""
Choose this when:
    •	The scene refers to the entity in familiar terms without descriptive introduction.
    •	The tone presumes the audience already knows them.
Examples:
    •	“Larry sits up.”
    •	“Maple nods.”
    •	“Grand Market bustles with noise.”
________________________________________
""ambiguous""
Use when:
    •	The phrasing is neutral and does not strongly imply familiarity or introduction.
    •	Or the entity does not appear.
Examples:
    •	No mention.
    •	Subtle or unclear references.
________________________________________
5. Semantic Roles
Use these SRL-style labels when applicable.
Allowed role strings:
    •	""agent"" — acting / doing something
    •	""patient"" — receiving action
    •	""experiencer"" — feeling / perceiving something
    •	""theme"" — entity moved, transferred, discussed
    •	""instrument"" — tool enabling action
    •	""location"" — place where events occur
    •	""source"" — where something comes from
    •	""goal"" — where something moves toward
    •	""possessor"" — owner / holder
    •	""recipient"" — receives an item or message
    •	""attribute"" — entity described by properties
    •	""co_agent"" — acts jointly with another character
    •	""other"" — special cases that don’t fit the above
If the entity is not present:
    •	semantic_roles = []
________________________________________
6. Confidence
Label with:
    •	""high"" — clear textual evidence; classification is obvious
    •	""medium"" — some ambiguity; inference required
    •	""low"" — weak evidence; guesswork
________________________________________
7. Proper Noun Flag (is_proper_noun)

For each entity, you must also provide a boolean is_proper_noun:

  • is_proper_noun = true
      - The entity is used as a specific named entity (a proper noun) in this scene.
      - Typical signs: capitalized name used as a unique label (""Larry"", ""Alice"", ""Grand Market"", ""Sunpeak Mountain"").
      - Phrases like ""a lynx named Larry"" clearly indicate a proper name.

  • is_proper_noun = false
      - The entity is only referred to generically (""the lynx"", ""a castle"", ""the market"", ""a tower"")
        without a unique, name-like usage.
      - Use false as well when the entity is not present in the scene (present_in_scene = false).

This flag is about *how the entity is referred to in this scene’s wording*, not about whether the entity is globally important.
________________________________________
8. Output Format
Always return a JSON object with:
    •	""scene_id"" – copied from input.
    •	""entity_classifications"" – array of per-entity objects.
Each entity object must have exactly:
    •	""name"" (string) – must match the candidate_entities.name
    •	""type"" (string) – ""character"", ""location"", ""item"", or ""concept""
    •	""present_in_scene"" (boolean)
    •	""introduction_status"" (string) – ""new"", ""reintroduced"", ""already_known"", ""not_present""
    •	""removal_status"" (string) – ""removed"" or ""not_removed""
    •	""semantic_roles"" (array of strings from the allowed list)
    •   ""local_usage_style"": ""clear_introduction"", ""already_known_style"", ""ambiguous""
    •   ""is_proper_noun"" (boolean) – whether the entity is used as a proper noun in this scene.
    •	""confidence"" (string) – ""high"", ""medium"", or ""low""
    •	""evidence_span"" (string) – a short quote or phrase from the scene that best supports your decision. Use """" if the entity is not present.
No extra fields. No explanations. No comments.
Example output shape:
{
  ""scene_id"": ""scene_12_market_square"",
  ""entity_classifications"": [
    {
      ""name"": ""Alice"",
      ""type"": ""character"",
      ""present_in_scene"": true,
      ""introduction_status"": ""already_known"",
      ""removal_status"": ""not_removed"",
      ""semantic_roles"": [""agent""],
      ""local_usage_style"": ""already_known_style"",
      ""is_proper_noun"": true,
      ""confidence"": ""high"",
      ""evidence_span"": ""Alice steps into the Grand Market""
    },
    {
      ""name"": ""Bob"",
      ""type"": ""character"",
      ""present_in_scene"": true,
      ""introduction_status"": ""new"",
      ""removal_status"": ""not_removed"",
      ""semantic_roles"": [""agent""],
      ""local_usage_style"": ""clear_introduction"",
      ""is_proper_noun"": true,
      ""confidence"": ""medium"",
      ""evidence_span"": ""spots Bob arguing with a merchant""
    },
    {
      ""name"": ""Grand Market"",
      ""type"": ""location"",
      ""present_in_scene"": true,
      ""introduction_status"": ""already_known"",
      ""removal_status"": ""not_removed"",
      ""semantic_roles"": [""location""],
      ""local_usage_style"": ""already_known_style"",
      ""is_proper_noun"": true,
      ""confidence"": ""high"",
      ""evidence_span"": ""steps into the Grand Market""
    },
    {
      ""name"": ""Old Tower"",
      ""type"": ""location"",
      ""present_in_scene"": false,
      ""introduction_status"": ""not_present"",
      ""removal_status"": ""not_removed"",
      ""semantic_roles"": [],
      ""local_usage_style"": ""ambiguous"",
      ""is_proper_noun"": false,
      ""confidence"": ""high"",
      ""evidence_span"": """"
    }
  ]
}
Remember:
•	Operate locally on this scene only, respecting known_active_entities and known_removed_entities as prior state.
•	Output only the JSON object in the specified format.
";
    }

    private string GetJsonSchemaFormat()
    {
        return @"
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""title"": ""SceneEntitySRLClassification"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [""scene_id"", ""entity_classifications""],
  ""properties"": {
    ""scene_id"": {
      ""type"": ""string"",
      ""description"": ""The unique identifier of the scene being analyzed.""
    },
    ""entity_classifications"": {
      ""type"": ""array"",
      ""description"": ""Per-entity SRL-based classification for this scene."",
      ""items"": {
        ""$ref"": ""#/definitions/EntityClassification""
      }
    }
  },
  ""definitions"": {
    ""EntityClassification"": {
      ""type"": ""object"",
      ""additionalProperties"": false,
      ""required"": [
        ""name"",
        ""type"",
        ""present_in_scene"",
        ""introduction_status"",
        ""removal_status"",
        ""semantic_roles"",
        ""local_usage_style"",
        ""is_proper_noun"",
        ""confidence"",
        ""evidence_span""
      ],
      ""properties"": {
        ""name"": {
          ""type"": ""string"",
          ""description"": ""Canonical name of the entity (e.g., 'Alice', 'Grand Market').""
        },
        ""type"": {
          ""type"": ""string"",
          ""description"": ""Coarse entity type."",
          ""enum"": [""character"", ""location"", ""item"", ""concept""]
        },
        ""present_in_scene"": {
          ""type"": ""boolean"",
          ""description"": ""True if this entity is actually mentioned / active in this scene.""
        },
        ""introduction_status"": {
          ""type"": ""string"",
          ""description"": ""How this scene treats the entity's narrative introduction."",
          ""enum"": [
            ""new"",
            ""reintroduced"",
            ""already_known"",
            ""not_present""
          ]
        },
        ""removal_status"": {
          ""type"": ""string"",
          ""description"": ""Whether this scene explicitly removes the entity from the narrative context."",
          ""enum"": [
            ""not_removed"",
            ""removed""
          ]
        },
        ""semantic_roles"": {
          ""type"": ""array"",
          ""description"": ""Semantic roles this entity plays in this scene (SRL-style)."",
          ""items"": {
            ""type"": ""string"",
            ""description"": ""Role labels such as 'agent', 'patient', 'experiencer', 'stimulus', 'location', 'goal', etc.""
          }
        },
        ""local_usage_style"": {
          ""type"": ""string"",
          ""enum"": [""clear_introduction"", ""already_known_style"", ""ambiguous""]
        },
        ""is_proper_noun"": {
          ""type"": ""boolean"",
          ""description"": ""True if the entity is used as a specific named entity (proper noun) in this scene text.""
        },
        ""confidence"": {
          ""type"": ""string"",
          ""description"": ""Model confidence in the correctness of this classification."",
          ""enum"": [""high"", ""medium"", ""low""]
        },
        ""evidence_span"": {
          ""type"": ""string"",
          ""description"": ""Exact or near-exact text span from the scene that justifies this classification.""
        }
      }
    }
  }
}
";
    }

    // Input shape expected by the SRL prompt/instructions
    private sealed class SrlInput
    {
        [JsonProperty("scene_id")]
        public string SceneId { get; set; } = string.Empty;

        [JsonProperty("scene_text")]
        public string SceneText { get; set; } = string.Empty;

        [JsonProperty("known_active_entities")]
        public SrlEntityRef[] KnownActiveEntities { get; set; } = [];

        [JsonProperty("known_removed_entities")]
        public SrlEntityRef[] KnownRemovedEntities { get; set; } = [];

        [JsonProperty("candidate_entities")]
        public SrlEntityRef[] CandidateEntities { get; set; } = [];
    }

    private sealed class SrlEntityRef
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public SceneEntityType Type { get; set; } // "character" | "location" | "item" | "concept"
    }
}
