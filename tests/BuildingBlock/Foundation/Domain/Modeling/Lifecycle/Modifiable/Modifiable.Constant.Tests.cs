using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.Modifiable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.Modifiable;

/// <summary>Tests for <see cref="ModifiableConstant"/> covering constant values and errors.</summary>
public sealed class ModifiableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        ModifiableConstant.Entity.Required.Error.Code.Should().Be("modifiable.entity.required");
        ModifiableConstant.State.NotInitialized.Error.Code.Should().Be("modifiable.not_initialized");
        ModifiableConstant.ModifiedAt.Required.Error.Code.Should().Be("modifiable.modified_at.required");
        ModifiableConstant.ModifiedAt.Future.Error.Code.Should().Be("modifiable.modified_at.in_future");
        ModifiableConstant.ModifiedAt.BeforeCreatedAt.Error.Code.Should().Be("modifiable.modified_at.invalid_range");
        ModifiableConstant.ModifiedBy.Required.Error.Code.Should().Be("modifiable.modified_by.required");
        ModifiableConstant.ModifiedBy.MinLength.Error.Code.Should().Be("modifiable.modified_by.too_short");
        ModifiableConstant.ModifiedBy.MaxLength.Error.Code.Should().Be("modifiable.modified_by.too_long");
        ModifiableConstant.ModifiedBy.Format.Error.Code.Should().Be("modifiable.actor.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchLifecycleDefaults()
    {
        ModifiableConstant.ModifiedBy.MinLength.Value.Should().Be(2);
        ModifiableConstant.ModifiedBy.MaxLength.Value.Should().Be(256);
        ModifiableConstant.ModifiedBy.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        LifecycleConstant.Timestamp.MaxFutureSkew.Value.Should().Be(300);
        LifecycleConstant.Defaults.RequireActor.Should().BeFalse();
    }
}
