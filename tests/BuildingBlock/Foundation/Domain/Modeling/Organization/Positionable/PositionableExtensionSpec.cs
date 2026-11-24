using BuildingBlock.Domain.Modeling.Organization.Positionable;

namespace BuildingBlock.Domain.Unit.Test.Modeling.Organization.Positionable;

/// <summary>Specifications for <see cref="PositionableExtensionSpecExtensions"/> covering extension behavior.</summary>
public class PositionableExtensionSpec
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
    public void SetPosition_PositivePosition_ShouldSetPosition()
    {
        // Arrange:
        var entity = new TestPositionable();

        // Act:
        var result = Success(entity).SetPosition(5);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(5);
    }

    [Fact]
    public void SetPosition_ZeroPosition_ShouldSetPosition()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 7 };

        // Act:
        var result = Success(entity).SetPosition(0);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(0);
    }

    [Fact]
    public void SetPosition_NegativePosition_ShouldFailWithoutMutation()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 3 };

        // Act:
        var result = Success(entity).SetPosition(-1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(
            "positionable.position.negative");
        entity.Position.Should().Be(3);
    }

    [Fact]
    public void MoveBy_PositiveOffset_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 5 };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(3);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(8);
    }

    [Fact]
    public void MoveBy_NegativeOverflow_ShouldFailPositionNegative()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 0 };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(-1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.position.negative");
        entity.Position.Should().Be(0);
    }

    [Fact]
    public void MoveBy_PositiveOverflow_ShouldFailPositionTooLarge()
    {
        // Arrange:
        var entity = new TestPositionable
        {
            Position = PositionableConstant.Position.Max.Value
        };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(1);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.position.too_large");
        entity.Position.Should().Be(PositionableConstant.Position.Max.Value);
    }

    [Fact]
    public void MoveNext_AtMax_ShouldFailPositionTooLarge()
    {
        // Arrange:
        var entity = new TestPositionable
        {
            Position = PositionableConstant.Position.Max.Value
        };

        // Act:
        var result = Success(entity).MoveNext<TestPositionable>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.position.too_large");
        entity.Position.Should().Be(PositionableConstant.Position.Max.Value);
    }

    [Fact]
    public void MoveNext_Valid_ShouldIncrementByStep()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 5 };

        // Act:
        var result = Success(entity).MoveNext<TestPositionable>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(5 + PositionableConstant.Position.Step.Value);
    }

    [Fact]
    public void MovePrevious_Valid_ShouldDecrementByStep()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 5 };

        // Act:
        var result = Success(entity).MovePrevious<TestPositionable>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(5 - PositionableConstant.Position.Step.Value);
    }

    [Fact]
    public void MovePrevious_AtMin_ShouldFailPositionNegative()
    {
        // Arrange:
        var entity = new TestPositionable
        {
            Position = PositionableConstant.Position.Min.Value
        };

        // Act:
        var result = Success(entity).MovePrevious<TestPositionable>();

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be("positionable.position.negative");
        entity.Position.Should().Be(PositionableConstant.Position.Min.Value);
    }

    [Fact]
    public void ResetPosition_AssignedPosition_ShouldRestoreDefault()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 7 };

        // Act:
        var result = Success(entity).ResetPosition<TestPositionable>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(PositionableConstant.Defaults.DefaultPosition);
    }

    [Fact]
    public void SetPosition_Max_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable();

        // Act:
        var result = Success(entity).SetPosition(PositionableConstant.Position.Max.Value);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(PositionableConstant.Position.Max.Value);
    }

    [Fact]
    public void MoveBy_ZeroOffset_ShouldSucceedWithoutChange()
    {
        // Arrange:
        var entity = new TestPositionable { Position = 5 };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(0);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(5);
    }

    [Fact]
    public void MoveBy_AtMaxZeroOffset_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable
        {
            Position = PositionableConstant.Position.Max.Value
        };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(0);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(PositionableConstant.Position.Max.Value);
    }

    [Theory]
    [InlineData(5, 3, 8)]
    [InlineData(0, 0, 0)]
    [InlineData(int.MaxValue, 0, int.MaxValue)]
    [InlineData(int.MaxValue, -int.MaxValue, 0)]
    public void MoveBy_SuccessBoundaries_ShouldSetExpectedPosition(int start, int offset, int expected)
    {
        // Arrange:
        var entity = new TestPositionable { Position = start };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(offset);

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, -1, "positionable.position.negative")]
    [InlineData(1, int.MinValue, "positionable.position.negative")]
    [InlineData(0, int.MinValue, "positionable.position.negative")]
    [InlineData(int.MaxValue, 1, "positionable.position.too_large")]
    [InlineData(int.MaxValue - 1, 2, "positionable.position.too_large")]
    [InlineData(int.MaxValue, int.MaxValue, "positionable.position.too_large")]
    public void MoveBy_Overflow_ShouldFailWithoutMutation(int start, int offset, string expectedCode)
    {
        // Arrange:
        var entity = new TestPositionable { Position = start };

        // Act:
        var result = Success(entity).MoveBy<TestPositionable>(offset);

        // Assert:
        result.IsFailure.Should().BeTrue();
        result.Errors![0].Code.Should().Be(expectedCode);
        entity.Position.Should().Be(start);
    }

    [Fact]
    public void ResetPosition_AlreadyDefault_ShouldSucceed()
    {
        // Arrange:
        var entity = new TestPositionable
        {
            Position = PositionableConstant.Defaults.DefaultPosition
        };

        // Act:
        var result = Success(entity).ResetPosition<TestPositionable>();

        // Assert:
        result.IsSuccess.Should().BeTrue();
        entity.Position.Should().Be(PositionableConstant.Defaults.DefaultPosition);
    }

    #endregion
}
