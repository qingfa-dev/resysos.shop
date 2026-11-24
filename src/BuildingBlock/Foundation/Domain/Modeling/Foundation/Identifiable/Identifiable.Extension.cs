namespace BuildingBlock.Domain.Modeling.Foundation.Identifiable;

/// <summary>Extension methods for <see cref="IIdentifiable{TKey}"/>.</summary>
public static class IdentifiableExtensions
{
    /// <summary>Validates that the result's entity carries a non-default identifier.</summary>
    /// <typeparam name="TValue">The type of the result value.</typeparam>
    /// <typeparam name="TKey">The type of the identifier.</typeparam>
    /// <param name="result">The result to validate.</param>
    /// <returns>The same result if identified; otherwise a failure.</returns>
    public static Result<TValue> EnsureIdentified<TValue, TKey>(
        this Result<TValue> result)
        where TValue : IIdentifiable<TKey>
    {
        return result.Bind(entity =>
            IdentifiableValidator.ValidateIdentification<TValue, TKey>(entity));
    }

    
}