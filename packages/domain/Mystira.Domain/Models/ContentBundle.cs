using Mystira.Domain.Entities;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a bundle of content (scenarios) that can be purchased or subscribed to.
/// </summary>
public class ContentBundle : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets the bundle title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle slug.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the bundle description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short summary.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets the publisher/author ID.
    /// </summary>
    public string PublisherId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cover image URL.
    /// </summary>
    public string? CoverImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the target age group ID.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the fantasy theme ID.
    /// </summary>
    public string? ThemeId { get; set; }

    /// <summary>
    /// Gets or sets the publication status.
    /// </summary>
    public PublicationStatus Status { get; set; } = PublicationStatus.Draft;

    /// <summary>
    /// Gets or sets whether this is a premium bundle.
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Gets or sets the price in cents (0 for free).
    /// </summary>
    public int PriceCents { get; set; }

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Gets or sets whether the bundle is featured.
    /// </summary>
    public bool IsFeatured { get; set; }

    /// <summary>
    /// Gets or sets when the bundle was published.
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the total play count across all scenarios.
    /// </summary>
    public int TotalPlayCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating.
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the number of ratings.
    /// </summary>
    public int RatingCount { get; set; }

    /// <summary>
    /// Gets or sets tags as JSON.
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Gets or sets the scenarios in this bundle.
    /// </summary>
    public virtual ICollection<Scenario> Scenarios { get; set; } = new List<Scenario>();

    /// <summary>
    /// Gets the target age group.
    /// </summary>
    public AgeGroup? AgeGroup => AgeGroup.FromId(AgeGroupId);

    /// <summary>
    /// Gets the fantasy theme.
    /// </summary>
    public FantasyTheme? Theme => FantasyTheme.FromValue(ThemeId);

    /// <summary>
    /// Gets the price in dollars.
    /// </summary>
    public decimal PriceDollars => PriceCents / 100m;

    /// <summary>
    /// Gets whether the bundle is free.
    /// </summary>
    public bool IsFree => PriceCents == 0;

    /// <summary>
    /// Publishes the bundle.
    /// </summary>
    public void Publish()
    {
        if (Status == PublicationStatus.Draft)
        {
            Status = PublicationStatus.Published;
            PublishedAt = DateTime.UtcNow;
            Version++;
        }
    }
}
