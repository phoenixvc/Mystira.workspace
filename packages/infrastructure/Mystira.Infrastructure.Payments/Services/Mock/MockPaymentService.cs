using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Payments;
using Mystira.Contracts.App.Enums;
using Mystira.Contracts.App.Requests.Payments;
using Mystira.Contracts.App.Responses.Payments;

namespace Mystira.Infrastructure.Payments.Services.Mock;

/// <summary>
/// Mock payment service for development and testing.
/// Simulates payment operations without calling external APIs.
/// </summary>
public class MockPaymentService : IPaymentService
{
    private readonly ILogger<MockPaymentService> _logger;
    private readonly Dictionary<string, PaymentStatus> _transactions = new();
    private readonly Dictionary<string, SubscriptionStatus> _subscriptions = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MockPaymentService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MockPaymentService(ILogger<MockPaymentService> logger)
    {
        _logger = logger;
        _logger.LogInformation("MockPaymentService initialized - payments will be simulated");
    }

    /// <summary>
    /// Creates a mock checkout session for testing purposes.
    /// </summary>
    /// <param name="request">The checkout request details.</param>
    /// <returns>A mock checkout result with simulated redirect URL.</returns>
    public Task<CheckoutResult> CreateCheckoutAsync(CheckoutRequest request)
    {
        var checkoutId = $"mock_checkout_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Mock checkout created: {CheckoutId} for account {AccountId}, amount {Amount} {Currency}",
            checkoutId, request.AccountId, request.Amount, request.Currency);

