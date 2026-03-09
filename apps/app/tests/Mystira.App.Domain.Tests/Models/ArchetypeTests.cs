using Mystira.App.Domain.Models;

namespace Mystira.App.Domain.Tests.Models;

public class ArchetypeTests
{
    #region Constructor Tests

    [Theory]
    [InlineData("Warrior")]
    [InlineData("Healer")]
    [InlineData("Explorer")]
    public void Constructor_WithValue_SetsValueProperty(string value)
    {
        // Act
        var archetype = new Archetype(value);

        // Assert
        archetype.Value.Should().Be(value);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var archetype = new Archetype("Protector");

        // Act
        var result = archetype.ToString();

        // Assert
        result.Should().Be("Protector");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        // Arrange
        var archetype1 = new Archetype("Guardian");
        var archetype2 = new Archetype("Guardian");

        // Assert
        archetype1.Equals(archetype2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentCase_ReturnsTrue()
    {
        // Arrange
        var archetype1 = new Archetype("WARRIOR");
        var archetype2 = new Archetype("warrior");

        // Assert
        archetype1.Equals(archetype2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        // Arrange
        var archetype1 = new Archetype("Warrior");
        var archetype2 = new Archetype("Healer");

        // Assert
        archetype1.Equals(archetype2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var archetype = new Archetype("Warrior");
        Archetype? nullArchetype = null;

        // Assert
        archetype.Equals(nullArchetype).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ReturnsFalse()
    {
        // Arrange
        var archetype = new Archetype("Warrior");
        object differentType = "Warrior";

        // Assert
        archetype.Equals(differentType).Should().BeFalse();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValue_ReturnsSameHash()
    {
        // Arrange
        var archetype1 = new Archetype("Explorer");
        var archetype2 = new Archetype("Explorer");

        // Assert
        archetype1.GetHashCode().Should().Be(archetype2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentCase_ReturnsSameHash()
    {
        // Arrange
        var archetype1 = new Archetype("EXPLORER");
        var archetype2 = new Archetype("explorer");

        // Assert
        archetype1.GetHashCode().Should().Be(archetype2.GetHashCode());
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void EqualityOperator_WithEqualValues_ReturnsTrue()
    {
        // Arrange
        var archetype1 = new Archetype("Thinker");
        var archetype2 = new Archetype("Thinker");

        // Assert
        (archetype1 == archetype2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithBothNull_ReturnsTrue()
    {
        // Arrange
        Archetype? archetype1 = null;
        Archetype? archetype2 = null;

        // Assert
        (archetype1 == archetype2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithOneNull_ReturnsFalse()
    {
        // Arrange
        var archetype1 = new Archetype("Leader");
        Archetype? archetype2 = null;

        // Assert
        (archetype1 == archetype2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentValues_ReturnsTrue()
    {
        // Arrange
        var archetype1 = new Archetype("Leader");
        var archetype2 = new Archetype("Follower");

        // Assert
        (archetype1 != archetype2).Should().BeTrue();
    }

    #endregion

    #region Parse Tests

    [Theory]
    [InlineData("Warrior")]
    [InlineData("Healer")]
    public void Parse_WithValidValue_ReturnsArchetype(string value)
    {
        // Act
        var result = Archetype.Parse(value);

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
        var result = Archetype.Parse(value);

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
