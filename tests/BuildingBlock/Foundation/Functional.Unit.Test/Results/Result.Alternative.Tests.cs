namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultAlternativeTests
{
    #region OrElse(IResult)

    [Fact]
    public void OrElse_WithSuccess_ReturnsSameInstance()
    {
        var success = Result<int>.Success(1);

        var result = success.OrElse(Result<int>.Success(99));

        result.Should().BeSameAs(success);
    }

    [Fact]
    public void OrElse_WithFailure_ReturnsFallbackValue()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .OrElse(Result<int>.Success(99));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public void OrElse_WithFailureAndFailingFallback_ReturnsFallbackErrors()
    {
        var result = Result<int>.Failure(ResultTestData.ErrorA)
            .OrElse(Result<int>.Failure(ResultTestData.ErrorB));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void OrElse_WithNullFallback_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA)
            .OrElse((IResult<int>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrElse(Func)

    [Fact]
    public void OrElseFactory_WithSuccess_DoesNotInvokeFallback()
    {
        var invoked = false;
        var success = Result<int>.Success(1);

        var result = success.OrElse(errors =>
        {
            invoked = true;
            return Result<int>.Success(errors.Count);
        });

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void OrElseFactory_WithFailure_PassesErrorsToFallback()
    {
        IReadOnlyList<Error>? seen = null;

        var result = Result<int>.Failure(ResultTestData.ErrorA, ResultTestData.ErrorB)
            .OrElse(errors =>
            {
                seen = errors;
                return Result<int>.Success(42);
            });

        result.Value.Should().Be(42);
        seen!.Select(error => error.Code)
            .Should().Equal(ResultTestData.ErrorA.Code, ResultTestData.ErrorB.Code);
    }

    [Fact]
    public void OrElseFactory_WithNullFallback_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Failure(ResultTestData.ErrorA)
            .OrElse((Func<IReadOnlyList<Error>, IResult<int>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region OrElseAsync

    [Fact]
    public async Task OrElseAsync_WithSuccess_ReturnsSameInstanceWithoutInvokingFallback()
    {
        var success = Result<int>.Success(1);
        var invoked = false;

        var result = await success.OrElseAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.FromResult<IResult<int>>(Result<int>.Success(99));
        });

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task OrElseAsync_WithFailure_ReturnsFallbackValue()
    {
        var result = await Result<int>.Failure(ResultTestData.ErrorA)
            .OrElseAsync((_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(99)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task OrElseAsync_WithNullFallback_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA)
            .OrElseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task OrElseAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).OrElseAsync(
            (_, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(1)),
            ResultTestData.AlreadyCanceled());

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task OrElseAsync_WhenFallbackCancels_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();

        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA).OrElseAsync(
            (_, _) =>
            {
                cts.Cancel();
                return ValueTask.FromResult<IResult<int>>(Result<int>.Success(1));
            },
            cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task OrElseAsync_WhenFallbackReturnsNull_ThrowsArgumentNullException()
    {
        Func<Task> act = async () => await Result<int>.Failure(ResultTestData.ErrorA)
            .OrElseAsync((_, _) => ValueTask.FromResult<IResult<int>>(null!));

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion
}
