namespace BuildingBlock.Domain.Unit.Test.Functional.Results;

/// <summary>Tests for <see cref="ResultTerminalExtensions"/>.</summary>
public sealed class ResultTerminalExtensionTests
{
    #region OrThrow

    [Fact]
    public void OrThrow_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);

        result.OrThrow().Should().Be(42);
    }

    [Fact]
    public void OrThrow_OnFailure_ShouldThrowAtTheEnd()
    {
        var result = Result.Failure<int>(ResultTestData.ErrorA);

        var act = () => result.OrThrow();

        act.Should().Throw<Exception>()
            .Which.Message.Should().Contain(ResultTestData.ErrorA.Code)
            .And.Contain(ResultTestData.ErrorA.Message);
    }

    [Fact]
    public void OrThrow_WithErrorFactory_ShouldThrowMappedException()
    {
        var result = Result.Failure<int>(ResultTestData.ErrorA, ResultTestData.ErrorB);
        var expected = new InvalidOperationException("mapped");
        Error? received = null;

        var act = () => result.OrThrow((Error error) =>
        {
            received = error;
            return expected;
        });

        act.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(expected);
        received.Should().Be(ResultTestData.ErrorA);
    }

    [Fact]
    public void OrThrow_WithErrorsFactory_ShouldReceiveAllErrors()
    {
        var result = Result.Failure<int>(ResultTestData.ErrorA, ResultTestData.ErrorB);
        var expected = new ApplicationException("mapped");
        IReadOnlyList<Error>? received = null;

        var act = () => result.OrThrow((IReadOnlyList<Error> errors) =>
        {
            received = errors;
            return expected;
        });

        act.Should().Throw<ApplicationException>().Which.Should().BeSameAs(expected);
        received.Should().Equal(ResultTestData.ErrorA, ResultTestData.ErrorB);
    }

    [Fact]
    public void OrThrow_OnSuccess_ShouldNotInvokeErrorFactory()
    {
        var invoked = false;

        var value = Result.Success(7).OrThrow((Error _) =>
        {
            invoked = true;
            return new Exception("unreachable");
        });

        value.Should().Be(7);
        invoked.Should().BeFalse();
    }

    [Fact]
    public void OrThrow_WithNullErrorFactory_ShouldThrowArgumentNullException()
    {
        var result = Result.Success(1);

        var act = () => result.OrThrow((Func<Error, Exception>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OrThrow_WithMalformedFailure_ShouldThrowMissingErrorsError()
    {
        IResult<int> result = new EmptyFailureResult();

        var act = () => result.OrThrow();

        act.Should().Throw<Exception>()
            .Which.Message.Should().Contain(ResultConstant.Constraints.Errors.Empty.Code);
    }

    [Fact]
    public void OrThrow_WithNullResult_ShouldThrowArgumentNullException()
    {
        IResult<int> result = null!;

        var act = () => result.OrThrow();

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ValueOr

    [Fact]
    public void ValueOr_OnSuccess_ShouldReturnValue()
    {
        Result.Success(5).ValueOr(99).Should().Be(5);
    }

    [Fact]
    public void ValueOr_OnFailure_ShouldReturnFallback()
    {
        Result.Failure<int>(ResultTestData.ErrorA).ValueOr(99).Should().Be(99);
    }

    [Fact]
    public void ValueOr_WithFallbackFactory_ShouldReceiveErrors()
    {
        var result = Result.Failure<int>(ResultTestData.ErrorA, ResultTestData.ErrorB);
        IReadOnlyList<Error>? received = null;

        var value = result.ValueOr(errors =>
        {
            received = errors;
            return -1;
        });

        value.Should().Be(-1);
        received.Should().Equal(ResultTestData.ErrorA, ResultTestData.ErrorB);
    }

    [Fact]
    public void ValueOr_WithNullFallbackFactory_ShouldThrowArgumentNullException()
    {
        var result = Result.Failure<int>(ResultTestData.ErrorA);

        var act = () => result.ValueOr((Func<IReadOnlyList<Error>, int>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TryGetValue

    [Fact]
    public void TryGetValue_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success("value");

        result.TryGetValue(out var value).Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void TryGetValue_OnFailure_ShouldReturnDefault()
    {
        var result = Result.Failure<string>(ResultTestData.ErrorA);

        result.TryGetValue(out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    #endregion

    #region ThrowIfFailure

    [Fact]
    public void ThrowIfFailure_OnSuccess_ShouldNotThrow()
    {
        IResult result = Result.Success(1);

        var act = () => result.ThrowIfFailure();

        act.Should().NotThrow();
    }

    [Fact]
    public void ThrowIfFailure_OnFailure_ShouldThrow()
    {
        IResult result = Result.Failure<int>(ResultTestData.ErrorA);

        var act = () => result.ThrowIfFailure();

        act.Should().Throw<Exception>()
            .Which.Message.Should().Contain(ResultTestData.ErrorA.Code);
    }

    [Fact]
    public void ThrowIfFailure_WithErrorFactory_ShouldThrowMappedException()
    {
        IResult result = Result.Failure<int>(ResultTestData.ErrorA);
        var expected = new InvalidOperationException("mapped");
        Error? received = null;

        var act = () => result.ThrowIfFailure((Error error) =>
        {
            received = error;
            return expected;
        });

        act.Should().Throw<InvalidOperationException>().Which.Should().BeSameAs(expected);
        received.Should().Be(ResultTestData.ErrorA);
    }

    [Fact]
    public void ThrowIfFailure_OnSuccess_ShouldNotInvokeFactory()
    {
        IResult result = Result.Success(1);
        var invoked = false;

        var act = () => result.ThrowIfFailure((Error _) =>
        {
            invoked = true;
            return new Exception("unreachable");
        });

        act.Should().NotThrow();
        invoked.Should().BeFalse();
    }

    [Fact]
    public void ThrowIfFailure_WithNullFactory_ShouldThrowArgumentNullException()
    {
        IResult result = Result.Failure<int>(ResultTestData.ErrorA);

        var act = () => result.ThrowIfFailure((Func<Error, Exception>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    private sealed class EmptyFailureResult : IResult<int>
    {
        public bool IsSuccess => false;

        public IReadOnlyList<Error> Errors => Array.Empty<Error>();

        public int Value => throw new InvalidOperationException();
    }
}
