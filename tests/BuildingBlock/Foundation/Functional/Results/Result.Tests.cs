namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultTests
{
    #region Success

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Success_SetsSuccessStateAndValue(int value)
    {
        var result = Result<int>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithReferenceType_ReturnsValue()
    {
        var result = Result<string>.Success("value");

        result.Value.Should().Be("value");
    }

    #endregion

    #region Failure

    [Fact]
    public void Failure_WithSingleError_SetsFailureState()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Failure_WithMultipleErrors_PreservesOrder(int errorCount)
    {
        var errors = new[] { ResultTestData.ErrorA, ResultTestData.ErrorB, ResultTestData.ErrorC }
            .Take(errorCount)
            .ToArray();

        var result = Result<int>.Failure(errors);

        result.Errors.Select(error => error.Code)
            .Should().Equal(errors.Select(error => error.Code));
    }

    [Fact]
    public void Failure_WithNullErrors_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure((IEnumerable<Error>)null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*result.errors.required*");
    }

    [Fact]
    public void Failure_WithEmptyErrors_ThrowsArgumentException()
    {
        Action act = () => Result<int>.Failure(Array.Empty<Error>());

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.empty*");
    }

    [Fact]
    public void Failure_WithDefaultError_ThrowsArgumentException()
    {
        Action act = () => Result<int>.Failure(default(Error));

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.contains_null*");
    }

    [Fact]
    public void Failure_Errors_IsReadOnly()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA);

        var errors = (ICollection<Error>)result.Errors;

        errors.IsReadOnly.Should().BeTrue();
    }

    #endregion

    #region Value

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA);

        Action act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*result.value.invalid*");
    }

    #endregion
}
