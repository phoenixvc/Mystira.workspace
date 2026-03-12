using Ardalis.Specification;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;

namespace Mystira.Core.Specifications;

/// <summary>Find a data deletion request by ID.</summary>
public sealed class DataDeletionByIdSpec : SingleResultSpecification<DataDeletionRequest>
{
    /// <summary>Initializes a new instance.</summary>
    public DataDeletionByIdSpec(string id)
    {
        Query.Where(d => d.Id == id);
    }
}

/// <summary>Find data deletion requests by child profile ID.</summary>
public sealed class DataDeletionByChildProfileSpec : Specification<DataDeletionRequest>
{
    /// <summary>Initializes a new instance.</summary>
    public DataDeletionByChildProfileSpec(string childProfileId)
    {
        Query
            .Where(d => d.ChildProfileId == childProfileId)
            .OrderByDescending(d => d.RequestedAt);
    }
}

/// <summary>Find pending data deletion requests that are due for processing.</summary>
public sealed class PendingDataDeletionsSpec : Specification<DataDeletionRequest>
{
    /// <summary>Initializes a new instance.</summary>
    public PendingDataDeletionsSpec()
    {
        Query
            .Where(d =>
                d.Status == DeletionStatus.Pending ||
                d.Status == DeletionStatus.SoftDeleted ||
                (d.Status == DeletionStatus.Failed && d.RetryCount < DataDeletionRequest.MaxRetries))
            .Where(d =>
                d.ScheduledDeletionAt <= DateTime.UtcNow ||
                (d.NextRetryAt != null && d.NextRetryAt <= DateTime.UtcNow))
            .OrderBy(d => d.ScheduledDeletionAt);
    }
}
