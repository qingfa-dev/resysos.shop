namespace BuildingBlock.Querying.Policy;

/// <summary>
/// Carries the caller identity, roles, and cancellation token threaded through query execution.
/// The engine falls back to <see cref="Anonymous"/> when no context is supplied.
/// </summary>
public sealed class QueryContext
{
    #region Construction

    private QueryContext(string? userId, IReadOnlyCollection<string> roles, CancellationToken cancellationToken)
    {
        UserId = userId;
        Roles = roles;
        CancellationToken = cancellationToken;
    }

    /// <summary>An anonymous caller: no identity, no roles, and no cancellation.</summary>
    public static QueryContext Anonymous { get; } =
        new(userId: null, roles: [], cancellationToken: CancellationToken.None);

    #endregion

    #region State

    /// <summary>Gets the caller's user identifier, or <see langword="null"/> when anonymous.</summary>
    public string? UserId { get; }

    /// <summary>Gets the roles granted to the caller; empty when none were supplied.</summary>
    public IReadOnlyCollection<string> Roles { get; }

    /// <summary>Gets the token used to cancel query execution.</summary>
    public CancellationToken CancellationToken { get; }

    #endregion

    #region Factory

    /// <summary>Creates a query context for a caller.</summary>
    /// <param name="userId">The caller's user identifier, or <see langword="null"/> when anonymous.</param>
    /// <param name="roles">The roles granted to the caller, or <see langword="null"/> when none.</param>
    /// <param name="ct">The token used to cancel query execution.</param>
    /// <returns>The context, or a validation error when a role is blank.</returns>
    public static Result<QueryContext> Create(
        string? userId,
        IEnumerable<string>? roles,
        CancellationToken ct = default)
    {
        var materialized = roles?.ToArray() ?? [];

        return materialized.Any(static role => string.IsNullOrWhiteSpace(role))
            ? Result.Failure<QueryContext>(QueryContextConstant.Constraints.Role.Required.Error)
            : Result.Success(new QueryContext(userId, Array.AsReadOnly(materialized), ct));
    }

    #endregion
}
