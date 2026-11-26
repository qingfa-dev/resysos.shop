using BuildingBlock.Clock.Abstractions;

namespace BuildingBlock.Clock.System;

/// <summary>
/// Clock implementation backed by <see cref="TimeProvider"/>. Tests replace the
/// provider with a fake to control time.
/// </summary>
public sealed class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;

    /// <summary>Initializes a new clock.</summary>
    /// <param name="timeProvider">The time provider to read.</param>
    public SystemClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();

    /// <inheritdoc />
    public DateOnly GetToday => DateOnly.FromDateTime(UtcNow.UtcDateTime);
}
