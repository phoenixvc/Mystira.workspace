using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Mystira.Shared.Resilience;

/// <summary>
/// Circuit breaker state enumeration.
/// </summary>
public enum CircuitState
{
    /// <summary>
    /// Circuit is closed, requests flow normally.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open, requests are rejected immediately.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is testing, a limited number of requests are allowed through.
    /// </summary>
    HalfOpen,

    /// <summary>
    /// Circuit is isolated (manually opened).
    /// </summary>
    Isolated
}

/// <summary>
/// Event arguments for circuit breaker state changes.
/// </summary>
public class CircuitBreakerStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Name of the circuit breaker (usually the client/service name).
    /// </summary>
    public required string CircuitName { get; init; }

    /// <summary>
    /// Previous state before the change.
    /// </summary>
    public required CircuitState PreviousState { get; init; }

    /// <summary>
    /// New state after the change.
    /// </summary>
    public required CircuitState NewState { get; init; }

    /// <summary>
    /// When the state change occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Duration the circuit will remain in the new state (for Open state).
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    /// The exception that caused the state change (if applicable).
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Additional context information.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Context { get; init; }
}

/// <summary>
/// Event arguments for circuit breaker request rejection.
/// </summary>
public class CircuitBreakerRejectionEventArgs : EventArgs
{
    /// <summary>
    /// Name of the circuit breaker.
    /// </summary>
    public required string CircuitName { get; init; }

    /// <summary>
    /// Current state of the circuit.
    /// </summary>
    public required CircuitState State { get; init; }

    /// <summary>
    /// When the rejection occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Time remaining until the circuit may transition to half-open.
    /// </summary>
    public TimeSpan? TimeUntilHalfOpen { get; init; }
}

/// <summary>
/// Observable circuit breaker that emits events for state changes.
/// Integrates with OpenTelemetry for metrics and tracing.
/// </summary>
public interface IObservableCircuitBreaker
{
    /// <summary>
    /// Name of the circuit breaker.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Current state of the circuit breaker.
    /// </summary>
    CircuitState State { get; }

    /// <summary>
    /// Event raised when the circuit breaker state changes.
    /// </summary>
    event EventHandler<CircuitBreakerStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Event raised when a request is rejected due to the circuit being open.
    /// </summary>
    event EventHandler<CircuitBreakerRejectionEventArgs>? RequestRejected;
}

/// <summary>
/// Circuit breaker metrics for OpenTelemetry integration.
/// </summary>
public class CircuitBreakerMetrics : IDisposable
{
    private static readonly Meter s_meter = new("Mystira.Shared.Resilience", "1.0.0");
    private static readonly ActivitySource s_activitySource = new("Mystira.Shared.Resilience");

    private readonly Counter<long> _stateChanges;
    private readonly Counter<long> _rejections;
    private readonly Counter<long> _successes;
    private readonly Counter<long> _failures;
    private readonly ObservableGauge<int> _circuitState;

    private readonly Dictionary<string, CircuitState> _circuitStates = new();
    private readonly object _lock = new();

    /// <summary>
    /// Creates a new circuit breaker metrics instance.
    /// </summary>
    public CircuitBreakerMetrics()
    {
        _stateChanges = s_meter.CreateCounter<long>(
            "mystira.circuit_breaker.state_changes",
            "changes",
            "Number of circuit breaker state changes");

        _rejections = s_meter.CreateCounter<long>(
            "mystira.circuit_breaker.rejections",
            "rejections",
            "Number of requests rejected by circuit breaker");

        _successes = s_meter.CreateCounter<long>(
            "mystira.circuit_breaker.successes",
            "requests",
            "Number of successful requests through circuit breaker");

        _failures = s_meter.CreateCounter<long>(
            "mystira.circuit_breaker.failures",
            "requests",
            "Number of failed requests through circuit breaker");

        _circuitState = s_meter.CreateObservableGauge<int>(
            "mystira.circuit_breaker.state",
            GetCircuitStates,
            "state",
            "Current state of circuit breakers (0=Closed, 1=Open, 2=HalfOpen, 3=Isolated)");
    }

    /// <summary>
    /// Records a circuit breaker state change.
    /// </summary>
    public void RecordStateChange(string circuitName, CircuitState previousState, CircuitState newState)
    {
        lock (_lock)
        {
            _circuitStates[circuitName] = newState;
        }

        _stateChanges.Add(1,
            new KeyValuePair<string, object?>("circuit.name", circuitName),
            new KeyValuePair<string, object?>("circuit.previous_state", previousState.ToString()),
            new KeyValuePair<string, object?>("circuit.new_state", newState.ToString()));

        // Create a span for traceability
        using var activity = s_activitySource.StartActivity("CircuitBreakerStateChange");
        activity?.SetTag("circuit.name", circuitName);
        activity?.SetTag("circuit.previous_state", previousState.ToString());
        activity?.SetTag("circuit.new_state", newState.ToString());
    }

    /// <summary>
    /// Records a request rejection by the circuit breaker.
    /// </summary>
    public void RecordRejection(string circuitName)
    {
        _rejections.Add(1,
            new KeyValuePair<string, object?>("circuit.name", circuitName));
    }

