using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Repositories;

/// <summary>
/// Repository implementation for BadgeImage entity with domain-specific queries.
/// Extends Repository base class and implements IBadgeImageRepository interface.
/// </summary>
public class BadgeImageRepository : Repository<BadgeImage>, IBadgeImageRepository
{
    private readonly MystiraAppDbContext _appContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeImageRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public BadgeImageRepository(MystiraAppDbContext context) : base(context)
    {
        _appContext = context;
    }

    /// <summary>
    /// Retrieves a badge image by its ID.
    /// </summary>
    /// <param name="imageId">The image ID.</param>
    /// <returns>The badge image, or null if not found.</returns>
    public async Task<BadgeImage?> GetByImageIdAsync(string imageId)
    {
        // BadgeImage uses Id as its identifier (there's no separate ImageId property)
        return await _appContext.BadgeImages
            .FirstOrDefaultAsync(x => x.Id == imageId);
    }
}
