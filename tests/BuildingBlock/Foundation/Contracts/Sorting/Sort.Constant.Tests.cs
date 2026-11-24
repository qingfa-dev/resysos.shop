namespace BuildingBlock.Contracts.Unit.Tests;

/// <summary>Tests for <see cref="SortConstant"/>.</summary>
public sealed class SortConstantTests
{
    #region Errors

    [Fact]
    public void FieldRequired_ShouldHaveDocumentedIdentity()
    {
        var error = SortConstant.Constraints.Field.Required.Error;

        error.Code.Should().Be("sort.field.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ClauseInvalid_ShouldUseDocumentedCodeAndMessage()
    {
        var error = SortConstant.Constraints.Clause.Invalid("boom");

        error.Code.Should().Be("query.invalid_sort");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be("boom");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldMatchDocumentedValues()
    {
        SortConstant.Defaults.Direction.Should().Be(SortDirection.Ascending);
        SortConstant.Defaults.ClauseSeparator.Should().Be(',');
        SortConstant.Defaults.DescendingPrefix.Should().Be('-');
        SortConstant.Defaults.AscendingPrefix.Should().Be('+');
        SortConstant.Defaults.AscendingSuffix.Should().Be("asc");
        SortConstant.Defaults.DescendingSuffix.Should().Be("desc");
    }

    #endregion
}
