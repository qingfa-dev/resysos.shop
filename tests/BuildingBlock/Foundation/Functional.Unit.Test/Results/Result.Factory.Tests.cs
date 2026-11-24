namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultFactoryTests
{
    [Fact]
    public void Result_Success_WithoutValue_ReturnsUnitSuccess()
    {
        var result = Result<Unit>.Success(Unit.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void Result_Success_WithValue_ReturnsSuccess(int value)
    {
        var result = Result<int>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Result_Failure_WithSingleError_ReturnsFailure()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Result_Failure_WithParamsErrors_ReturnsFailure()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA, ResultTestData.ErrorB);

        result.Errors.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Result_Failure_WithEnumerableErrors_ReturnsFailure()
    {
        IEnumerable<Error> errors = new[] { ResultTestData.ErrorA };

        var result = Result<int>.Failure(errors);

        result.Errors.Should().ContainSingle();
    }
}
