namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultErrorMappingTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void MapError_WithFailure_MapsEveryError(int errorCount)
    {
        var errors = new[] { ResultTestData.ErrorA, ResultTestData.ErrorB }
            .Take(errorCount)
            .ToArray();
        var result = Result<int>.Failure(errors)
            .MapError(error => error.WithCode($"mapped.{error.Code}"));

        result.Errors.Select(error => error.Code)
            .Should().Equal(errors.Select(error => $"mapped.{error.Code}"));
    }

    [Fact]
    public void MapError_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);

        var result = success.MapError(error => error);

        result.Should().BeSameAs(success);
    }

    [Fact]
    public void MapError_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).MapError(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MapError_WithMapperReturningDefaultError_ThrowsArgumentException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA)
            .MapError(_ => default);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*result.errors.contains_null*");
    }
}
