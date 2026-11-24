namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultMatchTests
{
    #region Match

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public void Match_WithSuccess_InvokesSuccessHandler(int value)
    {
        var failureInvoked = false;
        var result = Result<int>.Success(value).Match(
            success => success * 2,
            _ =>
            {
                failureInvoked = true;
                return -1;
            });

        result.Should().Be(value * 2);
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public void Match_WithFailure_InvokesFailureHandler()
    {
        var successInvoked = false;
        var result = Result<int>.Failure(ResultTestData.ErrorA).Match<string>(
            value =>
            {
                successInvoked = true;
                return value.ToString();
            },
            errors => errors[0].Code);

        result.Should().Be(ResultTestData.ErrorA.Code);
        successInvoked.Should().BeFalse();
    }

    [Fact]
    public void Match_WithNullSuccessHandler_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Match<int>(null!, _ => 0);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Match_WithNullFailureHandler_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Match(value => value, (Func<IReadOnlyList<Error>, int>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Switch

    [Fact]
    public void Switch_WithSuccess_InvokesSuccessHandler()
    {
        var valueSeen = 0;
        Result<int>.Success(3).Switch(
            value => valueSeen = value,
            _ => throw new InvalidOperationException("failure handler should not run"));

        valueSeen.Should().Be(3);
    }

    [Fact]
    public void Switch_WithFailure_InvokesFailureHandler()
    {
        Error? errorSeen = null;
        Result<int>.Failure(ResultTestData.ErrorA).Switch(
            _ => throw new InvalidOperationException("success handler should not run"),
            errors => errorSeen = errors[0]);

        errorSeen!.Value.Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Switch_WithNullSuccessHandler_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Switch(null!, _ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Switch_WithNullFailureHandler_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Switch(_ => { }, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region MatchAsync

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-3)]
    public async Task MatchAsync_WithSuccess_InvokesSuccessHandler(int value)
    {
        var failureInvoked = false;

        var result = await Result<int>.Success(value).MatchAsync(
            (number, _) => ValueTask.FromResult(number * 2),
            (_, _) =>
            {
                failureInvoked = true;
                return ValueTask.FromResult(-1);
            });

        result.Should().Be(value * 2);
        failureInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task MatchAsync_WithFailure_InvokesFailureHandler()
    {
        var successInvoked = false;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).MatchAsync<string>(
            (value, _) =>
            {
                successInvoked = true;
                return ValueTask.FromResult(value.ToString());
            },
            (errors, _) => ValueTask.FromResult(errors[0].Code));

        result.Should().Be(ResultTestData.ErrorA.Code);
        successInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task MatchAsync_WithNullSuccessHandler_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).MatchAsync<int>(
            null!,
            (_, _) => ValueTask.FromResult(0));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MatchAsync_WithNullFailureHandler_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).MatchAsync(
            (value, _) => ValueTask.FromResult(value),
            null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task MatchAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).MatchAsync(
            (value, _) => ValueTask.FromResult(value),
            (_, _) => ValueTask.FromResult(-1),
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task MatchAsync_WhenSuccessHandlerCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Success(1).MatchAsync(
            (value, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult(value);
            },
            (_, _) => ValueTask.FromResult(-1),
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task MatchAsync_WhenFailureHandlerCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).MatchAsync(
            (value, _) => ValueTask.FromResult(value),
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult(-1);
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
