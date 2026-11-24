namespace BuildingBlock.Domain.Modeling.Organization.Collectionable;

/// <summary>Validation logic for <see cref="ICollectionable{TCollection}"/>.</summary>
public static class CollectionableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the collectionable entity is present.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TCollection>(
        TValue auditable)
        where TValue : ICollectionable<TCollection>
    {
        return auditable is null
            ? Result<TValue>.Failure(CollectionableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Collection

    /// <summary>Validates a collection value.</summary>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>The normalized collection on success; otherwise the first failure.</returns>
    public static Result<TCollection> ValidateCollectionValue<TCollection>(
        TCollection collection)
    {
        if (collection is null)
        {
            return Result<TCollection>.Failure(
                CollectionableConstant.Collection.Required.Error);
        }

        if (collection is Guid guid && guid == Guid.Empty)
        {
            return Result<TCollection>.Failure(
                CollectionableConstant.Collection.Empty.Error);
        }

        var normalized = Normalize(collection);

        return Result<TCollection>.Success(normalized)
            .Ensure(
                predicate: candidate =>
                    candidate is not string name ||
                    !string.IsNullOrWhiteSpace(name),
                error: CollectionableConstant.Collection.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is not string name ||
                    name.Length <= CollectionableConstant.Collection.MaxLength.Value,
                error: CollectionableConstant.Collection.MaxLength.Error);
    }

    /// <summary>Validates that the collection is specified.</summary>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>The normalized collection on success; otherwise the required error.</returns>
    public static Result<TCollection> ValidateCollectionPresence<TCollection>(
        TCollection collection)
    {
        return collection is null
            ? Result<TCollection>.Failure(
                CollectionableConstant.Collection.Required.Error)
            : Result<TCollection>.Success(Normalize(collection));
    }

    /// <summary>Validates that the collection is not already assigned.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="collection">The collection to check.</param>
    /// <returns>The entity on success; otherwise the duplicate error.</returns>
    public static Result<TValue> ValidateCollectionUniqueness<TValue, TCollection>(
        TValue entity,
        TCollection collection)
        where TValue : ICollectionable<TCollection>
    {
        return entity.Collections.Contains(collection)
            ? Result<TValue>.Failure(CollectionableConstant.Collection.Duplicate.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the collection is assigned.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="collection">The collection to check.</param>
    /// <returns>The entity on success; otherwise the not-found error.</returns>
    public static Result<TValue> ValidateCollectionExists<TValue, TCollection>(
        TValue entity,
        TCollection collection)
        where TValue : ICollectionable<TCollection>
    {
        return entity.Collections.Contains(collection)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(CollectionableConstant.Collection.NotFound.Error);
    }

    #endregion Collection

    #region Capacity

    /// <summary>Validates that the entity can hold another collection.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <returns>The entity on success; otherwise the limit-exceeded error.</returns>
    public static Result<TValue> ValidateCapacity<TValue, TCollection>(
        TValue entity)
        where TValue : ICollectionable<TCollection>
    {
        return entity.Collections.Count < CollectionableConstant.Collection.MaxCount.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                CollectionableConstant.Collection.MaxCount.Error);
    }

    #endregion Capacity

    #endregion

    #region Composition

    /// <summary>
    /// Validates the collection that
    /// <see cref="CollectionableExtensions.AddCollection{TValue, TCollection}"/>
    /// is about to add, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateAddCollection<TValue, TCollection>(
        TValue auditable,
        TCollection collection)
        where TValue : ICollectionable<TCollection>
    {
        return ValidateEntity<TValue, TCollection>(auditable)
            .Bind(entity =>
                ValidateCollectionValue(collection)
                    .Bind(candidate =>
                        ValidateCollectionUniqueness(entity, candidate))
                    .Bind(_ => ValidateCapacity<TValue, TCollection>(entity)));
    }

    /// <summary>
    /// Validates that the collection that
    /// <see cref="CollectionableExtensions.RemoveCollection{TValue, TCollection}"/>
    /// targets is assigned, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCollection">The collection type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="collection">The collection to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateRemoveCollection<TValue, TCollection>(
        TValue auditable,
        TCollection collection)
        where TValue : ICollectionable<TCollection>
    {
        return ValidateEntity<TValue, TCollection>(auditable)
            .Bind(entity =>
                ValidateCollectionPresence(collection)
                    .Bind(candidate => ValidateCollectionExists(entity, candidate)));
    }

    #endregion

    #region Internal Methods

    /// <summary>Normalizes a collection value (trims strings when enabled).</summary>
    internal static TCollection Normalize<TCollection>(TCollection collection)
    {
        if (CollectionableConstant.Defaults.TrimValues
            && collection is string name)
        {
            return (TCollection)(object)name.Trim();
        }

        return collection;
    }

    #endregion
}
