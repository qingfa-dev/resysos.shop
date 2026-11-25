namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryFieldPathConstant"/>.</summary>
public sealed class QueryFieldPathConstantTests
{
    [Fact]
    public void UnknownField_ShouldHaveDocumentedIdentity()
    {
        var error = QueryFieldPathConstant.Constraints.Field.Unknown("Person", "nickname");

        error.Code.Should().Be("query.specification.field.unknown");
        error.Type.Should().Be(ErrorType.Validation);
        error.Message.Should().Contain("nickname").And.Contain("Person");
    }

    [Fact]
    public void UnknownCode_ShouldMatchTheFactory()
        => QueryFieldPathConstant.Constraints.Field.UnknownCode
            .Should().Be("query.specification.field.unknown");
}
