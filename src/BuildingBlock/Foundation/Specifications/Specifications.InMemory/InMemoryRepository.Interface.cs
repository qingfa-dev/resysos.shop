namespace BuildingBlock.Specifications.InMemory;

/// <summary>
/// In-memory repository contract. Extends the railway repository with test-friendly seeding.
/// </summary>
public interface IInMemoryRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    /// <summary>Inserts or replaces an aggregate without returning an error for duplicates.</summary>
    Result<TAggregate> Seed(TAggregate aggregate);

    /// <summary>Removes every aggregate from the store.</summary>
    Result<Unit> Clear();
}
