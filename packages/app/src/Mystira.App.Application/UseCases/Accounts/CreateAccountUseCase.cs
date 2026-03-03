using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.CQRS.Accounts.Commands;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for creating a new account.
/// Called by CreateAccountCommandHandler to perform the core business logic.
/// </summary>
public class CreateAccountUseCase : ICreateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountUseCase> _logger;

    public CreateAccountUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAccountUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UseCaseResult<Account>> ExecuteAsync(CreateAccountCommand command, CancellationToken ct = default)
    {
        Guard.AgainstNull(command, nameof(command));
        Guard.AgainstNullOrEmpty(command.Email, nameof(command.Email));
        Guard.AgainstNullOrEmpty(command.ExternalUserId, nameof(command.ExternalUserId));

        // Check if account with email already exists
        var existingAccount = await _repository.GetByEmailAsync(command.Email, ct);
        if (existingAccount != null)
        {
            _logger.LogWarning("Account already exists for email {Email}", PiiMask.MaskEmail(command.Email));
            return UseCaseResult<Account>.Failure($"Account with this email already exists");
        }

        var account = new Account
        {
            Id = Guid.NewGuid().ToString(),
            ExternalUserId = command.ExternalUserId,
            Email = command.Email,
            DisplayName = command.DisplayName ?? command.Email.Split('@')[0],
            UserProfileIds = command.UserProfileIds ?? new List<string>(),
            CompletedScenarioIds = new List<string>(),
            Subscription = command.Subscription ?? new SubscriptionDetails(),
            Settings = command.Settings ?? new AccountSettings(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        await _repository.AddAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created new account: {AccountId} for {Email}", PiiMask.HashId(account.Id), PiiMask.MaskEmail(account.Email));
        return UseCaseResult<Account>.Success(account);
    }
}
