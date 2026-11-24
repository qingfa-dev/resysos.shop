namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultLinqTests
{
    #region Select

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public void Select_WithSuccess_MapsValue(int value)
    {
        var result = Result<int>.Success(value).Select(number => number * 2);

        result.Value.Should().Be(value * 2);
    }

    [Fact]
    public void Select_WithFailure_PassesErrorsThrough()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA).Select(number => number * 2);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Select_WithNullResult_ThrowsArgumentNullException()
    {
        Action act = () => ResultLinqExtensions.Select<int, string>(null!, value => value.ToString());

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SelectMany

    [Fact]
    public void SelectMany_WithSuccess_ProjectsValue()
    {
        var result = Result<int>.Success(2)
            .SelectMany(
                value => Result<int>.Success(value * 3),
                (value, intermediate) => value + intermediate);

        result.Value.Should().Be(8);
    }

    [Fact]
    public void SelectMany_WithFailingBinder_ReturnsBinderFailure()
    {
        var projectorInvoked = false;
        var result = Result<int>.Success(2).SelectMany(
            _ => Result<int>.Failure(ResultTestData.ErrorA),
            (value, intermediate) =>
            {
                projectorInvoked = true;
                return value + intermediate;
            });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        projectorInvoked.Should().BeFalse();
    }

    [Fact]
    public void SelectMany_WithOriginalFailure_DoesNotInvokeBinder()
    {
        var invoked = false;
        var result = Result<int>.Failure(ResultTestData.ErrorA).SelectMany(
            value =>
            {
                invoked = true;
                return Result<int>.Success(value);
            },
            (value, intermediate) => value + intermediate);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void SelectMany_WithQuerySyntax_ComposesResults()
    {
        var result = from x in Result<int>.Success(2)
                     from y in Result<int>.Success(3)
                     select x + y;

        result.Value.Should().Be(5);
    }

    [Fact]
    public void SelectMany_WithNullResult_ThrowsArgumentNullException()
    {
        Action act = () => ResultLinqExtensions.SelectMany<int, int, int>(
            null!,
            value => Result<int>.Success(value),
            (value, intermediate) => value + intermediate);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectMany_WithNullBinder_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).SelectMany<int, int, int>(
            null!,
            (value, intermediate) => value + intermediate);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SelectMany_WithNullProjector_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).SelectMany<int, int, int>(
            value => Result<int>.Success(value),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Where

    [Fact]
    public void Where_WithSatisfiedPredicate_ReturnsSameInstance()
    {
        var success = Result<int>.Success(2);

        var result = success.Where(value => value > 0);

        result.Should().BeSameAs(success);
    }

    [Fact]
    public void Where_WithUnsatisfiedPredicate_ReturnsDefaultFilterError()
    {
        var result = Result<int>.Success(2).Where(value => value > 10);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultConstant.Constraints.Filter.Unsatisfied.Code);
    }

    [Fact]
    public void Where_WithUnsatisfiedPredicateAndCustomError_UsesCustomError()
    {
        var custom = Error.Validation("test.where", "custom");

        var result = Result<int>.Success(2).Where(value => value > 10, custom);

        result.Errors[0].Code.Should().Be("test.where");
    }

    [Fact]
    public void Where_WithFailure_DoesNotInvokePredicate()
    {
        var invoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = failure.Where(value =>
        {
            invoked = true;
            return value > 0;
        });

        result.Should().BeSameAs(failure);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Where_WithNullResult_ThrowsArgumentNullException()
    {
        Action act = () => ResultLinqExtensions.Where<int>(null!, value => value > 0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Where_WithNullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Where(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Where_WithQuerySyntax_FiltersValue()
    {
        var result = from value in Result<int>.Success(5)
                     where value > 0
                     select value * 2;

        result.Value.Should().Be(10);
    }

    [Fact]
    public void Where_WithQuerySyntaxUnsatisfied_ReturnsFailure()
    {
        var result = from value in Result<int>.Success(5)
                     where value > 10
                     select value * 2;

        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
