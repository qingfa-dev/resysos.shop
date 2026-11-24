using BuildingBlock.Domain.Modeling.Content.Localizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Localizable;

/// <summary>Tests for <see cref="LocalizableValidator"/> covering validation rules.</summary>
public class LocalizableValidatorTests
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
    public void ValidateLocalizedValue_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            (TestLocalizable)null!,
            "title",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.entity.required");
    }

    [Fact]
    public void ValidateLocalizedValue_BlankProperty_ShouldFailPropertyRequired()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "   ",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.property.required");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_BlankLocale_ShouldFailLocaleRequired()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "   ",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.locale.required");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_ShortLocale_ShouldFailLocaleTooShort()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "a",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.locale.too_short");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_InvalidLocale_ShouldFailLocaleInvalid()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en_us!",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.locale.invalid");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_TooLongValue_ShouldFailValueTooLong()
    {
        // Arrange:
        var entity = new TestLocalizable();
        var value = new string(
            'v',
            LocalizableConstant.Value.MaxLength.Value + 1);

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en",
            value);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.value.too_long");
        entity.LocalizedValues.Should().BeEmpty();
    }

    [Fact]
    public void ValidateLocalizedValue_TooManyTranslations_ShouldFailTranslationsTooMany()
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
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "new-prop",
            "en",
            "Hello");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.translations.too_many");
        entity.LocalizedValues.Count.Should().Be(
            LocalizableConstant.Collection.MaxTranslations.Value);
    }

    [Fact]
    public void ValidateLocalizedValue_UpdateExistingWhenFull_ShouldSucceed()
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
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "prop0",
            "en",
            "Updated");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_MissingEntry_ShouldFailValueNotFound()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            entity,
            "title",
            "en");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.value.not_found");
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            (TestLocalizable)null!,
            "title",
            "en");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.entity.required");
    }

    [Fact]
    public void ValidateClearLocalizedValues_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = LocalizableValidator.ValidateClearLocalizedValues(
            (TestLocalizable)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("localizable.entity.required");
    }

    [Fact]
    public void ValidateClearLocalizedValues_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        entity.LocalizedValues.Add(new LocalizedValue
        {
            Property = "title",
            Locale = "en",
            Value = "Hello"
        });

        // Act:
        var result = LocalizableValidator.ValidateClearLocalizedValues(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateLocalizedValue_NullValue_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValue(
            entity,
            "title",
            "en",
            null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateLocalizedValueRemoval_ExistingEntry_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestLocalizable();
        entity.LocalizedValues.Add(new LocalizedValue
        {
            Property = "title",
            Locale = "en",
            Value = "Hello"
        });

        // Act:
        var result = LocalizableValidator.ValidateLocalizedValueRemoval(
            entity,
            "title",
            "en");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
