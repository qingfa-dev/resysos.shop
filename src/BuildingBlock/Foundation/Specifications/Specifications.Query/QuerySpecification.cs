namespace BuildingBlock.Specifications.Query;

/// <summary>
/// Default mapper from <see cref="QueryRequest"/> to <see cref="ISpecification{T}"/>.
/// The mapper is stateless; <see cref="Instance"/> is shared by the extension helpers.
/// </summary>
public partial class QuerySpecificationMapper : IQuerySpecificationMapper
{
    #region Construction

    /// <summary>Protected so callers use <see cref="Instance"/> or supply their own mapper.</summary>
    protected QuerySpecificationMapper()
    {
    }

    #endregion

    #region Singleton

    /// <summary>Gets the shared stateless mapper.</summary>
    public static QuerySpecificationMapper Instance { get; } = new();

    #endregion

    #region Mapping

    /// <inheritdoc />
    public Result<ISpecification<T>> Map<T>(QueryRequest? request)
        where T : class
        => QuerySpecificationGuard.RequestRequired(request)
            .Bind(valid => BuildCriteria<T>(valid.Filters ?? [])
                .Bind(criteria => SpecificationExtensions.Create<T>(criteria))
                .Bind(specification => ApplySorts(specification, valid.Sorts ?? []))
                .Bind(specification => ApplyPaging(specification, valid.Paging)));

    #endregion
}
