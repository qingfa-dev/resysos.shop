namespace BuildingBlock.Domain.StronglyTypedIds;

/// <summary>
/// Base record for strongly typed identifiers. Wrapping a primitive identifier
/// in its own type prevents identifiers from different concepts being mixed up.
/// </summary>
/// <typeparam name="TValue">The underlying identifier value type.</typeparam>
/// <remarks>
/// Derive a sealed record for each identifier:
/// <code>
/// public sealed record OrderId : StronglyTypedId&lt;Guid&gt;
/// {
///     public OrderId(Guid value) : base(value) { }
/// }
/// </code>
/// Record equality includes the runtime type, so two identifiers with the same
/// value but different types are never equal.
/// </remarks>
public abstract record StronglyTypedId<TValue> : IComparable<StronglyTypedId<TValue>>
    where TValue : notnull, IComparable<TValue>
{
    #region Constructors

    /// <summary>
    /// Initializes a new identifier with the given value.
    /// </summary>
    /// <param name="value">The underlying identifier value.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is the default value for its type.
    /// </exception>
    protected StronglyTypedId(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (EqualityComparer<TValue>.Default.Equals(value, default!))
        {
            throw new ArgumentException(
                "Strongly typed identifier cannot use the default value.",
                nameof(value));
        }

        if (value is string text && string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Strongly typed identifier cannot use an empty string.",
                nameof(value));
        }

        Value = value;
    }

    #endregion

    #region Properties

    /// <summary>Gets the underlying identifier value.</summary>
    public TValue Value { get; }

    #endregion

    #region Equality

    /// <inheritdoc />
    public int CompareTo(StronglyTypedId<TValue>? other)
        => other is null ? 1 : Value.CompareTo(other.Value);

    #endregion

    #region Methods

    /// <inheritdoc />
    /// <remarks>
    /// Sealed so derived identifier records always print the underlying value
    /// instead of the compiler-generated record representation.
    /// </remarks>
    public sealed override string ToString() => Value.ToString()!;

    #endregion
}

/// <summary>Base record for identifiers backed by a <see cref="Guid"/>.</summary>
public abstract record GuidId : StronglyTypedId<Guid>
{
    /// <summary>Initializes a new identifier with the given value.</summary>
    /// <param name="value">The underlying identifier value.</param>
    protected GuidId(Guid value) : base(value) { }

    /// <summary>Creates a new, unique identifier.</summary>
    /// <returns>A new identifier value.</returns>
    public static Guid NewValue() => Guid.NewGuid();
}

/// <summary>Base record for identifiers backed by a non-empty string.</summary>
public abstract record StringId : StronglyTypedId<string>
{
    /// <summary>Initializes a new identifier with the given value.</summary>
    /// <param name="value">The underlying identifier value.</param>
    protected StringId(string value) : base(value) { }
}
