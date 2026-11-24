using System.Reflection;
using System.Runtime.CompilerServices;

namespace BuildingBlock.Domain.SmartEnums;

/// <summary>
/// Base class for smart enumerations: a fixed set of named values that behaves
/// like an enum but carries a stable identifier and supports lookup by id or name.
/// </summary>
/// <typeparam name="TEnum">The concrete smart enumeration type.</typeparam>
/// <remarks>
/// Declare one public static readonly field per member:
/// <code>
/// public sealed class OrderStatus : SmartEnum&lt;OrderStatus&gt;
/// {
///     public static readonly OrderStatus Pending = new(1, "Pending");
///     public static readonly OrderStatus Confirmed = new(2, "Confirmed");
///     private OrderStatus(int id, string name) : base(id, name) { }
/// }
/// </code>
/// </remarks>
public abstract class SmartEnum<TEnum> :
    IEquatable<SmartEnum<TEnum>>,
    IComparable<SmartEnum<TEnum>>
    where TEnum : SmartEnum<TEnum>
{
    #region Fields

    private static readonly Lazy<IReadOnlyList<TEnum>> Values =
        new(Discover, LazyThreadSafetyMode.ExecutionAndPublication);

    #endregion

    #region Constructors

    /// <summary>Initializes a new smart enumeration member.</summary>
    /// <param name="id">The stable numeric identifier.</param>
    /// <param name="name">The display name.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="id"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is missing or too long.
    /// </exception>
    protected SmartEnum(int id, string name)
    {
        SmartEnumGuard.ValidateId(id)
            .OnFailure(error => throw new ArgumentOutOfRangeException(
                paramName: nameof(id),
                actualValue: id,
                message: error.ToString()));

        SmartEnumGuard.ValidateName(name)
            .OnFailure(error => throw new ArgumentException(
                message: error.ToString(),
                paramName: nameof(name)));

        Id = id;
        Name = name.Trim();
    }

    #endregion

    #region Properties

    /// <summary>Gets the stable numeric identifier.</summary>
    public int Id { get; }

    /// <summary>Gets the display name.</summary>
    public string Name { get; }

    #endregion

    #region Lookup

    /// <summary>Gets every member, ordered by identifier.</summary>
    /// <returns>The registered members.</returns>
    public static IReadOnlyList<TEnum> GetAll() => Values.Value;

    /// <summary>Finds a member by identifier.</summary>
    /// <param name="id">The identifier to find.</param>
    /// <returns>The member on success; otherwise a not-found error.</returns>
    public static Result<TEnum> TryFromId(int id)
    {
        var match = GetAll().FirstOrDefault(value => value.Id == id);

        return match is null
            ? Result<TEnum>.Failure(SmartEnumConstant.Lookup.IdNotFound.Error)
            : Result<TEnum>.Success(match);
    }

    /// <summary>Finds a member by identifier, throwing when it is unknown.</summary>
    /// <param name="id">The identifier to find.</param>
    /// <returns>The member.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when no member has the requested identifier.
    /// </exception>
    public static TEnum FromId(int id)
    {
        var result = TryFromId(id);

        return result.IsSuccess
            ? result.Value
            : throw new ArgumentOutOfRangeException(
                nameof(id),
                id,
                SmartEnumConstant.Lookup.IdNotFound.Error.ToString());
    }

    /// <summary>Finds a member by name, ignoring case and surrounding whitespace.</summary>
    /// <param name="name">The name to find.</param>
    /// <returns>The member on success; otherwise a not-found error.</returns>
    public static Result<TEnum> TryFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<TEnum>.Failure(
                SmartEnumConstant.Lookup.NameNotFound.Error);
        }

        var normalized = name.Trim();

        var match = GetAll().FirstOrDefault(value =>
            string.Equals(value.Name, normalized, StringComparison.OrdinalIgnoreCase));

        return match is null
            ? Result<TEnum>.Failure(SmartEnumConstant.Lookup.NameNotFound.Error)
            : Result<TEnum>.Success(match);
    }

    /// <summary>Finds a member by name, throwing when it is unknown.</summary>
    /// <param name="name">The name to find.</param>
    /// <returns>The member.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when no member has the requested name.
    /// </exception>
    public static TEnum FromName(string? name)
    {
        var result = TryFromName(name);

        return result.IsSuccess
            ? result.Value
            : throw new ArgumentOutOfRangeException(
                nameof(name),
                name,
                SmartEnumConstant.Lookup.NameNotFound.Error.ToString());
    }

    #endregion

    #region Equality

    /// <inheritdoc />
    public bool Equals(SmartEnum<TEnum>? other)
        => other is not null
           && GetType() == other.GetType()
           && Id == other.Id;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is SmartEnum<TEnum> other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>Determines whether two members are equal.</summary>
    public static bool operator ==(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right)
        => left?.Equals(right) ?? right is null;

    /// <summary>Determines whether two members are not equal.</summary>
    public static bool operator !=(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right)
        => !(left == right);

    /// <inheritdoc />
    public int CompareTo(SmartEnum<TEnum>? other)
        => other is null ? 1 : Id.CompareTo(other.Id);

    #endregion

    #region Methods

    /// <inheritdoc />
    public override string ToString() => Name;

    #endregion

    #region Private Methods

    private static IReadOnlyList<TEnum> Discover()
    {
        // Ensure the derived type's static members are initialized before reading them.
        RuntimeHelpers.RunClassConstructor(typeof(TEnum).TypeHandle);

        var members = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(TEnum))
            .Select(field => (TEnum)field.GetValue(null)!)
            .OrderBy(member => member.Id)
            .ToArray();

        var duplicateId = members
            .GroupBy(member => member.Id)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateId is not null)
        {
            throw new InvalidOperationException(
                $"{typeof(TEnum).Name} declares duplicate smart enum id '{duplicateId.Key}'.");
        }

        var duplicateName = members
            .GroupBy(member => member.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateName is not null)
        {
            throw new InvalidOperationException(
                $"{typeof(TEnum).Name} declares duplicate smart enum name '{duplicateName.Key}'.");
        }

        return Array.AsReadOnly(members);
    }

    #endregion
}
