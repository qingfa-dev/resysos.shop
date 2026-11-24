using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Content.Metafieldable;

/// <summary>Validator for the <see cref="IMetafieldable"/> concern.</summary>
public static class MetafieldableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the metafieldable entity is present.</summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : IMetafieldable
    {
        return auditable is null
            ? Result<TValue>.Failure(MetafieldableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Namespace

    /// <summary>Validates a metafield namespace.</summary>
    /// <param name="namespace">The namespace to validate.</param>
    /// <returns>The namespace on success; otherwise the first failure.</returns>
    public static Result<string> ValidateNamespace(string? @namespace)
    {
        return ValidateNamespacePresence(@namespace)
            .Ensure(
                predicate: candidate =>
                    candidate.Length >=
                    MetafieldableConstant.Namespace.MinLength.Value,
                error: MetafieldableConstant.Namespace.MinLength.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <=
                    MetafieldableConstant.Namespace.MaxLength.Value,
                error: MetafieldableConstant.Namespace.MaxLength.Error)
            .Ensure(
                predicate: candidate =>
                    Regex.IsMatch(
                        candidate,
                        MetafieldableConstant.Namespace.Format.Pattern),
                error: MetafieldableConstant.Namespace.Format.Error);
    }

    /// <summary>Validates that a metafield namespace is present.</summary>
    /// <param name="namespace">The namespace to validate.</param>
    /// <returns>The namespace on success; otherwise the namespace-required error.</returns>
    public static Result<string> ValidateNamespacePresence(string? @namespace)
    {
        return string.IsNullOrWhiteSpace(@namespace)
            ? Result<string>.Failure(
                MetafieldableConstant.Namespace.Required.Error)
            : Result<string>.Success(@namespace);
    }

    #endregion Namespace

    #region Key

    /// <summary>Validates a metafield key.</summary>
    /// <param name="key">The key to validate.</param>
    /// <returns>The key on success; otherwise the first failure.</returns>
    public static Result<string> ValidateKey(string? key)
    {
        return ValidateKeyPresence(key)
            .Ensure(
                predicate: candidate =>
                    candidate.Length >=
                    MetafieldableConstant.Key.MinLength.Value,
                error: MetafieldableConstant.Key.MinLength.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <=
                    MetafieldableConstant.Key.MaxLength.Value,
                error: MetafieldableConstant.Key.MaxLength.Error)
            .Ensure(
                predicate: candidate =>
                    Regex.IsMatch(
                        candidate,
                        MetafieldableConstant.Key.Format.Pattern),
                error: MetafieldableConstant.Key.Format.Error);
    }

    /// <summary>Validates that a metafield key is present.</summary>
    /// <param name="key">The key to validate.</param>
    /// <returns>The key on success; otherwise the key-required error.</returns>
    public static Result<string> ValidateKeyPresence(string? key)
    {
        return string.IsNullOrWhiteSpace(key)
            ? Result<string>.Failure(MetafieldableConstant.Key.Required.Error)
            : Result<string>.Success(key);
    }

    #endregion Key

    #region Value

    /// <summary>Validates a metafield value (null is allowed).</summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>The value on success; otherwise the too-long error.</returns>
    public static Result<string?> ValidateValue(string? value)
    {
        return Result<string?>.Success(value)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <=
                    MetafieldableConstant.Value.MaxLength.Value,
                error: MetafieldableConstant.Value.MaxLength.Error);
    }

    #endregion Value

    #region Collection

    /// <summary>Validates that the metafield is not already present.</summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="namespace">The metafield namespace.</param>
    /// <param name="key">The metafield key.</param>
    /// <returns>The entity on success; otherwise the not-found error.</returns>
    public static Result<TValue> ValidateMetafieldExists<TValue>(
        TValue entity,
        string @namespace,
        string key)
        where TValue : IMetafieldable
    {
        return ContainsMetafield(entity, @namespace, key)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                MetafieldableConstant.Metafield.NotFound.Error);
    }

    /// <summary>Validates that the entity can hold another metafield.</summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="namespace">The metafield namespace.</param>
    /// <param name="key">The metafield key.</param>
    /// <returns>The entity on success; otherwise the too-many-metafields error.</returns>
    public static Result<TValue> ValidateCollection<TValue>(
        TValue entity,
        string @namespace,
        string key)
        where TValue : IMetafieldable
    {
        return ContainsMetafield(entity, @namespace, key) ||
               entity.Metafields.Count <
               MetafieldableConstant.Collection.MaxMetafields.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                MetafieldableConstant.Collection.MaxMetafields.Error);
    }

    #endregion Collection

    #endregion

    #region Composition

    /// <summary>
    /// Validates the metafield that
    /// <see cref="MetafieldableExtensions.SetMetafield{TValue}(Result{TValue}, string, string, MetafieldType, string)"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="namespace">The metafield namespace.</param>
    /// <param name="key">The metafield key.</param>
    /// <param name="value">The metafield value (null is allowed).</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateMetafield<TValue>(
        TValue auditable,
        string? @namespace,
        string? key,
        string? value = null)
        where TValue : IMetafieldable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateNamespace(@namespace)
                    .Bind(_ => ValidateKey(key))
                    .Bind(_ => ValidateValue(value))
                    .Bind(_ => ValidateCollection(entity, @namespace!, key!)));
    }

    /// <summary>
    /// Validates that the metafield that
    /// <see cref="MetafieldableExtensions.RemoveMetafield{TValue}"/>
    /// targets is present, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="namespace">The metafield namespace.</param>
    /// <param name="key">The metafield key.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateMetafieldRemoval<TValue>(
        TValue auditable,
        string? @namespace,
        string? key)
        where TValue : IMetafieldable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateNamespacePresence(@namespace)
                    .Bind(_ => ValidateKeyPresence(key))
                    .Bind(_ => ValidateMetafieldExists(
                        entity,
                        @namespace!,
                        key!)));
    }

    /// <summary>
    /// Validates that the entity that
    /// <see cref="MetafieldableExtensions.ClearMetafields{TValue}"/>
    /// is about to clear exists, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the metafieldable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateClearMetafields<TValue>(
        TValue auditable)
        where TValue : IMetafieldable
    {
        return ValidateEntity(auditable);
    }

    #endregion

    #region Private Methods

    /// <summary>Checks whether a (namespace, key) metafield already exists.</summary>
    private static bool ContainsMetafield<TValue>(
        TValue auditable,
        string @namespace,
        string key)
        where TValue : IMetafieldable
    {
        return auditable.Metafields.Any(metafield =>
            metafield.Namespace == @namespace &&
            metafield.Key == key);
    }

    #endregion
}
