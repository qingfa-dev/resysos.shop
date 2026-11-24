using BuildingBlock.Domain.Modeling.Content.MediaAttachable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.MediaAttachable;

/// <summary>Tests for <see cref="MediaAttachableExtensions"/> covering extension behavior.</summary>
public class MediaAttachableExtensionTests
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
    public void AddMedia_NewMedia_ShouldAttachMedia()
    {
        // Arrange:
        var entity = new TestMediaAttachable();

        // Act:
        var result = Success(entity).AddMedia("photo.jpg");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Media.Should().ContainSingle().Which.Should().Be("photo.jpg");
    }

    [Fact]
    public void AddMedia_DuplicateMedia_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        Success(entity).AddMedia("photo.jpg");

        // Act:
        var result = Success(entity).AddMedia("photo.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "media_attachable.media.duplicate");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void RemoveMedia_AttachedMedia_ShouldDetachMedia()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        Success(entity).AddMedia("photo.jpg");

        // Act:
        var result = Success(entity).RemoveMedia("photo.jpg");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMedia_MissingMedia_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        Success(entity).AddMedia("photo.jpg");

        // Act:
        var result = Success(entity).RemoveMedia("video.mp4");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.not_found");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void AddMedia_NullMedia_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();

        // Act:
        var result = Success(entity).AddMedia((string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.required");
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void ClearMedia_WithEntries_ShouldClearMedia()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        Success(entity).AddMedia("photo.jpg");
        Success(entity).AddMedia("video.mp4");

        // Act:
        var result = Success(entity).ClearMedia<TestMediaAttachable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void ClearMedia_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMediaAttachable();

        // Act:
        var result = Success(entity).ClearMedia<TestMediaAttachable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void AddMedia_TooManyViaExtension_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        for (var i = 0; i < MediaAttachableConstant.Collection.MaxAttachments.Value; i++)
        {
            entity.Media.Add($"media{i}.jpg");
        }

        // Act:
        var result = Success(entity).AddMedia("overflow.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("media_attachable.media.too_many");
        entity.Media.Count.Should().Be(
            MediaAttachableConstant.Collection.MaxAttachments.Value);
    }

    [Fact]
    public void AddMedia_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        var input = Result<TestMediaAttachable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.AddMedia("photo.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.Media.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMedia_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        entity.Media.Add("photo.jpg");
        var input = Result<TestMediaAttachable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.RemoveMedia("photo.jpg");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void ClearMedia_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestMediaAttachable();
        entity.Media.Add("photo.jpg");
        var input = Result<TestMediaAttachable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.ClearMedia<TestMediaAttachable, string>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.Media.Should().ContainSingle();
    }

    [Fact]
    public void NormalizeUrl_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeUrl(null);

        // Assert:
        normalized.Should().BeNull();
    }

    [Fact]
    public void NormalizeUrl_PaddedUrl_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeUrl("  https://example.com/a.jpg  ");

        // Assert:
        normalized.Should().Be("https://example.com/a.jpg");
    }

    [Fact]
    public void NormalizeUrl_PlainUrl_ShouldReturnSame()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeUrl("https://example.com/a.jpg");

        // Assert:
        normalized.Should().Be("https://example.com/a.jpg");
    }

    [Fact]
    public void NormalizeAltText_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeAltText(null);

        // Assert:
        normalized.Should().BeNull();
    }

    [Fact]
    public void NormalizeAltText_PaddedText_ShouldTrim()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeAltText("  A photo  ");

        // Assert:
        normalized.Should().Be("A photo");
    }

    [Fact]
    public void NormalizeMediaType_Null_ShouldReturnNull()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeMediaType(null);

        // Assert:
        normalized.Should().BeNull();
    }

    [Fact]
    public void NormalizeMediaType_MixedCasePadded_ShouldTrimAndLowercase()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeMediaType(" Video ");

        // Assert:
        normalized.Should().Be("video");
    }

    [Fact]
    public void NormalizeMedia_AllNull_ShouldReturnAllNull()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeMedia(null, null, null);

        // Assert:
        normalized.Url.Should().BeNull();
        normalized.AltText.Should().BeNull();
        normalized.MediaType.Should().BeNull();
    }

    [Fact]
    public void NormalizeMedia_MixedValues_ShouldNormalizeEach()
    {
        // Arrange:
        // Act:
        var normalized = MediaAttachableExtensions.NormalizeMedia(
            "  https://example.com/a.jpg  ",
            "  A photo  ",
            " Video ");

        // Assert:
        normalized.Url.Should().Be("https://example.com/a.jpg");
        normalized.AltText.Should().Be("A photo");
        normalized.MediaType.Should().Be("video");
    }

    #endregion
}
