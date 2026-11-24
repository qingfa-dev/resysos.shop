using BuildingBlock.Domain.Modeling.Content.Localizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Localizable;

/// <summary>Tests for <see cref="LocalizableExtensions"/> covering extension behavior.</summary>
public class LocalizableExtensionTests
{
    #region Helper Methods

    private sealed class TestLocalizable : ILocalizable
    {
        public ICollection<LocalizedValue> LocalizedValues { get; } =
            new List<LocalizedValue>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestLocalizable> Success(
        TestLocalizable entity)
    {
        return Result<TestLocalizable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetLocalizedValue_NewEntry_ShouldAddEntry()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", "en", "Hello");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Should().ContainSingle();
        entity.LocalizedValues.Single().Property.Should().Be("title");
        entity.LocalizedValues.Single().Locale.Should().Be("en");
        entity.LocalizedValues.Single().Value.Should().Be("Hello");
    }

    [Fact]
    public void SetLocalizedValue_ExistingEntry_ShouldUpdateValue()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Old");

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", "en", "New");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Should().ContainSingle();
        entity.LocalizedValues.Single().Value.Should().Be("New");
    }

    [Fact]
    public void SetLocalizedValue_NullProperty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue(null, "en", "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.property.required");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_TooLongProperty_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var property = new string(
            'p',
            LocalizableConstant.Property.MaxLength.Value + 1);

        // Act:
        var result = Success(entity)
            .SetLocalizedValue(property, "en", "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.property.too_long");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_NullLocale_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", null, "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.locale.required");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_TooLongLocale_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var locale = new string(
            'l',
            LocalizableConstant.Locale.MaxLength.Value + 1);

        // Act:
        var result = Success(entity)
            .SetLocalizedValue("title", locale, "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.locale.too_long");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLocalizedValue_ExistingEntry_ShouldRemoveEntry()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var result = Success(entity)
            .RemoveLocalizedValue("title", "en");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void RemoveLocalizedValue_MissingEntry_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var result = Success(entity)
            .RemoveLocalizedValue("title", "fr");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.value.not_found");
        entity.LocalizedValues.Should().ContainSingle();
    }

    [Fact]
    public void ClearLocalizedValues_WithEntries_ShouldClearEntries()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");
        Success(entity).SetLocalizedValue("title", "fr", "Bonjour");

        // Act:
        var result = Success(entity).ClearLocalizedValues();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ClearLocalizedValues_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).ClearLocalizedValues();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void SetLocalizedValue_NullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).SetLocalizedValue("title", "en", null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var entry = entity.LocalizedValues.Should().ContainSingle().Which;
        entry.Value.Should().BeNull();
    }

    [Fact]
    public void SetLocalizedValue_PaddedInputs_ShouldNormalizeBeforeStore()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = Success(entity).SetLocalizedValue("  title  ", "  en  ", "Hello");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var entry = entity.LocalizedValues.Should().ContainSingle().Which;
        entry.Property.Should().Be("title");
        entry.Locale.Should().Be("en");
    }

    [Fact]
    public void SetLocalizedValue_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        for (var i = 0; i < LocalizableConstant.Collection.MaxTranslations.Value; i++)
        {
            entity.LocalizedValues.Add(new LocalizedValue
            {
                Property = $"prop{i}",
                Locale = "en",
                Value = "Hello"
            });
        }

        // Act:
        var result = Success(entity).SetLocalizedValue("prop0", "en", "Updated");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.LocalizedValues.Count.Should().Be(
            LocalizableConstant.Collection.MaxTranslations.Value);
        entity.LocalizedValues.Single(e => e.Property == "prop0").Value.Should().Be("Updated");
    }

    [Fact]
    public void SetLocalizedValue_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var input = Result<TestLocalizable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.SetLocalizedValue("title", "en", "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void GetLocalizedValue_ExistingEntry_ShouldReturnValue()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", "en");

        // Assert:
        value.Should().Be("Hello");
    }

    [Fact]
    public void GetLocalizedValue_MissingEntry_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", "fr");

        // Assert:
        value.Should().BeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullEntity_ShouldReturnNull()
    {
        // Arrange:
        TestLocalizable entity = null!;

        // Act:
        var value = entity.GetLocalizedValue("title", "en");

        // Assert:
        value.Should().BeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullProperty_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue(null, "en");

        // Assert:
        value.Should().BeNull();
    }

    [Fact]
    public void GetLocalizedValue_NullLocale_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("title", null);

        // Assert:
        value.Should().BeNull();
    }

    [Fact]
    public void GetLocalizedValue_PaddedInputs_ShouldMatchNormalized()
    {
        // Arrange:
        var entity = new TestLocalizable();
        Success(entity).SetLocalizedValue("title", "en", "Hello");

        // Act:
        var value = entity.GetLocalizedValue("  title  ", "  en  ");

        // Assert:
        value.Should().Be("Hello");
    }

    [Fact]
    public void NormalizeProperty_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeProperty(null);

        // Assert:
        normalized.Should().BeNull();
    }

    [Fact]
    public void NormalizeProperty_Padded_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeProperty("  title  ");

        // Assert:
        normalized.Should().Be("title");
    }

    [Fact]
    public void NormalizeLocale_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeLocale(null);

        // Assert:
        normalized.Should().BeNull();
    }

    [Fact]
    public void NormalizeLocale_Padded_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = LocalizableExtensions.NormalizeLocale("  en  ");

        // Assert:
        normalized.Should().Be("en");
    }

    #endregion
}
