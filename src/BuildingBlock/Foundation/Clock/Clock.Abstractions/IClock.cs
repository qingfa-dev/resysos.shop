namespace BuildingBlock.Clock.Abstractions;

/// <summary>
/// Abstraction over the system clock. Business code uses this port so tests can
/// control time instead of reading <see cref="DateTimeOffset.UtcNow"/> directly.
/// </summary>
public interface IClock
{
    /// <summary>Gets the current UTC time.</summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>Gets the current UTC date.</summary>
    DateOnly GetToday => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
