namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultFunctorTests
{
    #region Map

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public void Map_WithSuccess_MapsValue(int value)
    {
        var result = Result<int>.Success(value).Map(number => $"n{number}");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be($"n{value}");
    }

    [Fact]
    public void Map_WithFailure_PassesErrorsThrough()
    {
        var invoked = false;
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .Map(number =>
            {
                invoked = true;
                return number;
            });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Map<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region MapAsync

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public async Task MapAsync_WithSuccess_MapsValue(int value)
    {
        var result = await Result<int>.Success(value)
            .MapAsync((number, _) => ValueTask.FromResult(number * 2));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value * 2);
    }

    [Fact]
    public async Task MapAsync_WithFailure_DoesNotInvokeMapper()
    {
        var invoked = false;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).MapAsync(
            (number, _) =>
            {
                invoked = true;
                return ValueTask.FromResult(number);
            });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_WithNullMapper_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).MapAsync<int>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MapAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Success(1)
            .MapAsync((number, _) => ValueTask.FromResult(number), ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task MapAsync_WhenMapperCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Success(1).MapAsync(
            (number, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult(number);
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
