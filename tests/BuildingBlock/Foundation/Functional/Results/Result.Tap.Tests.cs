namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultTapTests
{
    #region Tap

    [Fact]
    public void Tap_WithSuccess_InvokesActionAndReturnsSameInstance()
    {
        var success = Result<int>.Success(3);
        var valueSeen = 0;

        var result = success.Tap(value => valueSeen = value);

        result.Should().BeSameAs(success);
        valueSeen.Should().Be(3);
    }

    [Fact]
    public void Tap_WithFailure_DoesNotInvokeAction()
    {
        var invoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = failure.Tap(_ => invoked = true);

        result.Should().BeSameAs(failure);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Tap_WithNullAction_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Tap(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TapError_WithFailure_InvokesActionAndReturnsSameInstance()
    {
        var failure = Result<int>.Failure(ResultTestData.ErrorA);
        Error? errorSeen = null;

        var result = failure.TapError(errors => errorSeen = errors[0]);

        result.Should().BeSameAs(failure);
        errorSeen!.Value.Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void TapError_WithSuccess_DoesNotInvokeAction()
    {
        var invoked = false;
        var success = Result<int>.Success(1);

        var result = success.TapError(_ => invoked = true);

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void TapError_WithNullAction_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).TapError(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_WithSuccess_InvokesActionAndReturnsSameInstance()
    {
        var success = Result<int>.Success(3);
        var valueSeen = 0;

        var result = await success.TapAsync((value, _) =>
        {
            valueSeen = value;
            return ValueTask.CompletedTask;
        });

        result.Should().BeSameAs(success);
        valueSeen.Should().Be(3);
    }

    [Fact]
    public async Task TapAsync_WithFailure_DoesNotInvokeAction()
    {
        var invoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = await failure.TapAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        result.Should().BeSameAs(failure);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task TapAsync_WithNullAction_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).TapAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task TapAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).TapAsync(
            (_, _) => ValueTask.CompletedTask,
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TapAsync_WhenActionCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Success(1).TapAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.CompletedTask;
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region TapErrorAsync

    [Fact]
    public async Task TapErrorAsync_WithFailure_InvokesActionAndReturnsSameInstance()
    {
        var failure = Result<int>.Failure(ResultTestData.ErrorA);
        Error? errorSeen = null;

        var result = await failure.TapErrorAsync((errors, _) =>
        {
            errorSeen = errors[0];
            return ValueTask.CompletedTask;
        });

        result.Should().BeSameAs(failure);
        errorSeen!.Value.Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public async Task TapErrorAsync_WithSuccess_DoesNotInvokeAction()
    {
        var invoked = false;
        var success = Result<int>.Success(1);

        var result = await success.TapErrorAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task TapErrorAsync_WithNullAction_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).TapErrorAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task TapErrorAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).TapErrorAsync(
            (_, _) => ValueTask.CompletedTask,
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task TapErrorAsync_WhenActionCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).TapErrorAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.CompletedTask;
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
