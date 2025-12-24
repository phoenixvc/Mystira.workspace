using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Shared.Resilience;

namespace Mystira.Shared.Tests.Resilience;

public class CircuitBreakerMetricsTests : IDisposable
{
    private readonly CircuitBreakerMetrics _metrics;

    public CircuitBreakerMetricsTests()
    {
        _metrics = new CircuitBreakerMetrics();
    }

    [Fact]
    public void RecordStateChange_TracksState()
    {
        // Act
        _metrics.RecordStateChange("TestCircuit", CircuitState.Closed, CircuitState.Open);

        // Assert
        _metrics.GetState("TestCircuit").Should().Be(CircuitState.Open);
    }

    [Fact]
    public void RecordStateChange_UpdatesExistingState()
    {
        // Arrange
        _metrics.RecordStateChange("TestCircuit", CircuitState.Closed, CircuitState.Open);

        // Act
        _metrics.RecordStateChange("TestCircuit", CircuitState.Open, CircuitState.HalfOpen);

        // Assert
        _metrics.GetState("TestCircuit").Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void GetState_ReturnsClosedForUnknownCircuit()
    {
        // Act
        var state = _metrics.GetState("UnknownCircuit");

        // Assert
        state.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void GetAllStates_ReturnsAllTrackedCircuits()
    {
        // Arrange
        _metrics.RecordStateChange("Circuit1", CircuitState.Closed, CircuitState.Open);
        _metrics.RecordStateChange("Circuit2", CircuitState.Closed, CircuitState.HalfOpen);
        _metrics.RecordStateChange("Circuit3", CircuitState.Closed, CircuitState.Closed);

        // Act
        var states = _metrics.GetAllStates();

        // Assert
        states.Should().HaveCount(3);
        states["Circuit1"].Should().Be(CircuitState.Open);
        states["Circuit2"].Should().Be(CircuitState.HalfOpen);
        states["Circuit3"].Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void RecordRejection_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _metrics.RecordRejection("TestCircuit");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordSuccess_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _metrics.RecordSuccess("TestCircuit");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordFailure_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _metrics.RecordFailure("TestCircuit");
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _metrics.Dispose();
    }
}

public class CircuitBreakerEventPublisherTests : IDisposable
{
    private readonly CircuitBreakerMetrics _metrics;
    private readonly Mock<ILogger<CircuitBreakerEventPublisher>> _loggerMock;
    private readonly CircuitBreakerEventPublisher _publisher;

    public CircuitBreakerEventPublisherTests()
    {
        _metrics = new CircuitBreakerMetrics();
        _loggerMock = new Mock<ILogger<CircuitBreakerEventPublisher>>();
        _publisher = new CircuitBreakerEventPublisher("TestCircuit", _metrics, _loggerMock.Object);
    }

    [Fact]
    public void Name_ReturnsConfiguredName()
    {
        // Assert
        _publisher.Name.Should().Be("TestCircuit");
    }

    [Fact]
    public void State_InitiallyIsClosed()
    {
        // Assert
        _publisher.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void NotifyOpened_ChangesStateToOpen()
    {
        // Act
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30));

        // Assert
        _publisher.State.Should().Be(CircuitState.Open);
    }

    [Fact]
    public void NotifyHalfOpen_ChangesStateToHalfOpen()
    {
        // Arrange
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30));

        // Act
        _publisher.NotifyHalfOpen();

        // Assert
        _publisher.State.Should().Be(CircuitState.HalfOpen);
    }

    [Fact]
    public void NotifyClosed_ChangesStateToClosed()
    {
        // Arrange
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30));
        _publisher.NotifyHalfOpen();

        // Act
        _publisher.NotifyClosed();

        // Assert
        _publisher.State.Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void NotifyOpened_RaisesStateChangedEvent()
    {
        // Arrange
        CircuitBreakerStateChangedEventArgs? receivedArgs = null;
        _publisher.StateChanged += (sender, args) => receivedArgs = args;

        // Act
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30));

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.CircuitName.Should().Be("TestCircuit");
        receivedArgs.PreviousState.Should().Be(CircuitState.Closed);
        receivedArgs.NewState.Should().Be(CircuitState.Open);
        receivedArgs.Duration.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void NotifyOpened_WithException_IncludesExceptionInEvent()
    {
        // Arrange
        CircuitBreakerStateChangedEventArgs? receivedArgs = null;
        _publisher.StateChanged += (sender, args) => receivedArgs = args;
        var exception = new InvalidOperationException("Test error");

        // Act
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30), exception);

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.Exception.Should().Be(exception);
    }

    [Fact]
    public void NotifyRejection_RaisesRequestRejectedEvent()
    {
        // Arrange
        _publisher.NotifyOpened(TimeSpan.FromSeconds(30));
        CircuitBreakerRejectionEventArgs? receivedArgs = null;
        _publisher.RequestRejected += (sender, args) => receivedArgs = args;

        // Act
        _publisher.NotifyRejection(TimeSpan.FromSeconds(15));

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.CircuitName.Should().Be("TestCircuit");
        receivedArgs.State.Should().Be(CircuitState.Open);
        receivedArgs.TimeUntilHalfOpen.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Fact]
    public void RecordSuccess_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _publisher.RecordSuccess();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordFailure_DoesNotThrow()
    {
        // Act & Assert
        var act = () => _publisher.RecordFailure();
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _metrics.Dispose();
    }
}

public class CircuitStateTests
{
    [Theory]
    [InlineData(CircuitState.Closed, 0)]
    [InlineData(CircuitState.Open, 1)]
    [InlineData(CircuitState.HalfOpen, 2)]
    [InlineData(CircuitState.Isolated, 3)]
    public void CircuitState_HasCorrectUnderlyingValue(CircuitState state, int expectedValue)
    {
        // Assert
        ((int)state).Should().Be(expectedValue);
    }
}

public class CircuitBreakerStateChangedEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllRequiredProperties()
    {
        // Arrange & Act
        var args = new CircuitBreakerStateChangedEventArgs
        {
            CircuitName = "TestCircuit",
            PreviousState = CircuitState.Closed,
            NewState = CircuitState.Open,
            Duration = TimeSpan.FromSeconds(30),
            Exception = new InvalidOperationException("Test")
        };

        // Assert
        args.CircuitName.Should().Be("TestCircuit");
        args.PreviousState.Should().Be(CircuitState.Closed);
        args.NewState.Should().Be(CircuitState.Open);
        args.Duration.Should().Be(TimeSpan.FromSeconds(30));
        args.Exception.Should().NotBeNull();
        args.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_AllowsNullableProperties()
    {
        // Arrange & Act
        var args = new CircuitBreakerStateChangedEventArgs
        {
            CircuitName = "TestCircuit",
            PreviousState = CircuitState.Closed,
            NewState = CircuitState.Open
        };

        // Assert
        args.Duration.Should().BeNull();
        args.Exception.Should().BeNull();
        args.Context.Should().BeNull();
    }
}

public class CircuitBreakerRejectionEventArgsTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange & Act
        var args = new CircuitBreakerRejectionEventArgs
        {
            CircuitName = "TestCircuit",
            State = CircuitState.Open,
            TimeUntilHalfOpen = TimeSpan.FromSeconds(15)
        };

        // Assert
        args.CircuitName.Should().Be("TestCircuit");
        args.State.Should().Be(CircuitState.Open);
        args.TimeUntilHalfOpen.Should().Be(TimeSpan.FromSeconds(15));
        args.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }
}
