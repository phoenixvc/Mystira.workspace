using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find a character map by ID.</summary>
public sealed class CharacterMapByIdSpec : SingleResultSpecification<CharacterMap>
{
    /// <summary>Initializes a new instance.</summary>
    public CharacterMapByIdSpec(string id)
    {
        Query.Where(c => c.Id == id);
    }
}

/// <summary>Find a character map by name.</summary>
public sealed class CharacterMapByNameSpec : SingleResultSpecification<CharacterMap>
{
    /// <summary>Initializes a new instance.</summary>
    public CharacterMapByNameSpec(string name)
    {
        Query.Where(c => c.Name == name);
    }
}

/// <summary>Check if a character map exists by name.</summary>
public sealed class CharacterMapExistsByNameSpec : SingleResultSpecification<CharacterMap>
{
    /// <summary>Initializes a new instance.</summary>
    public CharacterMapExistsByNameSpec(string name)
    {
        Query.Where(c => c.Name == name);
    }
}
