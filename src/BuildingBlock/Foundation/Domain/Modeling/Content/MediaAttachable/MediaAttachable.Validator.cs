using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Content.MediaAttachable;

/// <summary>Validator for the <see cref="IMediaAttachable{TMedia}"/> concern.</summary>
public static class MediaAttachableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the media-attachable entity is present.</summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TMedia>(TValue auditable)
        where TValue : IMediaAttachable<TMedia>
    {
        return auditable is null
            ? Result<TValue>.Failure(
                MediaAttachableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Media

    /// <summary>Validates that the media is present.</summary>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="media">The media to validate.</param>
    /// <returns>The media on success; otherwise the media-required error.</returns>
    public static Result<TMedia> ValidateMedia<TMedia>(TMedia media)
    {
        return media is null
            ? Result<TMedia>.Failure(
                MediaAttachableConstant.Media.Required.Error)
            : Result<TMedia>.Success(media);
    }

    /// <summary>Validates that the media is not already attached.</summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="media">The media to check.</param>
    /// <returns>The entity on success; otherwise the duplicate-media error.</returns>
    public static Result<TValue> ValidateMediaUniqueness<TValue, TMedia>(
        TValue entity,
        TMedia media)
        where TValue : IMediaAttachable<TMedia>
    {
        return entity.Media.Contains(media)
            ? Result<TValue>.Failure(
                MediaAttachableConstant.Media.Duplicate.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the media is attached.</summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="media">The media to check.</param>
    /// <returns>The entity on success; otherwise the media-not-found error.</returns>
    public static Result<TValue> ValidateMediaAttached<TValue, TMedia>(
        TValue entity,
        TMedia media)
        where TValue : IMediaAttachable<TMedia>
    {
        return entity.Media.Contains(media)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                MediaAttachableConstant.Media.NotFound.Error);
    }

    #endregion Media

    #region Url

    /// <summary>Validates a media URL.</summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>The URL on success; otherwise the first failure.</returns>
    public static Result<string> ValidateUrl(string? url)
    {
        return ValidateUrlPresence(url)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <=
                    MediaAttachableConstant.Url.MaxLength.Value,
                error: MediaAttachableConstant.Url.MaxLength.Error)
            .Ensure(
                predicate: candidate =>
                    Regex.IsMatch(
                        candidate,
                        MediaAttachableConstant.Url.Format.Pattern),
                error: MediaAttachableConstant.Url.Format.Error);
    }

    /// <summary>Validates that a media URL is present.</summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>The URL on success; otherwise the URL-required error.</returns>
    public static Result<string> ValidateUrlPresence(string? url)
    {
        return string.IsNullOrWhiteSpace(url)
            ? Result<string>.Failure(
                MediaAttachableConstant.Url.Required.Error)
            : Result<string>.Success(url);
    }

    #endregion Url

    #region AltText

    /// <summary>Validates media alt text (null is allowed).</summary>
    /// <param name="altText">The alt text to validate.</param>
    /// <returns>The alt text on success; otherwise the too-long error.</returns>
    public static Result<string?> ValidateAltText(string? altText)
    {
        return Result<string?>.Success(altText)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <=
                    MediaAttachableConstant.AltText.MaxLength.Value,
                error: MediaAttachableConstant.AltText.MaxLength.Error);
    }

    #endregion AltText

    #region MediaType

    /// <summary>Validates a media type discriminator.</summary>
    /// <param name="mediaType">The media type to validate.</param>
    /// <returns>The media type on success; otherwise the first failure.</returns>
    public static Result<string> ValidateMediaType(string? mediaType)
    {
        return ValidateMediaTypePresence(mediaType)
            .Ensure(
                predicate: candidate =>
                    MediaAttachableConstant.MediaType.Allowed.Values.Contains(
                        candidate.Trim().ToLowerInvariant()),
                error: MediaAttachableConstant.MediaType.Allowed.Error);
    }

    /// <summary>Validates that a media type is present.</summary>
    /// <param name="mediaType">The media type to validate.</param>
    /// <returns>The media type on success; otherwise the required error.</returns>
    public static Result<string> ValidateMediaTypePresence(string? mediaType)
    {
        return string.IsNullOrWhiteSpace(mediaType)
            ? Result<string>.Failure(
                MediaAttachableConstant.MediaType.Required.Error)
            : Result<string>.Success(mediaType);
    }

    #endregion MediaType

    #region Collection

    /// <summary>Validates that the entity can hold another media item.</summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <returns>The entity on success; otherwise the too-many-media error.</returns>
    public static Result<TValue> ValidateCollection<TValue, TMedia>(
        TValue entity)
        where TValue : IMediaAttachable<TMedia>
    {
        return entity.Media.Count <
               MediaAttachableConstant.Collection.MaxAttachments.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                MediaAttachableConstant.Collection.MaxAttachments.Error);
    }

    #endregion Collection

    #endregion

    #region Composition

    /// <summary>
    /// Validates the media that <see cref="MediaAttachableExtensions.AddMedia{TValue, TMedia}"/>
    /// is about to attach, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="media">The media to attach.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateAddMedia<TValue, TMedia>(
        TValue auditable,
        TMedia media)
        where TValue : IMediaAttachable<TMedia>
    {
        return ValidateEntity<TValue, TMedia>(auditable)
            .Bind(entity =>
                ValidateMedia(media)
                    .Bind(_ => ValidateMediaUniqueness(entity, media))
                    .Bind(_ => ValidateCollection<TValue, TMedia>(entity)));
    }

    /// <summary>
    /// Validates that the media that
    /// <see cref="MediaAttachableExtensions.RemoveMedia{TValue, TMedia}"/>
    /// targets is attached, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="media">The media to detach.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateRemoveMedia<TValue, TMedia>(
        TValue auditable,
        TMedia media)
        where TValue : IMediaAttachable<TMedia>
    {
        return ValidateEntity<TValue, TMedia>(auditable)
            .Bind(entity =>
                ValidateMedia(media)
                    .Bind(_ => ValidateMediaAttached(entity, media)));
    }

    /// <summary>
    /// Validates that the entity that
    /// <see cref="MediaAttachableExtensions.ClearMedia{TValue, TMedia}"/>
    /// is about to clear exists, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the media-attachable entity.</typeparam>
    /// <typeparam name="TMedia">The type of the media.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateClearMedia<TValue, TMedia>(
        TValue auditable)
        where TValue : IMediaAttachable<TMedia>
    {
        return ValidateEntity<TValue, TMedia>(auditable);
    }

    #endregion
}
