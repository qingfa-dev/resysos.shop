namespace BuildingBlock.Specifications.Query;

/// <summary>Maps transport-neutral query contracts to specifications.</summary>
public interface IQuerySpecificationMapper
{
    #region Mapping

    /// <summary>
    /// Maps a query request to a specification: filters become criteria, sorts become
    /// ordering and paging becomes skip/take.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="request">The query request.</param>
    /// <returns>The mapped specification, or validation errors.</returns>
    Result<ISpecification<T>> Map<T>(QueryRequest? request)
        where T : class;

    #endregion
}
