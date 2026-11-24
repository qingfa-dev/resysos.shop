using BuildingBlock.Domain.Modeling.Content.Localizable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Localizable;

/// <summary>Specifications for <see cref="LocalizableConstant"/> covering constant values and errors.</summary>
public sealed class LocalizableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        LocalizableConstant.Entity.Required.Error.Code.Should().Be("localizable.entity.required");
        LocalizableConstant.Property.Required.Error.Code.Should().Be("localizable.property.required");
        LocalizableConstant.Property.MinLength.Error.Code.Should().Be("localizable.property.too_short");
        LocalizableConstant.Property.MaxLength.Error.Code.Should().Be("localizable.property.too_long");
        LocalizableConstant.Locale.Required.Error.Code.Should().Be("localizable.locale.required");
        LocalizableConstant.Locale.MinLength.Error.Code.Should().Be("localizable.locale.too_short");
        LocalizableConstant.Locale.MaxLength.Error.Code.Should().Be("localizable.locale.too_long");
        LocalizableConstant.Locale.Format.Error.Code.Should().Be("localizable.locale.invalid");
        LocalizableConstant.Value.MaxLength.Error.Code.Should().Be("localizable.value.too_long");
        LocalizableConstant.Value.NotFound.Error.Code.Should().Be("localizable.value.not_found");
        LocalizableConstant.Collection.MaxTranslations.Error.Code.Should().Be("localizable.translations.too_many");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        LocalizableConstant.Property.MinLength.Value.Should().Be(1);
        LocalizableConstant.Property.MaxLength.Value.Should().Be(128);
        LocalizableConstant.Locale.MinLength.Value.Should().Be(2);
        LocalizableConstant.Locale.MaxLength.Value.Should().Be(35);
        LocalizableConstant.Value.MaxLength.Value.Should().Be(4000);
        LocalizableConstant.Collection.MaxTranslations.Value.Should().Be(200);
        LocalizableConstant.Defaults.Locale.Should().Be("en");
        LocalizableConstant.Locale.Format.Pattern.Should().Be(@"^[A-Za-z]{2,8}(?:-[A-Za-z0-9]{2,8})*$");
    }
}
