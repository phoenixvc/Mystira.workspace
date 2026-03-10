using FluentAssertions;
using Mystira.Domain.ValueObjects;
using Xunit;

namespace Mystira.App.Application.Tests.Domain;

public class AgeGroupConstantsTests
{
    #region GetAll Tests

    [Fact]
    public void GetAll_ContainsExpectedGroups()
    {
        // Assert
        AgeGroupConstants.GetAll().Should().HaveCount(5);
        AgeGroupConstants.GetAll().Should().Contain(AgeGroupConstants.EarlyChildhood);
        AgeGroupConstants.GetAll().Should().Contain(AgeGroupConstants.MiddleChildhood);
        AgeGroupConstants.GetAll().Should().Contain(AgeGroupConstants.Preteen);
        AgeGroupConstants.GetAll().Should().Contain(AgeGroupConstants.Teen);
        AgeGroupConstants.GetAll().Should().Contain(AgeGroupConstants.Adult);
    }

    #endregion

    #region AgeGroup.ForAge Tests

    [Theory]
    [InlineData(4, "early_childhood")]
    [InlineData(6, "early_childhood")]
    [InlineData(7, "middle_childhood")]
    [InlineData(8, "middle_childhood")]
    [InlineData(9, "middle_childhood")]
    [InlineData(10, "preteen")]
    [InlineData(11, "preteen")]
    [InlineData(12, "preteen")]
    [InlineData(13, "teen")]
    [InlineData(15, "teen")]
    [InlineData(17, "teen")]
    [InlineData(18, "adult")]
    [InlineData(25, "adult")]
    [InlineData(50, "adult")]
    public void ForAge_ReturnsCorrectGroup(int age, string expectedGroupId)
    {
        // Act
        var result = AgeGroup.ForAge(age);

        // Assert
        result.Id.Should().Be(expectedGroupId);
    }

    #endregion

    #region GetDisplayName Tests

    [Theory]
    [InlineData("early_childhood", "Early Childhood (4-6)")]
    [InlineData("middle_childhood", "Middle Childhood (7-9)")]
    [InlineData("preteen", "Preteen (10-12)")]
    [InlineData("teen", "Teen (13-17)")]
    [InlineData("adult", "Adult (18+)")]
    public void GetDisplayName_ReturnsCorrectDisplayName(string ageGroupId, string expectedName)
    {
        // Act
        var result = AgeGroupConstants.GetDisplayName(ageGroupId);

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void GetDisplayName_WithUnknownGroup_ReturnsGroupAsIs()
    {
        // Act
        var result = AgeGroupConstants.GetDisplayName("unknown");

        // Assert
        result.Should().Be("unknown");
    }

    #endregion
}
