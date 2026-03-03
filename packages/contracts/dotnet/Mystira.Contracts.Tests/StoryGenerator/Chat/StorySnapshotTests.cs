using FluentAssertions;
using Mystira.Contracts.StoryGenerator.Chat;
using Xunit;

namespace Mystira.Contracts.Tests.StoryGenerator.Chat;

public class StorySnapshotTests
{
    [Fact]
    public void AgeGroup_WithEmptyContent_ReturnsNull()
    {
        var snapshot = new StorySnapshot { Content = "" };
        snapshot.AgeGroup.Should().BeNull();
    }

    [Fact]
    public void AgeGroup_WithValidJsonAndAgeGroup_ReturnsAgeGroup()
    {
        var snapshot = new StorySnapshot
        {
            Content = "{\"age_group\": \"10-12\", \"title\": \"Test\"}"
        };
        snapshot.AgeGroup.Should().Be("10-12");
    }

    [Fact]
    public void AgeGroup_WithValidJsonButNoAgeGroup_ReturnsNull()
    {
        var snapshot = new StorySnapshot
        {
            Content = "{\"title\": \"Test Story\"}"
        };
        snapshot.AgeGroup.Should().BeNull();
    }

    [Fact]
    public void AgeGroup_WithInvalidJson_ReturnsNull()
    {
        var snapshot = new StorySnapshot
        {
            Content = "{\"invalid\": json}"
        };
        snapshot.AgeGroup.Should().BeNull();
    }

    [Fact]
    public void AgeGroup_WithMalformedJson_ReturnsNull()
    {
        var snapshot = new StorySnapshot
        {
            Content = "{\"age_group\": \"10-12\""
        };
        snapshot.AgeGroup.Should().BeNull();
    }

    [Fact]
    public void StorySnapshot_DefaultValues_AreSetCorrectly()
    {
        var snapshot = new StorySnapshot();
        snapshot.StoryId.Should().Be(string.Empty);
        snapshot.StoryVersion.Should().Be(0);
        snapshot.Content.Should().Be(string.Empty);
        snapshot.AgeGroup.Should().BeNull();
    }
}
