using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Foundation.Referenceable;

/// <summary>Validates reference state for <see cref="IReferenceable"/> entities.</summary>
public static class ReferenceableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the referenceable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IReferenceable
    {
        return auditable is null
            ? Result<TValue>.Failure(ReferenceableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Reference

    /// <summary>Validates a reference value.</summary>
    /// <param name="reference">The reference value to validate.</param>
    /// <returns>The reference on success; otherwise the first failure.</returns>
    public static Result<string> ValidateReferenceValue(string? reference)
    {
        return string.IsNullOrWhiteSpace(reference)
            ? Result<string>.Failure(
                ReferenceableConstant.Reference.Required.Error)
            : Result<string>.Success(reference)
                .Ensure(
                    predicate: candidate =>
                        candidate.Length <=
                        ReferenceableConstant.Reference.MaxLength.Value,
                    error: ReferenceableConstant.Reference.MaxLength.Error)
                .Ensure(
                    predicate: candidate =>
                        Regex.IsMatch(
                            candidate,
                            ReferenceableConstant.Reference.Trimmed.Pattern),
                    error: ReferenceableConstant.Reference.Trimmed.Error)
                .Ensure(
                    predicate: candidate =>
                        Regex.IsMatch(
                            candidate,
                            ReferenceableConstant.Reference.Charset.Pattern),
                    error: ReferenceableConstant.Reference.Charset.Error);
    }

    #endregion Reference

    #endregion

    #region Composition

    /// <summary>Validates the reference that <see cref="ReferenceableExtensions.SetReference{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="reference">The reference value to validate.</param>
    /// <returns>Success if valid; otherwise a failure with the appropriate error.</returns>
    public static Result<TValue> ValidateReference<TValue>(
        TValue auditable,
        string? reference)
        where TValue : IReferenceable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateReferenceValue(reference).Map(_ => entity));
    }

    #endregion
}
