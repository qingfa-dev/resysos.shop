namespace BuildingBlock.Domain.Modeling.Organization.Hierarchical;

/// <summary>Validation logic for <see cref="IHierarchical{TKey}"/>.</summary>
public static class HierarchicalValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the hierarchical entity is present.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TKey>(TValue auditable)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return auditable is null
            ? Result<TValue>.Failure(HierarchicalConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Parent

    /// <summary>Validates a parent identifier value.</summary>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="parentId">The parent identifier to validate (null means root).</param>
    /// <returns>The parent identifier on success; otherwise the empty-parent error.</returns>
    public static Result<TKey?> ValidateParentValue<TKey>(TKey? parentId)
        where TKey : struct
    {
        return parentId is Guid guid && guid == Guid.Empty
            ? Result<TKey?>.Failure(HierarchicalConstant.Parent.Empty.Error)
            : Result<TKey?>.Success(parentId);
    }

    /// <summary>Validates that the parent identifier is not the entity itself.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <returns>The entity on success; otherwise the self-parent error.</returns>
    public static Result<TValue> ValidateSelfParent<TValue, TKey>(
        TValue entity,
        TKey? parentId,
        TKey? entityId)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return parentId is null
               || entityId is null
               || !EqualityComparer<TKey>.Default.Equals(
                   parentId.Value,
                   entityId.Value)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(HierarchicalConstant.Parent.Self.Error);
    }

    /// <summary>Validates that the parent assignment does not create a cycle.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="entityId">The entity identifier.</param>
    /// <param name="ancestors">Ancestor identifiers of the proposed parent.</param>
    /// <returns>The entity on success; otherwise the cycle-detected error.</returns>
    public static Result<TValue> ValidateCycle<TValue, TKey>(
        TValue entity,
        TKey? parentId,
        TKey? entityId,
        IEnumerable<TKey> ancestors)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return Result<TValue>.Success(entity)
            .Ensure(
                predicate: _ =>
                    parentId is null ||
                    !ancestors.Contains(parentId.Value),
                error: HierarchicalConstant.Parent.Cycle.Error)
            .Ensure(
                predicate: _ =>
                    entityId is null ||
                    !ancestors.Contains(entityId.Value),
                error: HierarchicalConstant.Parent.Cycle.Error);
    }

    #endregion Parent

    #region Depth

    /// <summary>Validates a hierarchy depth value against the allowed range.</summary>
    /// <param name="depth">The depth to validate (root = 0).</param>
    /// <returns>The depth on success; otherwise the depth-exceeded error.</returns>
    public static Result<int> ValidateDepthValue(int depth)
    {
        return Result<int>.Success(depth)
            .Ensure(
                predicate: candidate =>
                    candidate >= HierarchicalConstant.Level.Min.Value &&
                    candidate <= HierarchicalConstant.Level.Max.Value,
                error: HierarchicalConstant.Level.Max.DepthExceeded);
    }

    #endregion Depth

    #region Path

    /// <summary>Validates a materialized path against length constraints.</summary>
    /// <param name="path">The materialized path to validate (null is allowed).</param>
    /// <returns>The path on success; otherwise the path-too-long error.</returns>
    public static Result<string?> ValidatePathValue(string? path)
    {
        return Result<string?>.Success(path)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <=
                    HierarchicalConstant.Parent.MaxPathLength.Value,
                error: HierarchicalConstant.Parent.MaxPathLength.Error);
    }

    #endregion Path

    #endregion

    #region Composition

    /// <summary>
    /// Validates the parent assignment that <c>SetParent</c>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="parentId">The parent identifier to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateParent<TValue, TKey>(
        TValue auditable,
        TKey? parentId)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return ValidateEntity<TValue, TKey>(auditable)
            .Bind(entity => ValidateParentValue(parentId).Map(_ => entity));
    }

    /// <summary>
    /// Validates a parent assignment including a self-parent check,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="parentId">The parent identifier to validate.</param>
    /// <param name="entityId">The entity identifier; used to reject self-parent.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateParent<TValue, TKey>(
        TValue auditable,
        TKey? parentId,
        TKey? entityId)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return ValidateParent(auditable, parentId)
            .Bind(entity => ValidateSelfParent(entity, parentId, entityId));
    }

    /// <summary>
    /// Validates a parent assignment including self-parent and cycle checks,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TKey">The key type for the parent identifier.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="parentId">The parent identifier to validate.</param>
    /// <param name="entityId">The entity identifier; used to reject self-parent.</param>
    /// <param name="ancestors">Ancestor identifiers of the proposed parent; used to reject cycles.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateParent<TValue, TKey>(
        TValue auditable,
        TKey? parentId,
        TKey? entityId,
        IEnumerable<TKey> ancestors)
        where TValue : IHierarchical<TKey>
        where TKey : struct
    {
        return ValidateParent(auditable, parentId, entityId)
            .Bind(entity => ValidateCycle(entity, parentId, entityId, ancestors));
    }

    /// <summary>
    /// Validates a hierarchy depth value against the allowed range,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="depth">The depth to validate (root = 0).</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateDepth<TValue>(
        TValue auditable,
        int depth)
    {
        return auditable is null
            ? Result<TValue>.Failure(HierarchicalConstant.Entity.Required.Error)
            : ValidateDepthValue(depth).Map(_ => auditable);
    }

    /// <summary>
    /// Validates a materialized path against length constraints,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="path">The materialized path to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidatePath<TValue>(
        TValue auditable,
        string? path)
    {
        return auditable is null
            ? Result<TValue>.Failure(HierarchicalConstant.Entity.Required.Error)
            : ValidatePathValue(path).Map(_ => auditable);
    }

    #endregion
}
