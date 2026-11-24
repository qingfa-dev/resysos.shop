using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Activatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Activatable;

/// <summary>Tests for <see cref="ActivatableExtensions"/> covering extension behavior.</summary>
public class ActivatableExtensionTests
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestActivatable> Success(TestActivatable entity)
    {
        return Result<TestActivatable>.Success(entity);
    }

    private sealed class TestActivatable : IActivatable
    {
        public bool IsActive { get; set; }
        public DateTimeOffset? ActivatedAtUtc { get; set; }
        public string? ActivatedBy { get; set; }
    }

    private static TestActivatable ActiveEntity(
        string? activatedBy = "bob",
        DateTimeOffset? activatedAtUtc = null)
    {
        return new TestActivatable
        {
            IsActive = true,
            ActivatedAtUtc = activatedAtUtc ?? NowUtc,
            ActivatedBy = activatedBy
        };
    }

    #endregion

    #region Test Cases

    #region Activate

    [Fact]
    public void Activate_InactiveEntity_ShouldSetActivationFields()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("alice");
    }

    [Fact]
    public void Activate_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestActivatable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).Activate(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ActivatedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.ActivatedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void Activate_DefaultNowUtc_ShouldFailActivatedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(default, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_at.required");
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Activate_AlreadyActive_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Activate(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.already_active");
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("bob");
    }

    [Fact]
    public void Activate_ActorTooLong_ShouldFailActivatedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = new TestActivatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).Activate(NowUtc, actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_long");
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.actor.invalid");
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.actor.invalid");
    }

    [Fact]
    public void Deactivate_ActiveEntity_ShouldClearActivationFields()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Deactivate("carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
        entity.ActivatedBy.Should().BeNull();
    }

    [Fact]
    public void Deactivate_NotActive_ShouldFailNotActiveWithoutMutation()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Deactivate("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.not_active");
        entity.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_RequireActorWithoutActor_ShouldFailActivatedByRequiredWithoutMutation()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Deactivate(
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.required");
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Activate_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, "alice", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("alice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Deactivate_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Deactivate("carol", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
        entity.ActivatedBy.Should().BeNull();
    }

    [Fact]
    public void Activate_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Activate(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().BeNull();
    }

    [Fact]
    public void Deactivate_NullActorWithDefaultRequire_ShouldClearAndSucceed()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Deactivate(actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
        entity.ActivatedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "activatable.actor.invalid")]
    [InlineData(true, "activatable.activated_by.required")]
    public void Activate_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, " ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "activatable.actor.invalid")]
    [InlineData(true, "activatable.activated_by.required")]
    public void Deactivate_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Deactivate(" ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("bob");
    }

    [Fact]
    public void Activate_ActorTooShort_ShouldFailActivatedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_short");
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Deactivate_ActorTooShort_ShouldFailActivatedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = Success(entity).Deactivate("x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_short");
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("bob");
    }

    [Fact]
    public void Activate_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestActivatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value);

        // Act:
        var result = Success(entity).Activate(NowUtc, actor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ActivatedBy.Should().Be(actor);
    }

    [Fact]
    public void Activate_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestActivatable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).Activate(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ActivatedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.ActivatedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void Activate_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = Success(entity).Activate(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ActivatedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Activate_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestActivatable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value - 5);

        // Act:
        var result = Success(entity).Activate(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void Activate_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestActivatable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 5);

        // Act:
        var result = Success(entity).Activate(justOver, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_at.in_future");
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Activate_DoubleActivate_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestActivatable();
        var first = Success(entity).Activate(NowUtc, "alice");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).Activate(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.already_active");
        entity.IsActive.Should().BeTrue();
        entity.ActivatedAtUtc.Should().Be(NowUtc);
        entity.ActivatedBy.Should().Be("alice");
    }

    [Fact]
    public void Deactivate_DoubleDeactivate_ShouldFailSecondAndKeepCleared()
    {
        // Arrange:
        var entity = ActiveEntity();
        var first = Success(entity).Deactivate("carol");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).Deactivate("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.not_active");
        entity.IsActive.Should().BeFalse();
        entity.ActivatedAtUtc.Should().BeNull();
        entity.ActivatedBy.Should().BeNull();
    }

    [Fact]
    public void Activate_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestActivatable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.Activate(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void Deactivate_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestActivatable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.Deactivate("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    #endregion
    #endregion
}
