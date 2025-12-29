using FluentAssertions;
using Mystira.Contracts.StoryGenerator.Chat;
using Xunit;

namespace Mystira.Contracts.Tests.StoryGenerator.Chat;

public class StoryJsonRepairHelperTests
{
    [Fact]
    public void RepairPartialJson_WithNullInput_ReturnsEmptyObject()
    {
        var result = StoryJsonRepairHelper.RepairPartialJson(null!);
        result.Should().Be("{}");
    }

    [Fact]
    public void RepairPartialJson_WithEmptyString_ReturnsEmptyObject()
    {
        var result = StoryJsonRepairHelper.RepairPartialJson("");
        result.Should().Be("{}");
    }

    [Fact]
    public void RepairPartialJson_WithCompleteJson_ReturnsUnchanged()
    {
        var completeJson = "{\"name\": \"test\"}";
        var result = StoryJsonRepairHelper.RepairPartialJson(completeJson);
        result.Should().Be(completeJson);
    }

    [Fact]
    public void RepairPartialJson_WithUnclosedObject_AddsClosingBrace()
    {
        var partialJson = "{\"name\": \"test\"";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("{\"name\": \"test\"}");
    }

    [Fact]
    public void RepairPartialJson_WithUnclosedArray_AddsClosingBracket()
    {
        var partialJson = "[\"item1\", \"item2\"";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("[\"item1\", \"item2\"]");
    }

    [Fact]
    public void RepairPartialJson_WithNestedUnclosedObjects_AddsMultipleClosingBraces()
    {
        var partialJson = "{\"outer\": {\"inner\": {\"deep\": \"value\"";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("{\"outer\": {\"inner\": {\"deep\": \"value\"}}}");
    }

    [Fact]
    public void RepairPartialJson_WithMixedUnclosed_AddsCorrectClosing()
    {
        var partialJson = "{\"array\": [1, 2, {\"nested\": \"val\"";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("{\"array\": [1, 2, {\"nested\": \"val\"}]}");
    }

    [Fact]
    public void RepairPartialJson_WithUnclosedString_ClosesString()
    {
        var partialJson = "{\"name\": \"unfinished";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("{\"name\": \"unfinished\"}");
    }

    [Fact]
    public void RepairPartialJson_WithBracesInString_IgnoresThem()
    {
        var partialJson = "{\"message\": \"This has { and } inside\"";
        var result = StoryJsonRepairHelper.RepairPartialJson(partialJson);
        result.Should().Be("{\"message\": \"This has { and } inside\"}");
    }
}
