using System.Text.RegularExpressions;

namespace BuildingBlock.Domain.Modeling.Content.Localizable;

/// <summary>Validator for the <see cref="ILocalizable"/> concern.</summary>
public static class LocalizableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the localizable entity is present.</summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue>(TValue auditable)
        where TValue : ILocalizable
    {
        return auditable is null
            ? Result<TValue>.Failure(LocalizableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Property

    /// <summary>Validates a localized property name.</summary>
    /// <param name="property">The property name to validate.</param>
    /// <returns>The property name on success; otherwise the first failure.</returns>
    public static Result<string> ValidateProperty(string? property)
    {
        return ValidatePropertyPresence(property)
            .Ensure(
                predicate: candidate =>
                    candidate.Length >= LocalizableConstant.Property.MinLength.Value,
                error: LocalizableConstant.Property.MinLength.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <= LocalizableConstant.Property.MaxLength.Value,
                error: LocalizableConstant.Property.MaxLength.Error);
    }

    /// <summary>Validates that a localized property name is present.</summary>
    /// <param name="property">The property name to validate.</param>
    /// <returns>The property name on success; otherwise the property-required error.</returns>
    public static Result<string> ValidatePropertyPresence(string? property)
    {
        return string.IsNullOrWhiteSpace(property)
            ? Result<string>.Failure(LocalizableConstant.Property.Required.Error)
            : Result<string>.Success(property);
    }

    #endregion Property

    #region Locale

    /// <summary>Validates a locale identifier.</summary>
    /// <param name="locale">The locale identifier to validate.</param>
    /// <returns>The locale on success; otherwise the first failure.</returns>
    public static Result<string> ValidateLocale(string? locale)
    {
        return ValidateLocalePresence(locale)
            .Ensure(
                predicate: candidate =>
                    candidate.Length >= LocalizableConstant.Locale.MinLength.Value,
                error: LocalizableConstant.Locale.MinLength.Error)
            .Ensure(
                predicate: candidate =>
                    candidate.Length <= LocalizableConstant.Locale.MaxLength.Value,
                error: LocalizableConstant.Locale.MaxLength.Error)
            .Ensure(
                predicate: candidate =>
                    Regex.IsMatch(candidate, LocalizableConstant.Locale.Format.Pattern),
                error: LocalizableConstant.Locale.Format.Error);
    }

    /// <summary>Validates that a locale identifier is present.</summary>
    /// <param name="locale">The locale identifier to validate.</param>
    /// <returns>The locale on success; otherwise the locale-required error.</returns>
    public static Result<string> ValidateLocalePresence(string? locale)
    {
        return string.IsNullOrWhiteSpace(locale)
            ? Result<string>.Failure(LocalizableConstant.Locale.Required.Error)
            : Result<string>.Success(locale);
    }

    #endregion Locale

    #region Value

    /// <summary>Validates a localized value (null is allowed).</summary>
    /// <param name="value">The localized value to validate.</param>
    /// <returns>The value on success; otherwise the value-too-long error.</returns>
    public static Result<string?> ValidateValue(string? value)
    {
        return Result<string?>.Success(value)
            .Ensure(
                predicate: candidate =>
                    candidate is null ||
                    candidate.Length <= LocalizableConstant.Value.MaxLength.Value,
                error: LocalizableConstant.Value.MaxLength.Error);
    }

    #endregion Value

    #region Collection

    /// <summary>Validates that a translation is present for the given pair.</summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <returns>The entity on success; otherwise the value-not-found error.</returns>
    public static Result<TValue> ValidateTranslationPresence<TValue>(
        TValue entity,
        string property,
        string locale)
        where TValue : ILocalizable
    {
        return ContainsTranslation(entity, property, locale)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(LocalizableConstant.Value.NotFound.Error);
    }

    /// <summary>Validates that the entity can hold another translation.</summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <returns>The entity on success; otherwise the translations-too-many error.</returns>
    public static Result<TValue> ValidateCollection<TValue>(
        TValue entity,
        string property,
        string locale)
        where TValue : ILocalizable
    {
        return ContainsTranslation(entity, property, locale) ||
               entity.LocalizedValues.Count <
               LocalizableConstant.Collection.MaxTranslations.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                LocalizableConstant.Collection.MaxTranslations.Error);
    }

    #endregion Collection

    #endregion

    #region Composition

    /// <summary>
    /// Validates the localized value that
    /// <see cref="LocalizableExtensions.SetLocalizedValue{TValue}"/>
    /// is about to apply, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <param name="value">The localized value.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateLocalizedValue<TValue>(
        TValue auditable,
        string? property,
        string? locale,
        string? value)
        where TValue : ILocalizable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidateProperty(property)
                    .Bind(_ => ValidateLocale(locale))
                    .Bind(_ => ValidateValue(value))
                    .Bind(_ => ValidateCollection(entity, property!, locale!)));
    }

    /// <summary>
    /// Validates that the localized value that
    /// <see cref="LocalizableExtensions.RemoveLocalizedValue{TValue}"/>
    /// targets is present, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="property">The localized property name.</param>
    /// <param name="locale">The locale identifier.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateLocalizedValueRemoval<TValue>(
        TValue auditable,
        string? property,
        string? locale)
        where TValue : ILocalizable
    {
        return ValidateEntity(auditable)
            .Bind(entity =>
                ValidatePropertyPresence(property)
                    .Bind(_ => ValidateLocalePresence(locale))
                    .Bind(_ => ValidateTranslationPresence(
                        entity,
                        property!,
                        locale!)));
    }

    /// <summary>
    /// Validates that the entity that
    /// <see cref="LocalizableExtensions.ClearLocalizedValues{TValue}"/>
    /// is about to clear exists, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The type of the localizable entity.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateClearLocalizedValues<TValue>(
        TValue auditable)
        where TValue : ILocalizable
    {
        return ValidateEntity(auditable);
    }

    #endregion

    #region Private Methods

    /// <summary>Checks whether a (property, locale) translation already exists.</summary>
    private static bool ContainsTranslation<TValue>(
        TValue auditable,
        string property,
        string locale)
        where TValue : ILocalizable
    {
        return auditable.LocalizedValues.Any(entry =>
            entry.Property == property &&
            entry.Locale == locale);
    }

    #endregion
}
