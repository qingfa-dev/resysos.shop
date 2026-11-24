using BuildingBlock.Domain.Modeling.Content.Sluggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Sluggable;

/// <summary>Tests for <see cref="SluggableValidator"/> covering validation rules.</summary>
public class SluggableValidatorTests
{
    #region Helper Methods

    private sealed class TestSluggable : ISluggable
    {
        public string Slug { get; set; } = string.Empty;
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestSluggable> Success(
        TestSluggable entity)
    {
        return Result<TestSluggable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateSlug_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = SluggableValidator.ValidateSlug(
            (TestSluggable)null!,
            "hello-world");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.entity.required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateSlug_BlankSlug_ShouldFailSlugRequired(string? slug)
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void ValidateSlug_TooLongSlug_ShouldFailSlugTooLong()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };
        var slug = new string(
            's',
            SluggableConstant.Slug.MaxLength.Value + 1);

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.too_long");
        entity.Slug.Should().Be("keep-me");
    }

    [Theory]
    [InlineData("Hello-World")]
    [InlineData("hello--world")]
    [InlineData("hello_world")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    public void ValidateSlug_InvalidSlug_ShouldFailSlugInvalid(string slug)
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, slug);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.invalid");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void ValidateSlug_ValidSlug_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = SluggableValidator.ValidateSlug(entity, "hello-world-123");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
