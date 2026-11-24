namespace BuildingBlock.Functional.Results;

/// <summary>Transforms failure errors without changing success values.</summary>
public interface IResultErrorMapping<T> : IResult<T>
{
    Result<T> MapError(Func<Error, Error> mapper);
}

public partial class Result<T>
{
    public Result<T> MapError(Func<Error, Error> mapper)
    {
        ResultGuard.ParameterRequired(mapper, nameof(mapper));

        if (IsSuccess)
            return this;

        var mappedErrors = Errors.Select(mapper).ToArray();
        return Failure(mappedErrors);
    }
}
