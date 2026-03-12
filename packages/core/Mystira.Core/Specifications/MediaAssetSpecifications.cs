using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Core.Specifications;

/// <summary>Find a media asset by ID.</summary>
public sealed class MediaAssetByIdSpec : SingleResultSpecification<MediaAsset>
{
    /// <summary>Initializes a new instance.</summary>
    public MediaAssetByIdSpec(string id)
    {
        Query.Where(m => m.Id == id);
    }
}

/// <summary>Find a media asset by media ID.</summary>
public sealed class MediaAssetByMediaIdSpec : SingleResultSpecification<MediaAsset>
{
    /// <summary>Initializes a new instance.</summary>
    public MediaAssetByMediaIdSpec(string mediaId)
    {
        Query.Where(m => m.MediaId == mediaId);
    }
}

/// <summary>Find media assets by a collection of media IDs.</summary>
public sealed class MediaAssetsByMediaIdsSpec : Specification<MediaAsset>
{
    /// <summary>Initializes a new instance.</summary>
    public MediaAssetsByMediaIdsSpec(IEnumerable<string> mediaIds)
    {
        var ids = mediaIds.ToList();
        Query.Where(m => ids.Contains(m.MediaId));
    }
}

/// <summary>Check if a media asset exists by media ID.</summary>
public sealed class MediaAssetExistsByMediaIdSpec : SingleResultSpecification<MediaAsset>
{
    /// <summary>Initializes a new instance.</summary>
    public MediaAssetExistsByMediaIdSpec(string mediaId)
    {
        Query.Where(m => m.MediaId == mediaId);
    }
}
