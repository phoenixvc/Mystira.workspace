using Mystira.Domain.Models;

namespace Mystira.Application.Ports.Data;

public interface IAxisAchievementRepository : IRepository<AxisAchievement>
{
    Task<IEnumerable<AxisAchievement>> GetByAgeGroupAsync(string ageGroupId);
    Task<IEnumerable<AxisAchievement>> GetByCompassAxisAsync(string compassAxisId);
}
