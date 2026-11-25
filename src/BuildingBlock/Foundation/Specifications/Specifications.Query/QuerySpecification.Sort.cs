namespace BuildingBlock.Specifications.Query;

/// <summary>Sort mapping for <see cref="QuerySpecificationMapper"/>.</summary>
public partial class QuerySpecificationMapper
{
    #region Sorting

    private static Result<ISpecification<T>> ApplySorts<T>(
        ISpecification<T> specification,
        IReadOnlyList<SortDescriptor> sorts)
        where T : class
    {
        ISpecification<T> current = specification;

        foreach (SortDescriptor sort in sorts)
        {
            if (!Enum.IsDefined(sort.Direction))
            {
                return Result.Failure<ISpecification<T>>(
                    QuerySpecificationConstant.Constraints.Sort.Direction.Invalid.For(
                        sort.Direction));
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var path = QueryFieldResolver.Resolve<T>(sort.Field, parameter);
            if (path.IsFailure)
            {
                return Result.Failure<ISpecification<T>>(path.Errors);
            }

            Expression key = QueryFieldResolver.SafeMember(path.Value);
            var selector = Expression.Lambda<Func<T, object>>(
                Expression.Convert(key, typeof(object)),
                parameter);

            var ordered = current.OrderBy(
                selector,
                sort.Direction is SortDirection.Descending);

            if (ordered.IsFailure)
            {
                return Result.Failure<ISpecification<T>>(ordered.Errors);
            }

            current = ordered.Value;
        }

        return Result.Success(current);
    }

    #endregion
}
