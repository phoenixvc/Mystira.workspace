using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Api.Services.LLM;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services;

public class StoryGenerationService : IStoryGenerationService
{
    private readonly ILLMServiceFactory _llmFactory;
    private readonly AiSettings _aiSettings;
    private readonly ILogger<StoryGenerationService> _logger;

    private const string StoryGenerationFeatureKey = "story-generation";

    public StoryGenerationService(ILLMServiceFactory llmFactory, IOptions<AiSettings> aiOptions, ILogger<StoryGenerationService> logger)
    {
        _llmFactory = llmFactory;
        _aiSettings = aiOptions.Value;
        _logger = logger;
    }

    public async Task<GenerateYamlStoryResponse> GenerateYamlStoryAsync(GenerateYamlStoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = _llmFactory.GetService(request.Provider);

            if (service is null)
            {
                return new GenerateYamlStoryResponse
                {
                    Success = false,
                    Error = "No LLM service is available or properly configured",
                    Provider = request.Provider ?? _aiSettings.DefaultProvider ?? string.Empty,
                    Model = request.Model ?? string.Empty,
                    ModelId = request.ModelId
                };
            }

            var resolvedModelId =  request.ModelId;
            var resolvedModelName = request.Model ?? resolvedModelId;
            var systemPrompt = BuildSystemPrompt();
            var messages = BuildMessages(request);

            var baseMaxTokens = _aiSettings.DefaultMaxTokens;
            var requestedMaxTokens = _aiSettings.DefaultMaxTokens;
            var maxTokens = Math.Max(Math.Max(baseMaxTokens, requestedMaxTokens), 1500);
            var temperature = _aiSettings.DefaultTemperature;

            var chatRequest = new ChatCompletionRequest
            {
                Provider = service.ProviderName,
                ModelId = resolvedModelId,
                Model = resolvedModelName,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Messages = messages,
                SystemPrompt = systemPrompt
            };

            var response = await service.CompleteAsync(chatRequest, cancellationToken);

            if (string.IsNullOrWhiteSpace(response.ModelId) && !string.IsNullOrWhiteSpace(resolvedModelId))
            {
                response.ModelId = resolvedModelId;
            }

            if (string.IsNullOrWhiteSpace(response.Model) && !string.IsNullOrWhiteSpace(resolvedModelName))
            {
                response.Model = resolvedModelName;
            }

            if (!response.Success)
            {
                return new GenerateYamlStoryResponse
                {
                    Success = false,
                    Error = response.Error ?? "Generation failed",
                    Provider = response.Provider ?? service.ProviderName,
                    Model = response.Model ?? resolvedModelName ?? string.Empty,
                    ModelId = response.ModelId ?? resolvedModelId
                };
            }

            return new GenerateYamlStoryResponse
            {
                Success = true,
                Yaml = response.Content ?? string.Empty,
                Provider = response.Provider ?? service.ProviderName,
                Model = response.Model ?? resolvedModelName ?? string.Empty,
                ModelId = response.ModelId ?? resolvedModelId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during story generation");
            return new GenerateYamlStoryResponse
            {
                Success = false,
                Error = ex.Message,
                Provider = request.Provider ?? _aiSettings.DefaultProvider ?? string.Empty,
                Model = request.Model ?? string.Empty,
                ModelId = request.ModelId
            };
        }
    }

