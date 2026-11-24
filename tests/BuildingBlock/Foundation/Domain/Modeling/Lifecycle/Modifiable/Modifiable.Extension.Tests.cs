using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;
using BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Modifiable;

/// <summary>Tests for <see cref="ModifiableExtensions"/> covering extension behavior.</summary>
public class ModifiableExtensionTests
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
    public void MarkModified_InitializedEntity_ShouldSetModifiedFields()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc.Should().Be(NowUtc.AddHours(1));
        entity.ModifiedBy.Should().Be("carol");
        entity.CreatedAtUtc.Should().Be(NowUtc);
        entity.CreatedBy.Should().Be("alice");
    }

    [Fact]
    public void MarkModified_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = InitializedEntity();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).MarkModified(offset, "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.ModifiedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkModified_NullActor_ShouldClearModifiedBy()
    {
        // Arrange:
        var entity = InitializedEntity();
        entity.ModifiedBy = "stale";

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1));

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedBy.Should().BeNull();
    }

    [Fact]
    public void MarkModified_DefaultNowUtc_ShouldFailModifiedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(default, "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.required");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_UninitializedEntity_ShouldFailNotInitializedWithoutMutation()
    {
        // Arrange:
        var entity = new TestModifiable();

        // Act:
        var result = Success(entity).MarkModified(NowUtc, "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.not_initialized");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_BeforeCreatedAt_ShouldFailModifiedAtBeforeCreatedAt()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddMinutes(-1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.invalid_range");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_RequireModifiedByWithoutActor_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(
            NowUtc.AddHours(1),
            actor: null,
            requireModifiedBy: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.required");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_ActorTooLong_ShouldFailModifiedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.too_long");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestModifiable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.MarkModified(NowUtc, "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkModified_RequireMatrix_ValidActor_ShouldSucceed(bool requireModifiedBy)
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), "carol", requireModifiedBy);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc.Should().Be(NowUtc.AddHours(1));
        entity.ModifiedBy.Should().Be("carol");
    }

    [Theory]
    [InlineData(false, "modifiable.actor.invalid")]
    [InlineData(true, "modifiable.modified_by.required")]
    public void MarkModified_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireModifiedBy,
        string expectedCode)
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), " ", requireModifiedBy);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MarkModified_NullActor_RequireMatrix_ShouldMatchExpectation(bool requireModifiedBy)
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(
            NowUtc.AddHours(1),
            actor: null,
            requireModifiedBy: requireModifiedBy);

        // Assert:
        if (requireModifiedBy)
        {
            result.IsFailure.Should().BeTrue();
            result.Errors![0].Code.Should().Be("modifiable.modified_by.required");
            entity.ModifiedAtUtc.Should().BeNull();
        }
        else
        {
            result.IsSuccess.Should().BeTrue();
            entity.ModifiedAtUtc.Should().Be(NowUtc.AddHours(1));
            entity.ModifiedBy.Should().BeNull();
        }
    }

    [Fact]
    public void MarkModified_EqualToCreatedAt_ShouldSucceed()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act: nowUtc exactly equal to CreatedAtUtc satisfies nowUtc >= CreatedAtUtc.
        var result = Success(entity).MarkModified(NowUtc, "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc.Should().Be(NowUtc);
        entity.ModifiedBy.Should().Be("carol");
    }

    [Fact]
    public void MarkModified_OneTickBeforeCreatedAt_ShouldFailInvalidRangeWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddTicks(-1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.invalid_range");
        entity.ModifiedAtUtc.Should().BeNull();
        entity.ModifiedBy.Should().BeNull();
    }

    [Fact]
    public void MarkModified_ActorTooShort_ShouldFailModifiedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_by.too_short");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_UntrimmedActor_ShouldFailActorInvalidWithoutMutation()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), " carol ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.actor.invalid");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    [Fact]
    public void MarkModified_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = InitializedEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value);

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), actor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedBy.Should().Be(actor);
    }

    [Fact]
    public void MarkModified_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = InitializedEntity(
            createdAtUtc: new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5)));
        var offset = new DateTimeOffset(2024, 5, 1, 8, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).MarkModified(offset, "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.ModifiedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void MarkModified_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = InitializedEntity();

        // Act:
        var result = Success(entity).MarkModified(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void MarkModified_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var now = DateTimeOffset.UtcNow;
        var entity = InitializedEntity(createdAtUtc: now);
        var nearBoundary = now.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value - 5);

        // Act:
        var result = Success(entity).MarkModified(nearBoundary, "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.ModifiedAtUtc.Should().Be(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void MarkModified_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var now = DateTimeOffset.UtcNow;
        var entity = InitializedEntity(createdAtUtc: now);
        var justOver = now.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 5);

        // Act:
        var result = Success(entity).MarkModified(justOver, "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("modifiable.modified_at.in_future");
        entity.ModifiedAtUtc.Should().BeNull();
    }

    #endregion
    #endregion
}
