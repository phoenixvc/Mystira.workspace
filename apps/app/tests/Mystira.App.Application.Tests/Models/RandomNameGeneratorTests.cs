using FluentAssertions;
using Mystira.Shared.Utilities;

namespace Mystira.App.Application.Tests.Models;

public class RandomNameGeneratorTests
{
    [Fact]
    public void GenerateFirstName_ReturnsNonEmptyString()
    {
        // Act
        var name = RandomNameGenerator.GenerateFirstName();

        // Assert
        name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateFullName_ReturnsNameWithSpace()
    {
        // Act
        var name = RandomNameGenerator.GenerateFullName();

        // Assert
        name.Should().Contain(" ");
    }

    [Fact]
    public void GenerateGuestName_ReturnsPrefixedName()
    {
        // Act
        var guestName = RandomNameGenerator.GenerateGuestName();

        // Assert
        guestName.Should().NotBeNullOrEmpty();
        guestName.Should().StartWith("Guest_");
    }

    [Fact]
    public void GenerateFirstNames_ReturnsCorrectCount()
    {
        // Arrange
        const int count = 5;

        // Act
        var names = RandomNameGenerator.GenerateFirstNames(count);

        // Assert
        names.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateFullNames_ReturnsUniqueNames()
    {
        // Arrange
        const int count = 10;

        // Act
        var names = RandomNameGenerator.GenerateFullNames(count);

        // Assert
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task GenerateFullNames_ThreadSafetyTest()
    {
        // Arrange
        const int numThreads = 10;
        const int namesPerThread = 100;
        var tasks = new Task<List<string>>[numThreads];

        // Act
        for (var i = 0; i < numThreads; i++)
        {
            tasks[i] = Task.Run(() => RandomNameGenerator.GenerateFullNames(namesPerThread));
        }

        await Task.WhenAll(tasks);

        // Assert
        var allNames = new List<string>();
        foreach (var task in tasks)
        {
            allNames.AddRange(await task);
        }

        allNames.Should().HaveCount(numThreads * namesPerThread);
    }
}