    private static string BuildSystemPrompt()
    {
        return @"
You are a professional interactive storytelling engine trained to generate branching adventure scenarios for young players.
Each story consists of modular scenes that use one of three types: narrative, choice, or roll.

Each choice scene must include at least two branches. Roll scenes must include a clear challenge and a success/fail outcome,
with outcome established by rolling a D20 and beating a generated difficulty level for the challenge.
Stories must include moral decisions that affect one or more compass axes (e.g., honesty, bravery, kindness).
These are tracked via compass_change (with a strength number between -1.0 for very negative and 1.0 for very positive)
and echo_log (with a strength number between -1.0 for very negative and 1.0 for very positive).

The story must be formatted in structured YAML using the following keys:
- title, description, tags, difficulty, session_length, age_group, minimum_age, archetypes
- characters: array of character entries with id, name, optional image/audio URLs, and metadata (role, archetype, species, age, traits, backstory)
- scenes: each with id, title, type, description, optional media, branches, and optional echo_log, compass_change, echo_reveal_references

The story must be validated against the following schema:
{
  ""$schema"": ""http://json-schema.org/draft-07/schema#"",
  ""title"": ""Mystira Story Schema"",
  ""description"": ""Validation schema for Mystira story YAML/JSON payloads"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""required"": [
    ""title"",
    ""description"",
    ""tags"",
    ""difficulty"",
    ""session_length"",
    ""age_group"",
    ""minimum_age"",
    ""core_axes"",
    ""archetypes"",
    ""characters"",
    ""scenes""
  ],
  ""properties"": {
    ""title"": { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 200, ""description"": ""The title of the story"" },
    ""description"": { ""type"": ""string"", ""minLength"": 1, ""maxLength"": 1000, ""description"": ""Brief description of the story"" },
    ""tags"": {
      ""type"": ""array"",
      ""items"": { ""type"": ""string"" },
      ""minItems"": 1,
      ""description"": ""Array of story tags/categories""
    },
    ""difficulty"": { ""type"": ""string"", ""enum"": [""Easy"", ""Medium"", ""Hard""], ""description"": ""Story difficulty level"" },
    ""session_length"": { ""type"": ""string"", ""enum"": [""Short"", ""Medium"", ""Long""], ""description"": ""Expected session duration"" },
    ""age_group"": { ""type"": ""string"", ""enum"": [""1-2"", ""3-5"", ""6-9"", ""10-12"", ""13-18""], ""description"": ""Target age group"" },
    ""minimum_age"": { ""type"": ""integer"", ""enum"": [1, 3, 6, 10, 13], ""description"": ""Minimum recommended age (controls allowed age_group bands)"" },
    ""core_axes"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""minItems"": 1, ""description"": ""Core story themes/axes"" },
    ""archetypes"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""minItems"": 1, ""description"": ""Character archetypes present in the story"" },

    ""characters"": {
      ""type"": ""array"",
      ""minItems"": 1,
      ""description"": ""Array of story characters"",
      ""items"": {
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""id"", ""name"", ""metadata""],
        ""properties"": {
          ""id"": {
            ""type"": ""string"",
            ""pattern"": ""^[a-z0-9_]+$"",
            ""minLength"": 1,
            ""description"": ""Character id (lowercase snake_case)""
          },
          ""name"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Character name"" },
          ""image"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Image id"" },
          ""audio"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Audio id"" },
          ""metadata"": {
            ""type"": ""object"",
            ""additionalProperties"": false,
            ""required"": [""role"", ""archetype"", ""species"", ""age"", ""traits"", ""backstory""],
            ""properties"": {
              ""role"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""minItems"": 1, ""description"": ""Character's role in the story"" },
              ""archetype"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""minItems"": 1, ""description"": ""Character archetype"" },
              ""species"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Character species"" },
              ""age"": { ""type"": ""integer"", ""minimum"": 1, ""description"": ""Character age"" },
              ""traits"": { ""type"": ""array"", ""items"": { ""type"": ""string"" }, ""minItems"": 1, ""description"": ""Character traits"" },
              ""backstory"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Character backstory"" }
            }
          }
        }
      }
    },

    ""scenes"": {
      ""type"": ""array"",
      ""minItems"": 1,
      ""description"": ""Array of story scenes"",
      ""items"": {
        ""type"": ""object"",
        ""additionalProperties"": false,
        ""required"": [""id"", ""title"", ""type"", ""description""],
        ""properties"": {
          ""id"": {
            ""type"": ""string"",
            ""pattern"": ""^[A-Za-z0-9_]+$"",
            ""minLength"": 1,
            ""description"": ""Unique scene identifier (snake_case; letters/numbers/_ allowed)""
          },
          ""title"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Scene title"" },
          ""type"": { ""type"": ""string"", ""enum"": [""narrative"", ""choice"", ""roll"", ""special""], ""description"": ""Scene type"" },
          ""description"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Scene description"" },
          ""next_scene"": {
            ""anyOf"": [
              {
                ""type"": ""string"",
                ""minLength"": 1,
                ""pattern"": ""^[A-Za-z0-9_]+$""
              },
              {
                ""type"": ""null""
              }
            ],
            ""description"": ""Next scene id (for linear flow). Must be null for final/special scenes.""
          },
          ""difficulty"": { ""type"": ""number"", ""minimum"": 1, ""maximum"": 20, ""description"": ""Scene difficulty (required for roll type scenes)"" },
          ""media"": {
            ""type"": ""object"",
            ""description"": ""Optional media attached to this scene; at least one of image/audio/video should be present"",
            ""additionalProperties"": false,
            ""minProperties"": 1,
            ""properties"": {
              ""image"": { ""type"": ""string"", ""description"": ""Image id or path"" },
              ""audio"": { ""type"": ""string"", ""description"": ""Audio id or path"" },
              ""video"": { ""type"": ""string"", ""description"": ""Video id or path"" }
            }
          },

          ""branches"": {
            ""type"": ""array"",
            ""description"": ""Scene branches (required for choice and roll type scenes)"",
            ""items"": {
              ""type"": ""object"",
              ""additionalProperties"": false,
              ""required"": [""choice"", ""next_scene""],
              ""properties"": {
                ""choice"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Branch choice description"" },
                ""next_scene"": {
                  ""type"": ""string"",
                  ""minLength"": 1,
                  ""pattern"": ""^[A-Za-z0-9_]+$"",
                  ""description"": ""The ID of the next scene to which this choice should navigate""
                },
                ""echo_log"": {
                  ""type"": ""object"",
                  ""additionalProperties"": false,
                  ""description"": ""Echo recorded by taking this branch"",
                  ""required"": [""echo_type"", ""description"", ""strength""],
                  ""properties"": {
                    ""echo_type"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""The type of echo"" },
                    ""description"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""The description of the echo"" },
                    ""strength"": { ""type"": ""number"", ""minimum"": -1.0, ""maximum"": 1.0, ""description"": ""The strength of the echo"" }
                  }
                },

                ""compass_change"": {
                  ""type"": ""object"",
                  ""additionalProperties"": false,
                  ""description"": ""Optional compass change values"",
                  ""required"": [""axis"", ""delta""],
                  ""properties"": {
                    ""axis"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Axis name to adjust"" },
                    ""delta"": { ""type"": ""number"", ""description"": ""Change applied to the axis"" },
                    ""developmental_link"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Reference to developmental framework link"" }
                  }
                }
              }
            }
          },

          ""echo_reveals"": {
            ""type"": ""array"",
            ""description"": ""Echo reveal references"",
            ""items"": {
              ""type"": ""object"",
              ""additionalProperties"": false,
              ""required"": [""echo_type"", ""min_strength"", ""trigger_scene_id""],
              ""properties"": {
                ""echo_type"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Type of echo"" },
                ""min_strength"": { ""type"": ""number"", ""description"": ""Minimum strength required"" },
                ""trigger_scene_id"": {
                  ""type"": ""string"",
                  ""minLength"": 1,
                  ""pattern"": ""^[A-Za-z0-9_]+$"",
                  ""description"": ""Scene ID that triggers the reveal""
                },
                ""max_age_scenes"": { ""type"": ""integer"", ""minimum"": 0, ""description"": ""Optional max age in scenes for the echo"" },
                ""reveal_mechanic"": { ""type"": ""string"", ""minLength"": 1, ""description"": ""Mechanic used to reveal"" },
                ""required"": { ""type"": ""boolean"", ""description"": ""If true, this reveal must trigger when conditions are met"" }
              }
            }
          }
        },
        ""allOf"": [
          { ""if"": { ""properties"": { ""type"": { ""const"": ""roll"" } } }, ""then"": { ""required"": [""difficulty"", ""branches""] } },
          { ""if"": { ""properties"": { ""type"": { ""const"": ""choice"" } } }, ""then"": { ""required"": [""branches""] } },
          { ""if"": { ""properties"": { ""type"": { ""const"": ""narrative"" } } }, ""then"": { ""required"": [""next_scene""] } },
          { ""if"": { ""properties"": { ""type"": { ""const"": ""special"" } } }, ""then"": { ""properties"": { ""next_scene"": { ""type"": ""null"" } } } }
        ]
      }
    }
  },
  ""allOf"": [
    {
      ""if"": { ""properties"": { ""minimum_age"": { ""const"": 1 } } },
      ""then"": { ""properties"": { ""age_group"": { ""enum"": [""1-2"", ""3-5"", ""6-9"", ""10-12"", ""13-18""] } } }
    },
    {
      ""if"": { ""properties"": { ""minimum_age"": { ""const"": 3 } } },
      ""then"": { ""properties"": { ""age_group"": { ""enum"": [""3-5"", ""6-9"", ""10-12"", ""13-18""] } } }
    },
    {
      ""if"": { ""properties"": { ""minimum_age"": { ""const"": 6 } } },
      ""then"": { ""properties"": { ""age_group"": { ""enum"": [""6-9"", ""10-12"", ""13-18""] } } }
    },
    {
      ""if"": { ""properties"": { ""minimum_age"": { ""const"": 10 } } },
      ""then"": { ""properties"": { ""age_group"": { ""enum"": [""10-12"", ""13-18""] } } }
    },
    {
      ""if"": { ""properties"": { ""minimum_age"": { ""const"": 13 } } },
      ""then"": { ""properties"": { ""age_group"": { ""enum"": [""13-18""] } } }
    }
  ]
}



