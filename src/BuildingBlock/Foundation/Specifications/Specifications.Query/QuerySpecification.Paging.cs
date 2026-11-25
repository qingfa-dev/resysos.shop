namespace BuildingBlock.Specifications.Query;

/// <summary>Paging mapping for <see cref="QuerySpecificationMapper"/>.</summary>
public partial class QuerySpecificationMapper
{
    #region Paging

    private static Result<ISpecification<T>> ApplyPaging<T>(
        ISpecification<T> specification,
        PagedRequest? paging)
        where T : class
        => QuerySpecificationGuard.PagingRequired(paging)
            .Bind(valid => specification.WithPaging(valid.Page, valid.PageSize));

    #endregion
}
