using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Creatable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Creatable;

/// <summary>Specifications for <see cref="CreatableConstant"/> covering constant values and errors.</summary>
public sealed class CreatableConstantSpec
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        CreatableConstant.Entity.Required.Error.Code.Should().Be("creatable.entity.required");
        CreatableConstant.CreatedAt.Required.Error.Code.Should().Be("creatable.created_at.required");
        CreatableConstant.CreatedAt.Future.Error.Code.Should().Be("creatable.created_at.in_future");
        CreatableConstant.CreatedAt.AlreadyInitialized.Error.Code.Should().Be("creatable.created_at.already_initialized");
        CreatableConstant.CreatedBy.Required.Error.Code.Should().Be("creatable.created_by.required");
        CreatableConstant.CreatedBy.MinLength.Error.Code.Should().Be("creatable.created_by.too_short");
        CreatableConstant.CreatedBy.MaxLength.Error.Code.Should().Be("creatable.created_by.too_long");
        CreatableConstant.CreatedBy.Format.Error.Code.Should().Be("creatable.actor.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchLifecycleDefaults()
    {
        CreatableConstant.CreatedBy.MinLength.Value.Should().Be(2);
        CreatableConstant.CreatedBy.MaxLength.Value.Should().Be(256);
        CreatableConstant.CreatedBy.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        LifecycleConstant.Timestamp.MaxFutureSkew.Value.Should().Be(300);
        LifecycleConstant.Defaults.RequireActor.Should().BeFalse();
    }
}
