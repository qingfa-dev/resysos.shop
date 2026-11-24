using BuildingBlock.Domain.Modeling.Organization.Hierarchical;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Hierarchical;

/// <summary>Specifications for <see cref="HierarchicalExtensionSpecExtensions"/> covering extension behavior.</summary>
public class HierarchicalExtensionSpec
{
    #region Helper Methods

    private sealed class TestHierarchical : IHierarchical<Guid>
    {
        public Guid? ParentId { get; set; }
    }

    /// <summary>Creates a successful result for the test entity.</summary>
    private static Result<TestHierarchical> Success(
        TestHierarchical entity)
    {
        return Result<TestHierarchical>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void SetParent_WithParentId_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetParent(parentId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void SetParent_Null_ShouldClearParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity)
            .SetParent<TestHierarchical, Guid>(null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void SetParent_NewParent_ShouldReplaceParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).SetParent(parentId);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Move_ValidParent_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Move_SelfParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var id = Guid.NewGuid();
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            id,
            id);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.self_parent");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Reparent_Cycle_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { parentId };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid(),
            ancestors);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.cycle_detected");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Reparent_Valid_ShouldSetParentId()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void ClearParent_AssignedParent_ShouldClearParentId()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).ClearParent<TestHierarchical, Guid>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void SetParent_EmptyGuid_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };
        var original = entity.ParentId;

        // Act:
        var result = Success(entity)
            .SetParent<TestHierarchical, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.empty");
        entity.ParentId.Should().Be(original);
    }

    [Fact]
    public void SetParent_NonNullableEmptyGuid_ShouldFailParentEmpty()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).SetParent<TestHierarchical, Guid>(Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.empty");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Move_NullParent_ShouldMakeRoot()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            (Guid?)null,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Move_NullEntityId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var parentId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            parentId,
            (Guid?)null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Move_EmptyParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();

        // Act:
        var result = Success(entity).Move<TestHierarchical, Guid>(
            Guid.Empty,
            entityId);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.empty");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Reparent_NullParent_ShouldMakeRoot()
    {
        // Arrange:
        var entity = new TestHierarchical
        {
            ParentId = Guid.NewGuid()
        };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            (Guid?)null,
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Reparent_SelfParent_ShouldFailWithoutMutation()
    {
        // Arrange:
        var id = Guid.NewGuid();
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            id,
            id,
            Array.Empty<Guid>());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.self_parent");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void Reparent_EntityIdInAncestors_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { entityId, Guid.NewGuid() };

        // Act:
        var result = Success(entity).Reparent<TestHierarchical, Guid>(
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.cycle_detected");
        entity.ParentId.Should().BeNull();
    }

    [Fact]
    public void ClearParent_AlreadyRoot_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = Success(entity).ClearParent<TestHierarchical, Guid>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ParentId.Should().BeNull();
    }

    #endregion
}
