using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Activatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Activatable;

/// <summary>Tests for <see cref="ActivatableValidator"/> covering validation rules.</summary>
public class ActivatableValidatorTests
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
    public void ValidateActivation_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = ActivatableValidator.ValidateActivation<TestActivatable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.entity.required");
    }

    [Fact]
    public void ValidateActivation_DefaultTimestamp_ShouldFailActivatedAtRequired()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_at.required");
    }

    [Fact]
    public void ValidateActivation_FutureTimestampBeyondSkew_ShouldFailActivatedAtInFuture()
    {
        // Arrange:
        var entity = new TestActivatable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 60);

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_at.in_future");
    }

    [Fact]
    public void ValidateActivation_AlreadyActive_ShouldFailAlreadyActive()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.already_active");
    }

    [Fact]
    public void ValidateActivation_ActorTooLong_ShouldFailActivatedByTooLong()
    {
        // Arrange:
        var entity = new TestActivatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_long");
    }

    [Fact]
    public void ValidateActivation_RequireActorWithoutActor_ShouldFailActivatedByRequired()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.required");
    }

    [Fact]
    public void ValidateActivation_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.actor.invalid");
    }

    [Fact]
    public void ValidateActivation_ActorTooShort_ShouldFailActivatedByTooShort()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_short");
    }

    [Fact]
    public void ValidateActivation_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateActivation(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateDeactivation_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = ActivatableValidator.ValidateDeactivation<TestActivatable>(
            null!,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.entity.required");
    }

    [Fact]
    public void ValidateDeactivation_NotActive_ShouldFailNotActive()
    {
        // Arrange:
        var entity = new TestActivatable();

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.not_active");
    }

    [Fact]
    public void ValidateDeactivation_ActorTooLong_ShouldFailActivatedByTooLong()
    {
        // Arrange:
        var entity = ActiveEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_long");
    }

    [Fact]
    public void ValidateDeactivation_RequireActorWithoutActor_ShouldFailActivatedByRequired()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.required");
    }

    [Fact]
    public void ValidateDeactivation_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            "carol ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.actor.invalid");
    }

    [Fact]
    public void ValidateDeactivation_ActorTooShort_ShouldFailActivatedByTooShort()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("activatable.activated_by.too_short");
    }

    [Fact]
    public void ValidateDeactivation_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = ActiveEntity();

        // Act:
        var result = ActivatableValidator.ValidateDeactivation(
            entity,
            "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
    #endregion
}
