using BuildingBlock.Domain.Modeling.Content.Taggable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Taggable;

/// <summary>Specifications for <see cref="TaggableValidator"/> covering validation rules.</summary>
public sealed class TaggableValidatorSpec
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
    public void ValidateAddTag_NullEntity_ShouldFail()
    {
        var result = TaggableValidator.ValidateAddTag(
            (TestTaggable)null!,
            "sale");

        ShouldFailWith(result, EntityRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateAddTag_MissingTag_ShouldFail(string? tag)
    {
        var entity = new TestTaggable();

        var result = TaggableValidator.ValidateAddTag(entity, tag!);

        ShouldFailWith(result, TagRequired);
        entity.Tags.Should().BeEmpty();
    }

    [Theory]
    [InlineData(" leading")]
    [InlineData("trailing ")]
    public void ValidateAddTag_UntrimmedTag_ShouldFail(string tag)
    {
        var entity = new TestTaggable();

        // Direct validation rejects input that extensions normalize first.
        var result = TaggableValidator.ValidateAddTag(entity, tag);

        ShouldFailWith(result, TagInvalid);
        entity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void ValidateAddTag_TooManyTags_ShouldFailWithoutMutation()
    {
        var maxTags = TaggableConstant.Collection.MaxTags.Value;
        var entity = new TestTaggable();

        for (var i = 0; i < maxTags; i++)
            entity.Tags.Add($"tag{i}");

        var result = TaggableValidator.ValidateAddTag(entity, "overflow");

        ShouldFailWith(result, TagsTooMany);
        entity.Tags.Count.Should().Be(maxTags);
    }

    [Fact]
    public void ValidateRemoveTag_NullEntity_ShouldFail()
    {
        var result = TaggableValidator.ValidateRemoveTag(
            (TestTaggable)null!,
            "sale");

        ShouldFailWith(result, EntityRequired);
    }

    [Fact]
    public void ValidateRemoveTag_MissingTag_ShouldFail()
    {
        var entity = WithTags("sale");

        var result = TaggableValidator.ValidateRemoveTag(
            entity,
            "clearance");

        ShouldFailWith(result, TagNotFound);
        entity.Tags.Should().ContainSingle().Which.Should().Be("sale");
    }

    [Fact]
    public void ValidateRemoveTag_ExistingTag_ShouldSucceed()
    {
        var entity = WithTags("sale");

        var result = TaggableValidator.ValidateRemoveTag(entity, "sale");

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().ContainSingle();
    }

    [Fact]
    public void ValidateClearTags_NullEntity_ShouldFail()
    {
        var result = TaggableValidator.ValidateClearTags(
            (TestTaggable)null!);

        ShouldFailWith(result, EntityRequired);
    }

    [Fact]
    public void ValidateClearTags_ExistingEntity_ShouldSucceed()
    {
        var entity = WithTags("sale");

        var result = TaggableValidator.ValidateClearTags(entity);

        result.IsSuccess.Should().BeTrue();
        entity.Tags.Should().ContainSingle();
    }

}
