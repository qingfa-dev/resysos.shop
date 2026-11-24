using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.SoftDeletable;

/// <summary>Tests for <see cref="SoftDeletableValidator"/> covering validation rules.</summary>
public class SoftDeletableValidatorTests
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
    public void ValidateDeletion_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = SoftDeletableValidator.ValidateDeletion<TestSoftDeletable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.entity.required");
    }

    [Fact]
    public void ValidateDeletion_DefaultTimestamp_ShouldFailDeletedAtRequired()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_at.required");
    }

    [Fact]
    public void ValidateDeletion_FutureTimestampBeyondSkew_ShouldFailDeletedAtInFuture()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 60);

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_at.in_future");
    }

    [Fact]
    public void ValidateDeletion_AlreadyDeleted_ShouldFailAlreadyDeleted()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.already_deleted");
    }

    [Fact]
    public void ValidateDeletion_ActorTooLong_ShouldFailDeletedByTooLong()
    {
        // Arrange:
        var entity = new TestSoftDeletable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_long");
    }

    [Fact]
    public void ValidateDeletion_RequireActorWithoutActor_ShouldFailDeletedByRequired()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.required");
    }

    [Fact]
    public void ValidateDeletion_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.actor.invalid");
    }

    [Fact]
    public void ValidateDeletion_ActorTooShort_ShouldFailDeletedByTooShort()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_short");
    }

    [Fact]
    public void ValidateDeletion_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateDeletion(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateRestoration_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = SoftDeletableValidator.ValidateRestoration<TestSoftDeletable>(
            null!,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.entity.required");
    }

    [Fact]
    public void ValidateRestoration_NotDeleted_ShouldFailNotDeleted()
    {
        // Arrange:
        var entity = new TestSoftDeletable();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.not_deleted");
    }

    [Fact]
    public void ValidateRestoration_ActorTooLong_ShouldFailDeletedByTooLong()
    {
        // Arrange:
        var entity = DeletedEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_long");
    }

    [Fact]
    public void ValidateRestoration_RequireActorWithoutActor_ShouldFailDeletedByRequired()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.required");
    }

    [Fact]
    public void ValidateRestoration_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "carol ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.actor.invalid");
    }

    [Fact]
    public void ValidateRestoration_ActorTooShort_ShouldFailDeletedByTooShort()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("soft_deletable.deleted_by.too_short");
    }

    [Fact]
    public void ValidateRestoration_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = DeletedEntity();

        // Act:
        var result = SoftDeletableValidator.ValidateRestoration(
            entity,
            "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
    #endregion
}
