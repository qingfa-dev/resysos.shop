namespace BuildingBlock.Querying.Policy;

/// <summary>Attribute-driven registration for <see cref="QueryFieldRegistry"/>.</summary>
public sealed partial class QueryFieldRegistry
{
    #region Attributes

    /// <summary>
    /// Registers every field of an entity type that the attributes expose under the given
    /// exposure mode. <see cref="QueryExposure.DenyByDefault"/> exposes only attributed
    /// properties; <see cref="QueryExposure.AllowByDefault"/> exposes every property except
    /// those marked <see cref="HiddenAttribute"/>.
    /// </summary>
    /// <param name="entityType">The entity type to read.</param>
    /// <param name="exposure">The exposure mode that decides the default.</param>
    public void AddFromAttributes(Type entityType, QueryExposure exposure)
    {
        if (entityType is null)
        {
            return;
        }

        foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is null)
            {
                continue;
            }

            if (property.IsDefined(typeof(HiddenAttribute), inherit: true))
            {
                continue;
            }

            var queryable = property.GetCustomAttribute<QueryableAttribute>(inherit: true);
            var filterable = property.GetCustomAttribute<FilterableAttribute>(inherit: true);
            bool sortable = property.IsDefined(typeof(SortableAttribute), inherit: true);
            bool searchable = property.IsDefined(typeof(SearchableAttribute), inherit: true);

            bool exposed = queryable is not null || filterable is not null || sortable || searchable;

            if (exposure is QueryExposure.DenyByDefault && !exposed)
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType, QueryPlanConstant.Defaults.ParameterName);
            var accessor = Expression.Lambda(Expression.Property(parameter, property), parameter);

            Add(
                entityType,
                property.Name,
                accessor,
                filterable?.Operators ?? FilterOperators.None,
                sortable,
                selectable: true,
                searchable);
        }
    }

    /// <summary>Registers the attributed fields of an entity type.</summary>
    /// <typeparam name="T">The entity type to read.</typeparam>
    /// <param name="exposure">The exposure mode that decides the default.</param>
    public void AddFromAttributes<T>(QueryExposure exposure)
        => AddFromAttributes(typeof(T), exposure);

    #endregion
}
