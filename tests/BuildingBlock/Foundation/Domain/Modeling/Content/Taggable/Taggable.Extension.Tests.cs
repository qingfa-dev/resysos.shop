using BuildingBlock.Domain.Modeling.Content.Taggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Taggable;

/// <summary>Tests for <see cref="TaggableExtensions"/> covering extension behavior.</summary>
public sealed class TaggableExtensionTests
{
    private const string EntityRequired = "taggable.entity.required";
    private const string TagRequired = "taggable.tag.required";
    private const string TagTooLong = "taggable.tag.too_long";
    private const string TagInvalid = "taggable.tag.invalid";
    private const string TagDuplicate = "taggable.tag.duplicate";
    private const string TagNotFound = "taggable.tag.not_found";
    private const string TagsTooMany = "taggable.tags.too_many";
    private const string TestFailure = "test.fail";

    private sealed class TestTaggable : ITaggable
    {
        public ICollection<string> Tags { get; } = new List<string>();
    }

    private static Result<TestTaggable> Success(TestTaggable entity) =>
        Result<TestTaggable>.Success(entity);

    private static TestTaggable WithTags(params string[] tags)
    {
        var entity = new TestTaggable();

        foreach (var tag in tags)
            entity.Tags.Add(tag);

        return entity;
    }

    private static void ShouldFailWith(Result<TestTaggable> result, string expectedCode)
    {
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().NotBeNull();
        result.Errors!.Should().NotBeEmpty();
        result.Errors[0].Code.Should().Be(expectedCode);
    }

    private static Result<TestTaggable> FailedInput() =>
        Result<TestTaggable>.Failure(
            Error.Validation(TestFailure, "boom"));

    // AddTag

    [Fact]
    public void AddTag_NewTag_ShouldAddNormalizedTag()
    {
        var entity = new TestTaggable();

        var result = Success(entity).AddTag("sale");

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void AddTag_PaddedMixedCase_ShouldTrimAndLowercase()
    {
        var entity = new TestTaggable();

        var result = Success(entity).AddTag(" SALE ");

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddTag_MissingTag_ShouldFailWithoutMutation(string? tag)
    {
        var entity = new TestTaggable();

        var result = Success(entity).AddTag(tag!);

        ShouldFailWith(result, TagRequired);
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_TooLongTag_ShouldFailWithoutMutation()
    {
        var entity = new TestTaggable();
        var tag = new string(
            't',
            TaggableConstant.Tag.MaxLength.Value + 1);

        var result = Success(entity).AddTag(tag);

        ShouldFailWith(result, TagTooLong);
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_DuplicateTag_ShouldFailWithoutMutation()
    {
        var entity = WithTags("sale");

        var result = Success(entity).AddTag("sale");

        ShouldFailWith(result, TagDuplicate);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void AddTag_DuplicateAfterNormalization_ShouldFailWithoutMutation()
    {
        var entity = WithTags("sale");

        var result = Success(entity).AddTag(" SALE ");

        ShouldFailWith(result, TagDuplicate);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void AddTag_InputFailure_ShouldPropagateWithoutMutation()
    {
        var input = FailedInput();

        var result = input.AddTag("sale");

        ShouldFailWith(result, TestFailure);
    }

    [Fact]
    public void RemoveTag_ExistingTag_ShouldRemoveTag()
    {
        var entity = WithTags("sale");

        var result = Success(entity).RemoveTag("sale");

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_PaddedMixedCase_ShouldNormalizeAndRemove()
    {
        var entity = WithTags("sale");

        var result = Success(entity).RemoveTag(" SALE ");

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTag_MissingTag_ShouldFailWithoutMutation()
    {
        var entity = WithTags("sale");

        var result = Success(entity).RemoveTag("clearance");

        ShouldFailWith(result, TagNotFound);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void RemoveTag_NullTag_ShouldFailWithoutMutation()
    {
        var entity = WithTags("sale");

        var result = Success(entity).RemoveTag(null);

        ShouldFailWith(result, TagNotFound);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void RemoveTag_InputFailure_ShouldPropagateWithoutMutation()
    {
        var entity = WithTags("sale");
        var input = FailedInput();

        var result = input.RemoveTag("sale");

        ShouldFailWith(result, TestFailure);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void ClearTags_WithEntries_ShouldClearTags()
    {
        var entity = WithTags("sale", "new");

        var result = Success(entity).ClearTags();

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void ClearTags_EmptyCollection_ShouldSucceed()
    {
        var entity = new TestTaggable();

        var result = Success(entity).ClearTags();

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void ClearTags_InputFailure_ShouldPropagateWithoutMutation()
    {
        var entity = WithTags("sale");
        var input = FailedInput();

        var result = input.ClearTags();

        ShouldFailWith(result, TestFailure);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void NormalizeTag_Null_ShouldReturnNull()
    {
        TaggableExtensions.NormalizeTag(null).Should().BeNull();
    }

    [Theory]
    [InlineData(" SALE ", "sale")]
    [InlineData("Sale", "sale")]
    [InlineData("SALE", "sale")]
    [InlineData("clearance", "clearance")]
    public void NormalizeTag_ShouldTrimAndLowercase(
        string input,
        string expected)
    {
        TaggableExtensions.NormalizeTag(input).Should().Be(expected);
    }

}
