namespace BuildingBlock.Domain.Modeling.Foundation.Versionable;

/// <summary>Validates version state for <see cref="IVersionable"/> entities.</summary>
public static class VersionableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the versionable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IVersionable
    {
        return auditable is null
            ? Result<TValue>.Failure(VersionableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Version

    /// <summary>Validates that the entity's version can be incremented.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the first version failure.</returns>
    public static Result<TValue> ValidateBumpState<TValue>(TValue entity)
        where TValue : IVersionable
    {
        return Result<TValue>.Success(entity)
            .Ensure(
                predicate: candidate =>
                    candidate.Version >= VersionableConstant.Version.Min.Value,
                error: VersionableConstant.Version.Min.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Version <=
                    VersionableConstant.Version.Max.Value -
                    VersionableConstant.Defaults.Increment,
                error: VersionableConstant.Version.Max.Error);
    }

    /// <summary>Validates a candidate version value against the allowed range.</summary>
    /// <param name="version">The version value to validate.</param>
    /// <returns>The version on success; otherwise the out-of-range error.</returns>
    public static Result<long> ValidateVersionValue(long version)
    {
        return Result<long>.Success(version)
            .Ensure(
                predicate: candidate =>
                    candidate >= VersionableConstant.Version.Min.Value &&
                    candidate <= VersionableConstant.Version.Max.Value,
                error: VersionableConstant.Version.Range.Error);
    }

    /// <summary>Validates that the entity's version matches the expected version.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <param name="expectedVersion">The version the caller expects.</param>
    /// <returns>The entity on success; otherwise a concurrency-conflict error.</returns>
    public static Result<TValue> ValidateConcurrencyState<TValue>(
        TValue entity,
        long expectedVersion)
        where TValue : IVersionable
    {
        return entity.Version == expectedVersion
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                VersionableConstant.Version.Conflict.Concurrency(
                    expectedVersion,
                    entity.Version));
    }

    #endregion Version

    #endregion

    #region Composition

    /// <summary>Validates that the entity's version can be incremented,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>Success if the version can be incremented; otherwise a failure.</returns>
    public static Result<TValue> ValidateBump<TValue>(TValue auditable)
        where TValue : IVersionable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateBumpState(entity));
    }

    /// <summary>Validates a candidate version value against the allowed range,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="version">The version value to validate.</param>
    /// <returns>Success if in range; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateVersion<TValue>(
        TValue auditable,
        long version)
        where TValue : IVersionable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateVersionValue(version).Map(_ => entity));
    }

    /// <summary>Validates optimistic concurrency by comparing the entity's version
    /// against the expected version, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="expectedVersion">The version the caller expects.</param>
    /// <returns>Success on a match; otherwise a concurrency failure.</returns>
    public static Result<TValue> ValidateConcurrency<TValue>(
        TValue auditable,
        long expectedVersion)
        where TValue : IVersionable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateConcurrencyState(entity, expectedVersion));
    }

    #endregion
}
