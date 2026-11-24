using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Creatable;

/// <summary>Specifications for <see cref="CreatableValidator"/> covering validation rules.</summary>
public class CreatableValidatorSpec
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<ICreatable> Success(ICreatable entity)
    {
        return Result<ICreatable>.Success(entity);
    }

    private sealed class TestCreatable : ICreatable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
    }

    #endregion

    #region Test Cases

    #region InitializeAudit

    [Fact]
    public void ValidateCreation_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = CreatableValidator.ValidateCreation<ICreatable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.entity.required");
    }

    [Fact]
    public void ValidateCreation_DefaultTimestamp_ShouldFailCreatedAtRequired()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.required");
    }

    [Fact]
    public void ValidateCreation_FutureTimestampBeyondSkew_ShouldFailCreatedAtInFuture()
    {
        // Arrange:
        var entity = new TestCreatable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 60);

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.in_future");
    }

    [Fact]
    public void ValidateCreation_AlreadyInitialized_ShouldFailAlreadyInitialized()
    {
        // Arrange:
        var entity = new TestCreatable
        {
            CreatedAtUtc = NowUtc,
            CreatedBy = "bob"
        };

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.already_initialized");
    }

    [Fact]
    public void ValidateCreation_ActorTooLong_ShouldFailCreatedByTooLong()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.too_long");
    }

    [Fact]
    public void ValidateCreation_RequireCreatedByWithoutActor_ShouldFailCreatedByRequired()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor: null,
            requireCreatedBy: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.required");
    }

    [Fact]
    public void ValidateCreation_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.actor.invalid");
    }

    [Fact]
    public void ValidateCreation_ActorTooShort_ShouldFailCreatedByTooShort()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.too_short");
    }

    [Fact]
    public void ValidateCreation_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateCreation_NullActorWithDefaultRequire_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = CreatableValidator.ValidateCreation(
            (ICreatable)entity,
            NowUtc,
            actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
    #endregion
}
