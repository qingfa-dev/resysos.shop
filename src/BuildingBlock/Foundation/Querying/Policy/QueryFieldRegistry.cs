namespace BuildingBlock.Querying.Policy;

/// <summary>
/// The authoritative allowlist of queryable fields. It maps an external field name to the
/// member accessor that backs it, plus its filter/sort/select/search grants.
/// </summary>
/// <remarks>
/// Resolution never falls back to reflection: a field that was not registered is always
/// rejected with <c>query.security.field.not_allowed</c>. Field names match case-insensitively.
/// Attribute-driven registration lives in <c>QueryFieldRegistry.Attribute</c>.
/// </remarks>
public sealed partial class QueryFieldRegistry
{
    #region State

    private readonly Dictionary<Type, Dictionary<string, QueryField>> _fields = new();

    #endregion

    #region Registration

    /// <summary>Registers a queryable field for an entity type.</summary>
    /// <typeparam name="T">The entity type the field belongs to.</typeparam>
    /// <param name="name">The external field name used by clients.</param>
    /// <param name="accessor">The member access that backs the field.</param>
    /// <param name="filterable">The filter operators the field allows.</param>
    /// <param name="sortable">Whether the field may be used to order results.</param>
    /// <param name="selectable">Whether the field may be projected.</param>
    /// <param name="searchable">Whether the field participates in free-text search.</param>
    public void Add<T>(
        string name,
        Expression<Func<T, object>> accessor,
        FilterOperators filterable = FilterOperators.None,
        bool sortable = false,
        bool selectable = true,
        bool searchable = false)
    {
        if (accessor is null)
        {
            return;
        }

        Add(typeof(T), name, Unbox(accessor), filterable, sortable, selectable, searchable);
    }

    /// <summary>Registers a queryable field for an entity type.</summary>
    /// <param name="entityType">The entity type the field belongs to.</param>
    /// <param name="name">The external field name used by clients.</param>
    /// <param name="accessor">The member access that backs the field.</param>
    /// <param name="filterable">The filter operators the field allows.</param>
    /// <param name="sortable">Whether the field may be used to order results.</param>
    /// <param name="selectable">Whether the field may be projected.</param>
    /// <param name="searchable">Whether the field participates in free-text search.</param>
    public void Add(
        Type entityType,
        string name,
        LambdaExpression accessor,
        FilterOperators filterable = FilterOperators.None,
        bool sortable = false,
        bool selectable = true,
        bool searchable = false)
    {
        if (entityType is null || string.IsNullOrWhiteSpace(name) || accessor is null)
        {
            return;
        }

        if (!_fields.TryGetValue(entityType, out var byName))
        {
            byName = new Dictionary<string, QueryField>(StringComparer.OrdinalIgnoreCase);
            _fields[entityType] = byName;
        }

        byName[name] = new QueryField(name, accessor, filterable, sortable, selectable, searchable);
    }

    #endregion

    #region Resolution

    /// <summary>Resolves a field name to its registered entry for an entity type.</summary>
    /// <param name="entityType">The entity type the field belongs to.</param>
    /// <param name="fieldName">The external field name, matched case-insensitively.</param>
    /// <returns>The field, or <c>query.security.field.not_allowed</c> when it is not registered.</returns>
    public Result<QueryField> Resolve(Type entityType, string fieldName)
    {
        if (entityType is not null
            && !string.IsNullOrWhiteSpace(fieldName)
            && _fields.TryGetValue(entityType, out var byName)
            && byName.TryGetValue(fieldName, out var field))
        {
            return Result.Success(field);
        }

        return Result.Failure<QueryField>(
            QueryFieldRegistryConstant.Constraints.Field.NotAllowed(fieldName));
    }

    #endregion

    #region Helpers

    private static LambdaExpression Unbox(LambdaExpression accessor)
        => accessor.Body is UnaryExpression { NodeType: ExpressionType.Convert, Method: null } convert
            && convert.Type == typeof(object)
            && convert.Operand.Type != typeof(object)
                ? Expression.Lambda(convert.Operand, accessor.Parameters)
                : accessor;

    #endregion
}
