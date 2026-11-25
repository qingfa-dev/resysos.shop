namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryFilterCompilerConstant"/>.</summary>
public sealed class QueryFilterCompilerConstantTests
{
    #region Unsupported operator

    [Fact]
    public void UnsupportedOperator_ShouldHaveDocumentedIdentity()
    {
        var error = QueryFilterCompilerConstant.Constraints.Operator.Unsupported.For(
            FilterOperator.Contains,
            "age",
            typeof(int));

        error.Code.Should().Be("query.specification.operator.unsupported");
        error.Type.Should().Be(ErrorType.NotSupported);
        error.Message.Should().Contain("Contains").And.Contain("age").And.Contain("Int32");
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldNameTheStringMethods()
    {
        QueryFilterCompilerConstant.Defaults.Contains.Should().Be(nameof(string.Contains));
        QueryFilterCompilerConstant.Defaults.StartsWith.Should().Be(nameof(string.StartsWith));
        QueryFilterCompilerConstant.Defaults.EndsWith.Should().Be(nameof(string.EndsWith));
        QueryFilterCompilerConstant.Defaults.ToLower.Should().Be(nameof(string.ToLower));
        QueryFilterCompilerConstant.Defaults.EnumerableContains.Should().Be(nameof(Enumerable.Contains));
    }

    #endregion
}
