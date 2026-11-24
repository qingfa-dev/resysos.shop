using BuildingBlock.Domain.Modeling.Organization.Collectionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Collectionable;

/// <summary>Specifications for <see cref="CollectionableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class CollectionableExtensionSpec
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
    public void AddCollection_NewCollection_ShouldAddCollection()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).AddCollection("summer-2026");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().ContainSingle().Which.Should().Be("summer-2026");
    }

    [Fact]
    public void AddCollection_DuplicateCollection_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).AddCollection("summer-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "collectionable.collection.duplicate");
        entity.Collections.Should().ContainSingle();
    }

    [Fact]
    public void RemoveCollection_AssignedCollection_ShouldRemoveCollection()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).RemoveCollection("summer-2026");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCollection_MissingCollection_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).RemoveCollection("winter-2026");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "collectionable.collection.not_found");
        entity.Collections.Should().ContainSingle();
    }

    [Fact]
    public void ClearCollections_AssignedCollections_ShouldRemoveAll()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).ClearCollections<TestCollectionable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void AddCollection_TrimmedInput_ShouldStoreTrimmed()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).AddCollection("  summer-2026  ");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().ContainSingle().Which.Should().Be("summer-2026");
    }

    [Fact]
    public void AddCollection_NullCollection_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).AddCollection((string)null!);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.required");
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void RemoveCollection_TrimmedInput_ShouldRemove()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).RemoveCollection("  summer-2026  ");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void AddCollections_AllValid_ShouldAddAll()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).AddCollections<TestCollectionable, string>(new[] { "summer-2026", "winter-2026" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Count.Should().Be(2);
        entity.Collections.Should().Contain("summer-2026");
        entity.Collections.Should().Contain("winter-2026");
    }

    [Fact]
    public void AddCollections_DuplicateOfExisting_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("summer-2026");

        // Act:
        var result = Success(entity).AddCollections<TestCollectionable, string>(new[] { "winter-2026", "summer-2026" });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.duplicate");
        entity.Collections.Count.Should().Be(1);
        entity.Collections.Should().Contain("summer-2026");
        entity.Collections.Should().NotContain("winter-2026");
    }

    [Fact]
    public void AddCollections_NullItem_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).AddCollections<TestCollectionable, string>(new[] { "winter-2026", null! });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.required");
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void AddCollections_SecondItemTooLong_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        var longName = new string('x', CollectionableConstant.Collection.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).AddCollections<TestCollectionable, string>(new[] { "ok", longName });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.too_long");
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void AddCollections_ExceedingLimit_ShouldFailWithoutPartialMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        for (var i = 0; i < CollectionableConstant.Collection.MaxCount.Value - 1; i++)
        {
            entity.Collections.Add($"collection-{i}");
        }

        // Act:
        var result = Success(entity).AddCollections<TestCollectionable, string>(new[] { "extra-a", "extra-b" });

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.limit_exceeded");
        entity.Collections.Count.Should().Be(CollectionableConstant.Collection.MaxCount.Value - 1);
        entity.Collections.Should().NotContain("extra-a");
        entity.Collections.Should().NotContain("extra-b");
    }

    [Fact]
    public void SetCollections_Replace_ShouldReplaceAll()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("old");

        // Act:
        var result = Success(entity).SetCollections<TestCollectionable, string>(new[] { "a", "b" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Count.Should().Be(2);
        entity.Collections.Should().Contain("a");
        entity.Collections.Should().Contain("b");
        entity.Collections.Should().NotContain("old");
    }

    [Fact]
    public void SetCollections_Duplicates_ShouldDedup()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).SetCollections<TestCollectionable, string>(new[] { "  a  ", "a", "b" });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Count.Should().Be(2);
        entity.Collections.Should().Contain("a");
        entity.Collections.Should().Contain("b");
    }

    [Fact]
    public void SetCollections_ExceedingLimit_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCollectionable();
        Success(entity).AddCollection("keep");
        var oversized = Enumerable.Range(0, CollectionableConstant.Collection.MaxCount.Value + 1)
            .Select(i => $"collection-{i}")
            .ToList();

        // Act:
        var result = Success(entity).SetCollections<TestCollectionable, string>(oversized);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("collectionable.collection.limit_exceeded");
        entity.Collections.Should().ContainSingle().Which.Should().Be("keep");
    }

    [Fact]
    public void ClearCollections_Empty_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCollectionable();

        // Act:
        var result = Success(entity).ClearCollections<TestCollectionable, string>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().BeEmpty();
    }

    [Fact]
    public void AddCollection_GuidValid_ShouldAdd()
    {
        // Arrange:
        var entity = new TestCollectionableGuid();
        var id = Guid.NewGuid();

        // Act:
        var result = Result<TestCollectionableGuid>.Success(entity).AddCollection(id);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Collections.Should().ContainSingle().Which.Should().Be(id);
    }

    #endregion
}
