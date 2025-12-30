namespace Mystira.Application.CQRS.Badges.Queries;

public sealed record GetBadgeImageQuery(string ImageId) : IQuery<BadgeImageResult?>;

public sealed record BadgeImageResult(byte[] ImageData, string ContentType);
