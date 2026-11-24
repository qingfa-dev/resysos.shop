namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="FilterConstant"/>.</summary>
public sealed class FilterConstantTests
{
    #region Errors

    [Fact]
    public void FieldRequired_ShouldHaveDocumentedIdentity()
    {
        var error = FilterConstant.Constraints.Field.Required.Error;

        error.Code.Should().Be("filter.field.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ClauseInvalid_ShouldUseDocumentedCodeAndMessage()
    {
        var error = FilterConstant.Constraints.Clause.Invalid("boom");

        error.Code.Should().Be("query.invalid_filter");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be("boom");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        FilterConstant.Defaults.Operator.Should().Be(FilterOperator.Eq);
        FilterConstant.Defaults.ClauseSeparator.Should().Be(',');
        FilterConstant.Defaults.ValueSeparator.Should().Be('|');
        FilterConstant.Defaults.Quote.Should().Be('\'');
    }

    #endregion
}
