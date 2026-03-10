using FluentAssertions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Utilities;
using Xunit;

namespace Mystira.App.Application.Tests.Domain;

public class DomainModelTests
{
    #region CoreAxis Tests

    [Fact]
    public void CoreAxis_StaticInstance_HasValue()
    {
        // Arrange & Act
        var coreAxis = CoreAxis.Courage;

        // Assert
        coreAxis.Value.Should().Be("courage");
    }

    [Fact]
    public void CoreAxis_ToString_ReturnsValue()
    {
        // Arrange
        var coreAxis = CoreAxis.Wisdom;

        // Act & Assert
        coreAxis.ToString().Should().Be("wisdom");
    }

    [Fact]
    public void CoreAxis_Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var axis1 = CoreAxis.Parse("courage");
        var axis2 = CoreAxis.Parse("courage");

        // Act & Assert
        axis1.Equals(axis2).Should().BeTrue();
        (axis1 == axis2).Should().BeTrue();
    }

    [Fact]
    public void CoreAxis_Equals_CaseInsensitive()
    {
        // Arrange
        var axis1 = CoreAxis.FromValue("Courage");
        var axis2 = CoreAxis.FromValue("COURAGE");

        // Act & Assert
        axis1!.Equals(axis2).Should().BeTrue();
    }

    [Fact]
    public void CoreAxis_Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var axis1 = CoreAxis.Courage;
        var axis2 = CoreAxis.Wisdom;

        // Act & Assert
        (axis1 != axis2).Should().BeTrue();
    }

    [Fact]
    public void CoreAxis_GetHashCode_SameForEqualValues()
    {
        // Arrange
        var axis1 = CoreAxis.FromValue("Courage");
        var axis2 = CoreAxis.FromValue("courage");

        // Act & Assert
        axis1!.GetHashCode().Should().Be(axis2!.GetHashCode());
    }

    [Fact]
    public void CoreAxis_Parse_WithValidValue_ReturnsCoreAxis()
    {
        // Act
        var result = CoreAxis.Parse("courage");

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("courage");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CoreAxis_Parse_WithEmptyOrWhitespace_Throws(string? value)
    {
        // Act & Assert
        FluentActions.Invoking(() => CoreAxis.Parse(value!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CoreAxis_FromValue_WithNull_ReturnsNull()
    {
        // Act
        var result = CoreAxis.FromValue(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Archetype Tests

    [Fact]
    public void Archetype_StaticInstance_HasValue()
    {
        // Arrange & Act
        var archetype = Archetype.Hero;

        // Assert
        archetype.Value.Should().Be("hero");
    }

    [Fact]
    public void Archetype_ToString_ReturnsValue()
    {
        // Arrange
        var archetype = Archetype.Hero;

        // Act & Assert
        archetype.ToString().Should().Be("hero");
    }

    [Fact]
    public void Archetype_Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var arch1 = Archetype.Parse("hero");
        var arch2 = Archetype.Parse("hero");

        // Act & Assert
        arch1.Equals(arch2).Should().BeTrue();
        (arch1 == arch2).Should().BeTrue();
    }

    [Fact]
    public void Archetype_Equals_CaseInsensitive()
    {
        // Arrange
        var arch1 = Archetype.FromValue("Hero");
        var arch2 = Archetype.FromValue("HERO");

        // Act & Assert
        arch1!.Equals(arch2).Should().BeTrue();
    }

    [Fact]
    public void Archetype_Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var arch1 = Archetype.Hero;
        var arch2 = Archetype.Sage;

        // Act & Assert
        (arch1 != arch2).Should().BeTrue();
    }

    [Fact]
    public void Archetype_GetHashCode_SameForEqualValues()
    {
        // Arrange
        var arch1 = Archetype.FromValue("Hero");
        var arch2 = Archetype.FromValue("hero");

        // Act & Assert
        arch1!.GetHashCode().Should().Be(arch2!.GetHashCode());
    }

    [Fact]
    public void Archetype_Parse_WithValidValue_ReturnsArchetype()
    {
        // Act
        var result = Archetype.Parse("hero");

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("hero");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Archetype_Parse_WithEmptyOrWhitespace_Throws(string? value)
    {
        // Act & Assert
        FluentActions.Invoking(() => Archetype.Parse(value!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Archetype_FromValue_WithNull_ReturnsNull()
    {
        // Act
        var result = Archetype.FromValue(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region EchoType Tests

    [Fact]
    public void EchoType_StaticInstance_HasValue()
    {
        // Arrange & Act
        var echoType = EchoType.Memory;

        // Assert
        echoType.Value.Should().Be("memory");
    }

    [Fact]
    public void EchoType_ToString_ReturnsValue()
    {
        // Arrange
        var echoType = EchoType.Vision;

        // Act & Assert
        echoType.ToString().Should().Be("vision");
    }

    [Fact]
    public void EchoType_Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var echo1 = EchoType.Parse("memory");
        var echo2 = EchoType.Parse("memory");

        // Act & Assert
        echo1.Equals(echo2).Should().BeTrue();
        (echo1 == echo2).Should().BeTrue();
    }

    [Fact]
    public void EchoType_Equals_CaseInsensitive()
    {
        // Arrange
        var echo1 = EchoType.FromValue("Memory");
        var echo2 = EchoType.FromValue("MEMORY");

        // Act & Assert
        echo1!.Equals(echo2).Should().BeTrue();
    }

    [Fact]
    public void EchoType_Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var echo1 = EchoType.Memory;
        var echo2 = EchoType.Vision;

        // Act & Assert
        (echo1 != echo2).Should().BeTrue();
    }

    [Fact]
    public void EchoType_GetHashCode_SameForEqualValues()
    {
        // Arrange
        var echo1 = EchoType.FromValue("Vision");
        var echo2 = EchoType.FromValue("vision");

        // Act & Assert
        echo1!.GetHashCode().Should().Be(echo2!.GetHashCode());
    }

    [Fact]
    public void EchoType_Parse_WithValidValue_ReturnsEchoType()
    {
        // Act
        var result = EchoType.Parse("memory");

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be("memory");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void EchoType_Parse_WithEmptyOrWhitespace_Throws(string? value)
    {
        // Act & Assert
        FluentActions.Invoking(() => EchoType.Parse(value!))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EchoType_FromValue_WithNull_ReturnsNull()
    {
        // Act
        var result = EchoType.FromValue(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RandomNameGenerator Tests

    [Fact]
    public void RandomNameGenerator_GenerateFirstName_ReturnsNonEmptyString()
    {
        // Act
        var name = RandomNameGenerator.GenerateFirstName();

        // Assert
        name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RandomNameGenerator_GenerateFullName_ReturnsNameWithSpace()
    {
        // Act
        var name = RandomNameGenerator.GenerateFullName();

        // Assert
        name.Should().NotBeNullOrEmpty();
        name.Should().Contain(" ");
    }

    [Fact]
    public void RandomNameGenerator_GenerateGuestName_ReturnsPrefixedName()
    {
        // Act
        var name = RandomNameGenerator.GenerateGuestName();

        // Assert
        name.Should().NotBeNullOrEmpty();
        name.Should().StartWith("Guest_");
    }

    [Fact]
    public void RandomNameGenerator_GenerateFirstNames_ReturnsRequestedCount()
    {
        // Act
        var names = RandomNameGenerator.GenerateFirstNames(5);

        // Assert
        names.Should().HaveCount(5);
        names.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region AgeGroup Tests

    [Fact]
    public void AgeGroup_Parse_MiddleChildhood_ReturnsCorrectAgeGroup()
    {
        // Act
        var ageGroup = AgeGroup.Parse("middle_childhood");

        // Assert
        ageGroup.Should().NotBeNull();
        ageGroup!.Id.Should().Be("middle_childhood");
    }

    [Fact]
    public void AgeGroup_Parse_Preteen_ReturnsCorrectAgeGroup()
    {
        // Act
        var ageGroup = AgeGroup.Parse("preteen");

        // Assert
        ageGroup.Should().NotBeNull();
        ageGroup!.Id.Should().Be("preteen");
    }

    [Fact]
    public void AgeGroup_Parse_Teen_ReturnsCorrectAgeGroup()
    {
        // Act
        var ageGroup = AgeGroup.Parse("teen");

        // Assert
        ageGroup.Should().NotBeNull();
        ageGroup!.Id.Should().Be("teen");
    }

    [Fact]
    public void AgeGroup_FromId_WithValidValue_ReturnsAgeGroup()
    {
        // Act
        var result = AgeGroup.FromId("middle_childhood");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void AgeGroup_FromId_WithInvalidValue_ReturnsNull()
    {
        // Act
        var result = AgeGroup.FromId("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AgeGroup_All_ContainsExpectedValues()
    {
        // Assert
        AgeGroup.All.Should().Contain(ag => ag.Id == "middle_childhood");
        AgeGroup.All.Should().Contain(ag => ag.Id == "preteen");
        AgeGroup.All.Should().Contain(ag => ag.Id == "teen");
    }

    [Fact]
    public void AgeGroup_Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var group1 = AgeGroup.Parse("middle_childhood");
        var group2 = AgeGroup.Parse("middle_childhood");

        // Act & Assert
        group1!.Equals(group2).Should().BeTrue();
    }

    [Fact]
    public void AgeGroup_GetHashCode_SameForEqualValues()
    {
        // Arrange
        var group1 = AgeGroup.Parse("middle_childhood");
        var group2 = AgeGroup.Parse("middle_childhood");

        // Act & Assert
        group1!.GetHashCode().Should().Be(group2!.GetHashCode());
    }

    [Fact]
    public void AgeGroup_ForAge_ReturnsCorrectAgeGroup()
    {
        // Arrange & Act
        var ageGroup = AgeGroup.ForAge(10);

        // Assert
        ageGroup.MinAge.Should().Be(10);
        ageGroup.MaxAge.Should().Be(12);
    }

    [Fact]
    public void AgeGroup_Contains_ReturnsTrue_WhenAgeIsInRange()
    {
        // Arrange
        var ageGroup = AgeGroup.Preteen;

        // Assert
        ageGroup.Contains(10).Should().BeTrue();
        ageGroup.Contains(12).Should().BeTrue();
        ageGroup.Contains(9).Should().BeFalse();
        ageGroup.Contains(13).Should().BeFalse();
    }

    #endregion
}
