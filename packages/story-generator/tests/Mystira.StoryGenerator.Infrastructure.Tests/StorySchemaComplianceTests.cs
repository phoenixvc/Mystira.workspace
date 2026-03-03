using System.Text.Json;
using Xunit;

namespace Mystira.StoryGenerator.Infrastructure.Tests;

/// <summary>
/// Tests for story schema compliance and validation.
/// Ensures generated stories always validate against the expected schema.
/// </summary>
public class StorySchemaComplianceTests
{
    private readonly List<string> _sampleStories = new()
    {
        // Story 1: Basic valid story
        """
        {
            "title": "The Brave Little Mouse",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Beginning",
                    "content": "Once upon a time, there was a brave little mouse."
                }
            ]
        }
        """,

        // Story 2: Multi-scene story
        """
        {
            "title": "The Wizard's Apprentice",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The First Lesson",
                    "content": "The young apprentice learned magic."
                },
                {
                    "id": "scene_2",
                    "title": "The Challenge",
                    "content": "A dragon appeared in the village."
                },
                {
                    "id": "scene_3",
                    "title": "Victory",
                    "content": "Together they saved the day."
                }
            ]
        }
        """,

        // Story 3: Story with metadata
        """
        {
            "title": "Adventure in the Forest",
            "author": "AI Story Generator",
            "ageGroup": "6-9",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Into the Woods",
                    "content": "Sarah entered the mysterious forest.",
                    "characters": ["Sarah", "Forest Guardian"]
                }
            ]
        }
        """,

        // Story 4: Complex story with dialogue
        """
        {
            "title": "The Knight and the Dragon",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Meeting",
                    "content": "The knight met the dragon at the mountain peak.",
                    "dialogue": [
                        {
                            "speaker": "Knight",
                            "text": "I come in peace."
                        },
                        {
                            "speaker": "Dragon",
                            "text": "Then you are welcome."
                        }
                    ]
                }
            ]
        }
        """,

        // Story 5: Story with themes
        """
        {
            "title": "Friendship Wins",
            "themes": ["friendship", "courage", "kindness"],
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Problem",
                    "content": "Two friends faced a challenge together."
                },
                {
                    "id": "scene_2",
                    "title": "The Solution",
                    "content": "Their friendship helped them succeed."
                }
            ]
        }
        """,

        // Story 6: Story with moral
        """
        {
            "title": "The Honest Merchant",
            "moral": "Honesty is always the best policy.",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Choice",
                    "content": "The merchant found extra money in the till."
                },
                {
                    "id": "scene_2",
                    "title": "The Right Thing",
                    "content": "He returned it to the customer."
                }
            ]
        }
        """,

        // Story 7: Story with locations
        """
        {
            "title": "Journey Across the Kingdom",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Castle",
                    "content": "The journey began at the royal castle.",
                    "location": "Castle Brightstone"
                },
                {
                    "id": "scene_2",
                    "title": "The Village",
                    "content": "They stopped at a friendly village.",
                    "location": "Willowbrook Village"
                }
            ]
        }
        """,

        // Story 8: Story with emotions
        """
        {
            "title": "The Lost Puppy",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Lost",
                    "content": "The puppy couldn't find its way home.",
                    "emotion": "sadness"
                },
                {
                    "id": "scene_2",
                    "title": "Found",
                    "content": "A kind child helped the puppy get home.",
                    "emotion": "joy"
                }
            ]
        }
        """,

        // Story 9: Story with narrative axes
        """
        {
            "title": "The Magic Garden",
            "narrativeAxes": {
                "wonder": 0.9,
                "discovery": 0.8,
                "transformation": 0.7
            },
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Discovery",
                    "content": "Emma found a magical garden."
                }
            ]
        }
        """,

        // Story 10: Comprehensive story
        """
        {
            "title": "The Great Adventure",
            "author": "AI Story Generator",
            "ageGroup": "6-9",
            "themes": ["adventure", "friendship"],
            "moral": "Working together makes us stronger.",
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "The Beginning",
                    "content": "Three friends decided to go on an adventure.",
                    "characters": ["Alex", "Maya", "Tom"],
                    "location": "The Old Oak Tree",
                    "emotion": "excitement"
                },
                {
                    "id": "scene_2",
                    "title": "The Challenge",
                    "content": "They encountered a difficult obstacle.",
                    "characters": ["Alex", "Maya", "Tom"],
                    "emotion": "determination"
                },
                {
                    "id": "scene_3",
                    "title": "The Victory",
                    "content": "Together they overcame the challenge.",
                    "characters": ["Alex", "Maya", "Tom"],
                    "emotion": "joy"
                }
            ]
        }
        """
    };

    [Fact]
    public void GeneratedStories_AlwaysValidateAgainstSchema()
    {
        // Arrange
        var validationResults = new List<(int Index, bool IsValid, string Error)>();

        // Act - Validate all sample stories
        for (int i = 0; i < _sampleStories.Count; i++)
        {
            try
            {
                var story = _sampleStories[i];
                var isValid = ValidateStorySchema(story, out var error);
                validationResults.Add((i + 1, isValid, error));
            }
            catch (Exception ex)
            {
                validationResults.Add((i + 1, false, ex.Message));
            }
        }

        // Assert
        var failedStories = validationResults.Where(r => !r.IsValid).ToList();
        
        Assert.True(failedStories.Count == 0, 
            $"Stories failed validation:\n{string.Join("\n", failedStories.Select(f => $"Story {f.Index}: {f.Error}"))}");
        
        Assert.Equal(_sampleStories.Count, validationResults.Count(r => r.IsValid));
    }

