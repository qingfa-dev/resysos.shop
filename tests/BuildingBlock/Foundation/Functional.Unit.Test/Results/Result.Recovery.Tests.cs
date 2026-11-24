namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultRecoveryTests
{
    #region Recover

    [Fact]
    public void Recover_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);
        var invoked = false;

        var result = success.Recover(_ =>
        {
            invoked = true;
            return 99;
        });

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Recover_WithFailure_ReturnsRecoveredValue()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA).Recover(_ => 99);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void Recover_WithNullRecovery_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).Recover(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecoverWith_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);

        var result = success.RecoverWith(_ => Result<int>.Success(99));

        result.Should().BeSameAs(success);
    }

    [Fact]
    public void RecoverWith_WithFailureAndSuccessfulRecovery_ReturnsRecoveredResult()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .RecoverWith(_ => Result<int>.Success(99));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void RecoverWith_WithFailureAndFailingRecovery_ReturnsRecoveryFailure()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .RecoverWith(_ => Result<int>.Failure(ResultTestData.ErrorB));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void RecoverWith_WithNullRecovery_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).RecoverWith(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecoverWith_WithNullRecoveredResult_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).RecoverWith(_ => null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region RecoverAsync

    [Fact]
    public async Task RecoverAsync_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);
        var invoked = false;

        var result = await success.RecoverAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.FromResult(99);
        });

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task RecoverAsync_WithFailure_ReturnsRecoveredValue()
    {
        var result = await Result<int>.Failure(ResultTestData.ErrorA).RecoverAsync(
            (_, _) => ValueTask.FromResult(99));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task RecoverAsync_WithNullRecovery_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecoverAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverAsync(
            (_, _) => ValueTask.FromResult(1),
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RecoverAsync_WhenRecoveryCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult(1);
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region RecoverWithAsync

    [Fact]
    public async Task RecoverWithAsync_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);

        var result = await success.RecoverWithAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(99)));

        result.Should().BeSameAs(success);
    }

    [Fact]
    public async Task RecoverWithAsync_WithFailureAndSuccessfulRecovery_ReturnsRecoveredResult()
    {
        var result = await Result<int>.Failure(ResultTestData.ErrorA).RecoverWithAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(99)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task RecoverWithAsync_WithFailureAndFailingRecovery_ReturnsRecoveryFailure()
    {
        var result = await Result<int>.Failure(ResultTestData.ErrorA).RecoverWithAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Failure(ResultTestData.ErrorB)));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public async Task RecoverWithAsync_WithNullRecovery_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA)
            .RecoverWithAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecoverWithAsync_WhenRecoveryReturnsNull_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverWithAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(null!));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecoverWithAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverWithAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(1)),
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RecoverWithAsync_WhenRecoveryCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).RecoverWithAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult<IResult<int>>(Result<int>.Success(1));
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
