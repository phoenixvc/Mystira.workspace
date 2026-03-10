using Ardalis.Specification;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Specifications;

public sealed class BadgeImageByIdSpec : SingleResultSpecification<BadgeImage>
{
    public BadgeImageByIdSpec(string id)
    {
        Query.Where(b => b.Id == id);
    }
}

public sealed class BadgeImageByImageIdSpec : SingleResultSpecification<BadgeImage>
{
    public BadgeImageByImageIdSpec(string imageId)
    {
        Query.Where(b => b.ImageId == imageId);
    }
}
