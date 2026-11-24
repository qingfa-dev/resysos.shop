using BuildingBlock.Domain.Modeling.Content.Metafieldable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Metafieldable;

/// <summary>Specifications for <see cref="MetafieldableConstant"/> covering constant values and errors.</summary>
public sealed class MetafieldableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        MetafieldableConstant.Entity.Required.Error.Code.Should().Be("metafieldable.entity.required");
        MetafieldableConstant.Namespace.Required.Error.Code.Should().Be("metafieldable.namespace.required");
        MetafieldableConstant.Namespace.MinLength.Error.Code.Should().Be("metafieldable.namespace.too_short");
        MetafieldableConstant.Namespace.MaxLength.Error.Code.Should().Be("metafieldable.namespace.too_long");
        MetafieldableConstant.Namespace.Format.Error.Code.Should().Be("metafieldable.namespace.invalid");
        MetafieldableConstant.Key.Required.Error.Code.Should().Be("metafieldable.key.required");
        MetafieldableConstant.Key.MinLength.Error.Code.Should().Be("metafieldable.key.too_short");
        MetafieldableConstant.Key.MaxLength.Error.Code.Should().Be("metafieldable.key.too_long");
        MetafieldableConstant.Key.Format.Error.Code.Should().Be("metafieldable.key.invalid");
        MetafieldableConstant.Value.MaxLength.Error.Code.Should().Be("metafieldable.value.too_long");
        MetafieldableConstant.Collection.MaxMetafields.Error.Code.Should().Be("metafieldable.metafield.too_many");
        MetafieldableConstant.Metafield.NotFound.Error.Code.Should().Be("metafieldable.metafield.not_found");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        MetafieldableConstant.Namespace.MinLength.Value.Should().Be(1);
        MetafieldableConstant.Namespace.MaxLength.Value.Should().Be(64);
        MetafieldableConstant.Key.MinLength.Value.Should().Be(1);
        MetafieldableConstant.Key.MaxLength.Value.Should().Be(64);
        MetafieldableConstant.Value.MaxLength.Value.Should().Be(8000);
        MetafieldableConstant.Collection.MaxMetafields.Value.Should().Be(100);
        MetafieldableConstant.Namespace.Format.Pattern.Should().Be(@"^[a-z0-9_]+$");
        MetafieldableConstant.Key.Format.Pattern.Should().Be(@"^[a-z0-9_]+$");
        MetafieldableConstant.Defaults.Type.Default.Should().Be(MetafieldType.ShortText);
    }
}