        var result = new CheckoutResult
        {
            CheckoutId = checkoutId,
            Status = CheckoutResultStatus.Created,
            RedirectUrl = $"https://mock-payment.test/checkout/{checkoutId}",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Simulate a pending transaction
        _transactions[checkoutId] = new PaymentStatus
        {
            TransactionId = checkoutId,
            Status = PaymentResultStatus.Pending,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// Processes a mock payment with simulated success/failure (90% success rate).
    /// </summary>
    /// <param name="request">The payment request details.</param>
    /// <returns>A mock payment result.</returns>
    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        var transactionId = $"mock_txn_{Guid.NewGuid():N}";

        // Simulate payment success (90% success rate in mock)
        var success = Random.Shared.NextDouble() < 0.9;

        _logger.LogInformation(
            "Mock payment processed: {TransactionId}, success: {Success}, amount: {Amount} {Currency}",
            transactionId, success, request.Amount, request.Currency);

        var result = new PaymentResult
        {
            TransactionId = transactionId,
            Status = success ? PaymentResultStatus.Succeeded : PaymentResultStatus.Failed,
            Amount = request.Amount,
            Currency = request.Currency,
            ProcessedAt = DateTime.UtcNow,
            ErrorMessage = success ? null : "Mock payment declined",
            ErrorCode = success ? null : "MOCK_DECLINE"
        };

        _transactions[transactionId] = new PaymentStatus
        {
            TransactionId = transactionId,
            Status = result.Status,
            Amount = request.Amount,
            Currency = request.Currency,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// Verifies a mock webhook signature (always succeeds in mock mode).
    /// </summary>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">The webhook signature.</param>
    /// <returns>A simulated successful payment webhook event.</returns>
    public Task<WebhookEvent> VerifyWebhookAsync(string payload, string signature)
    {
        _logger.LogInformation("Mock webhook verification - always returns success");

        // In mock mode, we just return a simulated successful payment webhook
        var webhookEvent = new WebhookEvent
        {
            EventId = $"mock_evt_{Guid.NewGuid():N}",
            Type = WebhookEventType.PaymentSucceeded,
            TransactionId = $"mock_txn_{Guid.NewGuid():N}",
            Amount = 100m,
            Currency = "ZAR",
            PaymentStatus = PaymentResultStatus.Succeeded,
            Timestamp = DateTime.UtcNow
        };

        return Task.FromResult(webhookEvent);
    }

    /// <summary>
    /// Gets the payment status for a mock transaction.
    /// </summary>
    /// <param name="transactionId">The transaction identifier.</param>
    /// <returns>The mock payment status.</returns>
    public Task<PaymentStatus> GetPaymentStatusAsync(string transactionId)
    {
        if (_transactions.TryGetValue(transactionId, out var status))
        {
            return Task.FromResult(status);
        }

        // Return a mock status for unknown transactions
        return Task.FromResult(new PaymentStatus
        {
            TransactionId = transactionId,
            Status = PaymentResultStatus.Succeeded,
            Amount = 0,
            Currency = "ZAR",
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Processes a mock refund (always succeeds in mock mode).
    /// </summary>
    /// <param name="request">The refund request details.</param>
    /// <returns>A mock refund result.</returns>
    public Task<RefundResult> RefundPaymentAsync(RefundRequest request)
    {
        var refundId = $"mock_refund_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Mock refund processed: {RefundId} for transaction {TransactionId}",
            refundId, request.TransactionId);

        var result = new RefundResult
        {
            RefundId = refundId,
            TransactionId = request.TransactionId,
            Status = RefundStatus.Succeeded,
            Amount = request.Amount ?? 100m,
            ProcessedAt = DateTime.UtcNow
        };

        // Update transaction status
        if (_transactions.TryGetValue(request.TransactionId, out var txn))
        {
            _transactions[request.TransactionId] = txn with
            {
                Status = request.Amount.HasValue ? PaymentResultStatus.PartiallyRefunded : PaymentResultStatus.Refunded
            };
        }

        return Task.FromResult(result);
    }

    /// <summary>
    /// Creates a mock subscription for testing purposes.
    /// </summary>
    /// <param name="request">The subscription request details.</param>
    /// <returns>A mock subscription result.</returns>
    public Task<SubscriptionResult> CreateSubscriptionAsync(SubscriptionRequest request)
    {
        var subscriptionId = $"mock_sub_{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Mock subscription created: {SubscriptionId} for account {AccountId}, plan {PlanId}",
            subscriptionId, request.AccountId, request.PlanId);

        var now = DateTime.UtcNow;
        var result = new SubscriptionResult
        {
            SubscriptionId = subscriptionId,
            Status = request.TrialEndDate.HasValue ? SubscriptionResultStatus.Trialing : SubscriptionResultStatus.Active,
            PlanId = request.PlanId,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddMonths(1),
            TrialEnd = request.TrialEndDate
        };

        _subscriptions[subscriptionId] = new SubscriptionStatus
        {
            SubscriptionId = subscriptionId,
            Status = result.Status,
            PlanId = request.PlanId,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddMonths(1)
        };

        return Task.FromResult(result);
    }

    /// <summary>
    /// Cancels a mock subscription (always succeeds in mock mode).
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier to cancel.</param>
    /// <param name="cancelImmediately">If true, cancels immediately; otherwise cancels at period end.</param>
    /// <returns>A mock subscription cancellation result.</returns>
    public Task<SubscriptionCancellationResult> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false)
    {
        _logger.LogInformation(
            "Mock subscription cancelled: {SubscriptionId}, immediate: {Immediate}",
            subscriptionId, cancelImmediately);

        var effectiveDate = cancelImmediately ? DateTime.UtcNow : DateTime.UtcNow.AddMonths(1);

        if (_subscriptions.TryGetValue(subscriptionId, out var sub))
        {
            _subscriptions[subscriptionId] = sub with
            {
                Status = SubscriptionResultStatus.Cancelled,
                CancelledAt = DateTime.UtcNow,
                CancelAt = effectiveDate
            };
        }

        return Task.FromResult(new SubscriptionCancellationResult
        {
            SubscriptionId = subscriptionId,
            Success = true,
            EffectiveDate = effectiveDate
        });
    }

    /// <summary>
    /// Gets the status of a mock subscription.
    /// </summary>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <returns>The mock subscription status.</returns>
    public Task<SubscriptionStatus> GetSubscriptionStatusAsync(string subscriptionId)
    {
        if (_subscriptions.TryGetValue(subscriptionId, out var status))
        {
            return Task.FromResult(status);
        }

        // Return a mock active subscription for unknown IDs
        return Task.FromResult(new SubscriptionStatus
        {
            SubscriptionId = subscriptionId,
            Status = SubscriptionResultStatus.Active,
            PlanId = "mock_plan",
            CurrentPeriodStart = DateTime.UtcNow.AddMonths(-1),
            CurrentPeriodEnd = DateTime.UtcNow.AddMonths(1)
        });
    }

    /// <summary>
    /// Performs a health check on the mock payment service (always healthy).
    /// </summary>
    /// <returns>Always returns true for mock implementation.</returns>
    public Task<bool> IsHealthyAsync()
    {
        _logger.LogDebug("Mock payment service health check - always healthy");
        return Task.FromResult(true);
    }
}
