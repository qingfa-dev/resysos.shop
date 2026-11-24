using BuildingBlock.Domain.Modeling.Organization.Collectionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Collectionable;

/// <summary>Tests for <see cref="CollectionableValidator"/> covering validation rules.</summary>
public class CollectionableValidatorTests
{
    #region Helper Methods

    private sealed class TestCollectionable : ICollectionable<string>
    {
        public ICollection<string> Collections { get; } = new List<string>();
    }

    private sealed class TestCollectionableGuid : ICollectionable<Guid>
    {
        public ICollection<Guid> Collections { get; } = new List<Guid>();
    }

    /// <summary>Creates a successful result for the test entity.</summary>
    private static Result<TestCollectionable> Success(
        TestCollectionable entity)
    {
        return Result<TestCollectionable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidateAddCollection_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            (TestCollectionable)null!,
            "summer-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "collectionable.entity.required");
    }

    [Fact]
    public void ValidateAddCollection_NullCollection_ShouldFailCollectionRequired()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            (string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.required");
    }

    [Fact]
    public void ValidateAddCollection_EmptyGuid_ShouldFailCollectionEmpty()
    {
        // Arrange:
        var entity = new TestCollectionableGuid();

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.empty");
    }

    [Fact]
    public void ValidateAddCollection_BlankName_ShouldFailCollectionRequired()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            "   ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.required");
    }

    [Fact]
    public void ValidateAddCollection_TooLongName_ShouldFailCollectionTooLong()
    {
        // Arrange:
        var entity = new TestCollectionable();
        var longName = new string(
            'x',
            CollectionableConstant.Collection.MaxLength.Value + 1);

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            longName);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.too_long");
    }

    [Fact]
    public void ValidateAddCollection_Duplicate_ShouldFailCollectionDuplicate()
    {
        // Arrange:
        var entity = new TestCollectionable();
        entity.Collections.Add("summer-2026");

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            "summer-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.duplicate");
    }

    [Fact]
    public void ValidateAddCollection_LimitExceeded_ShouldFailCollectionLimitExceeded()
    {
        // Arrange:
        var entity = new TestCollectionable();
        for (var i = 0; i < CollectionableConstant.Collection.MaxCount.Value; i++)
        {
            entity.Collections.Add($"collection-{i}");
        }

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            "one-more");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.limit_exceeded");
    }

    [Fact]
    public void ValidateAddCollection_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(
            entity,
            "summer-2026");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRemoveCollection_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = CollectionableValidator.ValidateRemoveCollection(
            (TestCollectionable)null!,
            "summer-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.entity.required");
    }

    [Fact]
    public void ValidateRemoveCollection_NullCollection_ShouldFailCollectionRequired()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = CollectionableValidator.ValidateRemoveCollection(
            entity,
            (string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.required");
    }

    [Fact]
    public void ValidateRemoveCollection_Missing_ShouldFailCollectionNotFound()
    {
        // Arrange:
        var entity = new TestCollectionable();
        entity.Collections.Add("summer-2026");

        // Act:
        var result = CollectionableValidator.ValidateRemoveCollection(
            entity,
            "winter-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.not_found");
    }

    [Fact]
    public void ValidateRemoveCollection_Assigned_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCollectionable();
        entity.Collections.Add("summer-2026");

        // Act:
        var result = CollectionableValidator.ValidateRemoveCollection(
            entity,
            "summer-2026");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateAddCollection_GuidValid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCollectionableGuid();

        // Act:
        var result = CollectionableValidator.ValidateAddCollection(entity, Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
