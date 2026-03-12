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
    Task<BadgeImage?> GetByImageIdAsync(string imageId, CancellationToken ct = default);
}
