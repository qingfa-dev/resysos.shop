namespace BuildingBlock.Domain.Modeling.Foundation.StoreScoped;

/// <summary>Validates store-scoping state for <see cref="IStoreScoped{TStoreKey}"/> entities.</summary>
public static class StoreScopedValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the store-scoped entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TStoreKey">The type of the store identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TStoreKey>(
        TValue auditable)
        where TValue : IStoreScoped<TStoreKey>
    {
        return auditable is null
            ? Result<TValue>.Failure(StoreScopedConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Store

    /// <summary>Validates that the entity carries a non-default store identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TStoreKey">The type of the store identifier.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the store-id-required error.</returns>
    public static Result<TValue> ValidateScope<TValue, TStoreKey>(TValue entity)
        where TValue : IStoreScoped<TStoreKey>
    {
        return !EqualityComparer<TStoreKey>.Default.Equals(entity.StoreId, default!)
               && !IsEmptyStoreId(entity.StoreId)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(StoreScopedConstant.Store.Required.Error);
    }

    /// <summary>Validates a candidate store identifier value.</summary>
    /// <typeparam name="TStoreKey">The type of the store identifier.</typeparam>
    /// <param name="storeId">The store identifier value to validate.</param>
    /// <returns>The identifier on success; otherwise the store-id-required error.</returns>
    public static Result<TStoreKey> ValidateStoreIdValue<TStoreKey>(
        TStoreKey storeId)
    {
        return storeId is not null
               && !EqualityComparer<TStoreKey>.Default.Equals(storeId, default!)
               && !IsEmptyStoreId(storeId)
            ? Result<TStoreKey>.Success(storeId)
            : Result<TStoreKey>.Failure(
                StoreScopedConstant.Store.Required.Error);
    }

    #endregion Store

    #endregion

    #region Composition

    /// <summary>Validates that the entity carries a non-default store identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TStoreKey">The type of the store identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>Success if store-scoped; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateStoreScope<TValue, TStoreKey>(
        TValue auditable)
        where TValue : IStoreScoped<TStoreKey>
    {
        return ValidateEntity<TValue, TStoreKey>(auditable)
            .Bind(entity => ValidateScope<TValue, TStoreKey>(entity));
    }

    /// <summary>Validates the store identifier that
    /// <see cref="StoreScopedExtensions.SetStoreId{TValue, TStoreKey}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TStoreKey">The type of the store identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="storeId">The store identifier value to validate.</param>
    /// <returns>Success if usable; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateStoreId<TValue, TStoreKey>(
        TValue auditable,
        TStoreKey storeId)
        where TValue : IStoreScoped<TStoreKey>
    {
        return ValidateEntity<TValue, TStoreKey>(auditable)
            .Bind(entity =>
                ValidateStoreIdValue(storeId).Map(_ => entity));
    }

    #endregion

    #region Private Methods

    /// <summary>Checks <see cref="Guid.Empty"/> and blank-string sentinels for the store identifier.</summary>
    private static bool IsEmptyStoreId<TStoreKey>(TStoreKey storeId) =>
        (storeId is Guid guid && guid == StoreScopedConstant.Defaults.EmptyStoreId)
        || (storeId is string text && string.IsNullOrWhiteSpace(text));

    #endregion
}
