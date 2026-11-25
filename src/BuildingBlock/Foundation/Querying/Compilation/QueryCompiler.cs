namespace BuildingBlock.Querying.Compilation;

/// <summary>
/// The orchestrator that turns a <see cref="QueryRequest"/> into an immutable
/// <see cref="QueryPlan{T}"/>. It validates the request against the policy's security gate, then
/// compiles the filter into a predicate and the sorts into definitions, and maps the paging
/// request into an offset plan with an overflow-safe <c>Skip</c>/<c>Take</c>. Every failure stays
/// on the railway; compilation never throws.
/// </summary>
public static class QueryCompiler
{
    #region Compile

    /// <summary>Compiles a request into a validated query plan for an entity type.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="request">The request to compile, or <see langword="null"/> for a match-all plan.</param>
    /// <param name="policy">The policy supplying the field allowlist and caps.</param>
    /// <param name="context">The caller context, reserved for role-scoped grants.</param>
    /// <returns>The plan, or the first validation error encountered.</returns>
    public static Result<QueryPlan<T>> Compile<T>(
        QueryRequest? request,
        QueryPolicy policy,
        QueryContext context)
    {
        request ??= new QueryRequest();
        policy ??= new QueryPolicy();

        FilterNode? filter = ResolveFilter(request);

        var validated = QuerySecurity.Validate(
            filter,
            request.Sorts,
            policy,
            typeof(T),
            context);

        if (validated.IsFailure)
        {
            return Result.Failure<QueryPlan<T>>(validated.Errors);
        }

        var compiledFilter = QueryFilterCompiler.Compile<T>(filter, policy.Registry, context);
        if (compiledFilter.IsFailure)
        {
            return Result.Failure<QueryPlan<T>>(compiledFilter.Errors);
        }

        var sorts = QuerySortCompiler.Compile<T>(request.Sorts, policy.Registry, context);
        if (sorts.IsFailure)
        {
            return Result.Failure<QueryPlan<T>>(sorts.Errors);
        }

        var selectors = QuerySortCompiler.Selectors<T>(sorts.Value, policy.Registry);
        if (selectors.IsFailure)
        {
            return Result.Failure<QueryPlan<T>>(selectors.Errors);
        }

        return Result.Success(new QueryPlan<T>
        {
            Filter = compiledFilter.Value,
            Sorts = selectors.Value,
            Paging = Offset(request.Paging, policy),
            // Offset paging always reports a total count, and the executor gates counting on it.
            IncludeTotalCount = QueryCompilerConstant.Defaults.IncludeTotalCount
        });
    }

    #endregion

    #region Filter

    private static FilterNode? ResolveFilter(QueryRequest request)
    {
        if (request.Filter is not null)
        {
            return request.Filter;
        }

        if (request.Filters is null || request.Filters.Count == 0)
        {
            return null;
        }

        List<FilterNode> conditions = [];
        foreach (FilterDescriptor descriptor in request.Filters)
        {
            if (descriptor is null)
            {
                continue;
            }

            conditions.Add(new FilterCondition
            {
                Field = descriptor.Field,
                Operator = descriptor.Operator,
                Value = descriptor.Value,
                Values = descriptor.Values
            });
        }

        return conditions.Count switch
        {
            0 => null,
            1 => conditions[0],
            _ => new FilterGroup { Operator = FilterGroupOperator.And, Children = conditions }
        };
    }

    #endregion

    #region Paging

    private static PagingPlan Offset(PagedRequest? paging, QueryPolicy policy)
    {
        int maxPageSize = policy.MaxPageSize >= QueryPolicyConstant.Constraints.MaxPageSize.Min
            ? policy.MaxPageSize
            : QueryCompilerConstant.Constraints.PageSize.Max;

        int defaultPageSize = policy.DefaultPageSize >= QueryPolicyConstant.Constraints.DefaultPageSize.Min
            ? policy.DefaultPageSize
            : QueryCompilerConstant.Defaults.DefaultPageSize;

        int pageSize = Math.Clamp(
            paging?.PageSize ?? defaultPageSize,
            QueryCompilerConstant.Constraints.PageSize.Min,
            maxPageSize);
        int page = Math.Max(
            paging?.Page ?? QueryCompilerConstant.Constraints.Page.Min,
            QueryCompilerConstant.Constraints.Page.Min);

        long skip = (long)(page - 1) * pageSize;
        int clampedSkip = skip > int.MaxValue ? int.MaxValue : (int)skip;

        return new PagingPlan.Offset(clampedSkip, pageSize);
    }

    #endregion
}
