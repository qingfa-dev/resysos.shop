using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Content.Sluggable;

/// <summary>Validator for the <see cref="ISluggable"/> concern.</summary>
public static class SluggableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the sluggable entity is present.</summary>
    /// <typeparam name="TValue">The type of the sluggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ISluggable
    {
        return auditable is null
            ? Result<TValue>.Failure(SluggableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Slug

    /// <summary>Validates a slug's presence, length and format.</summary>
    /// <param name="slug">The slug to validate.</param>
    /// <returns>The slug on success; otherwise the first failure.</returns>
    public static Result<string> ValidateSlug(string? slug)
    {
        return string.IsNullOrWhiteSpace(slug)
            ? Result<string>.Failure(SluggableConstant.Slug.Required.Error)
            : Result<string>.Success(slug)
                .Ensure(
                    predicate: candidate =>
                        candidate.Length >=
                        SluggableConstant.Slug.MinLength.Value,
                    error: SluggableConstant.Slug.MinLength.Error)
                .Ensure(
                    predicate: candidate =>
                        candidate.Length <=
                        SluggableConstant.Slug.MaxLength.Value,
                    error: SluggableConstant.Slug.MaxLength.Error)
                .Ensure(
                    predicate: candidate =>
                        Regex.IsMatch(
                            candidate,
                            SluggableConstant.Slug.Format.Pattern),
                    error: SluggableConstant.Slug.Format.Error);
    }

    #endregion Slug

    #endregion

    #region Composition

    /// <summary>
    /// Validates the slug that <see cref="SluggableExtensions.SetSlug{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the sluggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="slug">The slug to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateSlug<TValue>(
        TValue auditable,
        string? slug)
        where TValue : ISluggable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateSlug(slug).Map(_ => entity));
    }

    #endregion
}
