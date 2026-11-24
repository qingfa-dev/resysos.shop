using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Activatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Activatable;

/// <summary>Specifications for <see cref="ActivatableConstant"/> covering constant values and errors.</summary>
public sealed class ActivatableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        ActivatableConstant.Entity.Required.Error.Code.Should().Be("activatable.entity.required");
        ActivatableConstant.ActivatedAt.Required.Error.Code.Should().Be("activatable.activated_at.required");
        ActivatableConstant.ActivatedAt.Future.Error.Code.Should().Be("activatable.activated_at.in_future");
        ActivatableConstant.State.AlreadyActive.Error.Code.Should().Be("activatable.already_active");
        ActivatableConstant.State.NotActive.Error.Code.Should().Be("activatable.not_active");
        ActivatableConstant.ActivatedBy.Required.Error.Code.Should().Be("activatable.activated_by.required");
        ActivatableConstant.ActivatedBy.MinLength.Error.Code.Should().Be("activatable.activated_by.too_short");
        ActivatableConstant.ActivatedBy.MaxLength.Error.Code.Should().Be("activatable.activated_by.too_long");
        ActivatableConstant.ActivatedBy.Format.Error.Code.Should().Be("activatable.actor.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchLifecycleDefaults()
    {
        ActivatableConstant.ActivatedBy.MinLength.Value.Should().Be(2);
        ActivatableConstant.ActivatedBy.MaxLength.Value.Should().Be(256);
        ActivatableConstant.ActivatedBy.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        LifecycleConstant.Timestamp.MaxFutureSkew.Value.Should().Be(300);
        LifecycleConstant.Defaults.RequireActor.Should().BeFalse();
    }
}
