namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

public sealed class ResultRailwayTests
{
    #region Then

    [Fact]
    public void Then_WithSuccess_ContinuesOnSuccessTrack()
    {
        var result = Result<int>.Success(2)
            .Then(value => Result<string>.Success($"n{value}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("n2");
    }

    [Fact]
    public void Then_WithFailure_ShortCircuits()
    {
        var invoked = false;

        var result = Result<int>.Failure(ResultTestData.ErrorA).Then(value =>
        {
            invoked = true;
            return Result<int>.Success(value);
        });

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(ResultTestData.ErrorA.Code);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void Then_WithNullNext_ThrowsArgumentNullException()
    {
        Action act = () => Result<int>.Success(1).Then<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ThenAsync_WithSuccess_ContinuesOnSuccessTrack()
    {
        var result = await Result<int>.Success(2).ThenAsync(
            (value, _) => ValueTask.FromResult<IResult<int>>(Result<int>.Success(value * 2)));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(4);
    }

    [Fact]
    public async Task ThenAsync_WithFailure_ShortCircuits()
    {
        var invoked = false;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).ThenAsync(
            (value, _) =>
            {
                invoked = true;
                return ValueTask.FromResult<IResult<int>>(Result<int>.Success(value));
            });

        result.IsFailure.Should().BeTrue();
        invoked.Should().BeFalse();
    }

    #endregion

    #region OnSuccess

    [Fact]
    public void OnSuccess_WithSuccess_InvokesAction()
    {
        var seen = 0;
        var success = Result<int>.Success(4);

        var result = success.OnSuccess(value => seen = value);

        result.Should().BeSameAs(success);
        seen.Should().Be(4);
    }

    [Fact]
    public void OnSuccess_WithFailure_DoesNotInvokeAction()
    {
        var invoked = false;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = failure.OnSuccess(_ => invoked = true);

        result.Should().BeSameAs(failure);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task OnSuccessAsync_WithSuccess_InvokesAction()
    {
        var seen = 0;

        var result = await Result<int>.Success(4).OnSuccessAsync((value, _) =>
        {
            seen = value;
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
        seen.Should().Be(4);
    }

    [Fact]
    public async Task OnSuccessAsync_WithFailure_DoesNotInvokeAction()
    {
        var invoked = false;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).OnSuccessAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        result.IsFailure.Should().BeTrue();
        invoked.Should().BeFalse();
    }

    #endregion

    #region OnFailure

    [Fact]
    public void OnFailure_WithFailure_InvokesAction()
    {
        Error? seen = null;
        var failure = Result<int>.Failure(ResultTestData.ErrorA);

        var result = failure.OnFailure(errors => seen = errors[0]);

        result.Should().BeSameAs(failure);
        seen!.Value.Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void OnFailure_WithSuccess_DoesNotInvokeAction()
    {
        var invoked = false;
        var success = Result<int>.Success(1);

        var result = success.OnFailure(_ => invoked = true);

        result.Should().BeSameAs(success);
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task OnFailureAsync_WithFailure_InvokesAction()
    {
        Error? seen = null;

        var result = await Result<int>.Failure(ResultTestData.ErrorA).OnFailureAsync((errors, _) =>
        {
            seen = errors[0];
            return ValueTask.CompletedTask;
        });

        result.IsFailure.Should().BeTrue();
        seen!.Value.Code.Should().Be(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public async Task OnFailureAsync_WithSuccess_DoesNotInvokeAction()
    {
        var invoked = false;

        var result = await Result<int>.Success(1).OnFailureAsync((_, _) =>
        {
            invoked = true;
            return ValueTask.CompletedTask;
        });

        result.IsSuccess.Should().BeTrue();
        invoked.Should().BeFalse();
    }

    #endregion
}
