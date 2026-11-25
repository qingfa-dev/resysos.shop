using BuildingBlock.Clock.Abstractions;

namespace BuildingBlock.Clock.System;

/// <summary>Identifier generator backed by <see cref="Guid.NewGuid"/>.</summary>
public sealed class GuidIdGenerator : IIdGenerator
{
    /// <inheritdoc />
    public Guid NewGuid() => Guid.NewGuid();

    /// <inheritdoc />
    public string NewId(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        return $"{prefix}_{Guid.NewGuid():N}";
    }
}
