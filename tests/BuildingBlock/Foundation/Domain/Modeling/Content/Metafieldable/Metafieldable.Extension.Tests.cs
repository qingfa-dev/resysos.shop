using BuildingBlock.Domain.Modeling.Content.Metafieldable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Content.Metafieldable;

/// <summary>Tests for <see cref="MetafieldableExtensions"/> covering extension behavior.</summary>
public class MetafieldableExtensionTests
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
    public void SetMetafield_NewKey_ShouldAddMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var metafield = entity.Metafields.Should().ContainSingle().Which;
        metafield.Namespace.Should().Be("specs");
        metafield.Key.Should().Be("weight");
        metafield.Type.Should().Be(MetafieldType.Number);
        metafield.Value.Should().Be("1.5");
    }

    [Fact]
    public void SetMetafield_ExistingKey_ShouldUpdateTypeAndValue()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.ShortText, "old");

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "2.0");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var metafield = entity.Metafields.Should().ContainSingle().Which;
        metafield.Type.Should().Be(MetafieldType.Number);
        metafield.Value.Should().Be("2.0");
    }

    [Fact]
    public void SetMetafield_InvalidNamespace_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("Bad NS", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "metafieldable.namespace.invalid");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void SetMetafield_TooLongNamespace_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var @namespace = new string(
            'n',
            MetafieldableConstant.Namespace.MaxLength.Value + 1);

        // Act:
        var result = Success(entity)
            .SetMetafield(@namespace, "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "metafieldable.namespace.too_long");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void SetMetafield_NullKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", null, MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.key.required");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void SetMetafield_InvalidKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "Bad-Key", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("metafieldable.key.invalid");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMetafield_ExistingKey_ShouldRemoveMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).RemoveMetafield("specs", "weight");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMetafield_MissingKey_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).RemoveMetafield("specs", "depth");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "metafieldable.metafield.not_found");
        entity.Metafields.Should().ContainSingle();
    }

    [Fact]
    public void SetMetafield_DefaultType_ShouldUseShortText()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", "1.5");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var metafield = entity.Metafields.Should().ContainSingle().Which;
        metafield.Type.Should().Be(MetafieldType.ShortText);
        metafield.Value.Should().Be("1.5");
    }

    [Fact]
    public void ClearMetafields_WithEntries_ShouldClearMetafields()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var result = Success(entity).ClearMetafields();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void ClearMetafields_EmptyCollection_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity).ClearMetafields();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void SetMetafield_NullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var metafield = entity.Metafields.Should().ContainSingle().Which;
        metafield.Value.Should().BeNull();
    }

    [Fact]
    public void SetMetafield_DefaultTypeNullValue_ShouldSucceedWithNullStored()
    {
        // Arrange:
        var entity = new TestMetafieldable();

        // Act:
        var result = Success(entity).SetMetafield("specs", "weight", null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        var metafield = entity.Metafields.Should().ContainSingle().Which;
        metafield.Type.Should().Be(MetafieldType.ShortText);
        metafield.Value.Should().BeNull();
    }

    [Fact]
    public void SetMetafield_UpdateExistingWhenFull_ShouldSucceed()
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
        var result = Success(entity)
            .SetMetafield("ns", "key0", MetafieldType.Number, "updated");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Metafields.Count.Should().Be(
            MetafieldableConstant.Collection.MaxMetafields.Value);
        entity.Metafields.Single(m => m.Key == "key0").Value.Should().Be("updated");
    }

    [Fact]
    public void SetMetafield_InputFailure_ShouldPropagateWithoutMutation()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        var input = Result<TestMetafieldable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
        entity.Metafields.Should().BeEmpty();
    }

    [Fact]
    public void GetMetafield_ExistingKey_ShouldReturnMetafield()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", "weight");

        // Assert:
        metafield.Should().NotBeNull();
        metafield.Namespace.Should().Be("specs");
        metafield.Key.Should().Be("weight");
        metafield.Value.Should().Be("1.5");
    }

    [Fact]
    public void GetMetafield_MissingKey_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", "depth");

        // Assert:
        metafield.Should().BeNull();
    }

    [Fact]
    public void GetMetafield_DifferentNamespace_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("other", "weight");

        // Assert:
        metafield.Should().BeNull();
    }

    [Fact]
    public void GetMetafield_NullEntity_ShouldReturnNull()
    {
        // Arrange:
        TestMetafieldable entity = null!;

        // Act:
        var metafield = entity.GetMetafield("specs", "weight");

        // Assert:
        metafield.Should().BeNull();
    }

    [Fact]
    public void GetMetafield_NullNamespace_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield(null, "weight");

        // Assert:
        metafield.Should().BeNull();
    }

    [Fact]
    public void GetMetafield_NullKey_ShouldReturnNull()
    {
        // Arrange:
        var entity = new TestMetafieldable();
        Success(entity)
            .SetMetafield("specs", "weight", MetafieldType.Number, "1.5");

        // Act:
        var metafield = entity.GetMetafield("specs", null);

        // Assert:
        metafield.Should().BeNull();
    }

    #endregion
}
