using Ardalis.Specification;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Specifications;

public sealed class MediaAssetByIdSpec : SingleResultSpecification<MediaAsset>
{
    public MediaAssetByIdSpec(string id)
    {
        Query.Where(m => m.Id == id);
    }
}

public sealed class MediaAssetByMediaIdSpec : SingleResultSpecification<MediaAsset>
{
    public MediaAssetByMediaIdSpec(string mediaId)
    {
        Query.Where(m => m.MediaId == mediaId);
    }
}

public sealed class MediaAssetsByMediaIdsSpec : Specification<MediaAsset>
{
    public MediaAssetsByMediaIdsSpec(IEnumerable<string> mediaIds)
    {
        var ids = mediaIds.ToList();
        Query.Where(m => ids.Contains(m.MediaId));
    }
}

public sealed class MediaAssetExistsByMediaIdSpec : SingleResultSpecification<MediaAsset>
{
    public MediaAssetExistsByMediaIdSpec(string mediaId)
    {
        Query.Where(m => m.MediaId == mediaId);
    }
}
