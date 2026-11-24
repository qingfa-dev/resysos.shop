using BuildingBlock.Domain.Modeling.Organization.Positionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Positionable;

/// <summary>Tests for <see cref="PositionableConstant"/> covering constant values and errors.</summary>
public sealed class PositionableConstantTests
{
    [Fact]
    public void FailureCodes_ShouldHaveExpectedCodes()
    {
        PositionableConstant.Entity.Required.Error.Code.Should().Be("positionable.entity.required");
        PositionableConstant.Position.Min.Error.Code.Should().Be("positionable.position.negative");
        PositionableConstant.Position.Max.Error.Code.Should().Be("positionable.position.too_large");
    }

    [Fact]
    public void Constants_ShouldMatchNestedConstraints()
    {
        PositionableConstant.Position.Min.Value.Should().Be(0);
        PositionableConstant.Position.Max.Value.Should().Be(int.MaxValue);
        PositionableConstant.Position.Step.Value.Should().Be(1);
        PositionableConstant.Defaults.DefaultPosition.Should().Be(0);
    }
}
