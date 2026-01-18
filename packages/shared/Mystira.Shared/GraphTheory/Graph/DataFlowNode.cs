namespace Mystira.Shared.GraphTheory.Graph;

/// <summary>
/// A node in a data flow graph that carries a value.
/// </summary>
/// <typeparam name="T">Type of the data value.</typeparam>
public class DataFlowNode<T>
{
    /// <summary>
    /// Creates a new data flow node.
    /// </summary>
    /// <param name="id">Unique identifier for the node.</param>
    /// <param name="value">The data value.</param>
    public DataFlowNode(string id, T value)
    {
        Id = id;
        Value = value;
    }

    /// <summary>
    /// Unique identifier for the node.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// The data value carried by this node.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// Optional label for the node.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Custom metadata associated with the node.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Whether this node has been visited (for traversal algorithms).
    /// </summary>
    public bool Visited { get; set; }

    /// <summary>
    /// Discovery time (for DFS algorithms).
    /// </summary>
    public int DiscoveryTime { get; set; } = -1;

    /// <summary>
    /// Finish time (for DFS algorithms).
    /// </summary>
    public int FinishTime { get; set; } = -1;

    /// <summary>
    /// Distance from source (for BFS/shortest path algorithms).
    /// </summary>
    public int Distance { get; set; } = int.MaxValue;

    /// <summary>
    /// Parent node in traversal tree.
    /// </summary>
    public DataFlowNode<T>? Parent { get; set; }

    /// <summary>
    /// Resets traversal state.
    /// </summary>
    public void ResetState()
    {
        Visited = false;
        DiscoveryTime = -1;
        FinishTime = -1;
        Distance = int.MaxValue;
        Parent = null;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is DataFlowNode<T> other)
        {
            return Id.Equals(other.Id);
        }
        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc />
    public override string ToString() =>
        $"Node({Id}: {Value}{(Label != null ? $" \"{Label}\"" : "")})";
}
