using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;
using BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Modifiable;

/// <summary>Tests for <see cref="ModifiableValidator"/> covering validation rules.</summary>
public class ModifiableValidatorTests
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestModifiable> Success(TestModifiable entity)
    {
        return Result<TestModifiable>.Success(entity);
    }

    private sealed class TestModifiable : ICreatable, IModifiable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAtUtc { get; set; }
        public string? ModifiedBy { get; set; }
    }

    private static TestModifiable InitializedEntity(
        string? createdBy = "alice",
        DateTimeOffset? createdAtUtc = null)
    {
        return new TestModifiable
        {
            CreatedAtUtc = createdAtUtc ?? NowUtc,
            CreatedBy = createdBy
        };
    }

    #endregion

    #region Test Cases

    #region MarkModified

    [Fact]
    public void ValidateModification_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = ModifiableValidator.ValidateModification<TestModifiable>(
            null!,
            NowUtc,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.entity.required");
    }

    [Fact]
    public void ValidateModification_DefaultTimestamp_ShouldFailModifiedAtRequired()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            default,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.required");
    }

    [Fact]
    public void ValidateModification_FutureTimestampBeyondSkew_ShouldFailModifiedAtInFuture()
    {
        // Arrange:
        var entity = InitializedEntity(
            createdAtUtc: DateTimeOffset.UtcNow);
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 60);

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            future,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.in_future");
    }

    [Fact]
    public void ValidateModification_UninitializedEntity_ShouldFailNotInitialized()
    {
        // Arrange:
        var entity = new TestModifiable();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.not_initialized");
    }

    [Fact]
    public void ValidateModification_BeforeCreatedAt_ShouldFailModifiedAtInvalidRange()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddMinutes(-1),
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.invalid_range");
    }

    [Fact]
    public void ValidateModification_ActorTooLong_ShouldFailModifiedByTooLong()
    {
        // Arrange:
        var entity = InitializedEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.too_long");
    }

    [Fact]
    public void ValidateModification_RequireModifiedByWithoutActor_ShouldFailModifiedByRequired()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            actor: null,
            requireModifiedBy: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.required");
    }

    [Fact]
    public void ValidateModification_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            " carol ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.actor.invalid");
    }

    [Fact]
    public void ValidateModification_ActorTooShort_ShouldFailModifiedByTooShort()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.too_short");
    }

    [Fact]
    public void ValidateModification_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateModification_NullActorWithDefaultRequire_ShouldSucceed()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = ModifiableValidator.ValidateModification(
            entity,
            NowUtc.AddHours(1),
            actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
    #endregion
}
