using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

public interface IAxisAchievementRepository : IRepository<AxisAchievement, string>
{
    Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId, CancellationToken ct = default);
    Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId, CancellationToken ct = default);
}