All IDs must be lowercase snake_case. Scene IDs must be unique and prefixed with the story ID. The story should reflect the selected tone, age group, difficulty, and core moral axis.

DO NOT include any UI elements, markdown, or extra commentary—only structured YAML output.

Scene structure rules:
- Choice scenes: Offer meaningful, moral, or strategic decisions; include echo_log and compass_change for each choice. Each choice must have a unique next_scene reference.
- Roll scenes: Describe a challenge with clear stakes; offer two branches (success and failure); include optional compass_change if applicable (e.g., bravery boost on success). Each branch must have a unique next_scene reference.
- Narrative scenes: Advance the plot or build atmosphere; may include echo_reveal_references. Each narrative scene must have a unique next_scene reference.
";
    }

    private static List<MystiraChatMessage> BuildMessages(GenerateYamlStoryRequest request)
    {
        var messages = new List<MystiraChatMessage>();

        // Optional few-shot example to anchor structure
        var exampleYaml = GetShortExampleYaml();
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.AI,
            Content = "Example output (for structure only; do not copy content, follow structure and keys):\n\n" + exampleYaml
        });

        var instruction = BuildInstructionPrompt(request);
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.System,
            Content = instruction
        });

        // Context JSON payload with selected parameters
        var payload = new
        {
            title = request.Title,
            difficulty = request.Difficulty,
            session_length = request.SessionLength,
            age_group = request.AgeGroup,
            minimum_age = request.MinimumAge,
            core_axes = request.CoreAxes,
            archetypes = request.Archetypes,
            character_count = request.CharacterCount,
            tags = request.Tags ?? new List<string>(),
            tone = request.Tone,
            min_scenes = request.MinScenes,
            max_scenes = request.MaxScenes
        };

        var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        messages.Add(new MystiraChatMessage
        {
            MessageType = ChatMessageType.User,
            Content = "Context parameters (use these exactly):\n" + jsonPayload
        });

        return messages;
    }

    private static string BuildInstructionPrompt(GenerateYamlStoryRequest request)
    {
        var min = Math.Max(1, request.MinScenes);
        var max = Math.Max(min + 1, request.MaxScenes);
        var sb = new StringBuilder();
        sb.AppendLine($"Generate a complete story with {min}–{max} scenes for a tabletop RPG session.");
        sb.AppendLine("The story must be appropriate for the given age group and contain diverse scenes including exploration, dialogue, obstacles, and meaningful choices.");
        sb.AppendLine();
        sb.AppendLine("Requirements:");
        sb.AppendLine("- Use the context parameters provided below (title, difficulty, session length, age group, minimum age, core axes, archetypes, character_count).");
        sb.AppendLine("- age group must be one of \"1-3\", \"4-5\", \"6-9\", \"10-12\", \"13-18\".");
        sb.AppendLine("- Format the result as structured YAML that includes metadata, a characters array, and a scenes array.");
        sb.AppendLine("- Generate a `characters` section with exactly the number of characters specified by `character_count`. Each character must include: id (lowercase snake_case), name, optional image/audio URLs, and a metadata block with role(s), archetype(s), species, age, traits[], and backstory.");
        sb.AppendLine("- Scene types must be well distributed across narrative, choice, and roll.");
        sb.AppendLine("- At least 3 scenes must be of type 'choice' and at least 1 must be of type 'roll'.");
        sb.AppendLine("- Include echo_log and compass_change for key decisions affecting compass axes.");
        sb.AppendLine("- Include optional media fields (image/audio/video URLs) for some scenes.");
        sb.AppendLine();
        sb.AppendLine("Important constraints:");
        sb.AppendLine("- Only output valid YAML. No commentary or markdown. No specification that it is yaml content, e.g. ```yaml");
        sb.AppendLine("- When generating YAML, please ensure all special characters are properly escaped according to YAML specification:1. Enclose strings with special characters in double quotes (\")\n2. Escape backslashes with another backslash (\\\\)\n3. For strings containing quotes, either:\n   - Use single quotes for strings with double quotes\n   - Use double quotes and escape contained double quotes with backslash (\\\\\")\n4. Escape newlines within strings as \\\\n\n5. For values starting with special characters like [, ], {, }, :, &, *, !, |, >, ', \", %, @, `, enclose in quotes\n6. Numeric-looking strings that should be treated as text must be quoted\n");
        sb.AppendLine("-- 1. Enclose strings with special characters in double quotes (\")");
        sb.AppendLine(@"-- 2. Escape backslashes with another backslash (\\)");
        sb.AppendLine("-- 3. For strings containing quotes, either: (a) Use single quotes for strings with double quotes, or (b) use double quotes and escape contained double quotes with backslash (\\\\\")");
        sb.AppendLine(@"-- 4. Escape newlines within strings as \\n");
        sb.AppendLine("-- 5. For values starting with special characters like [, ], {, }, :, &, *, !, |, >, ', \", %, @, `, enclose in quotes\n6. Numeric-looking strings that should be treated as text must be quoted\n");
        sb.AppendLine("- All IDs must be lowercase snake_case, with scene IDs prefixed by the story ID.");
        sb.AppendLine("- Ensure logical branching with unique scene IDs and complete next_scene references where applicable.");
        return sb.ToString();
    }

    private static string GetShortExampleYaml()
    {
        return @"title: The Mystery of the Missing Glow-Berries
description: Four friends from Whispering Woods discover the special Glow-Berries are gone. They must work together, follow clues, and learn about fairness and empathy to solve the mystery.
tags: [mystery, animals, friendship, forest]
difficulty: Easy
session_length: Short
age_group: 6-9
minimum_age: 6
archetypes:
  - the_helper
  - the_science_investigator
characters:
  - id: archimedes
    name: Archimedes
    image: media/images/archimedes.png
    audio: media/audio/archimedes.mp3
    metadata:
      role:
        - Thinker
        - Logic Builder
        - Truth Finder
      archetype:
        - the_science_investigator
      species: Owl
      age: 12
      traits:
        - wise
        - logical
        - calm
        - observant
        - thoughtful
      backstory: From high branches, Archimedes maps moonlit patterns, sharing insight only when it guides minds toward kinder truths.
  - id: nutmeg
    name: Nutmeg
    image: media/images/nutmeg.png
    audio: media/audio/nutmeg.mp3
    metadata:
      role:
        - Shadow Scout
        - Clue Collector
        - Resource Giver
      archetype:
        - the_helper
      species: Squirrel
      age: 5
      traits:
        - energetic
        - quick
        - observant
        - excitable
        - easily-flustered
      backstory: Nutmeg’s twitchy curiosity uncovers hidden paths and shiny clues, though his nerves squeak louder than his courage when shadows deepen.
core_axes:
  - Empathy
  - Accountability
scenes:
  - id: glow_berries_start
    type: narrative
    title: The Empty Bush
    description: The bush where the brightest Glow-Berries grew is empty. The friends gather to investigate.
    next_scene: glow_berries_first_choice
  - id: glow_berries_first_choice
    type: choice
    title: Where to Begin?
    description: How should the group start their investigation?
    branches:
      - choice: Ask Nutmeg if he saw anything this morning.
        next_scene: glow_berries_witness
        compass_change:
          axis: Empathy
          delta: 0.4
        echo_log:
          echo_type: inquisitive_asker
          description: Asked a witness for more details before acting.
          strength: 1
      - choice: Search the ground for tracks.
        next_scene: glow_berries_tracks
        compass_change:
          axis: Accountability
          delta: 0.3
        echo_log:
          echo_type: careful_investigator
          description: Looked for physical clues at the scene.
          strength: 1
  - id: glow_berries_tracks
    type: roll
    title: Searching for Tracks
    description: The group scans the ground for a clear trail.
    difficulty: 8
    branches:
      - choice: Success! Clear paw prints lead deeper into the woods.
        next_scene: glow_berries_trail
      - choice: Failure. The ground is too messy to read.
        next_scene: glow_berries_dead_end
  - id: glow_berries_trail
    type: narrative
    title: A Faint Shimmer
    description: A faint shimmer on the path suggests something sticky was rolled along the ground.
    next_scene: glow_berries_second_choice
  - id: glow_berries_second_choice
    type: choice
    title: Cautious or Bold?
    description: Ahead, a soft light glows behind rocks. How should they approach?
    branches:
      - choice: Approach quietly and speak gently.
        next_scene: glow_berries_gentle
        compass_change:
          axis: Empathy
          delta: 0.5
        echo_log:
          echo_type: empathic_listener
          description: Chose a gentle approach to avoid frightening someone.
          strength: 1
      - choice: Demand answers loudly.
        next_scene: glow_berries_loud
        compass_change:
          axis: Empathy
          delta: -0.4
        echo_log:
          echo_type: moral_judge
          description: Used a harsh approach that may harm trust.
          strength: 1
";
    }
}
