using Microsoft.Extensions.Time.Testing;

namespace Testing.Unit;

/// <summary>Creates deterministic clocks for tests that depend on <see cref="TimeProvider"/>.</summary>
public static class TestClock
{
    /// <summary>Creates a fake clock frozen at <paramref name="start"/>, defaulting to 2026-01-01 UTC.</summary>
    public static FakeTimeProvider Create(DateTimeOffset? start = null) =>
        new(start ?? new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
}
