namespace BuildingBlock.Domain.Unit.Tests.Functional.Results;

using BuildingBlock.Functional.Errors;
using BuildingBlock.Functional.Results;
using Xunit;

/// <summary>Specifications for the try/ensure railway helpers.</summary>
public sealed class ResultTryTests
{
    [Fact]
    public void Try_Should_Return_The_Value()
    {
        Result<int> result = Result.Try(() => 42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Try_Should_Capture_An_Exception_As_A_Failure()
    {
        Result<int> result = Result.Try<int>(() => throw new InvalidOperationException("boom"));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Type.Should().Be(ErrorType.Unexpected);
        result.Errors[0].Message.Should().Be("boom");
    }

    [Fact]
    public void Try_Should_Use_A_Custom_Mapper()
    {
        Result<int> result = Result.Try<int>(
            () => throw new InvalidOperationException("boom"),
            _ => Error.Conflict("custom.conflict", "mapped"));

        result.Errors[0].Code.Should().Be("custom.conflict");
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void Try_Action_Should_Return_Unit()
    {
        Result<Unit> result = Result.Try(() => { });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }

    [Fact]
    public void Try_Action_Should_Capture_A_Failure()
    {
        Result<Unit> result = Result.Try(() => throw new TimeoutException("slow"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Timeout);
    }

    [Fact]
    public async Task TryAsync_Should_Return_The_Value()
    {
        Result<int> result = await Result.TryAsync(_ => ValueTask.FromResult(7));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(7);
    }

    [Fact]
    public async Task TryAsync_Should_Capture_A_Failure()
    {
        Result<int> result = await Result.TryAsync<int>(
            _ => ValueTask.FromException<int>(new KeyNotFoundException("missing")));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task TryAsync_Should_Report_Cancellation()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        Result<int> result = await Result.TryAsync(
            token => ValueTask.FromCanceled<int>(token),
            source.Token);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Cancelled);
    }

    [Fact]
    public async Task TryAsync_Action_Should_Return_Unit()
    {
        Result<Unit> result = await Result.TryAsync(_ => ValueTask.CompletedTask);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void FromNullable_Should_Return_The_Value_Or_The_Error()
    {
        Error missing = Error.NotFound("test.missing", "missing");

        Result<string> present = Result.FromNullable("value", missing);
        Result<string> absent = Result.FromNullable<string>(null, missing);

        present.IsSuccess.Should().BeTrue();
        present.Value.Should().Be("value");
        absent.IsFailure.Should().BeTrue();
        absent.Errors[0].Should().Be(missing);
    }

    [Fact]
    public void NotFound_Should_Use_The_Type_Name_By_Default()
    {
        Result<Order> result = Result.NotFound<Order>();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
        result.Errors[0].Message.Should().Contain(nameof(Order));
    }

    [Fact]
    public void ToUnit_Should_Keep_The_Track()
    {
        Result<Unit> success = Result.Success(1).ToUnit();
        Result<Unit> failure = Result.Failure<int>(Error.Conflict("c", "m")).ToUnit();

        success.IsSuccess.Should().BeTrue();
        failure.IsFailure.Should().BeTrue();
        failure.Errors[0].Code.Should().Be("c");
    }

    [Fact]
    public void Ensure_Should_Keep_A_Passing_Value()
    {
        Result<int> result = Result.Success(5).Ensure(value => value > 0, Error.Validation());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_Should_Fail_A_Rejected_Value()
    {
        Result<int> result = Result.Success(5)
            .Ensure(value => value > 10, Error.Validation("too.small", "too small"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("too.small");
    }

    [Fact]
    public void Ensure_Should_Not_Invoke_The_Predicate_On_A_Failure()
    {
        bool invoked = false;

        Result<int> result = Result.Failure<int>(Error.Conflict())
            .Ensure(_ =>
            {
                invoked = true;
                return true;
            }, Error.Validation());

        invoked.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task EnsureAsync_Should_Keep_Or_Reject_A_Value()
    {
        Result<int> passed = await Result.Success(5)
            .EnsureAsync((value, _) => ValueTask.FromResult(value > 0), Error.Validation());
        Result<int> rejected = await Result.Success(5)
            .EnsureAsync((value, _) => ValueTask.FromResult(value > 10), Error.Validation("nope", "nope"));

        passed.IsSuccess.Should().BeTrue();
        rejected.IsFailure.Should().BeTrue();
        rejected.Errors[0].Code.Should().Be("nope");
    }

    [Fact]
    public async Task EnsureAsync_Should_Skip_A_Failure()
    {
        bool invoked = false;

        Result<int> result = await Result.Failure<int>(Error.Conflict())
            .EnsureAsync(
                (_, _) =>
                {
                    invoked = true;
                    return ValueTask.FromResult(true);
                },
                Error.Validation());

        invoked.Should().BeFalse();
        result.Errors[0].Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task AsResultAsync_Should_Capture_Task_Success_And_Failure()
    {
        Result<int> success = await Task.FromResult(3).AsResultAsync();
        Result<int> failure = await Task.FromException<int>(new InvalidOperationException("boom"))
            .AsResultAsync();

        success.Value.Should().Be(3);
        failure.IsFailure.Should().BeTrue();
        failure.Errors[0].Message.Should().Be("boom");
    }

    [Fact]
    public async Task AsResultAsync_Should_Capture_A_Plain_Task()
    {
        Result<Unit> success = await Task.CompletedTask.AsResultAsync();
        Result<Unit> failure = await Task.FromException(new NotSupportedException("no")).AsResultAsync();

        success.IsSuccess.Should().BeTrue();
        failure.IsFailure.Should().BeTrue();
        failure.Errors[0].Type.Should().Be(ErrorType.NotSupported);
    }

    [Fact]
    public void FromException_Should_Map_Common_Exception_Types()
    {
        Error.FromException(new OperationCanceledException()).Type.Should().Be(ErrorType.Cancelled);
        Error.FromException(new TimeoutException()).Type.Should().Be(ErrorType.Timeout);
        Error.FromException(new NotSupportedException()).Type.Should().Be(ErrorType.NotSupported);
        Error.FromException(new KeyNotFoundException()).Type.Should().Be(ErrorType.NotFound);
        Error.FromException(new UnauthorizedAccessException()).Type.Should().Be(ErrorType.Unauthorized);
        Error.FromException(new ArgumentException("bad")).Type.Should().Be(ErrorType.Validation);
        Error.FromException(new InvalidOperationException("boom")).Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void FromException_Should_Use_The_Type_Name_For_An_Empty_Message()
    {
        Error error = Error.FromException(new Exception(string.Empty));

        error.Message.Should().Be(nameof(Exception));
    }

    [Fact]
    public void FromException_Should_Accept_A_Code_Override()
    {
        Error error = Error.FromException(new TimeoutException("slow"), "custom.timeout");

        error.Code.Should().Be("custom.timeout");
    }

    private sealed record Order(int Id);
}
