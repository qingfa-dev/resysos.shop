namespace BuildingBlock.Querying.InMemory;

/// <summary>Applies a compiled query plan to an in-memory sequence.</summary>
public interface IInMemoryQueryPlanExecutor
{
    #region Build

    /// <summary>Filters, orders and pages the source sequence according to the plan.</summary>
    /// <typeparam name="T">The entity type the plan targets.</typeparam>
    /// <param name="source">The sequence the plan is applied to.</param>
    /// <param name="plan">The compiled plan to apply.</param>
    /// <returns>The composed sequence, or a validation error.</returns>
    Result<IEnumerable<T>> Build<T>(IEnumerable<T> source, QueryPlan<T> plan);

    #endregion
}