    /// <summary>
    /// Records a successful request through the circuit breaker.
    /// </summary>
    public void RecordSuccess(string circuitName)
    {
        _successes.Add(1,
            new KeyValuePair<string, object?>("circuit.name", circuitName));
    }

    /// <summary>
    /// Records a failed request through the circuit breaker.
    /// </summary>
    public void RecordFailure(string circuitName)
    {
        _failures.Add(1,
            new KeyValuePair<string, object?>("circuit.name", circuitName));
    }

    private IEnumerable<Measurement<int>> GetCircuitStates()
    {
        lock (_lock)
        {
            foreach (var kvp in _circuitStates)
            {
                yield return new Measurement<int>(
                    (int)kvp.Value,
                    new KeyValuePair<string, object?>("circuit.name", kvp.Key));
            }
        }
    }

    /// <summary>
    /// Gets the current state of a circuit breaker.
    /// </summary>
    public CircuitState GetState(string circuitName)
    {
        lock (_lock)
        {
            return _circuitStates.TryGetValue(circuitName, out var state) ? state : CircuitState.Closed;
        }
    }

    /// <summary>
    /// Gets all tracked circuit breakers and their states.
    /// </summary>
    public IReadOnlyDictionary<string, CircuitState> GetAllStates()
    {
        lock (_lock)
        {
            return new Dictionary<string, CircuitState>(_circuitStates);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Static fields are app-lifetime and should not be disposed per-instance
    }
}

/// <summary>
/// Circuit breaker event publisher for broadcasting state changes.
/// </summary>
public class CircuitBreakerEventPublisher : IObservableCircuitBreaker
{
    private readonly CircuitBreakerMetrics _metrics;
    private readonly ILogger<CircuitBreakerEventPublisher> _logger;
    private CircuitState _state = CircuitState.Closed;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public CircuitState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                var previous = _state;
                _state = value;
                OnStateChanged(previous, value);
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<CircuitBreakerStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public event EventHandler<CircuitBreakerRejectionEventArgs>? RequestRejected;

    /// <summary>
    /// Initializes a new instance of the <see cref="CircuitBreakerEventPublisher"/> class.
    /// </summary>
    /// <param name="name">The circuit breaker name.</param>
    /// <param name="metrics">The metrics collector.</param>
    /// <param name="logger">Logger instance.</param>
    public CircuitBreakerEventPublisher(
        string name,
        CircuitBreakerMetrics metrics,
        ILogger<CircuitBreakerEventPublisher> logger)
    {
        Name = name;
        _metrics = metrics;
        _logger = logger;
    }

    /// <summary>
    /// Notifies that the circuit has opened.
    /// </summary>
    public void NotifyOpened(TimeSpan duration, Exception? exception = null)
    {
        var previousState = State;
        State = CircuitState.Open;

        _metrics.RecordStateChange(Name, previousState, CircuitState.Open);

        _logger.LogWarning(
            "Circuit breaker {CircuitName} opened for {Duration}s. Reason: {Reason}",
            Name,
            duration.TotalSeconds,
            exception?.Message ?? "Threshold exceeded");

        StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs
        {
            CircuitName = Name,
            PreviousState = previousState,
            NewState = CircuitState.Open,
            Duration = duration,
            Exception = exception
        });
    }

    /// <summary>
    /// Notifies that the circuit has transitioned to half-open.
    /// </summary>
    public void NotifyHalfOpen()
    {
        var previousState = State;
        State = CircuitState.HalfOpen;

        _metrics.RecordStateChange(Name, previousState, CircuitState.HalfOpen);

        _logger.LogInformation("Circuit breaker {CircuitName} half-open, testing...", Name);

        StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs
        {
            CircuitName = Name,
            PreviousState = previousState,
            NewState = CircuitState.HalfOpen
        });
    }

    /// <summary>
    /// Notifies that the circuit has closed (reset).
    /// </summary>
    public void NotifyClosed()
    {
        var previousState = State;
        State = CircuitState.Closed;

        _metrics.RecordStateChange(Name, previousState, CircuitState.Closed);

        _logger.LogInformation("Circuit breaker {CircuitName} closed (reset)", Name);

        StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs
        {
            CircuitName = Name,
            PreviousState = previousState,
            NewState = CircuitState.Closed
        });
    }

    /// <summary>
    /// Notifies that a request was rejected.
    /// </summary>
    public void NotifyRejection(TimeSpan? timeUntilHalfOpen = null)
    {
        _metrics.RecordRejection(Name);

        _logger.LogDebug(
            "Request rejected by circuit breaker {CircuitName}. Time until half-open: {TimeUntilHalfOpen}",
            Name,
            timeUntilHalfOpen?.ToString() ?? "unknown");

        RequestRejected?.Invoke(this, new CircuitBreakerRejectionEventArgs
        {
            CircuitName = Name,
            State = State,
            TimeUntilHalfOpen = timeUntilHalfOpen
        });
    }

    /// <summary>
    /// Records a successful request.
    /// </summary>
    public void RecordSuccess()
    {
        _metrics.RecordSuccess(Name);
    }

    /// <summary>
    /// Records a failed request.
    /// </summary>
    public void RecordFailure()
    {
        _metrics.RecordFailure(Name);
    }

    private void OnStateChanged(CircuitState previous, CircuitState current)
    {
        _metrics.RecordStateChange(Name, previous, current);
    }
}
