using BuildingBlock.Domain.Modeling.Content.MediaAttachable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.MediaAttachable;

/// <summary>Tests for <see cref="MediaAttachableValidator"/> covering validation rules.</summary>
public class MediaAttachableValidatorTests
{
    #region Helper Methods

    private sealed class TestMediaAttachable : IMediaAttachable<string>
    {
        public ICollection<string> Media { get; } = new List<string>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestMediaAttachable> Success(
        TestMediaAttachable entity)
    {
        return Result<TestMediaAttachable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateAddMedia_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateAddMedia(
            (TestMediaAttachable)null!,
            "photo.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.entity.required");
    }

    [Fact]
    public void ValidateAddMedia_NullMedia_ShouldFailMediaRequired()
    {
        // Arrange:
        var entity = new TestMediaAttachable();

        // Act:
        var result = MediaAttachableValidator
            .ValidateAddMedia<TestMediaAttachable, string>(entity, null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.required");
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAddMedia_TooManyAttachments_ShouldFailMediaTooMany()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        for (var i = 0; i < MediaAttachableConstant.Collection.MaxAttachments.Value; i++)
        {
            entity.Media.Add($"media{i}.jpg");
        }

        // Act:
        var result = MediaAttachableValidator.ValidateAddMedia(
            entity,
            "overflow.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.too_many");
        entity.Media.Count.Should().Be(
            MediaAttachableConstant.Collection.MaxAttachments.Value);
    }

    [Fact]
    public void ValidateRemoveMedia_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateRemoveMedia(
            (TestMediaAttachable)null!,
            "photo.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.entity.required");
    }

    [Fact]
    public void ValidateRemoveMedia_NullMedia_ShouldFailMediaRequired()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        entity.Media.Add("photo.jpg");

        // Act:
        var result = MediaAttachableValidator
            .ValidateRemoveMedia<TestMediaAttachable, string>(entity, null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.required");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void ValidateRemoveMedia_MissingMedia_ShouldFailMediaNotFound()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        entity.Media.Add("photo.jpg");

        // Act:
        var result = MediaAttachableValidator.ValidateRemoveMedia(
            entity,
            "video.mp4");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.not_found");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void ValidateClearMedia_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator
            .ValidateClearMedia<TestMediaAttachable, string>(
                (TestMediaAttachable)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.entity.required");
    }

    [Fact]
    public void ValidateClearMedia_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        entity.Media.Add("photo.jpg");

        // Act:
        var result = MediaAttachableValidator
            .ValidateClearMedia<TestMediaAttachable, string>(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateUrl_BlankUrl_ShouldFailUrlRequired(string? url)
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateUrl(url);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.url.required");
    }

    [Fact]
    public void ValidateUrl_TooLongUrl_ShouldFailUrlTooLong()
    {
        // Arrange:
        var url = new string(
            'a',
            MediaAttachableConstant.Url.MaxLength.Value + 1);

        // Act:
        var result = MediaAttachableValidator.ValidateUrl(url);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.url.too_long");
    }

    [Theory]
    [InlineData("ftp://example.com/photo.jpg")]
    [InlineData("not a url")]
    [InlineData("https://exa mple.com/photo.jpg")]
    public void ValidateUrl_InvalidUrl_ShouldFailUrlInvalid(string url)
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateUrl(url);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.url.invalid");
    }

    [Fact]
    public void ValidateUrl_ValidUrl_ShouldSucceed()
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateUrl(
            "https://example.com/media/photo.jpg");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateAltText_TooLongAltText_ShouldFailAltTextTooLong()
    {
        // Arrange:
        var altText = new string(
            'a',
            MediaAttachableConstant.AltText.MaxLength.Value + 1);

        // Act:
        var result = MediaAttachableValidator.ValidateAltText(altText);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.alt_text.too_long");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A photo")]
    public void ValidateAltText_NullOrWithinLimit_ShouldSucceed(string? altText)
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateAltText(altText);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateMediaType_BlankType_ShouldFailMediaTypeRequired(string? mediaType)
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateMediaType(mediaType);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.media_type.required");
    }

    [Fact]
    public void ValidateMediaType_UnknownType_ShouldFailMediaTypeInvalid()
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateMediaType("exe");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("media_attachable.media_type.invalid");
    }

    [Theory]
    [InlineData("image")]
    [InlineData("video")]
    [InlineData("audio")]
    [InlineData("document")]
    [InlineData(" Video ")]
    public void ValidateMediaType_AllowedType_ShouldSucceed(string mediaType)
    {
        // Arrange:
        // Act:
        var result = MediaAttachableValidator.ValidateMediaType(mediaType);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
