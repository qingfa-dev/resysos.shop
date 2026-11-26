using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Persistence.InMemory.Repositories;

/// <summary>
/// In-memory repository contract. Extends the railway repository with test-friendly seeding.
/// </summary>
/// <typeparam name="TAggregate">The aggregate type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public interface IInMemoryRepository<TAggregate, TId> : IRepository<TAggregate, TId>
    where TAggregate : class, IIdentifiable<TId>
    where TId : notnull
{
    /// <summary>Inserts or replaces an aggregate without returning an error for duplicates.</summary>
    /// <param name="aggregate">The aggregate to seed.</param>
    /// <returns>The seeded aggregate, or a validation error.</returns>
    Result<TAggregate> Seed(TAggregate aggregate);

    /// <summary>Removes every aggregate from the store.</summary>
    /// <returns>A success result.</returns>
    Result<Unit> Clear();
}
