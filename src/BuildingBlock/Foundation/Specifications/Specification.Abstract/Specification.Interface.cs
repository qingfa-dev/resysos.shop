namespace BuildingBlock.Specifications.Abstractions;

/// <summary>
/// Interface for the Specification pattern.
/// Encapsulates business rules as reusable, composable predicates.
/// </summary>
/// <typeparam name="T">The type of entity the specification applies to.</typeparam>
public interface ISpecification<T>
{
    #region Query Definition

    /// <summary>
    /// Gets the predicate expression for querying.
    /// </summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>
    /// Gets the include expressions for eager loading (EF Core).
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// Gets the include expressions for eager loading string-based paths.
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Gets the ordering expressions.
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool Descending)> OrderBy { get; }

    #endregion
    #region Paging

    /// <summary>
    /// Gets the number of records to skip (for paging).
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Gets the number of records to take (for paging).
    /// </summary>
    int? Take { get; }

    #endregion
    #region Options

    /// <summary>
    /// Gets whether paging is enabled.
    /// </summary>
    bool IsPagingEnabled { get; }

    /// <summary>
    /// Gets whether the specification is for tracking (EF Core change tracking).
    /// </summary>
    bool IsTrackingEnabled { get; }

    #endregion
    #region Evaluation

    /// <summary>
    /// Evaluates the specification against an entity in memory.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns>True if the entity satisfies the specification.</returns>
    bool IsSatisfiedBy(T entity);
    #endregion
}
