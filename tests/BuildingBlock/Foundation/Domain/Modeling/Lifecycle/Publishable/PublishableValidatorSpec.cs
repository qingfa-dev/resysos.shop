using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Publishable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Publishable;

/// <summary>Specifications for <see cref="PublishableValidator"/> covering validation rules.</summary>
public class PublishableValidatorSpec
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
    public void ValidatePublication_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = PublishableValidator.ValidatePublication<TestPublishable>(
            null!,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.entity.required");
    }

    [Fact]
    public void ValidatePublication_DefaultTimestamp_ShouldFailPublishedAtRequired()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            default,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_at.required");
    }

    [Fact]
    public void ValidatePublication_FutureTimestampBeyondSkew_ShouldFailPublishedAtInFuture()
    {
        // Arrange:
        var entity = new TestPublishable();
        var future = DateTimeOffset.UtcNow.AddSeconds(
            LifecycleConstant.Timestamp.MaxFutureSkew.Value + 60);

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            future,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_at.in_future");
    }

    [Fact]
    public void ValidatePublication_AlreadyPublished_ShouldFailAlreadyPublished()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.already_published");
    }

    [Fact]
    public void ValidatePublication_ActorTooLong_ShouldFailPublishedByTooLong()
    {
        // Arrange:
        var entity = new TestPublishable();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_long");
    }

    [Fact]
    public void ValidatePublication_RequireActorWithoutActor_ShouldFailPublishedByRequired()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.required");
    }

    [Fact]
    public void ValidatePublication_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            " alice ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.actor.invalid");
    }

    [Fact]
    public void ValidatePublication_ActorTooShort_ShouldFailPublishedByTooShort()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_short");
    }

    [Fact]
    public void ValidatePublication_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidatePublication(
            entity,
            NowUtc,
            "alice");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidateUnpublication_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange + Act:
        var result = PublishableValidator.ValidateUnpublication<TestPublishable>(
            null!,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.entity.required");
    }

    [Fact]
    public void ValidateUnpublication_NotPublished_ShouldFailNotPublished()
    {
        // Arrange:
        var entity = new TestPublishable();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.not_published");
    }

    [Fact]
    public void ValidateUnpublication_ActorTooLong_ShouldFailPublishedByTooLong()
    {
        // Arrange:
        var entity = PublishedEntity();
        var actor = new string('a', LifecycleConstant.Actor.MaxLength.Value + 1);

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            actor);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_long");
    }

    [Fact]
    public void ValidateUnpublication_RequireActorWithoutActor_ShouldFailPublishedByRequired()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            actor: null,
            requireActor: true);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.required");
    }

    [Fact]
    public void ValidateUnpublication_UntrimmedActor_ShouldFailActorInvalid()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol ");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.actor.invalid");
    }

    [Fact]
    public void ValidateUnpublication_ActorTooShort_ShouldFailPublishedByTooShort()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "x");

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("publishable.published_by.too_short");
    }

    [Fact]
    public void ValidateUnpublication_ValidInput_ShouldSucceed()
    {
        // Arrange:
        var entity = PublishedEntity();

        // Act:
        var result = PublishableValidator.ValidateUnpublication(
            entity,
            "carol");

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
    #endregion
}
