using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

/// <summary>Validates creation operations for <see cref="ICreatable"/> entities.</summary>
public static class CreatableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the creatable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ICreatable
    {
        return auditable is null
            ? Result<TValue>.Failure(CreatableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region CreatedAt

    /// <summary>Validates the creation timestamp.</summary>
    /// <param name="nowUtc">The creation timestamp to validate.</param>
    /// <returns>The timestamp on success; otherwise the first failure.</returns>
    public static Result<DateTimeOffset> ValidateCreatedAt(
        DateTimeOffset nowUtc)
    {
        return nowUtc == default
            ? Result<DateTimeOffset>.Failure(
                CreatableConstant.CreatedAt.Required.Error)
            : Result<DateTimeOffset>.Success(nowUtc)
                .Ensure(
                    predicate: candidate =>
                        candidate <= DateTimeOffset.UtcNow.AddSeconds(
                            LifecycleConstant.Timestamp.MaxFutureSkew.Value),
                    error: CreatableConstant.CreatedAt.Future.Error);
    }

    /// <summary>Validates that creation metadata has not been set yet.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the already-initialized error.</returns>
    public static Result<TValue> ValidateInitialization<TValue>(TValue entity)
        where TValue : ICreatable
    {
        return entity.CreatedAtUtc == default
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                CreatableConstant.CreatedAt.AlreadyInitialized.Error);
    }

    #endregion CreatedAt

    #region CreatedBy

    /// <summary>Validates the creating actor.</summary>
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
                    candidate.Length <= CreatableConstant.CreatedBy.MaxLength.Value,
                error: CreatableConstant.CreatedBy.MaxLength.Error)
            .Ensure(
                predicate: candidate => !required || !string.IsNullOrWhiteSpace(candidate),
                error: CreatableConstant.CreatedBy.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    Regex.IsMatch(candidate, CreatableConstant.CreatedBy.Format.Pattern),
                error: CreatableConstant.CreatedBy.Format.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length >= CreatableConstant.CreatedBy.MinLength.Value,
                error: CreatableConstant.CreatedBy.MinLength.Error);
    }

    #endregion CreatedBy

    #endregion

    #region Composition

    /// <summary>
    /// Validates the audit metadata that <see cref="CreatableExtensions.InitializeAudit{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="nowUtc">The creation timestamp.</param>
    /// <param name="actor">The creating actor.</param>
    /// <param name="requireCreatedBy">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateCreation<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireCreatedBy = LifecycleConstant.Defaults.RequireActor)
        where TValue : ICreatable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateCreatedAt(nowUtc)
                    .Bind(_ => ValidateInitialization(entity))
                    .Bind(_ => ValidateActor(actor, requireCreatedBy))
                    .Map(_ => entity));
    }

    #endregion
}