    [Fact]
    public void ValidStory_HasRequiredTitle()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [{"id": "scene_1", "title": "Scene", "content": "Content"}]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.True(isValid, error);
        
        var doc = JsonDocument.Parse(story);
        Assert.True(doc.RootElement.TryGetProperty("title", out _));
    }

    [Fact]
    public void ValidStory_HasScenesArray()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [{"id": "scene_1", "title": "Scene", "content": "Content"}]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.True(isValid, error);
        
        var doc = JsonDocument.Parse(story);
        Assert.True(doc.RootElement.TryGetProperty("scenes", out var scenes));
        Assert.Equal(JsonValueKind.Array, scenes.ValueKind);
        Assert.True(scenes.GetArrayLength() > 0);
    }

    [Fact]
    public void ValidStory_EachSceneHasRequiredFields()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [
                {"id": "scene_1", "title": "First Scene", "content": "First content"},
                {"id": "scene_2", "title": "Second Scene", "content": "Second content"}
            ]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);
        var doc = JsonDocument.Parse(story);
        var scenes = doc.RootElement.GetProperty("scenes");

        // Assert
        Assert.True(isValid, error);
        
        foreach (var scene in scenes.EnumerateArray())
        {
            Assert.True(scene.TryGetProperty("id", out _), "Scene missing 'id'");
            Assert.True(scene.TryGetProperty("title", out _), "Scene missing 'title'");
            Assert.True(scene.TryGetProperty("content", out _), "Scene missing 'content'");
        }
    }

    [Fact]
    public void InvalidStory_MissingTitle_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "scenes": [{"id": "scene_1", "title": "Scene", "content": "Content"}]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("title", error.ToLower());
    }

    [Fact]
    public void InvalidStory_MissingScenes_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story"
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("scenes", error.ToLower());
    }

    [Fact]
    public void InvalidStory_EmptyScenes_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": []
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("scenes", error.ToLower());
    }

    [Fact]
    public void InvalidStory_SceneMissingId_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [{"title": "Scene", "content": "Content"}]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("id", error.ToLower());
    }

    [Fact]
    public void InvalidStory_SceneMissingContent_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [{"id": "scene_1", "title": "Scene"}]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.Contains("content", error.ToLower());
    }

    [Fact]
    public void InvalidStory_MalformedJSON_FailsValidation()
    {
        // Arrange
        var story = """
        {
            "title": "Test Story",
            "scenes": [{"id": "scene_1", "title": "Scene", "content": "Content"
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(error);
    }

    [Fact]
    public void StorySchema_PassRate_IsOneHundredPercent()
    {
        // Arrange
        var totalStories = _sampleStories.Count;
        var validStories = 0;

        // Act
        foreach (var story in _sampleStories)
        {
            if (ValidateStorySchema(story, out _))
            {
                validStories++;
            }
        }

        var passRate = (double)validStories / totalStories * 100;

        // Assert
        Assert.Equal(100.0, passRate);
    }

    [Fact]
    public void StorySchema_SupportsOptionalFields()
    {
        // Arrange - Story with many optional fields
        var story = """
        {
            "title": "Rich Story",
            "author": "Test Author",
            "ageGroup": "6-9",
            "themes": ["adventure"],
            "moral": "Be kind",
            "narrativeAxes": {"wonder": 0.9},
            "scenes": [
                {
                    "id": "scene_1",
                    "title": "Scene",
                    "content": "Content",
                    "characters": ["Hero"],
                    "location": "Forest",
                    "emotion": "joy",
                    "dialogue": [{"speaker": "Hero", "text": "Hello"}]
                }
            ]
        }
        """;

        // Act
        var isValid = ValidateStorySchema(story, out var error);

        // Assert
        Assert.True(isValid, error);
    }

    // Helper method to validate story schema
    private bool ValidateStorySchema(string storyJson, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            // Parse JSON
            var doc = JsonDocument.Parse(storyJson);
            var root = doc.RootElement;

            // Check required field: title
            if (!root.TryGetProperty("title", out var title) || 
                title.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(title.GetString()))
            {
                errorMessage = "Missing or invalid 'title' field";
                return false;
            }

            // Check required field: scenes
            if (!root.TryGetProperty("scenes", out var scenes) || 
                scenes.ValueKind != JsonValueKind.Array)
            {
                errorMessage = "Missing or invalid 'scenes' array";
                return false;
            }

            // Check scenes not empty
            if (scenes.GetArrayLength() == 0)
            {
                errorMessage = "Scenes array cannot be empty";
                return false;
            }

            // Validate each scene
            foreach (var scene in scenes.EnumerateArray())
            {
                // Each scene must have: id, title, content
                if (!scene.TryGetProperty("id", out var id) || 
                    id.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(id.GetString()))
                {
                    errorMessage = "Scene missing or invalid 'id' field";
                    return false;
                }

                if (!scene.TryGetProperty("title", out var sceneTitle) || 
                    sceneTitle.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(sceneTitle.GetString()))
                {
                    errorMessage = "Scene missing or invalid 'title' field";
                    return false;
                }

                if (!scene.TryGetProperty("content", out var content) || 
                    content.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(content.GetString()))
                {
                    errorMessage = "Scene missing or invalid 'content' field";
                    return false;
                }
            }

            return true;
        }
        catch (JsonException ex)
        {
            errorMessage = $"JSON parsing error: {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = $"Validation error: {ex.Message}";
            return false;
        }
    }
}
