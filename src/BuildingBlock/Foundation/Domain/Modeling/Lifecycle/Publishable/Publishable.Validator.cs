using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Lifecycle.Publishable;

/// <summary>Validates publication and unpublication operations for <see cref="IPublishable"/> entities.</summary>
public static class PublishableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the publishable entity is present.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IPublishable
    {
        return auditable is null
            ? Result<TValue>.Failure(PublishableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region PublishedAt

    /// <summary>Validates the publication timestamp.</summary>
    /// <param name="nowUtc">The publication timestamp to validate.</param>
    /// <returns>The timestamp on success; otherwise the first failure.</returns>
    public static Result<DateTimeOffset> ValidatePublishedAt(
        DateTimeOffset nowUtc)
    {
        return nowUtc == default
            ? Result<DateTimeOffset>.Failure(
                PublishableConstant.PublishedAt.Required.Error)
            : Result<DateTimeOffset>.Success(nowUtc)
                .Ensure(
                    predicate: candidate =>
                        candidate <= DateTimeOffset.UtcNow.AddSeconds(
                            LifecycleConstant.Timestamp.MaxFutureSkew.Value),
                    error: PublishableConstant.PublishedAt.Future.Error);
    }

    #endregion PublishedAt

    #region State

    /// <summary>Validates that the entity can be published.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the already-published error.</returns>
    public static Result<TValue> ValidatePublicationState<TValue>(TValue entity)
        where TValue : IPublishable
    {
        return entity.IsPublished
            ? Result<TValue>.Failure(
                PublishableConstant.State.AlreadyPublished.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the entity can be unpublished.</summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the not-published error.</returns>
    public static Result<TValue> ValidateUnpublicationState<TValue>(
        TValue entity)
        where TValue : IPublishable
    {
        return entity.IsPublished
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                PublishableConstant.State.NotPublished.Error);
    }

    #endregion State

    #region PublishedBy

    /// <summary>Validates the publishing actor.</summary>
    /// <param name="actor">The actor to validate (null is allowed).</param>
    /// <param name="required">Whether the actor must be specified.</param>
    /// <returns>The actor on success; otherwise the first failure.</returns>
    public static Result<string?> ValidateActor(
        string? actor,
        bool required)
    {
        return Result<string?>.Success(actor)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <= PublishableConstant.PublishedBy.MaxLength.Value,
                error: PublishableConstant.PublishedBy.MaxLength.Error)
            .Ensure(
                predicate: candidate => !required || !string.IsNullOrWhiteSpace(candidate),
                error: PublishableConstant.PublishedBy.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    Regex.IsMatch(candidate, PublishableConstant.PublishedBy.Format.Pattern),
                error: PublishableConstant.PublishedBy.Format.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length >= PublishableConstant.PublishedBy.MinLength.Value,
                error: PublishableConstant.PublishedBy.MinLength.Error);
    }

    #endregion PublishedBy

    #endregion

    #region Composition

    /// <summary>
    /// Validates the publication metadata that
    /// <see cref="PublishableExtensions.Publish{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="nowUtc">The publication timestamp.</param>
    /// <param name="actor">The publishing actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidatePublication<TValue>(
        TValue auditable,
        DateTimeOffset nowUtc,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IPublishable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidatePublishedAt(nowUtc)
                    .Bind(_ => ValidatePublicationState(entity))
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    /// <summary>
    /// Validates the unpublication metadata that
    /// <see cref="PublishableExtensions.Unpublish{TValue}"/> is about to apply,
    /// without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="actor">The unpublishing actor.</param>
    /// <param name="requireActor">Whether the actor must be specified.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateUnpublication<TValue>(
        TValue auditable,
        string? actor,
        bool requireActor = LifecycleConstant.Defaults.RequireActor)
        where TValue : IPublishable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateUnpublicationState(entity)
                    .Bind(_ => ValidateActor(actor, requireActor))
                    .Map(_ => entity));
    }

    #endregion
}
