using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Creatable;

/// <summary>Specifications for <see cref="CreatableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class CreatableExtensionSpec
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
    public void InitializeAudit_NewEntity_ShouldSetCreationFields()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().Be("alice");
    }

    [Fact]
    public void InitializeAudit_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestCreatable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).InitializeAudit(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Offset.Should().Be(TimeSpan.Zero);
        entity.CreatedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_DefaultNowUtc_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(default, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.required");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void InitializeAudit_RequireCreatedByWithoutActor_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(
            NowUtc,
            actor: null,
            requireCreatedBy: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.required");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_AlreadyInitialized_ShouldFailAndKeepOriginalValues()
    {
        // Arrange:
        var entity = new TestCreatable
        {
            CreatedAtUtc = NowUtc,
            CreatedBy = "bob"
        };

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.already_initialized");
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().Be("bob");
    }

    [Fact]
    public void InitializeAudit_ActorTooLong_ShouldFailCreatedByTooLong()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.too_long");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.actor.invalid");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
    }

    [Fact]
    public void InitializeAudit_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.actor.invalid");
    }

    [Fact]
    public void InitializeAudit_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<ICreatable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void InitializeAudit_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act: omit requireCreatedBy to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).InitializeAudit(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializeAudit_RequireMatrix_ValidActor_ShouldSucceed(bool requireCreatedBy)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice", requireCreatedBy);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().Be("alice");
    }

    [Theory]
    [InlineData(false, "creatable.actor.invalid")]
    [InlineData(true, "creatable.created_by.required")]
    public void InitializeAudit_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireCreatedBy,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, " ", requireCreatedBy);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
        entity.CreatedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializeAudit_NullActor_RequireMatrix_ShouldMatchExpectation(bool requireCreatedBy)
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor: null, requireCreatedBy: requireCreatedBy);

        // Assert:
        if (requireCreatedBy)
        {
            result.IsFailure.Should().BeTrue();
            result.Errors![0].Code.Should().Be("creatable.created_by.required");
            entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
        }
        else
        {
            result.IsSuccess.Should().BeTrue();
            entity.CreatedAtUtc.Should().Be(NowUtc);
            entity.CreatedBy.Should().BeNull();
        }
    }

    [Fact]
    public void InitializeAudit_ActorTooShort_ShouldFailCreatedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_by.too_short");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void InitializeAudit_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value);

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, actor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedBy.Should().Be(actor);
    }

    [Fact]
    public void InitializeAudit_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestCreatable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).InitializeAudit(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Offset.Should().Be(TimeSpan.Zero);
        entity.CreatedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestCreatable();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Offset.Should().Be(TimeSpan.Zero);
        entity.CreatedAtUtc.Should().Be(NowUtc.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestCreatable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value - 5);

        // Act:
        var result = Success(entity).InitializeAudit(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.CreatedAtUtc.Should().Be(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void InitializeAudit_JustOverSkewBoundary_ShouldFailCreatedAtInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestCreatable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 5);

        // Act:
        var result = Success(entity).InitializeAudit(justOver, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.in_future");
        entity.CreatedAtUtc.Should().Be(default(DateTimeOffset));
        entity.CreatedBy.Should().BeNull();
    }

    [Fact]
    public void InitializeAudit_DoubleInitialize_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestCreatable();
        var first = Success(entity).InitializeAudit(NowUtc, "alice");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).InitializeAudit(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("creatable.created_at.already_initialized");
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().Be("alice");
    }

    #endregion
    #endregion
}
