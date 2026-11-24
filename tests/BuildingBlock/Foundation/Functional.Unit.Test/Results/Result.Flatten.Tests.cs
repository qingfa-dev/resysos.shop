namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultFlattenTests
{
    #region Flatten(Result<Result<T>>)

    [Fact]
    public void Flatten_WithNestedSuccess_ReturnsInnerSuccess()
    {
        var nested = Result<Result<int>>.Success(Result<int>.Success(5));

        var result = nested.Flatten();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Flatten_WithInnerFailure_ReturnsInnerErrors()
    {
        var nested = Result<Result<int>>.Success(Result<int>.Failure(ResultTestData.ErrorA));

        var result = nested.Flatten();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Flatten_WithOuterFailure_ReturnsOuterErrors()
    {
        var nested = Result<Result<int>>.Failure(ResultTestData.ErrorB);

        var result = nested.Flatten();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Flatten_WithNullNested_ThrowsArgumentNullException()
    {
        Action act = () => ResultFlattenExtensions.Flatten<int>((Result<Result<int>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Flatten(Result<IResult<T>>)

    [Fact]
    public void FlattenInterface_WithNestedSuccess_ReturnsInnerValue()
    {
        var nested = Result<IResult<int>>.Success(Result<int>.Success(7));

        var result = nested.Flatten();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Fact]
    public void FlattenInterface_WithInnerFailure_ReturnsInnerErrors()
    {
        var nested = Result<IResult<int>>.Success(Result<int>.Failure(ResultTestData.ErrorA));

        var result = nested.Flatten();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void FlattenInterface_WithOuterFailure_ReturnsOuterErrors()
    {
        var nested = Result<IResult<int>>.Failure(ResultTestData.ErrorB);

        var result = nested.Flatten();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void FlattenInterface_WithNullNested_ThrowsArgumentNullException()
    {
        Action act = () => ResultFlattenExtensions.Flatten<int>((Result<IResult<int>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Join

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Join_DelegatesToFlatten(bool innerSucceeds)
    {
        var nested = innerSucceeds
            ? Result<Result<int>>.Success(Result<int>.Success(3))
            : Result<Result<int>>.Success(Result<int>.Failure(ResultTestData.ErrorA));

        var result = nested.Join();

        result.IsSuccess.Should().Be(innerSucceeds);
    }

    #endregion
}
