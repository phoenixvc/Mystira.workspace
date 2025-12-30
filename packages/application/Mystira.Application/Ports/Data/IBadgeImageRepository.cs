using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

public interface IBadgeImageRepository : IRepository<BadgeImage>
{
    Task<BadgeImage?> GetByImageIdAsync(string imageId);
}
