namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QuerySortCompiler"/>.</summary>
public sealed class QuerySortCompilerTests
{
    #region Definitions

    [Fact]
    public void Compile_MapsDescriptorsToDefinitions()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age", x => x.Age, FilterOperators.All, sortable: true);
        var sorts = new[]
        {
            new SortDescriptor { Field = "age", Direction = SortDirection.Descending }
        };

        var definitions = QuerySortCompiler.Compile<Person>(sorts, registry, QueryContext.Anonymous).Value;

        definitions.Should().ContainSingle();
        definitions[0].Field.Should().Be("age");
        definitions[0].Descending.Should().BeTrue();
    }

    [Fact]
    public void Compile_UnknownField_ShouldFail()
    {
        var registry = new QueryFieldRegistry();
        var sorts = new[] { new SortDescriptor { Field = "nope" } };

        var result = QuerySortCompiler.Compile<Person>(sorts, registry, QueryContext.Anonymous);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
    }

    [Fact]
    public void Compile_NullSortElement_ShouldNotThrow()
    {
        var registry = new QueryFieldRegistry();
        var sorts = new SortDescriptor[] { null! };

        var result = QuerySortCompiler.Compile<Person>(sorts, registry, QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region Selectors

    [Fact]
    public void Selectors_ShouldOrderByField()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var definitions = new[] { new SortDefinition("age", Descending: false) };

        var selector = QuerySortCompiler.Selectors<Person>(definitions, registry).Value.Single().KeySelector.Compile();

        selector(new Person { Age = 30 }).Should().Be(30);
    }

    [Fact]
    public void Selectors_AliasedField_ShouldUseRegisteredAccessor()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age_rank", x => x.Age, FilterOperators.All, sortable: true);
        var definitions = new[] { new SortDefinition("age_rank", Descending: false) };

        var selector = QuerySortCompiler.Selectors<Person>(definitions, registry).Value.Single().KeySelector.Compile();

        selector(new Person { Age = 42 }).Should().Be(42);
    }

    [Fact]
    public void Selectors_NullParent_ShouldYieldDefault()
    {
        var registry = Registry.With<Person>("address.country", x => x.Address!.Country);
        var definitions = new[] { new SortDefinition("address.country", Descending: false) };

        var selector = QuerySortCompiler.Selectors<Person>(definitions, registry).Value.Single().KeySelector.Compile();

        selector(new Person { Address = null }).Should().BeNull();
    }

    [Fact]
    public void Selectors_UnknownField_ShouldFail()
    {
        var registry = new QueryFieldRegistry();
        var definitions = new[] { new SortDefinition("nope", Descending: false) };

        var result = QuerySortCompiler.Selectors<Person>(definitions, registry);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
    }

    [Fact]
    public void Selectors_NullSortElement_ShouldNotThrow()
    {
        var registry = new QueryFieldRegistry();
        var definitions = new SortDefinition[] { null! };

        var result = QuerySortCompiler.Selectors<Person>(definitions, registry);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion
}
