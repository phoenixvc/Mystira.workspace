using System.Text.Json.Serialization;
using Mystira.Domain.Primitives;
using Mystira.Domain.Serialization;

namespace Mystira.Domain.ValueObjects;

/// <summary>
/// Represents a character personality trait in the Mystira universe.
/// </summary>
[JsonConverter(typeof(StringEnumJsonConverter<CharacterTrait>))]
public sealed class CharacterTrait : StringEnum<CharacterTrait>
{
    private readonly string _displayName;

    /// <inheritdoc />
    public override string DisplayName => _displayName;

    private CharacterTrait(string value, string displayName) : base(value)
    {
        _displayName = displayName;
    }

    // Positive traits
    /// <summary>Wise - possessing great knowledge and judgment.</summary>
    public static readonly CharacterTrait Wise = new("wise", "Wise");

    /// <summary>Brave - courageous in the face of danger.</summary>
    public static readonly CharacterTrait Brave = new("brave", "Brave");

    /// <summary>Kind - showing compassion and care for others.</summary>
    public static readonly CharacterTrait Kind = new("kind", "Kind");

    /// <summary>Calm - peaceful and composed under pressure.</summary>
    public static readonly CharacterTrait Calm = new("calm", "Calm");

    /// <summary>Curious - eager to learn and explore.</summary>
    public static readonly CharacterTrait Curious = new("curious", "Curious");

    /// <summary>Loyal - devoted and faithful.</summary>
    public static readonly CharacterTrait Loyal = new("loyal", "Loyal");

    /// <summary>Honest - truthful and sincere.</summary>
    public static readonly CharacterTrait Honest = new("honest", "Honest");

    /// <summary>Creative - imaginative and inventive.</summary>
    public static readonly CharacterTrait Creative = new("creative", "Creative");

    /// <summary>Patient - able to wait calmly.</summary>
    public static readonly CharacterTrait Patient = new("patient", "Patient");

    /// <summary>Mysterious - enigmatic and intriguing.</summary>
    public static readonly CharacterTrait Mysterious = new("mysterious", "Mysterious");

    /// <summary>Playful - fun-loving and lighthearted.</summary>
    public static readonly CharacterTrait Playful = new("playful", "Playful");

    /// <summary>Funny - humorous and entertaining.</summary>
    public static readonly CharacterTrait Funny = new("funny", "Funny");

    /// <summary>Protective - guarding and defending others.</summary>
    public static readonly CharacterTrait Protective = new("protective", "Protective");

    /// <summary>Adventurous - seeking new experiences.</summary>
    public static readonly CharacterTrait Adventurous = new("adventurous", "Adventurous");

    /// <summary>Gentle - soft and mild in manner.</summary>
    public static readonly CharacterTrait Gentle = new("gentle", "Gentle");

    // Neutral/Complex traits
    /// <summary>Sneaky - stealthy and cunning.</summary>
    public static readonly CharacterTrait Sneaky = new("sneaky", "Sneaky");

    /// <summary>Chaotic - unpredictable and wild.</summary>
    public static readonly CharacterTrait Chaotic = new("chaotic", "Chaotic");

    /// <summary>Stubborn - determined but inflexible.</summary>
    public static readonly CharacterTrait Stubborn = new("stubborn", "Stubborn");

    /// <summary>Proud - self-respecting but sometimes arrogant.</summary>
    public static readonly CharacterTrait Proud = new("proud", "Proud");

    /// <summary>Shy - reserved and quiet.</summary>
    public static readonly CharacterTrait Shy = new("shy", "Shy");

    /// <summary>Fierce - intense and passionate.</summary>
    public static readonly CharacterTrait Fierce = new("fierce", "Fierce");

    /// <summary>Cunning - clever and crafty.</summary>
    public static readonly CharacterTrait Cunning = new("cunning", "Cunning");

    /// <summary>Ambitious - driven to achieve.</summary>
    public static readonly CharacterTrait Ambitious = new("ambitious", "Ambitious");

    /// <summary>Independent - self-reliant.</summary>
    public static readonly CharacterTrait Independent = new("independent", "Independent");

    /// <summary>Mischievous - playfully troublesome.</summary>
    public static readonly CharacterTrait Mischievous = new("mischievous", "Mischievous");
}
