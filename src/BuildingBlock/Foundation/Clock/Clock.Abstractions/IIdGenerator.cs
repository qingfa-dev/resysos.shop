namespace BuildingBlock.Clock.Abstractions;

/// <summary>
/// Abstraction over identifier generation. Business code uses this port so tests
/// can produce deterministic identifiers.
/// </summary>
public interface IIdGenerator
{
    /// <summary>Creates a new globally unique identifier.</summary>
    Guid NewGuid();

    /// <summary>Creates a new identifier with a human-readable prefix.</summary>
    /// <param name="prefix">A short prefix such as <c>ord</c>.</param>
    /// <returns>The prefixed identifier.</returns>
    string NewId(string prefix);
}
