using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for Account entity with domain-specific queries
/// </summary>
public interface IAccountRepository : IRepository<Account, string>
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Account?> GetByExternalUserIdAsync(string externalUserId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}
