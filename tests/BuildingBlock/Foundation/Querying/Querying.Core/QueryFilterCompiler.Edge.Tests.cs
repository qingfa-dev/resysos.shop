namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Edge cases for <see cref="QueryFilterCompiler"/>: unsupported types and missing input.</summary>
public sealed class QueryFilterCompilerEdgeTests
{
    #region Unsupported

    [Fact]
    public void Compile_StringOrdering_ShouldBeUnsupported()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var filter = new FilterCondition { Field = "name", Operator = FilterOperator.Gt, Value = "A" };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.operator.unsupported");
    }

    [Fact]
    public void Compile_NonStringContains_ShouldBeUnsupported()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.Contains, Value = "1" };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.operator.unsupported");
    }

    [Fact]
    public void Compile_StringBetween_ShouldBeUnsupported()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var filter = new FilterCondition
        {
            Field = "name",
            Operator = FilterOperator.Between,
            Values = ["a", "z"]
        };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.operator.unsupported");
    }

    #endregion

    #region Missing input

    [Fact]
    public void Compile_TextWithoutValue_ShouldReturnRequired()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var filter = new FilterCondition { Field = "name", Operator = FilterOperator.Contains };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.required");
    }

    [Fact]
    public void Compile_SetWithoutValues_ShouldReturnRequired()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.In };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.required");
    }

    [Fact]
    public void Compile_BetweenWithOneValue_ShouldReturnRequired()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var filter = new FilterCondition
        {
            Field = "age",
            Operator = FilterOperator.Between,
            Values = ["18"]
        };

        var result = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.specification.value.required");
    }

    [Fact]
    public void Compile_NullRegistry_ShouldReturnFieldNotAllowed()
    {
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.Eq, Value = "1" };

        var result = QueryFilterCompiler.Compile<Person>(filter, null!, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Case-insensitive

    [Fact]
    public void Compile_CaseInsensitiveIn_ShouldIgnoreCase()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var filter = new FilterCondition
        {
            Field = "name",
            Operator = FilterOperator.In,
            Values = ["ADA", "GRACE"],
            CaseInsensitive = true
        };

        var predicate = QueryFilterCompiler.Compile<Person>(filter, registry, QueryContext.Anonymous)
            .Value.Compile();

        predicate(People.Ada).Should().BeTrue();
        predicate(People.Grace).Should().BeTrue();
        predicate(People.Alan).Should().BeFalse();
    }

    #endregion
}
