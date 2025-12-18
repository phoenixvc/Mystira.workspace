using Mystira.StoryGenerator.Web.Services;

namespace Mystira.StoryGenerator.Api.Tests;

public class JsonYamlConverterTests
{
    [Fact]
    public void ToYaml_ReturnsEmpty_OnNullOrWhitespace()
    {
        Assert.Equal(string.Empty, JsonYamlConverter.ToYaml(null));
        Assert.Equal(string.Empty, JsonYamlConverter.ToYaml(string.Empty));
        Assert.Equal(string.Empty, JsonYamlConverter.ToYaml("   \n\t   "));
    }

    [Fact]
    public void ToYaml_InvalidJson_ReturnsOriginalString()
    {
        var input = "{ not json }";
        var yaml = JsonYamlConverter.ToYaml(input);
        Assert.Equal(input, yaml);
    }

    [Fact]
    public void ToYaml_ConvertsValidJson_ToReadableYaml()
    {
        var json = @"{
  ""title"": ""A Little Tale"",
  ""minimum_age"": 7,
  ""core_axes"": [""Honesty"", ""Bravery""],
  ""flag"": true,
  ""count"": 3,
  ""pi"": 3.5,
  ""nested"": { ""x"": 1 },
  ""arr"": [{ ""y"": 2 }, 3]
}";

        var yaml = JsonYamlConverter.ToYaml(json);

        // Basic expectations: YAML-like output and no JsonElement artifacts
        Assert.False(string.IsNullOrWhiteSpace(yaml));
        Assert.DoesNotContain("value_kind", yaml, StringComparison.OrdinalIgnoreCase);

        // Spot-check some key values exist in the YAML
        Assert.Contains("title: A Little Tale", yaml);
        Assert.Contains("minimum_age: 7", yaml);
        Assert.Contains("core_axes:", yaml);
        Assert.Contains("- Honesty", yaml);
        Assert.Contains("- Bravery", yaml);
        Assert.Contains("flag: true", yaml);
        Assert.Contains("count: 3", yaml);
        Assert.Contains("pi: 3.5", yaml);
        Assert.Contains("nested:", yaml);
        Assert.Contains("x: 1", yaml);
    }

    [Fact]
    public void ToYaml_HandlesQuotesCorrectly()
    {
        // Case 1: Plain scalar (no quotes needed in YAML)
        var json1 = @"{ ""description"": ""He says, \""Hello!\"" and walks away."" }";
        var yaml1 = JsonYamlConverter.ToYaml(json1);
        Assert.Contains("Hello!", yaml1);
        Assert.DoesNotContain("\\\"", yaml1);

        // Case 2: Forced quoting (contains ': ') - should use single quotes in YAML to avoid escaping double quotes
        var json2 = @"{ ""description"": ""He says: \""Hello!\"" and walks away."" }";
        var yaml2 = JsonYamlConverter.ToYaml(json2);
        Assert.Contains("Hello!", yaml2);
        Assert.DoesNotContain("\\\"", yaml2);

        // Case 3: Both single and double quotes
        var json3 = @"{ ""description"": ""It's a \""magic\"" day."" }";
        var yaml3 = JsonYamlConverter.ToYaml(json3);
        Assert.Contains("magic", yaml3);
        Assert.DoesNotContain("\\\"", yaml3);
    }
}
