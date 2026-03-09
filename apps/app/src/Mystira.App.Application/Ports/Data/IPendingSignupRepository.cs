using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Ports.Data;

/// <summary>
/// Repository interface for pending email signup records.
/// </summary>
public interface IPendingSignupRepository
{
    Task<PendingSignup?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PendingSignup?> GetByVerificationTokenHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(PendingSignup pendingSignup, CancellationToken ct = default);
    Task UpdateAsync(PendingSignup pendingSignup, CancellationToken ct = default);
}
