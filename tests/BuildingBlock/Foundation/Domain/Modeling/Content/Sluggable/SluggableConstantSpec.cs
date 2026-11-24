using BuildingBlock.Domain.Modeling.Content.Sluggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Sluggable;

/// <summary>Specifications for <see cref="SluggableConstant"/> covering constant values and errors.</summary>
public sealed class SluggableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        SluggableConstant.Entity.Required.Error.Code.Should().Be("sluggable.entity.required");
        SluggableConstant.Slug.Required.Error.Code.Should().Be("sluggable.slug.required");
        SluggableConstant.Slug.MinLength.Error.Code.Should().Be("sluggable.slug.too_short");
        SluggableConstant.Slug.MaxLength.Error.Code.Should().Be("sluggable.slug.too_long");
        SluggableConstant.Slug.Format.Error.Code.Should().Be("sluggable.slug.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        SluggableConstant.Slug.MinLength.Value.Should().Be(1);
        SluggableConstant.Slug.MaxLength.Value.Should().Be(200);
        SluggableConstant.Defaults.FallbackSlug.Should().Be("n-a");
        SluggableConstant.Slug.Format.Pattern.Should().Be(@"^[a-z0-9]+(?:-[a-z0-9]+)*$");
    }
}
