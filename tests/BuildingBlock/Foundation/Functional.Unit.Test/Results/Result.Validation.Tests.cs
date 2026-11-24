namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultValidationTests
{
    #region Ensure

    [Fact]
    public void Ensure_WithSatisfiedPredicate_ReturnsSameInstance()
    {
        var success = Result<int>.Success(2);

        var result = success.Ensure(value => value > 0, ResultTestData.ErrorA);

        result.Should().BeSameAs(success);
    }

    [Fact]
    public void Ensure_WithUnsatisfiedPredicate_ReturnsFailure()
    {
        var result = Result<int>.Success(2).Ensure(value => value > 10, ResultTestData.ErrorA);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void Ensure_WithFailure_DoesNotInvokePredicate()
    {
        var invoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = failure.Ensure(
            _ =>
            {
                invoked = true;
                return true;
            },
            ResultTestData.ErrorB);

        result.Should().BeSameAs(failure);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Ensure_WithErrorFactory_PassesValueToFactory()
    {
        var result = Result<int>.Success(7).Ensure(
            _ => false,
            value => Error.Validation("test.ensure", $"value {value}"));

        result.Errors[0].Code.Should().Be("test.ensure");
        result.Errors[0].Message.Should().Be("value 7");
    }

    [Fact]
    public void Ensure_WithNullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Ensure(null!, ResultTestData.ErrorA);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_WithNullErrorFactory_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Ensure(_ => true, (Func<int, Error>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Ensure_OnFailureWithNullPredicate_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA).Ensure(null!, ResultTestData.ErrorB);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region EnsureAsync

    [Fact]
    public async Task EnsureAsync_WithSatisfiedPredicate_ReturnsSameInstance()
    {
        var success = Result<int>.Success(2);

        var result = await success.EnsureAsync(
            (_, _) => ValueTask.FromResult(true),
            _ => ResultTestData.ErrorA);

        result.Should().BeSameAs(success);
    }

    [Fact]
    public async Task EnsureAsync_WithUnsatisfiedPredicate_ReturnsFailureFromFactory()
    {
        var result = await Result<int>.Success(2).EnsureAsync(
            (_, _) => ValueTask.FromResult(false),
            _ => ResultTestData.ErrorB);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public async Task EnsureAsync_WithFailure_DoesNotInvokePredicateOrFactory()
    {
        var predicateInvoked = false;
        var factoryInvoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = await failure.EnsureAsync(
            (_, _) =>
            {
                predicateInvoked = true;
                return ValueTask.FromResult(true);
            },
            _ =>
            {
                factoryInvoked = true;
                return ResultTestData.ErrorB;
            });

        result.Should().BeSameAs(failure);
        predicateInvoked.Should().BeFalse();
        factoryInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1)
            .EnsureAsync(null!, _ => ResultTestData.ErrorA);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureAsync_WithNullErrorFactory_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Success(1)
            .EnsureAsync((_, _) => ValueTask.FromResult(true), null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Success(1).EnsureAsync(
            (_, _) => ValueTask.FromResult(true),
            _ => ResultTestData.ErrorA,
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EnsureAsync_WhenPredicateCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        Func<Task> act = async () => await Result<int>.Success(1).EnsureAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult(true);
            },
            _ => ResultTestData.ErrorA,
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
