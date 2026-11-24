namespace BuildingBlock.Contracts.Querying;

/// <summary>Guards for query requests. Failures stay on the railway.</summary>
public static class QueryGuard
{
    #region Request

    /// <summary>Checks that an assembled query request is present and complete.</summary>
    /// <param name="request">The candidate request.</param>
    /// <returns>The request, or a validation error.</returns>
    public static Result<QueryRequest> Complete(QueryRequest? request)
        => request is null
            ? Result.Failure<QueryRequest>(QueryConstant.Constraints.Request.Required.Error)
            : Result.Success(request);

    #endregion
}
