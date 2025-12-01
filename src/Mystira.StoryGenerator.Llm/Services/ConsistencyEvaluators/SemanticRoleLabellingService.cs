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
        _settings = aiOptions.Value.SemanticRoleLabellingSettings;
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
    5.	Overall confidence and a short evidence span.
You are doing local classification: only this scene’s text plus the provided known_active / known_removed context.
________________________________________
3. Local Introduction / Removal Status
Use these labels for introduction_status:
    •	""new""
        o	The entity appears in this scene and is not in known_active_entities or known_removed_entities.
        o  	The scene clearly brings it into the story: meets, sees, finds, arrives at, is described for the first time, etc.
    •	""reintroduced""
        o	The entity appears in this scene and is present in known_removed_entities (previously gone) but is now back in focus, seen again, returned, rebuilt, revived, etc.
    •	""already_known""
        o	The entity appears in this scene and is present in known_active_entities.
        o	This is a normal continued use, not a (re)introduction.
    •	""not_present""
        o	The entity does not appear in this scene at all (no mention, no obvious pronoun or description clearly referring to it).
Use these labels for removal_status:
•	""removed""
    o	This scene clearly removes the entity from the current story state:
        	character dies or departs “for good”
        	item is destroyed, lost irretrievably, given away with a strong sense of finality
        	location or concept is shattered, ended, sealed away permanently
•	""not_removed""
    o	The scene does not clearly remove the entity.
If the text is ambiguous about permanence, you may still use ""removed"" with ""confidence"": ""medium"" or ""low"", but never mark removal on weak or speculative hints.
________________________________________
4. Semantic Roles
For each entity that is present in the scene, assign zero or more semantic roles that describe how it participates in the main actions. Use this controlled vocabulary:
    •	""agent"" – the doer of an action (“Alice opens the door.”)
    •	""patient"" – the entity acted upon (“Bob drops the lantern.”)
    •	""experiencer"" – the feeler or perceiver (“Fear grips Leo.” / “Leo hears the drums.”)
    •	""stimulus"" – what causes an experience (“The drums terrify Leo.”)
    •	""owner"" / ""possessor"" – entity that owns or holds something (“Nia clutches the map.”)
    •	""item"" – item being held, used, moved, traded, etc. (“Nia clutches the map.”)
    •	""location"" – where the action is happening (“At the Grand Market, …”)
    •	""source"" – where something comes from (“They flee from the Old Tower.”)
    •	""goal"" – destination or target (“They march toward Rivermoor.”)
    •	""instrument"" – tool used for an action (“With the Silver Key, she unlocks the gate.”)
    •	""group"" – crowd or group acting together if the entity is a faction/organization.
You should choose roles that are clearly supported by the text. If no role fits confidently, use an empty list [].
________________________________________
5. Confidence
Use:
    •	""high"" – explicit, unambiguous mention and role.
    •	""medium"" – clearly present, but role / introduction / removal has some ambiguity.
    •	""low"" – vague, metaphorical, or heavily inferred.
confidence is for the overall classification of that entity in this scene.
________________________________________
6. Output Format
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
            ""new"",           /* clearly introduced for the first time here */
            ""already_known"", /* assumed known from earlier scenes/prefix summary */
            ""not_present""    /* not present in this scene, but in global entity list */
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
