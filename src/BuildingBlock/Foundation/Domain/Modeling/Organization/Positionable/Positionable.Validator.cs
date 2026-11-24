namespace BuildingBlock.Domain.Modeling.Organization.Positionable;

/// <summary>Validation logic for <see cref="IPositionable"/>.</summary>
public static class PositionableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the positionable entity is present.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IPositionable
    {
        return auditable is null
            ? Result<TValue>.Failure(PositionableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Position

    /// <summary>Validates a position value.</summary>
    /// <param name="position">The position to validate.</param>
    /// <returns>The position on success; otherwise the first failure.</returns>
    public static Result<int> ValidatePositionValue(int position)
    {
        return Result<int>.Success(position)
            .Ensure(
                predicate: candidate =>
                    candidate >= PositionableConstant.Position.Min.Value,
                error: PositionableConstant.Position.Min.Error)
            .Ensure(
                predicate: candidate =>
                    candidate <= PositionableConstant.Position.Max.Value,
                error: PositionableConstant.Position.Max.Error);
    }

    #endregion Position

    #endregion

    #region Composition

    /// <summary>
    /// Validates the position that <see cref="PositionableExtensions.SetPosition{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="position">The position to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidatePosition<TValue>(
        TValue auditable,
        int position)
        where TValue : IPositionable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidatePositionValue(position).Map(_ => entity));
    }

    #endregion
}
