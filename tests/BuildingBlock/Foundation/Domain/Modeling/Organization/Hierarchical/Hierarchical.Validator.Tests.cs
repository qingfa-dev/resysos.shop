using BuildingBlock.Domain.Modeling.Organization.Hierarchical;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Hierarchical;

/// <summary>Tests for <see cref="HierarchicalValidator"/> covering validation rules.</summary>
public class HierarchicalValidatorTests
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
    public void ValidateParent_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            (TestHierarchical)null!,
            Guid.NewGuid());

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.entity.required");
    }

    [Fact]
    public void ValidateParent_EmptyGuid_ShouldFailParentEmpty()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.empty");
    }

    [Fact]
    public void ValidateParent_NullParent_ShouldSucceedAsRoot()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            (Guid?)null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParent_ValidParent_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParent_SelfParent_ShouldFailSelfParent()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var id = Guid.NewGuid();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            id,
            id);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.self_parent");
    }

    [Fact]
    public void ValidateParent_DifferentEntityId_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateParent_ParentInAncestors_ShouldFailCycleDetected()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { parentId, Guid.NewGuid() };

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.cycle_detected");
    }

    [Fact]
    public void ValidateParent_EntityInAncestors_ShouldFailCycleDetected()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var entityId = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var ancestors = new[] { entityId, Guid.NewGuid() };

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            parentId,
            entityId,
            ancestors);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.parent.cycle_detected");
    }

    [Fact]
    public void ValidateParent_NoCycle_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateParent<TestHierarchical, Guid>(
            entity,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new[] { Guid.NewGuid() });

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateDepth_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            (TestHierarchical)null!,
            0);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.entity.required");
    }

    [Fact]
    public void ValidateDepth_Negative_ShouldFailDepthExceeded()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Level.Min.Value - 1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.depth.exceeded");
    }

    [Fact]
    public void ValidateDepth_TooDeep_ShouldFailDepthExceeded()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Level.Max.Value + 1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.depth.exceeded");
    }

    [Fact]
    public void ValidateDepth_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var root = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Level.Min.Value);
        var max = HierarchicalValidator.ValidateDepth(
            entity,
            HierarchicalConstant.Level.Max.Value);

        // Assert:
        root.IsSuccess.Should().BeTrue();
        max.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidatePath_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = HierarchicalValidator.ValidatePath(
            (TestHierarchical)null!,
            "a/b");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.entity.required");
    }

    [Fact]
    public void ValidatePath_TooLong_ShouldFailPathTooLong()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var longPath = new string(
            'a',
            HierarchicalConstant.Parent.MaxPathLength.Value + 1);

        // Act:
        var result = HierarchicalValidator.ValidatePath(
            entity,
            longPath);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("hierarchical.path.too_long");
    }

    [Fact]
    public void ValidatePath_Valid_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var nullPath = HierarchicalValidator.ValidatePath(
            entity,
            null);
        var normalPath = HierarchicalValidator.ValidatePath(
            entity,
            "root/child");

        // Assert:
        nullPath.IsSuccess.Should().BeTrue();
        normalPath.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(32, true)]
    [InlineData(33, false)]
    public void ValidateDepth_Boundaries_ShouldMatchRange(int depth, bool expectedSuccess)
    {
        // Arrange:
        var entity = new TestHierarchical();

        // Act:
        var result = HierarchicalValidator.ValidateDepth(entity, depth);

        // Assert:
        if (expectedSuccess)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else
        {
            result.IsFailure.Should().BeTrue();
            result.Errors![0].Code.Should().Be("hierarchical.depth.exceeded");
        }
    }

    [Fact]
    public void ValidatePath_AtLimit_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestHierarchical();
        var atLimit = new string(
            'a',
            HierarchicalConstant.Parent.MaxPathLength.Value);

        // Act:
        var result = HierarchicalValidator.ValidatePath(entity, atLimit);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
