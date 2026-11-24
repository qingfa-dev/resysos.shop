using BuildingBlock.Domain.Modeling.Content.Taggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Taggable;

/// <summary>Tests for <see cref="TaggableConstant"/> covering constant values and errors.</summary>
public sealed class TaggableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        TaggableConstant.Entity.Required.Error.Code.Should().Be("taggable.entity.required");
        TaggableConstant.Tag.Required.Error.Code.Should().Be("taggable.tag.required");
        TaggableConstant.Tag.MinLength.Error.Code.Should().Be("taggable.tag.too_short");
        TaggableConstant.Tag.MaxLength.Error.Code.Should().Be("taggable.tag.too_long");
        TaggableConstant.Tag.Format.Error.Code.Should().Be("taggable.tag.invalid");
        TaggableConstant.Tag.Duplicate.Error.Code.Should().Be("taggable.tag.duplicate");
        TaggableConstant.Tag.NotFound.Error.Code.Should().Be("taggable.tag.not_found");
        TaggableConstant.Collection.MaxTags.Error.Code.Should().Be("taggable.tags.too_many");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        TaggableConstant.Tag.MinLength.Value.Should().Be(1);
        TaggableConstant.Tag.MaxLength.Value.Should().Be(128);
        TaggableConstant.Collection.MaxTags.Value.Should().Be(50);
        TaggableConstant.Defaults.Normalization.TrimWhitespace.Should().BeTrue();
        TaggableConstant.Defaults.Normalization.ToLowercase.Should().BeTrue();
        TaggableConstant.Tag.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
    }
}
