using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find a badge image by ID.</summary>
public sealed class BadgeImageByIdSpec : SingleResultSpecification<BadgeImage>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgeImageByIdSpec(string id)
    {
        Query.Where(b => b.Id == id);
    }
}

/// <summary>Find a badge image by image ID.</summary>
public sealed class BadgeImageByImageIdSpec : SingleResultSpecification<BadgeImage>
{
    /// <summary>Initializes a new instance.</summary>
    public BadgeImageByImageIdSpec(string imageId)
    {
        Query.Where(b => b.ImageId == imageId);
    }
}
