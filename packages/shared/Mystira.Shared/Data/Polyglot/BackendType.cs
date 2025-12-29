namespace Mystira.Shared.Data.Polyglot;

/// <summary>
/// Represents the type of backend in a polyglot persistence setup.
/// </summary>
public enum BackendType
{
    /// <summary>
    /// The primary backend (source of truth).
    /// </summary>
    Primary,

    /// <summary>
    /// The secondary backend (synchronized copy).
    /// </summary>
    Secondary
}
