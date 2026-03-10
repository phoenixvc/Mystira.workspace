using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

public interface IBadgeImageRepository : IRepository<BadgeImage, string>
{
    Task<BadgeImage?> GetByImageIdAsync(string imageId, CancellationToken ct = default);
}
