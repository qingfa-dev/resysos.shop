namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Base implementation of the Specification pattern with composition support.
/// Instances are built through the Result-returning factory helpers; the
/// constructor is protected so creation always passes through validation.
/// </summary>
/// <typeparam name="T">The type of entity the specification applies to.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = new();
    private readonly List<string> _includeStrings = new();
    private readonly List<(Expression<Func<T, object>> KeySelector, bool Descending)> _orderBy = new();

    #region Construction

    /// <summary>
    /// Protected so only derived specifications and the factory helpers can create instances.
    /// </summary>
    protected Specification()
    {
    }

    #endregion
    #region State

    /// <inheritdoc />
    public abstract Expression<Func<T, bool>> Criteria { get; }

    /// <inheritdoc />
    public List<Expression<Func<T, object>>> Includes => _includes;

    /// <inheritdoc />
    public List<string> IncludeStrings => _includeStrings;

    /// <inheritdoc />
    public List<(Expression<Func<T, object>> KeySelector, bool Descending)> OrderBy => _orderBy;

    /// <inheritdoc />
    public int? Skip { get; private set; }

    /// <inheritdoc />
    public int? Take { get; private set; }

    /// <inheritdoc />
    public bool IsPagingEnabled { get; private set; }

    /// <inheritdoc />
    public bool IsTrackingEnabled { get; private set; } = true;

    #endregion
    #region Mutators

    /// <summary>
    /// Adds an include expression for eager loading.
    /// </summary>
    protected internal void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        _includes.Add(includeExpression);
    }

    /// <summary>
    /// Adds a string-based include path for eager loading.
    /// </summary>
    protected internal void AddInclude(string includeString)
    {
        _includeStrings.Add(includeString);
    }

    /// <summary>
    /// Adds an ordering expression (ascending).
    /// </summary>
    protected internal void AddOrderBy(Expression<Func<T, object>> keySelector)
    {
        _orderBy.Add((keySelector, false));
    }

    /// <summary>
    /// Adds an ordering expression (descending).
    /// </summary>
    protected internal void AddOrderByDescending(Expression<Func<T, object>> keySelector)
    {
        _orderBy.Add((keySelector, true));
    }

    /// <summary>
    /// Enables paging with skip and take.
    /// </summary>
    protected internal void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    /// <summary>
    /// Disables change tracking for read-only queries.
    /// </summary>
    protected internal void DisableTracking()
    {
        IsTrackingEnabled = false;
    }

    #endregion
    #region Evaluation

    /// <inheritdoc />
    public bool IsSatisfiedBy(T entity)
    {
        var compiled = Criteria.Compile();
        return compiled(entity);
    }

    #endregion
    #region Composition

    /// <summary>
    /// Combines this specification with another using AND.
    /// </summary>
    public Result<Specification<T>> And(ISpecification<T> other)
        => AndSpecification<T>.Create(this, other);

    /// <summary>
    /// Combines this specification with another using OR.
    /// </summary>
    public Result<Specification<T>> Or(ISpecification<T> other)
        => OrSpecification<T>.Create(this, other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    public Result<Specification<T>> Not()
        => NotSpecification<T>.Create(this);

    #endregion
    #region Operators

    /// <summary>
    /// Combines this specification with another using AND (operator overload).
    /// </summary>
    public static Result<Specification<T>> operator &(Specification<T> left, ISpecification<T> right)
        => SpecificationGuard.NotNull(left, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(valid => AndSpecification<T>.Create(valid, right));

    /// <summary>
    /// Combines this specification with another using OR (operator overload).
    /// </summary>
    public static Result<Specification<T>> operator |(Specification<T> left, ISpecification<T> right)
        => SpecificationGuard.NotNull(left, SpecificationConstant.Constraints.Specification.Required.Error)
            .Bind(valid => OrSpecification<T>.Create(valid, right));

    /// <summary>
    /// Negates this specification (operator overload).
    /// </summary>
    public static Result<Specification<T>> operator !(Specification<T> spec)
        => NotSpecification<T>.Create(spec);
    #endregion
}
