using Ardalis.Specification;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Specifications;

public sealed class DataDeletionByIdSpec : SingleResultSpecification<DataDeletionRequest>
{
    public DataDeletionByIdSpec(string id)
    {
        Query.Where(d => d.Id == id);
    }
}

public sealed class DataDeletionByChildProfileSpec : Specification<DataDeletionRequest>
{
    public DataDeletionByChildProfileSpec(string childProfileId)
    {
        Query
            .Where(d => d.ChildProfileId == childProfileId)
            .OrderByDescending(d => d.RequestedAt);
    }
}

public sealed class PendingDataDeletionsSpec : Specification<DataDeletionRequest>
{
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
