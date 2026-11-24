using System.Text.RegularExpressions;

using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

namespace BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

/// <summary>Validates modification operations for <see cref="ICreatable"/> and <see cref="IModifiable"/> entities.</summary>
public static class ModifiableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the modifiable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ICreatable, IModifiable
    {
        return auditable is null
            ? Result<TValue>.Failure(ModifiableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region ModifiedAt

    /// <summary>Validates the modification timestamp.</summary>
    /// <param name="nowUtc">The modification timestamp to validate.</param>
    /// <returns>The timestamp on success; otherwise the first failure.</returns>
    public static Result<DateTimeOffset> ValidateModifiedAt(
        DateTimeOffset nowUtc)
    {
        return nowUtc == default
            ? Result<DateTimeOffset>.Failure(
                ModifiableConstant.ModifiedAt.Required.Error)
            : Result<DateTimeOffset>.Success(nowUtc)
                .Ensure(
                    predicate: candidate =>
                        candidate <= DateTimeOffset.UtcNow.AddSeconds(
                            LifecycleConstant.Timestamp.MaxFutureSkew.Value),
                    error: ModifiableConstant.ModifiedAt.Future.Error);
    }

    /// <summary>Validates that the entity has creation audit metadata.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the not-initialized error.</returns>
    public static Result<TValue> ValidateInitialization<TValue>(TValue entity)
        where TValue : ICreatable
    {
        return entity.CreatedAtUtc != default
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(ModifiableConstant.State.NotInitialized.Error);
    }

    /// <summary>Validates that the modification timestamp follows the creation timestamp.</summary>
    /// <param name="nowUtc">The modification timestamp.</param>
    /// <param name="createdAtUtc">The creation timestamp.</param>
    /// <returns>The modification timestamp on success; otherwise the invalid-range error.</returns>
    public static Result<DateTimeOffset> ValidateChronology(
        DateTimeOffset nowUtc,
        DateTimeOffset createdAtUtc)
    {
        return nowUtc >= createdAtUtc
            ? Result<DateTimeOffset>.Success(nowUtc)
            : Result<DateTimeOffset>.Failure(
                ModifiableConstant.ModifiedAt.BeforeCreatedAt.Error);
    }

    #endregion ModifiedAt

    #region ModifiedBy

    /// <summary>Validates the modifying actor.</summary>
    /// <param name="actor">The actor to validate (null is allowed).</param>
    /// <param name="required">Whether the actor must be specified.</param>
    /// <returns>The actor on success; otherwise the first failure.</returns>
    public static Result<string?> ValidateActor(
        string? actor,
        bool required)
    {
        return Result<string?>.Success(actor)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <= ModifiableConstant.ModifiedBy.MaxLength.Value,
                error: ModifiableConstant.ModifiedBy.MaxLength.Error)
            .Ensure(
                predicate: candidate => !required || !string.IsNullOrWhiteSpace(candidate),
                error: ModifiableConstant.ModifiedBy.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    Regex.IsMatch(candidate, ModifiableConstant.ModifiedBy.Format.Pattern),
                error: ModifiableConstant.ModifiedBy.Format.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length >= ModifiableConstant.ModifiedBy.MinLength.Value,
                error: ModifiableConstant.ModifiedBy.MinLength.Error);
    }

    #endregion ModifiedBy

    #endregion

    #region Composition

    /// <summary>
    /// Validates the audit metadata that <see cref="ModifiableExtensions.MarkModified{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="nowUtc">The modification timestamp.</param>
    /// <param name="actor">The modifying actor.</param>
    /// <param name="requireModifiedBy">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateModification<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireModifiedBy = LifecycleConstant.Defaults.RequireActor)
        where TValue : ICreatable, IModifiable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateModifiedAt(nowUtc)
                    .Bind(_ => ValidateInitialization(entity))
                    .Bind(_ => ValidateChronology(nowUtc, entity.CreatedAtUtc))
                    .Bind(_ => ValidateActor(actor, requireModifiedBy))
                    .Map(_ => entity));
    }

    #endregion
}
