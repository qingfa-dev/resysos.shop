using BuildingBlock.Persistence.InMemory.UnitOfWorks;

using Testing.Unit;

namespace BuildingBlock.Persistence.InMemory.Unit.Tests;

/// <summary>Tests for <see cref="InMemoryUnitOfWork"/> and <see cref="InMemoryTransaction"/>.</summary>
public sealed class InMemoryUnitOfWorkTests
{
    #region Save

    [Fact]
    public async Task SaveChangesAsync_ShouldSucceedWithNoAffectedEntities()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;

        var result = await unitOfWork.SaveChangesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenDisposed_ShouldReturnError()
    {
        InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        unitOfWork.Dispose();

        var result = await unitOfWork.SaveChangesAsync();

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.disposed");
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCancelled_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var result = await unitOfWork.SaveChangesAsync(cancellationToken: cancellation.Token);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.cancelled");
    }

    [Fact]
    public async Task SaveChangesAsync_WithInvalidRetryBudget_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;

        var result = await unitOfWork.SaveChangesAsync(
            UnitOfWorkConstant.Constraints.MaxRetries.Max.Value + 1);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("unit_of_work.max_retries.invalid");
    }

    #endregion

    #region Transactions

    [Fact]
    public async Task BeginTransactionAsync_ShouldCreateAnActiveTransaction()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;

        var begun = await unitOfWork.BeginTransactionAsync();

        begun.IsSuccess.Should().BeTrue();
        begun.Value.Should().BeOfType<InMemoryTransaction>();
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenAlreadyActive_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();

        var result = await unitOfWork.BeginTransactionAsync();

        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.active");

        await begun.Value.DisposeAsync();
    }

    [Fact]
    public async Task BeginTransactionAsync_WithInvalidTimeout_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;

        var result = await unitOfWork.BeginTransactionAsync(TimeSpan.Zero);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.timeout.invalid");
    }

    [Fact]
    public async Task CommitAsync_ShouldCompleteTheTransaction()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();
        var transaction = (InMemoryTransaction)begun.Value;

        var result = await transaction.CommitAsync();

        result.IsSuccess.Should().BeTrue();
        transaction.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task CommitAsync_WhenAlreadyCompleted_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();
        await begun.Value.CommitAsync();

        var result = await begun.Value.CommitAsync();

        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.completed");
    }

    [Fact]
    public async Task CommitAsync_WhenExpired_ShouldReturnError()
    {
        var clock = TestClock.Create();
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create(clock).Value;
        var begun = await unitOfWork.BeginTransactionAsync(TimeSpan.FromSeconds(1));

        clock.Advance(TimeSpan.FromSeconds(2));
        var result = await begun.Value.CommitAsync();

        result.Errors.Should().ContainSingle().Which.Code
            .Should().Be("unit_of_work.transaction.expired");
    }

    [Fact]
    public async Task RollbackAsync_ShouldCompleteTheTransaction()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();

        var result = await begun.Value.RollbackAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Transaction_WhenDisposed_ShouldReturnError()
    {
        await using InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();

        await begun.Value.DisposeAsync();
        var result = await begun.Value.CommitAsync();

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("unit_of_work.disposed");
    }

    #endregion

    #region Lifecycle

    [Fact]
    public async Task Dispose_ShouldDisposeTheActiveTransaction()
    {
        InMemoryUnitOfWork unitOfWork = InMemoryUnitOfWork.Create().Value;
        var begun = await unitOfWork.BeginTransactionAsync();
        var transaction = (InMemoryTransaction)begun.Value;

        unitOfWork.Dispose();

        transaction.IsCompleted.Should().BeTrue();
        (await transaction.CommitAsync()).Errors.Should().ContainSingle()
            .Which.Code.Should().Be("unit_of_work.disposed");
    }

    #endregion
}
