using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Publishable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Publishable;

/// <summary>Tests for <see cref="PublishableConstant"/> covering constant values and errors.</summary>
public sealed class PublishableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        PublishableConstant.Entity.Required.Error.Code.Should().Be("publishable.entity.required");
        PublishableConstant.PublishedAt.Required.Error.Code.Should().Be("publishable.published_at.required");
        PublishableConstant.PublishedAt.Future.Error.Code.Should().Be("publishable.published_at.in_future");
        PublishableConstant.State.AlreadyPublished.Error.Code.Should().Be("publishable.already_published");
        PublishableConstant.State.NotPublished.Error.Code.Should().Be("publishable.not_published");
        PublishableConstant.PublishedBy.Required.Error.Code.Should().Be("publishable.published_by.required");
        PublishableConstant.PublishedBy.MinLength.Error.Code.Should().Be("publishable.published_by.too_short");
        PublishableConstant.PublishedBy.MaxLength.Error.Code.Should().Be("publishable.published_by.too_long");
        PublishableConstant.PublishedBy.Format.Error.Code.Should().Be("publishable.actor.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchLifecycleDefaults()
    {
        PublishableConstant.PublishedBy.MinLength.Value.Should().Be(2);
        PublishableConstant.PublishedBy.MaxLength.Value.Should().Be(256);
        PublishableConstant.PublishedBy.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        LifecycleConstant.Timestamp.MaxFutureSkew.Value.Should().Be(300);
        LifecycleConstant.Defaults.RequireActor.Should().BeFalse();
    }
}
