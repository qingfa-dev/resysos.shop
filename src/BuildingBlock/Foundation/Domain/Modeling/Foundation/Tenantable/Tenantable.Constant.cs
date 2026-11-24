namespace BuildingBlock.Domain.Modeling.Foundation.Tenantable;

/// <summary>Unified constants and error definitions for the Tenantable concern.</summary>
public static partial class TenantableConstant
{
    #region Constraints

    /// <summary>Constraints and error definitions for the tenantable entity.</summary>
    public static class Entity
    {
        /// <summary>Error when the entity itself is missing.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "tenantable.entity.required",
                message: "Tenantable entity is required.");
        }
    }

    /// <summary>Constraints and error definitions for the tenant identifier.</summary>
    public static class Tenant
    {
        /// <summary>Error when the tenant identifier is the default value.</summary>
        public static class Required
        {
            public static readonly Error Error = Error.Validation(
                code: "tenantable.tenant_id.required",
                message: "Tenant identifier must be specified (default values are not allowed).");
        }
    }

    #endregion

    #region Defaults

    /// <summary>Default values for the Tenantable concern.</summary>
    public static class Defaults
    {
        /// <summary>Empty tenant identifier sentinel for <see cref="Guid"/> keys.</summary>
        public static readonly Guid EmptyTenantId = Guid.Empty;
    }

    #endregion
}
