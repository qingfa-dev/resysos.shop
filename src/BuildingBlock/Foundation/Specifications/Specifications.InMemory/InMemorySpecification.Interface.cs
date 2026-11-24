namespace BuildingBlock.Specifications.InMemory;

/// <summary>Applies a specification to an in-memory sequence.</summary>
public interface IInMemorySpecificationEvaluator
{
    /// <summary>
    /// Filters, orders and pages the source sequence according to the specification.
    /// </summary>
    Result<IEnumerable<T>> Apply<T>(IEnumerable<T> source, ISpecification<T> specification);
}
