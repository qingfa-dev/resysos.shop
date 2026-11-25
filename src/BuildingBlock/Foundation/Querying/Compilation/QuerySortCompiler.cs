namespace BuildingBlock.Querying.Compilation;

/// <summary>
/// Compiles sort clauses for the query engine. <see cref="Compile{T}"/> resolves client sort
/// descriptors through the field registry into <see cref="SortDefinition"/>s, and
/// <see cref="Selectors{T}"/> turns those definitions into resolved, null-safe
/// <see cref="QuerySort{T}"/> orderings built from the registered accessors. Failures stay on the
/// railway; the compiler never throws.
/// </summary>
public static class QuerySortCompiler
{
    #region Definitions

    /// <summary>Resolves sort descriptors through the registry into plan sort definitions.</summary>
    /// <typeparam name="T">The entity type the sorts target.</typeparam>
    /// <param name="sorts">The sort descriptors as the client sent them.</param>
    /// <param name="registry">The field allowlist the sorts are resolved against.</param>
    /// <param name="context">The caller context, reserved for role-scoped grants.</param>
    /// <returns>The sort definitions, or the first validation error encountered.</returns>
    /// <remarks>
    /// This compiler does not enforce the policy's security gate (including the per-field
    /// <c>Sortable</c> flag). Callers must run <see cref="QuerySecurity.Validate"/> first; the
    /// <see cref="QueryCompiler"/> facade does.
    /// </remarks>
    public static Result<IReadOnlyList<SortDefinition>> Compile<T>(
        IReadOnlyList<SortDescriptor> sorts,
        QueryFieldRegistry registry,
        QueryContext context)
    {
        if (sorts is null || sorts.Count == 0)
        {
            return Result.Success<IReadOnlyList<SortDefinition>>([]);
        }

        List<SortDefinition> definitions = new(sorts.Count);

        foreach (SortDescriptor sort in sorts)
        {
            if (sort is null)
            {
                continue;
            }

            if (registry is null)
            {
                return Result.Failure<IReadOnlyList<SortDefinition>>(
                    QueryFieldRegistryConstant.Constraints.Field.NotAllowed(sort.Field));
            }

            var resolved = registry.Resolve(typeof(T), sort.Field);
            if (resolved.IsFailure)
            {
                return Result.Failure<IReadOnlyList<SortDefinition>>(resolved.Errors);
            }

            definitions.Add(new SortDefinition(
                resolved.Value.Name,
                sort.Direction is SortDirection.Descending));
        }

        return Result.Success<IReadOnlyList<SortDefinition>>(definitions);
    }

    #endregion

    #region Selectors

    /// <summary>Builds a resolved, null-safe ordering for each sort definition.</summary>
    /// <typeparam name="T">The entity type the orderings target.</typeparam>
    /// <param name="sorts">The sort definitions to build orderings for.</param>
    /// <param name="registry">The field allowlist that backs the orderings' accessors.</param>
    /// <returns>
    /// One <see cref="QuerySort{T}"/> per definition, in order, or a validation error when a field
    /// no longer resolves to a registered member path.
    /// </returns>
    public static Result<IReadOnlyList<QuerySort<T>>> Selectors<T>(
        IReadOnlyList<SortDefinition> sorts,
        QueryFieldRegistry registry)
    {
        if (sorts is null || sorts.Count == 0)
        {
            return Result.Success<IReadOnlyList<QuerySort<T>>>([]);
        }

        List<QuerySort<T>> selectors = new(sorts.Count);

        foreach (SortDefinition sort in sorts)
        {
            if (sort is null)
            {
                continue;
            }

            if (registry is null)
            {
                return Result.Failure<IReadOnlyList<QuerySort<T>>>(
                    QueryFieldRegistryConstant.Constraints.Field.NotAllowed(sort.Field));
            }

            var resolved = registry.Resolve(typeof(T), sort.Field);
            if (resolved.IsFailure)
            {
                return Result.Failure<IReadOnlyList<QuerySort<T>>>(resolved.Errors);
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var path = QueryFieldPathResolver.Resolve<T>(resolved.Value.Accessor, parameter, sort.Field);
            if (path.IsFailure)
            {
                return Result.Failure<IReadOnlyList<QuerySort<T>>>(path.Errors);
            }

            Expression key = QueryFieldPathResolver.SafeMember(path.Value);
            var selector = Expression.Lambda<Func<T, object>>(
                Expression.Convert(key, typeof(object)),
                parameter);

            selectors.Add(new QuerySort<T>(selector, sort.Descending));
        }

        return Result.Success<IReadOnlyList<QuerySort<T>>>(selectors);
    }

    #endregion
}
