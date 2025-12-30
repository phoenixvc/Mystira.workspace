using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a fantasy theme setting for scenarios.
/// Themes define the overall aesthetic and world-building elements.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<FantasyTheme>))]
public sealed class FantasyTheme : StringEnum<FantasyTheme>
{
    private readonly string _displayName;
    private readonly string _description;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    /// <summary>
    /// Gets the description of this theme.
    /// </summary>
    public string Description => _description;

    private FantasyTheme(string value, string displayName, string description) : base(value)
    {
        _displayName = displayName;
        _description = description;
    }

    /// <summary>
    /// Classic high fantasy with magic, mythical creatures, and epic quests.
    /// </summary>
    public static readonly FantasyTheme HighFantasy = new(
        "high_fantasy",
        "High Fantasy",
        "Classic fantasy with magic, mythical creatures, and epic quests");

    /// <summary>
    /// Low fantasy with subtle magic and more grounded storytelling.
    /// </summary>
    public static readonly FantasyTheme LowFantasy = new(
        "low_fantasy",
        "Low Fantasy",
        "Grounded fantasy with subtle magic and realistic elements");

    /// <summary>
    /// Urban fantasy set in modern cities with supernatural elements.
    /// </summary>
    public static readonly FantasyTheme UrbanFantasy = new(
        "urban_fantasy",
        "Urban Fantasy",
        "Modern city settings with hidden supernatural elements");

    /// <summary>
    /// Fairy tale inspired stories with classic folklore elements.
    /// </summary>
    public static readonly FantasyTheme FairyTale = new(
        "fairy_tale",
        "Fairy Tale",
        "Classic fairy tale and folklore inspired stories");

    /// <summary>
    /// Mythological settings based on ancient legends.
    /// </summary>
    public static readonly FantasyTheme Mythology = new(
        "mythology",
        "Mythology",
        "Settings based on ancient myths and legends");

    /// <summary>
    /// Steampunk fantasy with Victorian aesthetics and clockwork technology.
    /// </summary>
    public static readonly FantasyTheme Steampunk = new(
        "steampunk",
        "Steampunk",
        "Victorian-era aesthetics with clockwork and steam technology");

    /// <summary>
    /// Science fantasy blending magic and advanced technology.
    /// </summary>
    public static readonly FantasyTheme ScienceFantasy = new(
        "science_fantasy",
        "Science Fantasy",
        "Blending magical elements with advanced technology");

    /// <summary>
    /// Dark fantasy with horror elements and moral ambiguity.
    /// </summary>
    public static readonly FantasyTheme DarkFantasy = new(
        "dark_fantasy",
        "Dark Fantasy",
        "Darker themes with horror elements and moral complexity");

    /// <summary>
    /// Whimsical fantasy with lighthearted, playful elements.
    /// </summary>
    public static readonly FantasyTheme Whimsical = new(
        "whimsical",
        "Whimsical",
        "Lighthearted and playful with imaginative elements");

    /// <summary>
    /// Historical fantasy set in a specific historical period with magical elements.
    /// </summary>
    public static readonly FantasyTheme Historical = new(
        "historical",
        "Historical Fantasy",
        "Real historical periods with added magical elements");

    /// <summary>
    /// Animal fantasy featuring anthropomorphic or intelligent animal characters.
    /// </summary>
    public static readonly FantasyTheme AnimalFantasy = new(
        "animal_fantasy",
        "Animal Fantasy",
        "Stories featuring anthropomorphic or intelligent animals");

    /// <summary>
    /// Portal fantasy involving travel between worlds.
    /// </summary>
    public static readonly FantasyTheme PortalFantasy = new(
        "portal_fantasy",
        "Portal Fantasy",
        "Stories involving travel between different worlds");
}
