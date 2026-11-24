using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Publishable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Publishable;

/// <summary>Tests for <see cref="PublishableExtensions"/> covering extension behavior.</summary>
public class PublishableExtensionTests
{
    private static readonly DateTimeOffset NowUtc =
        new(2024, 5, 1, 12, 0, 0, TimeSpan.Zero);

    #region Helper Methods

    /// <summary>Creates a successful result wrapping the given entity.</summary>
    private static Result<TestPublishable> Success(TestPublishable entity)
    {
        return Result<TestPublishable>.Success(entity);
    }

    private sealed class TestPublishable : IPublishable
    {
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAtUtc { get; set; }
        public string? PublishedBy { get; set; }
    }

    private static TestPublishable PublishedEntity(
        string? publishedBy = "bob",
        DateTimeOffset? publishedAtUtc = null)
    {
        return new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = publishedAtUtc ?? NowUtc,
            PublishedBy = publishedBy
        };
    }

    #endregion

    #region Test Cases

    #region Publish

    [Fact]
    public void Publish_UnpublishedEntity_ShouldSetPublicationFields()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("alice");
    }

    [Fact]
    public void Publish_OffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestPublishable();
        var offset = new DateTimeOffset(2024, 5, 1, 20, 0, 0, TimeSpan.FromHours(8));

        // Act:
        var result = Success(entity).Publish(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.PublishedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.PublishedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void Publish_DefaultNowUtc_ShouldFailPublishedAtRequiredWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(default, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_at.required");
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Publish_AlreadyPublished_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.already_published");
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("bob");
    }

    [Fact]
    public void Publish_ActorTooLong_ShouldFailPublishedByTooLongWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = Success(entity).Publish(NowUtc, actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_long");
        entity.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Publish_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.actor.invalid");
        entity.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Publish_EmptyActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, string.Empty);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.actor.invalid");
    }

    [Fact]
    public void Unpublish_PublishedEntity_ShouldClearPublicationFields()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
        entity.PublishedBy.Should().BeNull();
    }

    [Fact]
    public void Unpublish_NotPublished_ShouldFailNotPublishedWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.not_published");
        entity.IsPublished.Should().BeFalse();
    }

    [Fact]
    public void Unpublish_RequireActorWithoutActor_ShouldFailPublishedByRequiredWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish(
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.required");
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Publish_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("alice");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unpublish_RequireMatrix_ValidActor_ShouldSucceed(bool requireActor)
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("carol", requireActor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
        entity.PublishedBy.Should().BeNull();
    }

    [Fact]
    public void Publish_NullActorWithDefaultRequire_ShouldSucceedAndStoreNull()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Publish(NowUtc, actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().BeNull();
    }

    [Fact]
    public void Unpublish_NullActorWithDefaultRequire_ShouldClearAndSucceed()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act: omit requireActor to exercise LifecycleConstant.Defaults.RequireActor path.
        var result = Success(entity).Unpublish(actor: null);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
        entity.PublishedBy.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "publishable.actor.invalid")]
    [InlineData(true, "publishable.published_by.required")]
    public void Publish_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, " ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(false, "publishable.actor.invalid")]
    [InlineData(true, "publishable.published_by.required")]
    public void Unpublish_WhitespaceActor_RequireMatrix_ShouldFailWithExpectedCode(
        bool requireActor,
        string expectedCode)
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish(" ", requireActor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("bob");
    }

    [Fact]
    public void Publish_ActorTooShort_ShouldFailPublishedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_short");
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Unpublish_ActorTooShort_ShouldFailPublishedByTooShortWithoutMutation()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = Success(entity).Unpublish("x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_short");
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("bob");
    }

    [Fact]
    public void Publish_ActorAtMaxLength_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value);

        // Act:
        var result = Success(entity).Publish(NowUtc, actor);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.PublishedBy.Should().Be(actor);
    }

    [Fact]
    public void Publish_NegativeOffsetTimestamp_ShouldStoreUtcInstant()
    {
        // Arrange:
        var entity = new TestPublishable();
        var offset = new DateTimeOffset(2024, 5, 1, 7, 0, 0, TimeSpan.FromHours(-5));

        // Act:
        var result = Success(entity).Publish(offset, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.PublishedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
        entity.PublishedAtUtc.Should().Be(offset.ToUniversalTime());
    }

    [Fact]
    public void Publish_AlreadyUtcTimestamp_ShouldKeepZeroOffset()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = Success(entity).Publish(NowUtc, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.PublishedAtUtc!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Publish_NearSkewBoundary_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();
        var nearBoundary = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value - 5);

        // Act:
        var result = Success(entity).Publish(nearBoundary, "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(nearBoundary.ToUniversalTime());
    }

    [Fact]
    public void Publish_JustOverSkewBoundary_ShouldFailInFutureWithoutMutation()
    {
        // Arrange:
        var entity = new TestPublishable();
        var justOver = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 5);

        // Act:
        var result = Success(entity).Publish(justOver, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_at.in_future");
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
    }

    [Fact]
    public void Publish_DoublePublish_ShouldFailSecondAndKeepOriginal()
    {
        // Arrange:
        var entity = new TestPublishable();
        var first = Success(entity).Publish(NowUtc, "alice");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).Publish(NowUtc.AddHours(1), "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.already_published");
        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(NowUtc);
        entity.PublishedBy.Should().Be("alice");
    }

    [Fact]
    public void Unpublish_DoubleUnpublish_ShouldFailSecondAndKeepCleared()
    {
        // Arrange:
        var entity = PublishedEntity();
        var first = Success(entity).Unpublish("carol");
        first.IsSuccess.Should().BeTrue();

        // Act:
        var result = Success(entity).Unpublish("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.not_published");
        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
        entity.PublishedBy.Should().BeNull();
    }

    [Fact]
    public void Publish_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestPublishable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.Publish(NowUtc, "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    [Fact]
    public void Unpublish_InputFailure_ShouldPropagateErrors()
    {
        // Arrange:
        var input = Result<TestPublishable>.Failure(
            Error.Validation("test.fail", "boom"));

        // Act:
        var result = input.Unpublish("carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("test.fail");
    }

    #endregion
    #endregion
}
