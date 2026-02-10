using FluentAssertions;
using Mystira.App.PWA.Services;
using Xunit;

namespace Mystira.App.PWA.Tests.Services;

public class AgeGroupHelperTests
{
    #region Exact String Match Tests

    [Theory]
    [InlineData("6-9", "6-9", true)]
    [InlineData("10-12", "10-12", true)]
    [InlineData("3-5", "3-5", true)]
    public void AgeGroupMatches_WithExactMatch_ReturnsTrue(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("6-9", "10-12", false)]
    [InlineData("3-5", "6-9", false)]
    [InlineData("10-12", "3-5", false)]
    public void AgeGroupMatches_WithDifferentGroups_ReturnsFalse(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Case Insensitive Match Tests

    [Theory]
    [InlineData("Ages 6-9", "ages 6-9", true)]
    [InlineData("AGES 10-12", "ages 10-12", true)]
    [InlineData("Young Kids", "young kids", true)]
    public void AgeGroupMatches_WithDifferentCasing_ReturnsTrue(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Whitespace Handling Tests

    [Theory]
    [InlineData("  6-9  ", "6-9", true)]
    [InlineData("6-9", "  6-9  ", true)]
    [InlineData("  6-9  ", "  6-9  ", true)]
    public void AgeGroupMatches_WithLeadingTrailingWhitespace_ReturnsTrue(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Numeric Bounds Fallback Tests

    [Theory]
    [InlineData("06-09", "6-9", true)]  // Leading zeros
    [InlineData("6-9", "06-09", true)]  // Leading zeros other way
    [InlineData("6–9", "6-9", true)]    // En-dash vs hyphen
    [InlineData("Ages 6 to 9", "6-9", true)]  // Different format with same numbers
    [InlineData("(6-9)", "[6-9]", true)]  // Different bracket styles
    public void AgeGroupMatches_WithNumericBoundsFallback_ReturnsTrue(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("6-9", "10-12", false)]
    [InlineData("Ages 3-5", "6-9", false)]
    [InlineData("06-09", "10-12", false)]
    public void AgeGroupMatches_WithDifferentNumericBounds_ReturnsFalse(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Null and Empty Handling Tests

    [Theory]
    [InlineData(null, "6-9", false)]
    [InlineData("6-9", null, false)]
    [InlineData(null, null, false)]
    [InlineData("", "6-9", false)]
    [InlineData("6-9", "", false)]
    [InlineData("", "", false)]
    [InlineData("   ", "6-9", false)]
    [InlineData("6-9", "   ", false)]
    [InlineData("   ", "   ", false)]
    public void AgeGroupMatches_WithNullOrEmptyInputs_ReturnsFalse(string? scenario, string? profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AgeGroupMatches_WithReversedBounds_StillMatches()
    {
        // The helper should normalize reversed bounds (e.g., "9-6" should match "6-9")
        // Act
        var result = AgeGroupHelper.AgeGroupMatches("9-6", "6-9");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AgeGroupMatches_WithSingleNumber_ReturnsFalse()
    {
        // Single number doesn't have two bounds
        // Act
        var result = AgeGroupHelper.AgeGroupMatches("6", "6-9");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("6-9 years", "Ages 6-9", true)]
    [InlineData("Children 10-12", "10-12 age group", true)]
    public void AgeGroupMatches_WithDescriptiveText_MatchesOnNumbers(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("100-200", "100-200", true)]
    [InlineData("0-3", "0-3", true)]
    public void AgeGroupMatches_WithLargeOrZeroNumbers_WorksCorrectly(string scenario, string profile, bool expected)
    {
        // Act
        var result = AgeGroupHelper.AgeGroupMatches(scenario, profile);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
