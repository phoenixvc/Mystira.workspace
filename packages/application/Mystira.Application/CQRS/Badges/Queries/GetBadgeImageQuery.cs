namespace Mystira.Application.CQRS.Badges.Queries;

/// <summary>
/// Query to retrieve a badge image by its identifier.
/// </summary>
/// <param name="ImageId">The unique identifier of the badge image.</param>
public sealed record GetBadgeImageQuery(string ImageId) : IQuery<BadgeImageResult?>;

/// <summary>
/// Represents the result of a badge image query containing the image data and content type.
/// </summary>
/// <param name="ImageData">The binary image data.</param>
/// <param name="ContentType">The MIME content type of the image.</param>
public sealed record BadgeImageResult(byte[] ImageData, string ContentType);
