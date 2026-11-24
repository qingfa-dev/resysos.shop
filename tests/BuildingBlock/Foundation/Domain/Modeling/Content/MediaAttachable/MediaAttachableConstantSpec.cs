using BuildingBlock.Domain.Modeling.Content.MediaAttachable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.MediaAttachable;

/// <summary>Specifications for <see cref="MediaAttachableConstant"/> covering constant values and errors.</summary>
public sealed class MediaAttachableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        MediaAttachableConstant.Entity.Required.Error.Code.Should().Be("media_attachable.entity.required");
        MediaAttachableConstant.Media.Required.Error.Code.Should().Be("media_attachable.media.required");
        MediaAttachableConstant.Media.Duplicate.Error.Code.Should().Be("media_attachable.media.duplicate");
        MediaAttachableConstant.Media.NotFound.Error.Code.Should().Be("media_attachable.media.not_found");
        MediaAttachableConstant.Url.Required.Error.Code.Should().Be("media_attachable.url.required");
        MediaAttachableConstant.Url.MaxLength.Error.Code.Should().Be("media_attachable.url.too_long");
        MediaAttachableConstant.Url.Format.Error.Code.Should().Be("media_attachable.url.invalid");
        MediaAttachableConstant.AltText.MaxLength.Error.Code.Should().Be("media_attachable.alt_text.too_long");
        MediaAttachableConstant.MediaType.Required.Error.Code.Should().Be("media_attachable.media_type.required");
        MediaAttachableConstant.MediaType.Allowed.Error.Code.Should().Be("media_attachable.media_type.invalid");
        MediaAttachableConstant.Collection.MaxAttachments.Error.Code.Should().Be("media_attachable.media.too_many");
    }

    [Fact]
    public void Constants_ShouldMatchDocumentedValues()
    {
        MediaAttachableConstant.Url.MaxLength.Value.Should().Be(2048);
        MediaAttachableConstant.AltText.MaxLength.Value.Should().Be(500);
        MediaAttachableConstant.Collection.MaxAttachments.Value.Should().Be(20);
        MediaAttachableConstant.Defaults.MediaType.Should().Be("image");
        MediaAttachableConstant.Url.Format.Pattern.Should().Be(@"^https?://[^\s/$.?#].[^\s]*$");
        MediaAttachableConstant.MediaType.Allowed.Values.Should()
            .BeEquivalentTo("image", "video", "audio", "document");
    }
}
