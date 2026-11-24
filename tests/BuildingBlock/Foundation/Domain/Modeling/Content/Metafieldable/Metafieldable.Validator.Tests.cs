using BuildingBlock.Domain.Modeling.Content.Metafieldable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Metafieldable;

/// <summary>Tests for <see cref="MetafieldableValidator"/> covering validation rules.</summary>
public class MetafieldableValidatorTests
{
    #region Helper Methods

    private sealed class TestMetafieldable : IMetafieldable
    {
        public ICollection<Metafield> Metafields { get; } =
            new List<Metafield>();
    }

    /// <summary>Creates a successful result wrapping the test entity.</summary>
    /// <param name="entity">The test entity.</param>
    /// <returns>A successful result.</returns>
    private static Result<TestMetafieldable> Success(
        TestMetafieldable entity)
    {
        return Result<TestMetafieldable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateMetafield_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            (TestMetafieldable)null!,
            "specs",
            "weight");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.entity.required");
    }

    [Fact]
    public void ValidateMetafield_BlankNamespace_ShouldFailNamespaceRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "   ",
            "weight",
            "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.namespace.required");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void ValidateMetafield_TooLongKey_ShouldFailKeyTooLong()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var key = new string(
            'k',
            MetafieldableConstant.Key.MaxLength.Value + 1);

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            key,
            "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.key.too_long");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void ValidateMetafield_TooLongValue_ShouldFailValueTooLong()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var value = new string(
            'v',
            MetafieldableConstant.Value.MaxLength.Value + 1);

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            "weight",
            value);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.value.too_long");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void ValidateMetafield_NullValue_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "specs",
            "weight",
            null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateMetafield_TooManyMetafields_ShouldFailMetafieldsTooMany()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        for (var i = 0; i < MetafieldableConstant.Collection.MaxMetafields.Value; i++)
        {
            entity.Metafields.Add(new Metafield
            {
                Namespace = "ns",
                Key = $"key{i}",
                Type = MetafieldType.ShortText,
                Value = "v"
            });
        }

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "ns",
            "new_key",
            "v");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.metafield.too_many");
        entity.Metafields.Count.Should().Be(
            MetafieldableConstant.Collection.MaxMetafields.Value);
    }

    [Fact]
    public void ValidateMetafield_UpdateExistingWhenFull_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        for (var i = 0; i < MetafieldableConstant.Collection.MaxMetafields.Value; i++)
        {
            entity.Metafields.Add(new Metafield
            {
                Namespace = "ns",
                Key = $"key{i}",
                Type = MetafieldType.ShortText,
                Value = "v"
            });
        }

        // Act:
        var result = MetafieldableValidator.ValidateMetafield(
            entity,
            "ns",
            "key0",
            "updated");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateMetafieldRemoval_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            (TestMetafieldable)null!,
            "specs",
            "weight");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.entity.required");
    }

    [Fact]
    public void ValidateMetafieldRemoval_BlankNamespace_ShouldFailNamespaceRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            entity,
            "   ",
            "weight");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.namespace.required");
    }

    [Fact]
    public void ValidateMetafieldRemoval_BlankKey_ShouldFailKeyRequired()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = MetafieldableValidator.ValidateMetafieldRemoval(
            entity,
            "specs",
            "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.key.required");
    }

    [Fact]
    public void ValidateClearMetafields_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = MetafieldableValidator.ValidateClearMetafields(
            (TestMetafieldable)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.entity.required");
    }

    [Fact]
    public void ValidateClearMetafields_ExistingEntity_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        entity.Metafields.Add(new Metafield
        {
            Namespace = "specs",
            Key = "weight",
            Type = MetafieldType.Number,
            Value = "1.5"
        });

        // Act:
        var result = MetafieldableValidator.ValidateClearMetafields(entity);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
