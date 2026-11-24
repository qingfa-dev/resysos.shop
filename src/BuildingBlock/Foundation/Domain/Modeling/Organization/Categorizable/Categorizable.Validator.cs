namespace BuildingBlock.Domain.Modeling.Organization.Categorizable;

/// <summary>Validation logic for <see cref="ICategorizable{TCategory}"/>.</summary>
public static class CategorizableValidator
{
    #region Properties

    #region Entity

    /// <summary>Validates that the categorizable entity is present.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <returns>The entity on success; otherwise the entity-required error.</returns>
    public static Result<TValue> ValidateEntity<TValue, TCategory>(TValue auditable)
        where TValue : ICategorizable<TCategory>
    {
        return auditable is null
            ? Result<TValue>.Failure(CategorizableConstant.Entity.Required.Error)
            : Result<TValue>.Success(auditable);
    }

    #endregion Entity

    #region Category

    /// <summary>Validates a category value.</summary>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="category">The category to validate.</param>
    /// <returns>The normalized category on success; otherwise the first failure.</returns>
    public static Result<TCategory> ValidateCategoryValue<TCategory>(
        TCategory category)
    {
        if (category is null)
        {
            return Result<TCategory>.Failure(
                CategorizableConstant.Category.Required.Error);
        }

        if (category is Guid guid && guid == Guid.Empty)
        {
            return Result<TCategory>.Failure(
                CategorizableConstant.Category.Empty.Error);
        }

        var normalized = Normalize(category);

        return Result<TCategory>.Success(normalized)
            .Ensure(
                predicate: candidate =>
                    candidate is not string name ||
                    !string.IsNullOrWhiteSpace(name),
                error: CategorizableConstant.Category.Required.Error)
            .Ensure(
                predicate: candidate =>
                    candidate is not string name ||
                    name.Length <= CategorizableConstant.Category.MaxLength.Value,
                error: CategorizableConstant.Category.MaxLength.Error);
    }

    /// <summary>Validates that the category is specified.</summary>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="category">The category to validate.</param>
    /// <returns>The normalized category on success; otherwise the required error.</returns>
    public static Result<TCategory> ValidateCategoryPresence<TCategory>(
        TCategory category)
    {
        return category is null
            ? Result<TCategory>.Failure(
                CategorizableConstant.Category.Required.Error)
            : Result<TCategory>.Success(Normalize(category));
    }

    /// <summary>Validates that the category is not already assigned.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="category">The category to check.</param>
    /// <returns>The entity on success; otherwise the duplicate error.</returns>
    public static Result<TValue> ValidateCategoryUniqueness<TValue, TCategory>(
        TValue entity,
        TCategory category)
        where TValue : ICategorizable<TCategory>
    {
        return entity.Categories.Contains(category)
            ? Result<TValue>.Failure(CategorizableConstant.Category.Duplicate.Error)
            : Result<TValue>.Success(entity);
    }

    /// <summary>Validates that the category is assigned.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <param name="category">The category to check.</param>
    /// <returns>The entity on success; otherwise the not-found error.</returns>
    public static Result<TValue> ValidateCategoryExists<TValue, TCategory>(
        TValue entity,
        TCategory category)
        where TValue : ICategorizable<TCategory>
    {
        return entity.Categories.Contains(category)
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(CategorizableConstant.Category.NotFound.Error);
    }

    #endregion Category

    #region Collection

    /// <summary>Validates that the entity can hold another category.</summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="entity">The entity to inspect.</param>
    /// <returns>The entity on success; otherwise the limit-exceeded error.</returns>
    public static Result<TValue> ValidateCollection<TValue, TCategory>(
        TValue entity)
        where TValue : ICategorizable<TCategory>
    {
        return entity.Categories.Count < CategorizableConstant.Category.MaxCount.Value
            ? Result<TValue>.Success(entity)
            : Result<TValue>.Failure(
                CategorizableConstant.Category.MaxCount.Error);
    }

    #endregion Collection

    #endregion

    #region Composition

    /// <summary>
    /// Validates the category that
    /// <see cref="CategorizableExtensions.AddCategory{TValue, TCategory}"/>
    /// is about to add, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="category">The category to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateAddCategory<TValue, TCategory>(
        TValue auditable,
        TCategory category)
        where TValue : ICategorizable<TCategory>
    {
        return ValidateEntity<TValue, TCategory>(auditable)
            .Bind(entity =>
                ValidateCategoryValue(category)
                    .Bind(candidate =>
                        ValidateCategoryUniqueness(entity, candidate))
                    .Bind(_ => ValidateCollection<TValue, TCategory>(entity)));
    }

    /// <summary>
    /// Validates that the category that
    /// <see cref="CategorizableExtensions.RemoveCategory{TValue, TCategory}"/>
    /// targets is assigned, without mutating the entity.
    /// </summary>
    /// <typeparam name="TValue">The entity type.</typeparam>
    /// <typeparam name="TCategory">The category type.</typeparam>
    /// <param name="auditable">The entity to validate.</param>
    /// <param name="category">The category to validate.</param>
    /// <returns>A result indicating success or failure.</returns>
    public static Result<TValue> ValidateRemoveCategory<TValue, TCategory>(
        TValue auditable,
        TCategory category)
        where TValue : ICategorizable<TCategory>
    {
        return ValidateEntity<TValue, TCategory>(auditable)
            .Bind(entity =>
                ValidateCategoryPresence(category)
                    .Bind(candidate => ValidateCategoryExists(entity, candidate)));
    }

    #endregion

    #region Internal Methods

    /// <summary>Normalizes a category value (trims strings when enabled).</summary>
    internal static TCategory Normalize<TCategory>(TCategory category)
    {
        if (CategorizableConstant.Defaults.TrimValues
            && category is string name)
        {
            return (TCategory)(object)name.Trim();
        }

        return category;
    }

    #endregion
}
