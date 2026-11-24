using BuildingBlock.Domain.Modeling.Content.Sluggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Sluggable;

/// <summary>Tests for <see cref="SluggableExtensions"/> covering extension behavior.</summary>
public class SluggableExtensionTests
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
    public void SetSlug_ValidSlug_ShouldSetSlug()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity).SetSlug("hello-world");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Slug.Should().Be("hello-world");
    }

    [Fact]
    public void SetSlug_NullSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug(null);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlug_UppercaseSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug("Hello-World");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.invalid");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlug_TooLongSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };
        var slug = new string(
            's',
            SluggableConstant.Slug.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).SetSlug(slug);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.too_long");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlugFromText_PlainText_ShouldSlugify()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity)
            .SetSlugFromText("Hello, World!");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Slug.Should().Be("hello-world");
    }

    [Fact]
    public void SetSlugFromText_Diacritics_ShouldFoldToAscii()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity)
            .SetSlugFromText("Crème Brûlée");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Slug.Should().Be("creme-brulee");
    }

    [Fact]
    public void SetSlugFromText_SymbolsOnly_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText("!!!");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlug_WhitespaceSlug_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlug("   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlugFromText_NullText_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText(null);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlugFromText_WhitespaceText_ShouldFailSlugRequired()
    {
        // Arrange:
        var entity = new TestSluggable { Slug = "keep-me" };

        // Act:
        var result = Success(entity).SetSlugFromText("   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("sluggable.slug.required");
        entity.Slug.Should().Be("keep-me");
    }

    [Fact]
    public void SetSlugFromText_TrailingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        var entity = new TestSluggable();

        // Act:
        var result = Success(entity).SetSlugFromText("Hello World! ");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Slug.Should().Be("hello-world");
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Slugify_BlankText_ShouldReturnEmpty(string? text, string expected)
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify(text);

        // Assert:
        slug.Should().Be(expected);
    }

    [Fact]
    public void Slugify_PaddedText_ShouldTrimHyphens()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("--Hello--");

        // Assert:
        slug.Should().Be("hello");
    }

    [Fact]
    public void Slugify_SymbolsOnly_ShouldReturnEmpty()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("!!!");

        // Assert:
        slug.Should().Be(string.Empty);
    }

    [Fact]
    public void Slugify_ConsecutiveSeparators_ShouldCollapseToSingleHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("hello   world");

        // Assert:
        slug.Should().Be("hello-world");
    }

    [Fact]
    public void Slugify_TrailingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("hello-");

        // Assert:
        slug.Should().Be("hello");
    }

    [Fact]
    public void Slugify_LeadingSeparator_ShouldTrimHyphen()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("   hello");

        // Assert:
        slug.Should().Be("hello");
    }

    [Fact]
    public void Slugify_Digits_ShouldPreserveDigits()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("product 123");

        // Assert:
        slug.Should().Be("product-123");
    }

    [Fact]
    public void Slugify_MixedSeparators_ShouldCollapseToSingleHyphens()
    {
        // Arrange:
        // Act:
        var slug = SluggableExtensions.Slugify("a--b__c");

        // Assert:
        slug.Should().Be("a-b-c");
    }

    #endregion
}
