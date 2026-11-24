using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Content.Taggable;

/// <summary>Validator for the <see cref="ITaggable"/> concern.</summary>
public static class TaggableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the taggable entity is present.</summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ITaggable
    {
        return auditable is null
            ? Result<TValue>.Failure(TaggableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Tag

    /// <summary>Validates a tag's presence, length and format.</summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>The tag on success; otherwise the first failure.</returns>
    public static Result<string> ValidateTag(string? tag)
    {
        return ValidateTagPresence(tag)
            .Ensure(
                predicate: candidate =>
                    candidate.Length >= TaggableConstant.Tag.MinLength.Value,
                error: TaggableConstant.Tag.MinLength.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <= TaggableConstant.Tag.MaxLength.Value,
                error: TaggableConstant.Tag.MaxLength.Error)
            .Ensure(
                predicate: candidate =>
                    Regex.IsMatch(candidate, TaggableConstant.Tag.Format.Pattern),
                error: TaggableConstant.Tag.Format.Error);
    }

    /// <summary>Validates that a tag is present.</summary>
    /// <param name="tag">The tag to validate.</param>
    /// <returns>The tag on success; otherwise the tag-required error.</returns>
    public static Result<string> ValidateTagPresence(string? tag)
    {
        return string.IsNullOrWhiteSpace(tag)
            ? Result<string>.Failure(TaggableConstant.Tag.Required.Error)
            : Result<string>.Success(tag);
    }

    /// <summary>Validates that the tag is not already present on the entity.</summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="tag">The tag to check.</param>
    /// <returns>The entity on success; otherwise the duplicate-tag error.</returns>
    public static Result<TValue> ValidateTagUniqueness<TValue>(
        TValue entity,
        string tag)
        where TValue : ITaggable
    {
        return entity.Tags.Contains(tag)
            ? Result<TValue>.Failure(TaggableConstant.Tag.Duplicate.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the tag is present on the entity.</summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="tag">The tag to check.</param>
    /// <returns>The entity on success; otherwise the tag-not-found error.</returns>
    public static Result<TValue> ValidateTagExists<TValue>(
        TValue entity,
        string? tag)
        where TValue : ITaggable
    {
        return entity.Tags.Contains(tag!)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(TaggableConstant.Tag.NotFound.Error);
    }

    #endregion Tag

    #region Collection

    /// <summary>Validates that the entity can hold another tag.</summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <returns>The entity on success; otherwise the too-many-tags error.</returns>
    public static Result<TValue> ValidateCollection<TValue>(TValue entity)
        where TValue : ITaggable
    {
        return entity.Tags.Count < TaggableConstant.Collection.MaxTags.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(TaggableConstant.Collection.MaxTags.Error);
    }

    #endregion Collection

    #endregion

    #region Composition

    /// <summary>
    /// Validates the tag that <see cref="TaggableExtensions.AddTag{TValue}"/>
    /// is about to add, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="tag">The tag to add.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateAddTag<TValue>(
        TValue auditable,
        string? tag)
        where TValue : ITaggable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateTag(tag)
                    .Bind(candidate => ValidateTagUniqueness(entity, candidate))
                    .Bind(_ => ValidateCollection(entity)));
    }

    /// <summary>
    /// Validates that the tag that
    /// <see cref="TaggableExtensions.RemoveTag{TValue}"/> is about to remove
    /// is present, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="tag">The tag to remove.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateRemoveTag<TValue>(
        TValue auditable,
        string? tag)
        where TValue : ITaggable
    {
        return ValidateEntity(auditable)
            .Bind(entity => ValidateTagExists(entity, tag));
    }

    /// <summary>
    /// Validates that the entity that
    /// <see cref="TaggableExtensions.ClearTags{TValue}"/> is about to clear
    /// exists, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the taggable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateClearTags<TValue>(
        TValue auditable)
        where TValue : ITaggable
    {
        return ValidateEntity(auditable);
    }

    #endregion
}
