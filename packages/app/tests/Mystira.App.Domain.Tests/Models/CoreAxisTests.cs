using Mystira.App.Domain.Models;
using FluentAssertions;

namespace Mystira.App.Domain.Tests.Models;

public class CoreAxisTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("Courage")]
    [InlineData("Compassion")]
    [InlineData("Wisdom")]
    public void Constructor_WithValue_SetsValueProperty(string value)
    {
        // Act
        var coreAxis = new CoreAxis(value);

        // Assert
        coreAxis.Value.Should().Be(value);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var coreAxis = new CoreAxis("Resilience");

        // Act
        var result = coreAxis.ToString();

        // Assert
        result.Should().Be("Resilience");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var axis1 = new CoreAxis("Empathy");
        var axis2 = new CoreAxis("Empathy");

        // Assert
        axis1.Equals(axis2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCase_ReturnsTrue()
    {
        // Arrange
        var axis1 = new CoreAxis("COURAGE");
        var axis2 = new CoreAxis("courage");

        // Assert
        axis1.Equals(axis2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var axis1 = new CoreAxis("Courage");
        var axis2 = new CoreAxis("Compassion");

        // Assert
        axis1.Equals(axis2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var axis = new CoreAxis("Wisdom");
        CoreAxis? nullAxis = null;

        // Assert
        axis.Equals(nullAxis).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var axis = new CoreAxis("Integrity");
        object differentType = "Integrity";

        // Assert
        axis.Equals(differentType).Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var axis1 = new CoreAxis("Perseverance");
        var axis2 = new CoreAxis("Perseverance");

        // Assert
        axis1.GetHashCode().Should().Be(axis2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentCase_ReturnsSameHash()
    {
        // Arrange
        var axis1 = new CoreAxis("KINDNESS");
        var axis2 = new CoreAxis("kindness");

        // Assert
        axis1.GetHashCode().Should().Be(axis2.GetHashCode());
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_WithEqualValues_ReturnsTrue()
    {
        // Arrange
        var axis1 = new CoreAxis("Honesty");
        var axis2 = new CoreAxis("Honesty");

        // Assert
        (axis1 == axis2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        CoreAxis? axis1 = null;
        CoreAxis? axis2 = null;

        // Assert
        (axis1 == axis2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithOneNull_ReturnsFalse()
    {
        // Arrange
        var axis1 = new CoreAxis("Loyalty");
        CoreAxis? axis2 = null;

        // Assert
        (axis1 == axis2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ReturnsTrue()
    {
        // Arrange
        var axis1 = new CoreAxis("Patience");
        var axis2 = new CoreAxis("Creativity");

        // Assert
        (axis1 != axis2).Should().BeTrue();
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("Courage")]
    [InlineData("Compassion")]
    public void Parse_WithValidValue_ReturnsCoreAxis(string value)
    {
        // Act
        var result = CoreAxis.Parse(value);

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_WithInvalidValue_ReturnsNull(string? value)
    {
        // Act
        var result = CoreAxis.Parse(value);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
