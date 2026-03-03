using Ardalis.Specification;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Specifications;

public sealed class CharacterMapByIdSpec : SingleResultSpecification<CharacterMap>
{
    public CharacterMapByIdSpec(string id)
    {
        Query.Where(c => c.Id == id);
    }
}

public sealed class CharacterMapByNameSpec : SingleResultSpecification<CharacterMap>
{
    public CharacterMapByNameSpec(string name)
    {
        Query.Where(c => c.Name == name);
    }
}

public sealed class CharacterMapExistsByNameSpec : SingleResultSpecification<CharacterMap>
{
    public CharacterMapExistsByNameSpec(string name)
    {
        Query.Where(c => c.Name == name);
    }
}
