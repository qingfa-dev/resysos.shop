namespace BuildingBlock.Domain.Modeling.Foundation.Identifiable;

/// <summary>Validates identification state for <see cref="IIdentifiable{TKey}"/> entities.</summary>
public static class IdentifiableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the identifiable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TKey>(TValue auditable)
        where TValue : IIdentifiable<TKey>
    {
        return auditable is null
            ? Result<TValue>.Failure(IdentifiableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Identity

    /// <summary>Validates a candidate identifier value.</summary>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="id">The identifier value to validate.</param>
    /// <returns>The identifier on success; otherwise the id-required error.</returns>
    public static Result<TKey> ValidateIdValue<TKey>(TKey id)
    {
        if (id is null)
        {
            return Result<TKey>.Failure(
                IdentifiableConstant.Id.Required.Error);
        }

        return Result<TKey>.Success(id)
            .Ensure(
                predicate: candidate =>
                    !EqualityComparer<TKey>.Default.Equals(
                        candidate,
                        default!)
                    && !IsEmptyId(candidate),
                error: IdentifiableConstant.Id.Required.Error);
    }

    /// <summary>Validates that the entity carries a non-default identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the id-required error.</returns>
    public static Result<TValue> ValidateIdentity<TValue, TKey>(TValue entity)
        where TValue : IIdentifiable<TKey>
    {
        return ValidateIdValue(entity.Id)
            .Map(_ => entity);
    }

    #endregion Identity

    #endregion

    #region Composition

    /// <summary>Validates that the entity carries a non-default identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>Success if identified; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateIdentification<TValue, TKey>(
        TValue auditable)
        where TValue : IIdentifiable<TKey>
    {
        return ValidateEntity<TValue, TKey>(auditable)
            .Bind(entity => ValidateIdentity<TValue, TKey>(entity));
    }

    /// <summary>Validates a candidate identifier value without an entity.</summary>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="id">The identifier value to validate.</param>
    /// <returns>Success if usable; otherwise a failure with the appropriate error.</returns>
    public static Result<TKey> ValidateId<TKey>(TKey id)
    {
        return ValidateIdValue(id);
    }

    #endregion

    #region Private Methods

    /// <summary>Checks <see cref="Guid.Empty"/> and blank-string sentinels for the identifier.</summary>
    private static bool IsEmptyId<TKey>(TKey id) =>
        (id is Guid guid && guid == IdentifiableConstant.Defaults.EmptyId)
        || (id is string text && string.IsNullOrWhiteSpace(text));

    #endregion
}
