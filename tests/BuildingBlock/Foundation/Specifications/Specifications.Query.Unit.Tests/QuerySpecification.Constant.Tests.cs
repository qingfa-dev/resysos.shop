namespace BuildingBlock.Specifications.Query.Unit.Tests;

/// <summary>Tests for <see cref="QuerySpecificationConstant"/>.</summary>
public sealed class QuerySpecificationConstantTests
{
    #region Required Errors

    [Fact]
    public void RequestRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Request.Required.Error;

        error.Code.Should().Be("query.specification.request.required");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Be("A query request is required.");
    }

    [Fact]
    public void MapperRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Mapper.Required.Error;

        error.Code.Should().Be("query.specification.mapper.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ParametersRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Parameters.Required.Error;

        error.Code.Should().Be("query.specification.parameters.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void FieldRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Field.Required.Error;

        error.Code.Should().Be("query.specification.field.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ValueRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Value.Required.Error;

        error.Code.Should().Be("query.specification.value.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void PagingRequired_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Paging.Required.Error;

        error.Code.Should().Be("query.specification.paging.required");
        error.Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Factory Errors

    [Fact]
    public void FieldUnknown_ShouldNameTypeAndField()
    {
        var error = QuerySpecificationConstant.Constraints.Field.Unknown.For("Person", "nickname");

        error.Code.Should().Be("query.specification.field.unknown");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Contain("nickname").And.Contain("Person");
    }

    [Fact]
    public void ValueInvalid_ShouldNameFieldValueAndType()
    {
        var error = QuerySpecificationConstant.Constraints.Value.Invalid.For("age", "abc", typeof(int));

        error.Code.Should().Be("query.specification.value.invalid");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Contain("abc").And.Contain("age").And.Contain("Int32");
    }

    [Fact]
    public void OperatorUnsupported_ShouldBeNotSupported()
    {
        var error = QuerySpecificationConstant.Constraints.Operator.Unsupported.For(
            FilterOperator.Contains,
            "age",
            typeof(int));

        error.Code.Should().Be("query.specification.operator.unsupported");
        error.Type.Should().Be(ErrorType.NotSupported);
        error.Message.Should().Contain("Contains").And.Contain("age").And.Contain("Int32");
    }

    [Fact]
    public void SortDirectionInvalid_ShouldHaveDocumentedIdentity()
    {
        var error = QuerySpecificationConstant.Constraints.Sort.Direction.Invalid.For(
            (SortDirection)99);

        error.Code.Should().Be("query.specification.sort.direction.invalid");
        error.Type.Should().Be(ErrorType.Validation);
    }

    #endregion

    #region Defaults

    [Fact]
    public void Defaults_ShouldUseDotAsFieldSeparator()
    {
        QuerySpecificationConstant.Defaults.FieldSeparator.Should().Be('.');
    }

    #endregion
}
