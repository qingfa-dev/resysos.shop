namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultInterfaceTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void IResult_IsFailure_IsInverseOfIsSuccess(bool isSuccess, bool expectedIsFailure)
    {
        IResult result = new MinimalResult { IsSuccess = isSuccess };

        result.IsFailure.Should().Be(expectedIsFailure);
    }

    private sealed class MinimalResult : IResult
    {
        public bool IsSuccess { get; init; }

        public IReadOnlyList<Error> Errors { get; init; } = Array.Empty<Error>();
    }
}
