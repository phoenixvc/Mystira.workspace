using System.Collections.Concurrent;
using System.Threading.Channels;
using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services.ContinuityAsync;

public class ContinuityJob
{
    public required string OperationId { get; init; }
    public required EvaluateStoryContinuityRequest Request { get; init; }
}

public interface IContinuityOperationStore
{
    ContinuityOperationInfo CreateNew();
    bool TryGet(string id, out ContinuityOperationInfo? info);
    void MarkRunning(string id);
    void MarkSucceeded(string id, EvaluateStoryContinuityResponse result);
    void MarkFailed(string id, string error);
}

public class InMemoryContinuityOperationStore : IContinuityOperationStore
{
    private readonly ConcurrentDictionary<string, ContinuityOperationInfo> _ops = new();

    public ContinuityOperationInfo CreateNew()
    {
        var id = Guid.NewGuid().ToString("N");
        var info = new ContinuityOperationInfo
        {
            OperationId = id,
            Status = ContinuityOperationStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _ops[id] = info;
        return info;
    }

    public bool TryGet(string id, out ContinuityOperationInfo? info)
    {
        var ok = _ops.TryGetValue(id, out var found);
        info = found;
        return ok;
    }

    public void MarkRunning(string id)
    {
        if (_ops.TryGetValue(id, out var info))
        {
            info.Status = ContinuityOperationStatus.Running;
            info.StartedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkSucceeded(string id, EvaluateStoryContinuityResponse result)
    {
        if (_ops.TryGetValue(id, out var info))
        {
            info.Status = ContinuityOperationStatus.Succeeded;
            info.Result = result;
            info.CompletedAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkFailed(string id, string error)
    {
        if (_ops.TryGetValue(id, out var info))
        {
            info.Status = ContinuityOperationStatus.Failed;
            info.Error = error;
            info.CompletedAt = DateTimeOffset.UtcNow;
        }
    }
}

public interface IContinuityBackgroundQueue
{
    ValueTask QueueAsync(ContinuityJob job, CancellationToken ct = default);
    ValueTask<ContinuityJob> DequeueAsync(CancellationToken ct);
}

public class ContinuityBackgroundQueue : IContinuityBackgroundQueue
{
    private readonly Channel<ContinuityJob> _channel = Channel.CreateUnbounded<ContinuityJob>();

    public ValueTask QueueAsync(ContinuityJob job, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(job, ct);

    public async ValueTask<ContinuityJob> DequeueAsync(CancellationToken ct)
        => await _channel.Reader.ReadAsync(ct);
}
