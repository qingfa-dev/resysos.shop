using UnitResult = BuildingBlock.Functional.Results.Unit;

namespace BuildingBlock.Persistence.Abstractions.Unit.Tests;

/// <summary>Tests for <see cref="UnitOfWorkGuard"/>.</summary>
public sealed class UnitOfWorkGuardTests
{
    #region Helper Types

    private sealed class StubTransaction : ITransaction
    {
        public Task<Result<UnitResult>> CommitAsync(
            int maxRetries = UnitOfWorkConstant.Defaults.MaxRetries,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result<UnitResult>> RollbackAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    #endregion

    #region Retries

    [Fact]
    public void ValidMaxRetries_ShouldPassValueWithinConstraints()
    {
        var result = UnitOfWorkGuard.ValidMaxRetries(UnitOfWorkConstant.Defaults.MaxRetries);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UnitOfWorkConstant.Defaults.MaxRetries);
    }

    [Fact]
    public void ValidMaxRetries_BelowMinimum_ShouldFail()
    {
        var result = UnitOfWorkGuard.ValidMaxRetries(
            UnitOfWorkConstant.Constraints.MaxRetries.Min.Value - 1);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.max_retries.invalid");
    }

    [Fact]
    public void ValidMaxRetries_AboveMaximum_ShouldFail()
    {
        var result = UnitOfWorkGuard.ValidMaxRetries(
            UnitOfWorkConstant.Constraints.MaxRetries.Max.Value + 1);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.max_retries.invalid");
    }

    #endregion

    #region Transactions

    [Fact]
    public void ValidTimeout_WhenOmitted_ShouldUseDefault()
    {
        var result = UnitOfWorkGuard.ValidTimeout(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UnitOfWorkConstant.Defaults.TransactionTimeout);
    }

    [Fact]
    public void ValidTimeout_WithinConstraints_ShouldPass()
    {
        var timeout = TimeSpan.FromSeconds(10);

        UnitOfWorkGuard.ValidTimeout(timeout).Value.Should().Be(timeout);
    }

    [Fact]
    public void ValidTimeout_BelowMinimum_ShouldFail()
    {
        var result = UnitOfWorkGuard.ValidTimeout(
            UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Value - TimeSpan.FromMilliseconds(1));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.timeout.invalid");
    }

    [Fact]
    public void ValidTimeout_AboveMaximum_ShouldFail()
    {
        var result = UnitOfWorkGuard.ValidTimeout(
            UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Value + TimeSpan.FromSeconds(1));

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.timeout.invalid");
    }

    [Fact]
    public void TransactionRequired_ShouldGuardNull()
    {
        var result = UnitOfWorkGuard.TransactionRequired(null);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.required");
    }

    [Fact]
    public void TransactionRequired_ShouldPassTransaction()
    {
        var transaction = new StubTransaction();

        UnitOfWorkGuard.TransactionRequired(transaction).Value.Should().BeSameAs(transaction);
    }

    #endregion

    #region State

    [Fact]
    public void NotDisposed_ShouldPassActiveUnitOfWork()
        => UnitOfWorkGuard.NotDisposed(isDisposed: false).IsSuccess.Should().BeTrue();

    [Fact]
    public void NotDisposed_ShouldGuardDisposedUnitOfWork()
    {
        var result = UnitOfWorkGuard.NotDisposed(isDisposed: true);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.disposed");
    }

    #endregion
}
