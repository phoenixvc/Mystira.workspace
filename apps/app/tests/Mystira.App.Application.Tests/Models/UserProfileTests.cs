using FluentAssertions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.Models;

public class UserProfileTests
{
    [Fact]
    public void UserProfile_DefaultConstructor_SetsDefaultValues()
    {
        // Act
        var userProfile = new UserProfile();

        // Assert
        userProfile.Name.Should().BeEmpty();
        userProfile.PreferredFantasyThemes.Should().NotBeNull().And.BeEmpty();
        userProfile.AgeGroupId.Should().BeNull();
        userProfile.HasCompletedOnboarding.Should().BeFalse();
        userProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserProfile_Age_CalculatesCorrectly()
    {
        // Arrange
        var userProfile = new UserProfile();
        var today = DateOnly.FromDateTime(DateTime.Today);
        userProfile.DateOfBirth = new DateOnly(today.Year - 10, today.Month, today.Day);

        // Act
        var age = userProfile.Age;

        // Assert
        age.Should().Be(10);
    }
}
