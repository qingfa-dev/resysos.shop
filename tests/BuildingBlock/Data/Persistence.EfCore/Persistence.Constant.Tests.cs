namespace BuildingBlock.Persistence.EfCore.Unit.Tests;

/// <summary>Tests for <see cref="PersistenceConstant"/>.</summary>
public sealed class PersistenceConstantTests
{
    #region Constraints

    [Fact]
    public void SaveFailed_ShouldHaveDocumentedIdentity()
    {
        var error = PersistenceConstant.Constraints.SaveFailed.Error;

        error.Code.Should().Be("persistence.save_failed");
        error.Type.Should().Be(ErrorType.Dependency);
    }

    [Fact]
    public void SaveFailedFor_ShouldNameTheFailure()
    {
        var error = PersistenceConstant.Constraints.SaveFailed.For(new InvalidOperationException());

        error.Code.Should().Be("persistence.save_failed");
        error.Message.Should().Contain(nameof(InvalidOperationException));
    }

    [Fact]
    public void Concurrency_ShouldHaveDocumentedIdentity()
    {
        var error = PersistenceConstant.Constraints.Concurrency.Error;

        error.Code.Should().Be("persistence.concurrency_conflict");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void TransactionFailed_ShouldHaveDocumentedIdentity()
    {
        PersistenceConstant.Constraints.TransactionFailed.Error.Code
            .Should().Be("persistence.transaction_failed");
        PersistenceConstant.Constraints.TransactionFailed.For(new InvalidOperationException())
            .Message.Should().Contain(nameof(InvalidOperationException));
    }

    [Fact]
    public void CommitFailed_ShouldHaveDocumentedIdentity()
    {
        PersistenceConstant.Constraints.CommitFailed.Error.Code
            .Should().Be("persistence.commit_failed");
        PersistenceConstant.Constraints.CommitFailed.For(new InvalidOperationException())
            .Message.Should().Contain(nameof(InvalidOperationException));
    }

    [Fact]
    public void RollbackFailed_ShouldHaveDocumentedIdentity()
    {
        PersistenceConstant.Constraints.RollbackFailed.Error.Code
            .Should().Be("persistence.rollback_failed");
        PersistenceConstant.Constraints.RollbackFailed.For(new InvalidOperationException())
            .Message.Should().Contain(nameof(InvalidOperationException));
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        PersistenceConstant.Defaults.MigrationsHistoryTable.Should().Be("__ef_migrations_history");
        PersistenceConstant.Defaults.CommandTimeoutSeconds.Should().Be(30);
        PersistenceConstant.Defaults.RetryDelay.Should().Be(TimeSpan.FromMilliseconds(50));
    }

    #endregion
}
