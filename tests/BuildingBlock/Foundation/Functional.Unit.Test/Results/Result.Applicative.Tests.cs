namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultApplicativeTests
{
    #region Apply

    [Fact]
    public void Apply_WithBothSuccess_AppliesFunction()
    {
        var value = Result<int>.Success(2);
        var function = Result<Func<int, string>>.Success(number => $"n{number}");

        var result = value.Apply(function);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("n2");
    }

    [Fact]
    public void Apply_WithValueFailure_ReturnsValueErrors()
    {
        var value = Result<int>.Failure(ResultTestData.ErrorA);
        var function = Result<Func<int, string>>.Success(number => number.ToString());

        var result = value.Apply(function);

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code).Should().Equal(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Apply_WithFunctionFailure_ReturnsFunctionErrors()
    {
        var value = Result<int>.Success(2);
        var function = Result<Func<int, string>>.Failure(ResultTestData.ErrorB);

        var result = value.Apply(function);

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code).Should().Equal(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Apply_WithBothFailures_AccumulatesValueThenFunctionErrors()
    {
        var value = Result<int>.Failure(ResultTestData.ErrorA);
        var function = Result<Func<int, string>>.Failure(ResultTestData.ErrorB);

        var result = value.Apply(function);

        result.Errors.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Apply_WithNullFunctionResult_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Apply<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Apply_WithNullFunction_ThrowsArgumentNullException()
    {
        var function = Result<Func<int, string>>.Success(null!);

        Action act = () => Result<int>.Success(1).Apply(function);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Lift2

    [Fact]
    public void Lift2_WithBothSuccess_ReturnsMappedValue()
    {
        var result = Result<int>.Success(2)
            .Lift2(Result<int>.Success(3), (left, right) => left + right);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Lift2_WithFirstFailure_ReturnsFirstErrors()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .Lift2(Result<int>.Success(3), (left, right) => left + right);

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code).Should().Equal(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Lift2_WithSecondFailure_ReturnsSecondErrors()
    {
        var result = Result<int>.Success(2)
            .Lift2(Result<int>.Failure(ResultTestData.ErrorB), (left, right) => left + right);

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code).Should().Equal(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Lift2_WithBothFailures_AccumulatesInInputOrder()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .Lift2(Result<int>.Failure(ResultTestData.ErrorB), (left, right) => left + right);

        result.Errors.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Lift2_WithNullSecond_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Lift2<int, int>(null!, (left, right) => left + right);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Lift2_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Lift2(Result<int>.Success(2), (Func<int, int, int>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
