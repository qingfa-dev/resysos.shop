namespace BuildingBlock.Domain.ValueObjects;

/// <summary>
/// Base class for value objects: immutable concepts compared by their
/// structural equality components rather than by identity.
/// </summary>
/// <remarks>
/// Derive from this type and return every value that defines equality from
/// <see cref="GetEqualityComponents"/>. Two value objects of the same concrete
/// type are equal when their components are equal in the same order.
/// </remarks>
public abstract class ValueObject : IEquatable<ValueObject>
{
    #region Equality

    /// <summary>
    /// Gets the components that define structural equality.
    /// </summary>
    /// <returns>The ordered equality components.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <inheritdoc />
    public bool Equals(ValueObject? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is ValueObject other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in GetEqualityComponents())
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }

    /// <summary>Determines whether two value objects are equal.</summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Determines whether two value objects are not equal.</summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
        => !(left == right);

    #endregion

    #region Methods

    /// <summary>
    /// Creates a shallow copy of this value object.
    /// </summary>
    /// <remarks>
    /// Value objects should normally be immutable. Use this helper only when a
    /// framework requires a copy, and never expose mutable state through it.
    /// </remarks>
    /// <returns>A shallow copy of this instance.</returns>
    protected ValueObject GetCopy() => (ValueObject)MemberwiseClone();

    /// <inheritdoc />
    public override string ToString()
        => $"{GetType().Name} {{ {string.Join(", ", GetEqualityComponents())} }}";

    #endregion
}
