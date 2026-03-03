namespace Mystira.Shared.Utilities;

/// <summary>
/// Generates random fantasy-themed names for characters, places, and other game elements.
/// </summary>
public static class RandomNameGenerator
{
    private static readonly Random _random = new();

    private static readonly string[] FirstNamePrefixes =
    {
        "Ae", "Al", "Ar", "Az", "Be", "Br", "Ca", "Ce", "Da", "De",
        "El", "Em", "Er", "Fa", "Fe", "Ga", "Ge", "Ha", "He", "Il",
        "Is", "Ja", "Ka", "Ke", "La", "Le", "Li", "Lo", "Lu", "Ma",
        "Me", "Mi", "Mo", "Na", "Ne", "No", "Or", "Pa", "Pe", "Ra",
        "Re", "Ri", "Ro", "Sa", "Se", "Si", "So", "Ta", "Te", "Th",
        "Ti", "To", "Tr", "Va", "Ve", "Vi", "Wa", "We", "Za", "Ze"
    };

    private static readonly string[] FirstNameSuffixes =
    {
        "ara", "ari", "ath", "el", "ella", "en", "eon", "era", "ia", "ian",
        "ien", "ina", "ion", "ira", "is", "ius", "la", "len", "lia", "lin",
        "lor", "lora", "mir", "mon", "na", "nia", "nor", "on", "or", "ora",
        "ra", "ren", "ria", "rin", "ron", "sa", "sha", "sia", "son", "ston",
        "ta", "tan", "th", "tha", "tion", "tis", "tor", "us", "ven", "vin"
    };

    private static readonly string[] LastNamePrefixes =
    {
        "Black", "Bright", "Dark", "Dawn", "Dusk", "Ever", "Fair", "Fire",
        "Frost", "Gold", "Gray", "Green", "High", "Iron", "Light", "Moon",
        "Night", "Oak", "Rain", "Red", "Shadow", "Silver", "Snow", "Star",
        "Stone", "Storm", "Sun", "Swift", "Thunder", "White", "Wild", "Wind"
    };

    private static readonly string[] LastNameSuffixes =
    {
        "arrow", "bane", "blade", "born", "brook", "crest", "dale", "fall",
        "fang", "field", "fire", "flame", "forge", "frost", "gale", "guard",
        "hammer", "haven", "heart", "helm", "hollow", "keep", "kin", "leaf",
        "light", "lock", "mane", "mark", "mere", "mist", "moon", "peak",
        "ridge", "river", "shade", "shadow", "shield", "song", "spear", "spring",
        "star", "stone", "storm", "stride", "thorn", "vale", "ward", "weaver",
        "wind", "wing", "wood", "worth"
    };

    private static readonly string[] PlacePrefixes =
    {
        "Amber", "Ancient", "Azure", "Bright", "Crystal", "Dark", "Dawn",
        "Dragon", "Dusk", "Elder", "Emerald", "Eternal", "Ever", "Fallen",
        "Frost", "Golden", "Hidden", "High", "Iron", "Lost", "Misty",
        "Moon", "Mystic", "Night", "Obsidian", "Old", "Radiant", "Ruby",
        "Sacred", "Shadow", "Silent", "Silver", "Storm", "Sun", "Thunder",
        "Twilight", "Verdant", "White", "Wild", "Winter"
    };

    private static readonly string[] PlaceSuffixes =
    {
        "bay", "bridge", "castle", "citadel", "cliff", "cove", "crossing",
        "dale", "dell", "depths", "falls", "ford", "forge", "fortress",
        "gate", "glade", "grove", "hall", "harbor", "haven", "heights",
        "hold", "hollow", "isle", "keep", "lake", "landing", "marsh",
        "meadow", "moor", "pass", "peak", "port", "reach", "ridge",
        "sanctuary", "shore", "spire", "springs", "tower", "vale", "valley",
        "watch", "waters", "wood", "woods"
    };

    /// <summary>
    /// Generates a random first name.
    /// </summary>
    /// <returns>A randomly generated first name.</returns>
    public static string GenerateFirstName()
    {
        var prefix = FirstNamePrefixes[_random.Next(FirstNamePrefixes.Length)];
        var suffix = FirstNameSuffixes[_random.Next(FirstNameSuffixes.Length)];
        return prefix + suffix;
    }

    /// <summary>
    /// Generates a random last name.
    /// </summary>
    /// <returns>A randomly generated last name.</returns>
    public static string GenerateLastName()
    {
        var prefix = LastNamePrefixes[_random.Next(LastNamePrefixes.Length)];
        var suffix = LastNameSuffixes[_random.Next(LastNameSuffixes.Length)];
        return prefix + suffix;
    }

    /// <summary>
    /// Generates a random full name (first and last).
    /// </summary>
    /// <returns>A randomly generated full name.</returns>
    public static string GenerateFullName()
    {
        return $"{GenerateFirstName()} {GenerateLastName()}";
    }

    /// <summary>
    /// Generates a random place name.
    /// </summary>
    /// <returns>A randomly generated place name.</returns>
    public static string GeneratePlaceName()
    {
        var prefix = PlacePrefixes[_random.Next(PlacePrefixes.Length)];
        var suffix = PlaceSuffixes[_random.Next(PlaceSuffixes.Length)];
        return $"{prefix} {char.ToUpper(suffix[0])}{suffix[1..]}";
    }

    /// <summary>
    /// Generates a random character name appropriate for a given age group.
    /// </summary>
    /// <param name="ageGroupId">The age group identifier.</param>
    /// <returns>A randomly generated name appropriate for the age group.</returns>
    public static string GenerateCharacterName(string? ageGroupId = null)
    {
        return ageGroupId switch
        {
            "early_childhood" or "middle_childhood" => GenerateFirstName(),
            _ => GenerateFullName()
        };
    }

    /// <summary>
    /// Generates multiple unique first names.
    /// </summary>
    /// <param name="count">The number of names to generate.</param>
    /// <returns>A list of unique first names.</returns>
    public static List<string> GenerateFirstNames(int count)
    {
        var names = new HashSet<string>();
        var maxAttempts = count * 10;
        var attempts = 0;

        while (names.Count < count && attempts < maxAttempts)
        {
            names.Add(GenerateFirstName());
            attempts++;
        }

        return names.ToList();
    }

    /// <summary>
    /// Generates multiple unique full names.
    /// </summary>
    /// <param name="count">The number of names to generate.</param>
    /// <returns>A list of unique full names.</returns>
    public static List<string> GenerateFullNames(int count)
    {
        var names = new HashSet<string>();
        var maxAttempts = count * 10;
        var attempts = 0;

        while (names.Count < count && attempts < maxAttempts)
        {
            names.Add(GenerateFullName());
            attempts++;
        }

        return names.ToList();
    }

    /// <summary>
    /// Generates a random username-style name.
    /// </summary>
    /// <returns>A randomly generated username.</returns>
    public static string GenerateUsername()
    {
        var firstName = GenerateFirstName();
        var number = _random.Next(100, 9999);
        return $"{firstName}{number}";
    }

    /// <summary>
    /// Generates a random guest name with a number suffix.
    /// </summary>
    /// <returns>A randomly generated guest name.</returns>
    public static string GenerateGuestName()
    {
        var prefix = FirstNamePrefixes[_random.Next(FirstNamePrefixes.Length)];
        var suffix = FirstNameSuffixes[_random.Next(Math.Min(10, FirstNameSuffixes.Length))];
        var number = _random.Next(1, 999);
        return $"Guest_{prefix}{suffix}_{number}";
    }
}
