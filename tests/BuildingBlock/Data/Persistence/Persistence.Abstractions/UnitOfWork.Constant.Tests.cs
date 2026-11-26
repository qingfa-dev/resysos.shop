namespace BuildingBlock.Persistence.Abstractions.Unit.Tests;

/// <summary>Tests for <see cref="UnitOfWorkConstant"/>.</summary>
public sealed class UnitOfWorkConstantTests
{
    #region Retries

    [Fact]
    public void MaxRetriesConstraints_ShouldHaveDocumentedValues()
    {
        UnitOfWorkConstant.Constraints.MaxRetries.Min.Value.Should().Be(0);
        UnitOfWorkConstant.Constraints.MaxRetries.Max.Value.Should().Be(10);
    }

    [Fact]
    public void MaxRetriesErrors_ShouldHaveDocumentedIdentity()
    {
        var error = UnitOfWorkConstant.Constraints.MaxRetries.Min.Error;

        error.Code.Should().Be("unit_of_work.max_retries.invalid");
        error.Type.Should().Be(ErrorType.Validation);
        UnitOfWorkConstant.Constraints.MaxRetries.Max.Error.Code.Should().Be(error.Code);
    }

    #endregion

    #region Transactions

    [Fact]
    public void TransactionTimeoutConstraints_ShouldHaveDocumentedValues()
    {
        UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Value.Should().Be(TimeSpan.FromSeconds(1));
        UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Value.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void TransactionTimeoutErrors_ShouldHaveDocumentedIdentity()
    {
        var error = UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Error;

        error.Code.Should().Be("unit_of_work.timeout.invalid");
        error.Type.Should().Be(ErrorType.Validation);
        UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Error.Code.Should().Be(error.Code);
    }

    [Fact]
    public void TransactionRequired_ShouldHaveDocumentedIdentity()
    {
        var error = UnitOfWorkConstant.Constraints.Transaction.Required.Error;

        error.Code.Should().Be("unit_of_work.transaction.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Disposed_ShouldHaveDocumentedIdentity()
    {
        var error = UnitOfWorkConstant.Constraints.Disposed.Error;

        error.Code.Should().Be("unit_of_work.disposed");
        error.Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        UnitOfWorkConstant.Defaults.MaxRetries.Should().Be(3);
        UnitOfWorkConstant.Defaults.TransactionTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void Defaults_ShouldStayWithinConstraints()
    {
        UnitOfWorkConstant.Defaults.MaxRetries.Should()
            .BeInRange(
                UnitOfWorkConstant.Constraints.MaxRetries.Min.Value,
                UnitOfWorkConstant.Constraints.MaxRetries.Max.Value);

        UnitOfWorkConstant.Defaults.TransactionTimeout.Should()
            .BeGreaterThanOrEqualTo(UnitOfWorkConstant.Constraints.TransactionTimeout.Min.Value)
            .And.BeLessThanOrEqualTo(UnitOfWorkConstant.Constraints.TransactionTimeout.Max.Value);
    }

    #endregion
}
