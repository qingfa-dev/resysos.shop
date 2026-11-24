using BuildingBlock.Domain.Modeling.Lifecycle;
using BuildingBlock.Domain.Modeling.Lifecycle.SoftDeletable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Lifecycle.SoftDeletable;

/// <summary>Tests for <see cref="SoftDeletableConstant"/> covering constant values and errors.</summary>
public sealed class SoftDeletableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldMatchDocumentedCodes()
    {
        SoftDeletableConstant.Entity.Required.Error.Code.Should().Be("soft_deletable.entity.required");
        SoftDeletableConstant.DeletedAt.Required.Error.Code.Should().Be("soft_deletable.deleted_at.required");
        SoftDeletableConstant.DeletedAt.Future.Error.Code.Should().Be("soft_deletable.deleted_at.in_future");
        SoftDeletableConstant.State.AlreadyDeleted.Error.Code.Should().Be("soft_deletable.already_deleted");
        SoftDeletableConstant.State.NotDeleted.Error.Code.Should().Be("soft_deletable.not_deleted");
        SoftDeletableConstant.DeletedBy.Required.Error.Code.Should().Be("soft_deletable.deleted_by.required");
        SoftDeletableConstant.DeletedBy.MinLength.Error.Code.Should().Be("soft_deletable.deleted_by.too_short");
        SoftDeletableConstant.DeletedBy.MaxLength.Error.Code.Should().Be("soft_deletable.deleted_by.too_long");
        SoftDeletableConstant.DeletedBy.Format.Error.Code.Should().Be("soft_deletable.actor.invalid");
    }

    [Fact]
    public void Constants_ShouldMatchLifecycleDefaults()
    {
        SoftDeletableConstant.DeletedBy.MinLength.Value.Should().Be(2);
        SoftDeletableConstant.DeletedBy.MaxLength.Value.Should().Be(256);
        SoftDeletableConstant.DeletedBy.Format.Pattern.Should().Be(@"^\S(?:.*\S)?$");
        LifecycleConstant.Timestamp.MaxFutureSkew.Value.Should().Be(300);
        LifecycleConstant.Defaults.RequireActor.Should().BeFalse();
    }
}
