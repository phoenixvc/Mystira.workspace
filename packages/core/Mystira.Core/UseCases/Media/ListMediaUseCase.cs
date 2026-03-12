using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Requests.Media;
using Mystira.Contracts.App.Responses.Media;
using System.Threading;

namespace Mystira.Core.UseCases.Media;

/// <summary>
/// Use case for listing media assets with filtering and pagination
/// </summary>
public class ListMediaUseCase
{
    private readonly IMediaAssetRepository _repository;
    private readonly ILogger<ListMediaUseCase> _logger;

    public ListMediaUseCase(
        IMediaAssetRepository repository,
        ILogger<ListMediaUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<MediaQueryResponse> ExecuteAsync(MediaQueryRequest request, CancellationToken ct = default)
    {
        var query = _repository.GetQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Search))
        {
            query = query.Where(m => m.Id.Contains(request.Search) ||
                                    m.Url.Contains(request.Search) ||
                                    (m.AltText != null && m.AltText.Contains(request.Search)));
        }

        if (!string.IsNullOrEmpty(request.MediaType))
        {
            query = query.Where(m => m.MediaType == request.MediaType);
        }

        if (request.Tags != null && request.Tags.Count > 0)
        {
            foreach (var tag in request.Tags)
            {
                query = query.Where(m => m.Tags.Contains(tag));
            }
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "filename" => request.SortDescending ? query.OrderByDescending(m => m.Url) : query.OrderBy(m => m.Url),
            "mediatype" => request.SortDescending ? query.OrderByDescending(m => m.MediaType) : query.OrderBy(m => m.MediaType),
            "filesize" => request.SortDescending ? query.OrderByDescending(m => m.SizeBytes) : query.OrderBy(m => m.SizeBytes),
            "updatedat" => request.SortDescending ? query.OrderByDescending(m => m.UpdatedAt) : query.OrderBy(m => m.UpdatedAt),
            _ => request.SortDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var media = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new MediaItem
            {
                Id = m.Id,
                Url = m.Url,
                MediaType = m.MediaType
            })
            .ToListAsync(ct);

        return new MediaQueryResponse
        {
            Media = media,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
    }
}

