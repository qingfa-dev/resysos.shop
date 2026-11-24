using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.SoftDeletable;

/// <summary>Specifications for <see cref="SoftDeletableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class SoftDeletableExtensionSpec
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestSoftDeletable> Success(TestSoftDeletable entity)
    {
        return Result<TestSoftDeletable>.Success(entity);
    }

    private sealed class TestSoftDeletable : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }
        public string? DeletedBy { get; set; }
    }

    private static TestSoftDeletable DeletedEntity(
        string? deletedBy = "bob",
        DateTimeOffset? deletedAtUtc = null)
    {
        return new TestSoftDeletable
        {
            IsDeleted = true,
            DeletedAtUtc = deletedAtUtc ?? NowUtc,
            DeletedBy = deletedBy
        };
    }

    #endregion

    #region Test Cases

    #region MarkDeleted

    [Fact]
    public void MarkDeleted_ActiveEntity_ShouldSetDeletionFields()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("alice");
    }

    [Fact]
    public void MarkDeleted_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).MarkDeleted(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.DeletedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.DeletedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_DefaultNowUtc_ShouldFailDeletedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(default, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_at.required");
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_AlreadyDeleted_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.already_deleted");
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("bob");
    }

    [Fact]
    public void MarkDeleted_ActorTooLong_ShouldFailDeletedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_long");
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkDeleted_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.actor.invalid");
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void MarkDeleted_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.actor.invalid");
    }

    [Fact]
    public void Restore_DeletedEntity_ShouldClearDeletionFields()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Restore_NotDeleted_ShouldFailNotDeletedWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.not_deleted");
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_RequireActorWithoutActor_ShouldFailDeletedByRequiredWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore(
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.required");
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkDeleted_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("alice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Restore_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("carol", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).MarkDeleted(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void Restore_NullActorWithDefaultRequire_ShouldClearAndSucceed()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Restore(actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "soft_deletable.actor.invalid")]
    [InlineData(true, "soft_deletable.deleted_by.required")]
    public void MarkDeleted_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, " ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "soft_deletable.actor.invalid")]
    [InlineData(true, "soft_deletable.deleted_by.required")]
    public void Restore_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore(" ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("bob");
    }

    [Fact]
    public void MarkDeleted_ActorTooShort_ShouldFailDeletedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_short");
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Restore_ActorTooShort_ShouldFailDeletedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = Success(entity).Restore("x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_short");
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("bob");
    }

    [Fact]
    public void MarkDeleted_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value);

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, actor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.DeletedBy.Should().Be(actor);
    }

    [Fact]
    public void MarkDeleted_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).MarkDeleted(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.DeletedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.DeletedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.DeletedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void MarkDeleted_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value - 5);

        // Act:
        var result = Success(entity).MarkDeleted(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void MarkDeleted_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 5);

        // Act:
        var result = Success(entity).MarkDeleted(justOver, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_at.in_future");
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_DoubleDelete_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var first = Success(entity).MarkDeleted(NowUtc, "alice");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).MarkDeleted(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.already_deleted");
        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(NowUtc);
        entity.DeletedBy.Should().Be("alice");
    }

    [Fact]
    public void Restore_DoubleRestore_ShouldFailSecondAndKeepCleared()
    {
        // Arrange:
        var entity = DeletedEntity();
        var first = Success(entity).Restore("carol");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).Restore("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.not_deleted");
        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestSoftDeletable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.MarkDeleted(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void Restore_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestSoftDeletable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.Restore("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    #endregion
    #endregion
}
