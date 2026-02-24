using Mystira.App.Domain.Models;

namespace Mystira.App.Domain.Tests.Models;

public class RandomNameGeneratorTests
{
    #region GenerateFantasyName Tests

    [Fact]
    public void GenerateFantasyName_ReturnsNonEmptyString()
    {
        // Act
        var name = RandomNameGenerator.GenerateFantasyName();

        // Assert
        name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateFantasyName_ReturnsNameFromFantasyNames()
    {
        // Act
        var name = RandomNameGenerator.GenerateFantasyName();

        // Assert
        RandomNameGenerator.FantasyNames.Should().Contain(name);
    }

    [Fact]
    public void GenerateFantasyName_CalledMultipleTimes_ReturnsNames()
    {
        // Act - Generate multiple names to verify consistency
        var names = Enumerable.Range(0, 10)
            .Select(_ => RandomNameGenerator.GenerateFantasyName())
            .ToList();

        // Assert
        names.Should().AllSatisfy(n => n.Should().NotBeNullOrWhiteSpace());
    }

    #endregion

    #region GenerateAdjectiveName Tests

    [Fact]
    public void GenerateAdjectiveName_ReturnsNonEmptyString()
    {
        // Act
        var name = RandomNameGenerator.GenerateAdjectiveName();

        // Assert
        name.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAdjectiveName_ContainsSpace()
    {
        // Act
        var name = RandomNameGenerator.GenerateAdjectiveName();

        // Assert
        name.Should().Contain(" ", "adjective names should be in 'Adjective Name' format");
    }

    [Fact]
    public void GenerateAdjectiveName_ContainsTwoParts()
    {
        // Act
        var name = RandomNameGenerator.GenerateAdjectiveName();
        var parts = name.Split(' ');

        // Assert
        parts.Should().HaveCount(2);
        parts[0].Should().NotBeNullOrWhiteSpace("adjective should not be empty");
        parts[1].Should().NotBeNullOrWhiteSpace("name should not be empty");
    }

    #endregion

    #region GenerateGuestName Tests

    [Fact]
    public void GenerateGuestName_WithoutAdjective_ReturnsSimpleName()
    {
        // Act
        var name = RandomNameGenerator.GenerateGuestName(useAdjective: false);

        // Assert
        name.Should().NotBeNullOrWhiteSpace();
        name.Should().NotContain(" ", "simple names should not contain spaces");
    }

    [Fact]
    public void GenerateGuestName_WithAdjective_ReturnsCompoundName()
    {
        // Act
        var name = RandomNameGenerator.GenerateGuestName(useAdjective: true);

        // Assert
        name.Should().NotBeNullOrWhiteSpace();
        name.Should().Contain(" ", "adjective names should contain a space");
    }

    [Fact]
    public void GenerateGuestName_DefaultParameter_ReturnsSimpleName()
    {
        // Act
        var name = RandomNameGenerator.GenerateGuestName();

        // Assert - Default should be useAdjective: false
        name.Should().NotContain(" ");
    }

    #endregion

    #region GenerateUniqueGuestNames Tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void GenerateUniqueGuestNames_ReturnsRequestedCount(int count)
    {
        // Act
        var names = RandomNameGenerator.GenerateUniqueGuestNames(count);

        // Assert
        names.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateUniqueGuestNames_ReturnsUniqueNames()
    {
        // Act
        var names = RandomNameGenerator.GenerateUniqueGuestNames(5);

        // Assert
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateUniqueGuestNames_WithAdjective_ReturnsCompoundNames()
    {
        // Act
        var names = RandomNameGenerator.GenerateUniqueGuestNames(3, useAdjective: true);

        // Assert
        names.Should().AllSatisfy(n => n.Should().Contain(" "));
    }

    [Fact]
    public void GenerateUniqueGuestNames_WithZeroCount_ReturnsEmptyList()
    {
        // Act
        var names = RandomNameGenerator.GenerateUniqueGuestNames(0);

        // Assert
        names.Should().BeEmpty();
    }

    [Fact]
    public void GenerateUniqueGuestNames_WithTooManyRequested_ThrowsArgumentException()
    {
        // Arrange - Request more unique names than possible without adjectives
        var tooMany = RandomNameGenerator.FantasyNames.Length + 1;

        // Act
        var act = () => RandomNameGenerator.GenerateUniqueGuestNames(tooMany, useAdjective: false);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("count");
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public void GenerateFantasyName_CalledFromMultipleThreads_DoesNotThrow()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() => RandomNameGenerator.GenerateFantasyName()));
        }

        var act = () => Task.WaitAll(tasks.ToArray());

        // Assert
        act.Should().NotThrow();
        tasks.Select(t => t.Result).Should().AllSatisfy(n => n.Should().NotBeNullOrWhiteSpace());
    }

    #endregion

    #region Data Loading Tests

    [Fact]
    public void FantasyNames_IsNotEmpty()
    {
        // Assert
        RandomNameGenerator.FantasyNames.Should().NotBeEmpty("embedded FantasyNames.json should contain names");
    }

    [Fact]
    public void AdjectiveNames_IsNotEmpty()
    {
        // Assert
        RandomNameGenerator.AdjectiveNames.Should().NotBeEmpty("embedded AdjectiveNames.json should contain adjectives");
    }

    #endregion
}
