namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultBindTests
{
    #region Bind

    [Fact]
    public void Bind_WithSuccess_ReturnsBinderResult()
    {
        var result = Result<int>.Success(2)
            .Bind(value => Result<string>.Success($"n{value}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("n2");
    }

    [Fact]
    public void Bind_WithSuccessAndFailingBinder_ReturnsBinderFailure()
    {
        var result = Result<int>.Success(2)
            .Bind(_ => Result<string>.Failure(ResultTestData.ErrorB));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void Bind_WithFailure_DoesNotInvokeBinder()
    {
        var invoked = false;
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .Bind(value =>
            {
                invoked = true;
                return Result<int>.Success(value);
            });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Bind_WithNullBinder_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Bind<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region BindAsync

    [Fact]
    public async Task BindAsync_WithSuccess_ReturnsBinderResult()
    {
        var result = await Result<int>.Success(2).BindAsync(
            (number, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(number * 3)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public async Task BindAsync_WithSuccessAndFailingBinder_ReturnsBinderFailure()
    {
        var result = await Result<int>.Success(2).BindAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Failure(ResultTestData.ErrorB)));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public async Task BindAsync_WithFailure_DoesNotInvokeBinder()
    {
        var invoked = false;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).BindAsync(
            (number, _) =>
            {
                invoked = true;
                return ValueTask.FromResult<IResult<int>>(Result<int>.Success(number));
            });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task BindAsync_WithNullBinder_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).BindAsync<int>(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BindAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).BindAsync(
            (number, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(number)),
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task BindAsync_WhenBinderCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Success(1).BindAsync(
            (number, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult<IResult<int>>(Result<int>.Success(number));
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task BindAsync_WhenBinderReturnsNull_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).BindAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(null!));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
