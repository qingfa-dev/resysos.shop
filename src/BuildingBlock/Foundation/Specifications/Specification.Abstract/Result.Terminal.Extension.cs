namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Terminal conversions: the only place where a railway failure becomes an
/// exception. Call these at the edge of the application, after every other
/// step has stayed on the result path.
/// </summary>
public static class ResultTerminalExtensions
{
    #region Terminal

    /// <summary>
    /// Returns the successful value, or throws the failure's error at the edge.
    /// </summary>
    public static T OrThrow<T>(this Result<T> result)
        => result.IsSuccess
            ? result.Value
            : throw result.Errors[0].ToException();
    #endregion
}
