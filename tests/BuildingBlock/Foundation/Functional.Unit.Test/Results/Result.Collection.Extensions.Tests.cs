namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultCollectionExtensionsTests
{
    [Fact]
    public void SequenceExtension_WithResults_ReturnsValues()
    {
        var results = new IResult<int>[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
        };

        var result = results.Sequence();

        result.Value.Should().Equal(1, 2);
    }

    [Fact]
    public void SequenceExtension_WithNullResults_ThrowsArgumentNullException()
    {
        Action act = () => ((IEnumerable<IResult<int>>)null!).Sequence();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TraverseExtension_WithItems_ReturnsValues()
    {
        var result = new[] { 1, 2 }.Traverse(value => Result<string>.Success($"n{value}"));

        result.Value.Should().Equal("n1", "n2");
    }

    [Fact]
    public void TraverseExtension_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => new[] { 1 }.Traverse<int, string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CombineResultsExtension_WithEnumerable_AccumulatesErrors()
    {
        var results = (IEnumerable<IResult>)new IResult[]
        {
            Result<int>.Failure(ResultTestData.ErrorA),
            Result<string>.Failure(ResultTestData.ErrorB),
        };

        var result = results.CombineResults();

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void CombineResultsExtension_WithNullEnumerable_ThrowsArgumentNullException()
    {
        Action act = () => ((IEnumerable<IResult>)null!).CombineResults();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CombineResultsExtension_WithParams_ReturnsSuccess()
    {
        var result = ResultCollectionExtensions.CombineResults(
            Result<int>.Success(1),
            Result<string>.Success("value"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineResultsExtension_WithNoParams_ReturnsSuccess()
    {
        var result = ResultCollectionExtensions.CombineResults();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineResultsExtension_WithNullParams_ThrowsArgumentNullException()
    {
        Action act = () => ResultCollectionExtensions.CombineResults((IResult[])null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
