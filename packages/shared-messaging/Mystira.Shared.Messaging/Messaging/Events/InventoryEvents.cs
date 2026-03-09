namespace Mystira.Shared.Messaging.Events;

/// <summary>
/// Published when an item is acquired.
/// </summary>
public sealed record ItemAcquired : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Item type (cosmetic, consumable, collectible, currency).
    /// </summary>
    public required string ItemType { get; init; }

    /// <summary>
    /// Item name.
    /// </summary>
    public required string ItemName { get; init; }

    /// <summary>
    /// Quantity acquired.
    /// </summary>
    public required int Quantity { get; init; }

    /// <summary>
    /// How it was acquired (purchase, reward, achievement, drop, trade).
    /// </summary>
    public required string AcquisitionMethod { get; init; }

    /// <summary>
    /// Related source (achievement ID, scenario ID, payment ID).
    /// </summary>
    public string? SourceId { get; init; }
}

/// <summary>
/// Published when an item is used/consumed.
/// </summary>
public sealed record ItemUsed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Quantity used.
    /// </summary>
    public required int Quantity { get; init; }

    /// <summary>
    /// Context where it was used (session ID, scenario ID).
    /// </summary>
    public string? ContextId { get; init; }

    /// <summary>
    /// Remaining quantity.
    /// </summary>
    public required int RemainingQuantity { get; init; }
}

/// <summary>
/// Published when a cosmetic item is equipped.
/// </summary>
public sealed record ItemEquipped : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Slot where equipped (avatar, frame, badge, theme).
    /// </summary>
    public required string Slot { get; init; }

    /// <summary>
    /// Previous item in slot if any.
    /// </summary>
    public string? PreviousItemId { get; init; }
}

/// <summary>
/// Published when an item is unequipped.
/// </summary>
public sealed record ItemUnequipped : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// The item ID.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Slot it was removed from.
    /// </summary>
    public required string Slot { get; init; }
}

/// <summary>
/// Published when virtual currency is earned.
/// </summary>
public sealed record CurrencyEarned : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Currency type (coins, gems, tokens).
    /// </summary>
    public required string CurrencyType { get; init; }

    /// <summary>
    /// Amount earned.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// Source (daily_reward, achievement, purchase, gameplay).
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// New balance.
    /// </summary>
    public required long NewBalance { get; init; }
}

/// <summary>
/// Published when virtual currency is spent.
/// </summary>
public sealed record CurrencySpent : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Currency type.
    /// </summary>
    public required string CurrencyType { get; init; }

    /// <summary>
    /// Amount spent.
    /// </summary>
    public required int Amount { get; init; }

    /// <summary>
    /// What it was spent on (item, unlock, boost).
    /// </summary>
    public required string SpentOn { get; init; }

    /// <summary>
    /// Item ID if applicable.
    /// </summary>
    public string? ItemId { get; init; }

    /// <summary>
    /// New balance.
    /// </summary>
    public required long NewBalance { get; init; }
}

/// <summary>
/// Published when items are traded between users.
/// </summary>
public sealed record ItemTraded : IntegrationEventBase
{
    /// <summary>
    /// Trade ID.
    /// </summary>
    public required string TradeId { get; init; }

    /// <summary>
    /// First user's account ID.
    /// </summary>
    public required string User1AccountId { get; init; }

    /// <summary>
    /// Second user's account ID.
    /// </summary>
    public required string User2AccountId { get; init; }

    /// <summary>
    /// Items given by user 1.
    /// </summary>
    public required string[] User1ItemIds { get; init; }

    /// <summary>
    /// Items given by user 2.
    /// </summary>
    public required string[] User2ItemIds { get; init; }
}

/// <summary>
/// Published when daily/login reward is claimed.
/// </summary>
public sealed record DailyRewardClaimed : IntegrationEventBase
{
    /// <summary>
    /// The user's account ID.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Current streak day.
    /// </summary>
    public required int StreakDay { get; init; }

    /// <summary>
    /// Reward type (currency, item, xp).
    /// </summary>
    public required string RewardType { get; init; }

    /// <summary>
    /// Reward amount or item ID.
    /// </summary>
    public required string RewardValue { get; init; }

    /// <summary>
    /// Whether streak bonus was applied.
    /// </summary>
    public bool StreakBonusApplied { get; init; }
}
