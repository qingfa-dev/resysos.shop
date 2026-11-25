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

    [Fact]
    public void OperatorUnsupported_ShouldUseDocumentedCodeAndMessage()
    {
        var error = FilterConstant.Constraints.Operator.Unsupported("foo");

        error.Code.Should().Be("filter.operator.unsupported");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Contain("foo");
    }

    #endregion

    #region Operators

    [Fact]
    public void Operators_ShouldExposeCanonicalWordNames()
    {
        FilterConstant.Operators.Eq.Should().Be("eq");
        FilterConstant.Operators.Ne.Should().Be("neq");
        FilterConstant.Operators.Gt.Should().Be("gt");
        FilterConstant.Operators.Gte.Should().Be("gte");
        FilterConstant.Operators.Lt.Should().Be("lt");
        FilterConstant.Operators.Lte.Should().Be("lte");
        FilterConstant.Operators.Contains.Should().Be("contains");
        FilterConstant.Operators.StartsWith.Should().Be("startswith");
        FilterConstant.Operators.EndsWith.Should().Be("endswith");
        FilterConstant.Operators.In.Should().Be("in");
        FilterConstant.Operators.IsNull.Should().Be("isnull");
        FilterConstant.Operators.IsNotNull.Should().Be("isnotnull");
        FilterConstant.Operators.NotIn.Should().Be("notin");
        FilterConstant.Operators.Between.Should().Be("between");
        FilterConstant.Operators.Any.Should().Be("any");
        FilterConstant.Operators.All.Should().Be("all");
    }

    [Fact]
    public void GroupOperators_ShouldExposeCanonicalWordNames()
    {
        FilterConstant.GroupOperators.And.Should().Be("and");
        FilterConstant.GroupOperators.Or.Should().Be("or");
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
