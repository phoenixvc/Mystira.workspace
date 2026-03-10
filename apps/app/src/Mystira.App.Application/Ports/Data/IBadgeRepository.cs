using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

public interface IBadgeRepository : IRepository<Badge, string>
{
    Task<IEnumerable<Badge>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default);
    Task<IEnumerable<Badge>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default);
    Task<Badge?> GetByAgeGroupAxisAndTierAsync(string ageGroupId, string compassAxisId, int tierOrder, CancellationToken ct = default);
}
