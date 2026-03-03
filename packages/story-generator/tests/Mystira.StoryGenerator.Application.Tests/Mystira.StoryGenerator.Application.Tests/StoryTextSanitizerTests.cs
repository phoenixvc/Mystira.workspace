using System.Text.Json;
using Mystira.StoryGenerator.Application.Utilities;

namespace Mystira.StoryGenerator.Application.Tests;

public class StoryTextSanitizerTests
{
    [Fact]
    public void CollapseNewlinesToSpace_JsonInput_ReplacesNewlinesOnlyInStringValues()
    {
        // Arrange: JSON with string values containing escaped newlines (\n, \r\n)
        var json = "{" +
                   "\"title\":\"Test\"," +
                   "\"description\":\"Line1\\n\\nLine2\\r\\nLine3\"," +
                   "\"count\":2," +
                   "\"done\":false," +
                   "\"meta\":{\"note\":\"Hello\\r\\n\\r\\nWorld\"}," +
                   "\"scenes\":[{" +
                   "  \"id\":\"s1\",\"title\":\"Start\",\"type\":\"narrative\",\"description\":\"A\\nB\",\"next_scene\":\"s2\"},{" +
                   "  \"id\":\"s2\",\"title\":\"End\",\"type\":\"special\",\"description\":\"End\"}" +
                   "]}";

        // Act
        var sanitized = StoryTextSanitizer.CollapseNewlinesToSpace(json);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(sanitized));

        using var doc = JsonDocument.Parse(sanitized!);
        var root = doc.RootElement;

        Assert.Equal("Test", root.GetProperty("title").GetString());
        Assert.Equal("Line1 Line2 Line3", root.GetProperty("description").GetString());

        // Ensure primitives are preserved
        Assert.Equal(2, root.GetProperty("count").GetInt32());
        Assert.False(root.GetProperty("done").GetBoolean());

        // Nested string value sanitized
        Assert.Equal("Hello World", root.GetProperty("meta").GetProperty("note").GetString());

        // Array contents sanitized for string fields only
        var scenes = root.GetProperty("scenes");
        Assert.Equal(JsonValueKind.Array, scenes.ValueKind);
        Assert.Equal(2, scenes.GetArrayLength());
        Assert.Equal("A B", scenes[0].GetProperty("description").GetString());
        Assert.Equal("End", scenes[1].GetProperty("description").GetString());
    }
}
