namespace BuildingBlock.Domain.Modeling.Foundation.Tenantable;

/// <summary>Validates tenancy state for <see cref="ITenantable{TTenantKey}"/> entities.</summary>
public static class TenantableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the tenantable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TTenantKey">The type of the tenant identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TTenantKey>(
        TValue auditable)
        where TValue : ITenantable<TTenantKey>
    {
        return auditable is null
            ? Result<TValue>.Failure(TenantableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Tenant

    /// <summary>Validates that the entity carries a non-default tenant identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TTenantKey">The type of the tenant identifier.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the tenant-id-required error.</returns>
    public static Result<TValue> ValidateTenant<TValue, TTenantKey>(
        TValue entity)
        where TValue : ITenantable<TTenantKey>
    {
        return !EqualityComparer<TTenantKey>.Default.Equals(
                   entity.TenantId,
                   default!)
               && !IsEmptyTenantId(entity.TenantId)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(TenantableConstant.Tenant.Required.Error);
    }

    /// <summary>Validates a candidate tenant identifier value.</summary>
    /// <typeparam name="TTenantKey">The type of the tenant identifier.</typeparam>
    /// <param name="tenantId">The tenant identifier value to validate.</param>
    /// <returns>The identifier on success; otherwise the tenant-id-required error.</returns>
    public static Result<TTenantKey> ValidateTenantIdValue<TTenantKey>(
        TTenantKey tenantId)
    {
        return tenantId is not null
               && !EqualityComparer<TTenantKey>.Default.Equals(tenantId, default!)
               && !IsEmptyTenantId(tenantId)
            ? Result<TTenantKey>.Success(tenantId)
            : Result<TTenantKey>.Failure(
                TenantableConstant.Tenant.Required.Error);
    }

    #endregion Tenant

    #endregion

    #region Composition

    /// <summary>Validates that the entity carries a non-default tenant identifier.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TTenantKey">The type of the tenant identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>Success if tenanted; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateTenancy<TValue, TTenantKey>(
        TValue auditable)
        where TValue : ITenantable<TTenantKey>
    {
        return ValidateEntity<TValue, TTenantKey>(auditable)
            .Bind(entity => ValidateTenant<TValue, TTenantKey>(entity));
    }

    /// <summary>Validates the tenant identifier that
    /// <see cref="TenantableExtensions.SetTenantId{TValue, TTenantKey}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <typeparam name="TTenantKey">The type of the tenant identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="tenantId">The tenant identifier value to validate.</param>
    /// <returns>Success if usable; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateTenantId<TValue, TTenantKey>(
        TValue auditable,
        TTenantKey tenantId)
        where TValue : ITenantable<TTenantKey>
    {
        return ValidateEntity<TValue, TTenantKey>(auditable)
            .Bind(entity =>
                ValidateTenantIdValue(tenantId).Map(_ => entity));
    }

    #endregion

    #region Private Methods

    /// <summary>Checks <see cref="Guid.Empty"/> and blank-string sentinels for the tenant identifier.</summary>
    private static bool IsEmptyTenantId<TTenantKey>(TTenantKey tenantId) =>
        (tenantId is Guid guid && guid == TenantableConstant.Defaults.EmptyTenantId)
        || (tenantId is string text && string.IsNullOrWhiteSpace(text));

    #endregion
}
