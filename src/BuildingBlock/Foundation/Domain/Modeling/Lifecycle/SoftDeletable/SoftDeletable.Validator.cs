using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

/// <summary>Validates deletion and restoration operations for <see cref="ISoftDeletable"/> entities.</summary>
public static class SoftDeletableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the soft-deletable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ISoftDeletable
    {
        return auditable is null
            ? Result<TValue>.Failure(SoftDeletableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region DeletedAt

    /// <summary>Validates the deletion timestamp.</summary>
    /// <param name="nowUtc">The deletion timestamp to validate.</param>
    /// <returns>The timestamp on success; otherwise the first failure.</returns>
    public static Result<DateTimeOffset> ValidateDeletedAt(
        DateTimeOffset nowUtc)
    {
        return nowUtc == default
            ? Result<DateTimeOffset>.Failure(
                SoftDeletableConstant.DeletedAt.Required.Error)
            : Result<DateTimeOffset>.Success(nowUtc)
                .Ensure(
                    predicate: candidate =>
                        candidate <= DateTimeOffset.UtcNow.AddSeconds(
                            LifecycleConstant.Timestamp.MaxFutureSkew.Value),
                    error: SoftDeletableConstant.DeletedAt.Future.Error);
    }

    #endregion DeletedAt

    #region State

    /// <summary>Validates that the entity can be deleted.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the already-deleted error.</returns>
    public static Result<TValue> ValidateDeletionState<TValue>(TValue entity)
        where TValue : ISoftDeletable
    {
        return entity.IsDeleted
            ? Result<TValue>.Failure(
                SoftDeletableConstant.State.AlreadyDeleted.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the entity can be restored.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the not-deleted error.</returns>
    public static Result<TValue> ValidateRestorationState<TValue>(TValue entity)
        where TValue : ISoftDeletable
    {
        return entity.IsDeleted
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                SoftDeletableConstant.State.NotDeleted.Error);
    }

    #endregion State

    #region DeletedBy

    /// <summary>Validates the deleting actor.</summary>
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
                    candidate.Length <= SoftDeletableConstant.DeletedBy.MaxLength.Value,
                error: SoftDeletableConstant.DeletedBy.MaxLength.Error)
            .Ensure(
                predicate: candidate => !required || !string.IsNullOrWhiteSpace(candidate),
                error: SoftDeletableConstant.DeletedBy.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    Regex.IsMatch(candidate, SoftDeletableConstant.DeletedBy.Format.Pattern),
                error: SoftDeletableConstant.DeletedBy.Format.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length >= SoftDeletableConstant.DeletedBy.MinLength.Value,
                error: SoftDeletableConstant.DeletedBy.MinLength.Error);
    }

    #endregion DeletedBy

    #endregion

    #region Composition

    /// <summary>
    /// Validates the deletion metadata that
    /// <see cref="SoftDeletableExtensions.MarkDeleted{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="nowUtc">The deletion timestamp.</param>
    /// <param name="actor">The deleting actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateDeletion<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : ISoftDeletable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateDeletedAt(nowUtc)
                    .Bind(_ => ValidateDeletionState(entity))
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    /// <summary>
    /// Validates the restoration metadata that
    /// <see cref="SoftDeletableExtensions.Restore{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="actor">The restoring actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateRestoration<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : ISoftDeletable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateRestorationState(entity)
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    #endregion
}
