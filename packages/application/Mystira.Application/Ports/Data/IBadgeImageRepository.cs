using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

/// <summary>
/// Repository for managing BadgeImage entities.
/// </summary>
public interface IBadgeImageRepository : IRepository<BadgeImage>
{
    /// <summary>
    /// Gets a badge image by its image identifier.
    /// </summary>
    /// <param name="imageId">The image identifier.</param>
    /// <returns>The badge image if found; otherwise, null.</returns>
    Task<BadgeImage?> GetByImageIdAsync(string imageId);
}
