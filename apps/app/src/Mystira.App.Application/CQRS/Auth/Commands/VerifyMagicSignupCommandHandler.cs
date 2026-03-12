using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public static class VerifyMagicSignupCommandHandler
{
    private static readonly TimeSpan VerifiedWindow = TimeSpan.FromDays(7);

    public static async Task<VerifyMagicSignupResult> Handle(
        VerifyMagicSignupCommand command,
        IPendingSignupRepository pendingSignupRepository,
        IUnitOfWork unitOfWork,
        ILogger<VerifyMagicSignupCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new ValidationException("token", "Token is required");
        }

        var tokenHash = MagicLinkTokenService.HashToken(command.Token);
        var pendingSignup = await pendingSignupRepository.GetByVerificationTokenHashAsync(tokenHash, ct);

        if (pendingSignup == null)
        {
            return new VerifyMagicSignupResult("Invalid", "Invalid verification token", false, true);
        }

        if (pendingSignup.IsVerificationTokenExpired)
        {
            pendingSignup.MarkExpired();
            await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return new VerifyMagicSignupResult("Expired", "Verification token has expired", false, true);
        }

        if (pendingSignup.Status == PendingSignupStatus.Consumed)
        {
            return new VerifyMagicSignupResult("Consumed", "This magic link has already been used", false, true);
        }

        if (pendingSignup.Status != PendingSignupStatus.Verified)
        {
            pendingSignup.MarkVerified(DateTime.UtcNow.Add(VerifiedWindow));
            await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }

        logger.LogInformation("Magic signup token verified for {EmailHash}", EmailHasher.Hash(pendingSignup.Email));

        return new VerifyMagicSignupResult("Verified", "Email verified", true, true);
    }
}
