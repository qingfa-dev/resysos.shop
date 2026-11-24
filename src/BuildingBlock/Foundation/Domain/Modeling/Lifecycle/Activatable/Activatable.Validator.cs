using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Lifecycle.Activatable;

/// <summary>Validates activation and deactivation operations for <see cref="IActivatable"/> entities.</summary>
public static class ActivatableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the activatable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IActivatable
    {
        return auditable is null
            ? Result<TValue>.Failure(ActivatableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region ActivatedAt

    /// <summary>Validates the activation timestamp.</summary>
    /// <param name="nowUtc">The activation timestamp to validate.</param>
    /// <returns>The timestamp on success; otherwise the first failure.</returns>
    public static Result<DateTimeOffset> ValidateActivatedAt(
        DateTimeOffset nowUtc)
    {
        return nowUtc == default
            ? Result<DateTimeOffset>.Failure(
                ActivatableConstant.ActivatedAt.Required.Error)
            : Result<DateTimeOffset>.Success(nowUtc)
                .Ensure(
                    predicate: candidate =>
                        candidate <= DateTimeOffset.UtcNow.AddSeconds(
                            LifecycleConstant.Timestamp.MaxFutureSkew.Value),
                    error: ActivatableConstant.ActivatedAt.Future.Error);
    }

    #endregion ActivatedAt

    #region State

    /// <summary>Validates that the entity can be activated.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the already-active error.</returns>
    public static Result<TValue> ValidateActivationState<TValue>(TValue entity)
        where TValue : IActivatable
    {
        return entity.IsActive
            ? Result<TValue>.Failure(
                ActivatableConstant.State.AlreadyActive.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the entity can be deactivated.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the not-active error.</returns>
    public static Result<TValue> ValidateDeactivationState<TValue>(TValue entity)
        where TValue : IActivatable
    {
        return entity.IsActive
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                ActivatableConstant.State.NotActive.Error);
    }

    #endregion State

    #region ActivatedBy

    /// <summary>Validates the activating actor.</summary>
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
                    candidate.Length <= ActivatableConstant.ActivatedBy.MaxLength.Value,
                error: ActivatableConstant.ActivatedBy.MaxLength.Error)
            .Ensure(
                predicate: candidate => !required || !string.IsNullOrWhiteSpace(candidate),
                error: ActivatableConstant.ActivatedBy.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    Regex.IsMatch(candidate, ActivatableConstant.ActivatedBy.Format.Pattern),
                error: ActivatableConstant.ActivatedBy.Format.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length >= ActivatableConstant.ActivatedBy.MinLength.Value,
                error: ActivatableConstant.ActivatedBy.MinLength.Error);
    }

    #endregion ActivatedBy

    #endregion

    #region Composition

    /// <summary>
    /// Validates the activation metadata that
    /// <see cref="ActivatableExtensions.Activate{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="nowUtc">The activation timestamp.</param>
    /// <param name="actor">The activating actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateActivation<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IActivatable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateActivatedAt(nowUtc)
                    .Bind(_ => ValidateActivationState(entity))
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    /// <summary>
    /// Validates the deactivation metadata that
    /// <see cref="ActivatableExtensions.Deactivate{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="actor">The deactivating actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateDeactivation<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IActivatable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateDeactivationState(entity)
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    #endregion
}
