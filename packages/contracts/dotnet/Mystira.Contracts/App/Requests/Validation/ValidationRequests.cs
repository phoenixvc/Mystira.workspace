namespace Mystira.Contracts.App.Requests.Validation;

/// <summary>
/// Request to validate a compass axis name.
/// </summary>
public record ValidateCompassAxisRequest
{
    /// <summary>
    /// The compass axis name to validate.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request to validate an age group value.
/// </summary>
public record ValidateAgeGroupRequest
{
    /// <summary>
    /// The age group value to validate.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Request to validate an archetype name.
/// </summary>
public record ValidateArchetypeRequest
{
    /// <summary>
    /// The archetype name to validate.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
