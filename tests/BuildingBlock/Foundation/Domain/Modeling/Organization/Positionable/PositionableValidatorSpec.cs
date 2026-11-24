using BuildingBlock.Domain.Modeling.Organization.Positionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Positionable;

/// <summary>Specifications for <see cref="PositionableValidator"/> covering validation rules.</summary>
public class PositionableValidatorSpec
{
    #region Helper Methods

    private sealed class TestPositionable : IPositionable
    {
        public int Position { get; set; }
    }

    /// <summary>Creates a successful result for the test entity.</summary>
    private static Result<TestPositionable> Success(
        TestPositionable entity)
    {
        return Result<TestPositionable>.Success(entity);
    }

    #endregion

    #region Test Cases

    [Fact]
    public void ValidatePosition_NullEntity_ShouldFailEntityRequired()
    {
        // Arrange:
        // Act:
        var result = PositionableValidator.ValidatePosition(
            (TestPositionable)null!,
            1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.entity.required");
    }

    [Fact]
    public void ValidatePosition_Negative_ShouldFailPositionNegative()
    {
        // Arrange:
        var entity = new TestPositionable();

        // Act:
        var result = PositionableValidator.ValidatePosition(
            entity,
            PositionableConstant.Position.Min.Value - 1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.position.negative");
    }

    [Fact]
    public void ValidatePosition_Min_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable();

        // Act:
        var result = PositionableValidator.ValidatePosition(
            entity,
            PositionableConstant.Position.Min.Value);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ValidatePosition_Max_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable();

        // Act:
        var result = PositionableValidator.ValidatePosition(
            entity,
            PositionableConstant.Position.Max.Value);

        // Assert:
        result.IsSuccess.Should().BeTrue();
    }

    #endregion
}
