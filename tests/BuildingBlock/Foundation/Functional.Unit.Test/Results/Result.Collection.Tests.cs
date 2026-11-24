namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultCollectionTests
{
    private static readonly ResultCollection Collection = new();

    #region Combine

    [Fact]
    public void Combine_WithNoResults_ReturnsSuccess()
    {
        var result = Collection.Combine(Array.Empty<IResult>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Combine_WithAllSuccessfulResults_ReturnsSuccess()
    {
        var result = Collection.Combine(new IResult[]
        {
            Result<int>.Success(1),
            Result<string>.Success("value"),
            Result<Unit>.Success(Unit.Value),
        });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_WithFailures_AccumulatesEveryErrorInOrder()
    {
        var result = Collection.Combine(new IResult[]
        {
            Result<int>.Failure(ResultTestData.ErrorA),
            Result<int>.Success(1),
            Result<string>.Failure(ResultTestData.ErrorB, ResultTestData.ErrorC),
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code).Should().Equal(
            ResultTestData.ErrorA.Code,
            ResultTestData.ErrorB.Code,
            ResultTestData.ErrorC.Code);
    }

    [Fact]
    public void Combine_WithNullResults_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Combine(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Combine_WithNullItem_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Combine(new IResult[] { null! });

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Sequence

    [Fact]
    public void Sequence_WithNoResults_ReturnsEmptySuccess()
    {
        var result = Collection.Sequence(Array.Empty<IResult<int>>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Sequence_WithAllSuccessfulResults_ReturnsValuesInOrder()
    {
        var result = Collection.Sequence(new IResult<int>[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3),
        });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Sequence_WithFailures_ReturnsOnlyErrors()
    {
        var result = Collection.Sequence(new IResult<int>[]
        {
            Result<int>.Success(1),
            Result<int>.Failure(ResultTestData.ErrorA),
            Result<int>.Failure(ResultTestData.ErrorB),
        });

        result.IsFailure.Should().BeTrue();
        result.Errors.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Sequence_WithNullResults_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Sequence<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Sequence_WithNullItem_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Sequence(new IResult<int>[] { null! });

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Traverse

    [Fact]
    public void Traverse_WithNoItems_ReturnsEmptySuccess()
    {
        var result = Collection.Traverse<int, string>(Array.Empty<int>(), value => Result<string>.Success($"n{value}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Traverse_WithAllMappedResults_ReturnsValues()
    {
        var result = Collection.Traverse(
            new[] { 1, 2, 3 },
            value => Result<string>.Success($"n{value}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Equal("n1", "n2", "n3");
    }

    [Fact]
    public void Traverse_WithMappingFailures_ReturnsErrors()
    {
        var result = Collection.Traverse(
            new[] { 1, 2, 3 },
            value => value == 2
                ? Result<int>.Failure(ResultTestData.ErrorA)
                : Result<int>.Success(value));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Traverse_WithNullSource_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Traverse<int, int>(null!, value => Result<int>.Success(value));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Traverse_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => Collection.Traverse<int, int>(new[] { 1 }, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}
