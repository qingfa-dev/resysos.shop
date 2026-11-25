namespace BuildingBlock.Specifications.Query;

/// <summary>Convenience mapping from query contracts to specifications.</summary>
public static class QuerySpecificationExtensions
{
    #region Mapping

    /// <summary>
    /// Maps a query request to a specification. Filters become criteria, sorts become
    /// ordering and paging becomes skip/take. Failures stay on the railway.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="request">The query request.</param>
    /// <param name="mapper">An optional mapper; defaults to the shared mapper.</param>
    /// <returns>The mapped specification, or validation errors.</returns>
    public static Result<ISpecification<T>> ToSpecification<T>(
        this QueryRequest? request,
        IQuerySpecificationMapper? mapper = null)
        where T : class
        => QuerySpecificationGuard
            .MapperRequired(mapper ?? QuerySpecificationMapper.Instance)
            .Bind(valid => valid.Map<T>(request));

    /// <summary>
    /// Parses raw query parameters and maps them to a specification in one step.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="parameters">The raw query parameters.</param>
    /// <param name="mapper">An optional mapper; defaults to the shared mapper.</param>
    /// <returns>The mapped specification, or validation errors.</returns>
    public static Result<ISpecification<T>> ToSpecification<T>(
        this QueryParameters? parameters,
        IQuerySpecificationMapper? mapper = null)
        where T : class
        => QuerySpecificationGuard.ParametersRequired(parameters)
            .Bind(valid => valid.ToRequest()
                .Bind(request => request.ToSpecification<T>(mapper)));

    #endregion
}
