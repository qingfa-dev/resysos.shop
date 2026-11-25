namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryFilterCompiler"/>.</summary>
public sealed class QueryFilterCompilerTests
{
    #region Groups

    [Fact]
    public void Compile_OrGroup_ShouldMatchEitherBranch()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterGroup
        {
            Operator = FilterGroupOperator.Or,
            Children =
            [
                new FilterCondition { Field = "age", Operator = FilterOperator.Eq, Value = "30" },
                new FilterCondition { Field = "age", Operator = FilterOperator.Eq, Value = "45" }
            ]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 45 }).Should().BeTrue();
        predicate(new Person { Age = 17 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_AndGroup_ShouldRequireBothBranches()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterGroup
        {
            Operator = FilterGroupOperator.And,
            Children =
            [
                new FilterCondition { Field = "age", Operator = FilterOperator.Gte, Value = "18" },
                new FilterCondition { Field = "age", Operator = FilterOperator.Lte, Value = "40" }
            ]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 45 }).Should().BeFalse();
        predicate(new Person { Age = 17 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_Negation_ShouldInvertChild()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterNegation
        {
            Child = new FilterCondition { Field = "age", Operator = FilterOperator.Eq, Value = "30" }
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeFalse();
        predicate(new Person { Age = 17 }).Should().BeTrue();
    }

    #endregion

    #region NullChildren

    [Fact]
    public void Compile_NullChildrenAndGroup_ShouldMatchEverything()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterGroup { Operator = FilterGroupOperator.And, Children = null! };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person()).Should().BeTrue();
    }

    [Fact]
    public void Compile_NullChildrenOrGroup_ShouldMatchNothing()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterGroup { Operator = FilterGroupOperator.Or, Children = null! };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeFalse();
    }

    #endregion

    #region Comparison

    [Fact]
    public void Compile_Ne_ShouldExcludeEqualValue()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition { Field = "age", Operator = FilterOperator.Ne, Value = "30" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeFalse();
        predicate(new Person { Age = 45 }).Should().BeTrue();
    }

    [Fact]
    public void Compile_Gt_ShouldMatchGreaterValues()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition { Field = "age", Operator = FilterOperator.Gt, Value = "30" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 45 }).Should().BeTrue();
        predicate(new Person { Age = 30 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_Lt_ShouldMatchLesserValues()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition { Field = "age", Operator = FilterOperator.Lt, Value = "30" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 17 }).Should().BeTrue();
        predicate(new Person { Age = 30 }).Should().BeFalse();
    }

    #endregion

    #region String

    [Fact]
    public void Compile_Contains_ShouldMatchSubstring()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var node = new FilterCondition { Field = "name", Operator = FilterOperator.Contains, Value = "ra" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Name = "Grace" }).Should().BeTrue();
        predicate(new Person { Name = "Ada" }).Should().BeFalse();
    }

    [Fact]
    public void Compile_StartsWith_ShouldMatchPrefix()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var node = new FilterCondition { Field = "name", Operator = FilterOperator.StartsWith, Value = "Gr" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Name = "Grace" }).Should().BeTrue();
        predicate(new Person { Name = "Ada" }).Should().BeFalse();
    }

    [Fact]
    public void Compile_EndsWith_ShouldMatchSuffix()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var node = new FilterCondition { Field = "name", Operator = FilterOperator.EndsWith, Value = "ce" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Name = "Grace" }).Should().BeTrue();
        predicate(new Person { Name = "Ada" }).Should().BeFalse();
    }

    #endregion

    #region Set

    [Fact]
    public void Compile_In_ShouldMatchAnyValue()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition
        {
            Field = "age",
            Operator = FilterOperator.In,
            Values = ["30", "45"]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 45 }).Should().BeTrue();
        predicate(new Person { Age = 17 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_NotIn_ShouldExcludeListedValues()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition
        {
            Field = "age",
            Operator = FilterOperator.NotIn,
            Values = ["30", "45"]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 17 }).Should().BeTrue();
        predicate(new Person { Age = 30 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_In_WithNullLeaf_ShouldNotMatch()
    {
        var registry = Registry.With<Person>("score", x => x.Score);
        var node = new FilterCondition
        {
            Field = "score",
            Operator = FilterOperator.In,
            Values = ["1", "2"]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Score = 1 }).Should().BeTrue();
        predicate(new Person { Score = 3 }).Should().BeFalse();
        predicate(new Person { Score = null }).Should().BeFalse();
    }

    [Fact]
    public void Compile_NotIn_WithNullLeaf_ShouldNotMatch()
    {
        var registry = Registry.With<Person>("score", x => x.Score);
        var node = new FilterCondition
        {
            Field = "score",
            Operator = FilterOperator.NotIn,
            Values = ["1", "2"]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Score = 3 }).Should().BeTrue();
        predicate(new Person { Score = 1 }).Should().BeFalse();
        predicate(new Person { Score = null }).Should().BeFalse();
    }

    #endregion

    #region Range

    [Fact]
    public void Compile_Between_ShouldMatchInclusiveRange()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition
        {
            Field = "age",
            Operator = FilterOperator.Between,
            Values = ["18", "40"]
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 18 }).Should().BeTrue();
        predicate(new Person { Age = 30 }).Should().BeTrue();
        predicate(new Person { Age = 40 }).Should().BeTrue();
        predicate(new Person { Age = 17 }).Should().BeFalse();
        predicate(new Person { Age = 45 }).Should().BeFalse();
    }

    #endregion

    #region NullChecks

    [Fact]
    public void Compile_IsNull_ShouldMatchNullLeaf()
    {
        var registry = Registry.With<Person>("score", x => x.Score);
        var node = new FilterCondition { Field = "score", Operator = FilterOperator.IsNull };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Score = null }).Should().BeTrue();
        predicate(new Person { Score = 1 }).Should().BeFalse();
    }

    [Fact]
    public void Compile_IsNotNull_ShouldMatchNonNullLeaf()
    {
        var registry = Registry.With<Person>("score", x => x.Score);
        var node = new FilterCondition { Field = "score", Operator = FilterOperator.IsNotNull };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Score = 1 }).Should().BeTrue();
        predicate(new Person { Score = null }).Should().BeFalse();
    }

    #endregion

    #region CaseInsensitive

    [Fact]
    public void Compile_CaseInsensitiveEq_ShouldIgnoreCase()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var node = new FilterCondition
        {
            Field = "name",
            Operator = FilterOperator.Eq,
            Value = "ada",
            CaseInsensitive = true
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Name = "Ada" }).Should().BeTrue();
        predicate(new Person { Name = "ADA" }).Should().BeTrue();
        predicate(new Person { Name = "Grace" }).Should().BeFalse();
    }

    [Fact]
    public void Compile_CaseInsensitiveContains_ShouldIgnoreCase()
    {
        var registry = Registry.With<Person>("name", x => x.Name);
        var node = new FilterCondition
        {
            Field = "name",
            Operator = FilterOperator.Contains,
            Value = "AD",
            CaseInsensitive = true
        };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Name = "Ada" }).Should().BeTrue();
        predicate(new Person { Name = "Grace" }).Should().BeFalse();
    }

    #endregion

    #region Paths

    [Fact]
    public void Compile_NestedPathWithNullParent_ShouldNotThrow()
    {
        var registry = Registry.With<Person>("address.country", x => x.Address!.Country);
        var node = new FilterCondition { Field = "address.country", Operator = FilterOperator.Eq, Value = "UK" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Address = null }).Should().BeFalse();
    }

    [Fact]
    public void Compile_NestedPath_ShouldMatchLeafValue()
    {
        var registry = Registry.With<Person>("address.country", x => x.Address!.Country);
        var node = new FilterCondition { Field = "address.country", Operator = FilterOperator.Eq, Value = "UK" };

        var predicate = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Address = new Address { Country = "UK" } }).Should().BeTrue();
        predicate(new Person { Address = new Address { Country = "US" } }).Should().BeFalse();
    }

    #endregion

    #region Values

    [Fact]
    public void Compile_UnconvertibleValue_ShouldReturnValueError()
    {
        var registry = Registry.With<Person>("age", x => x.Age);
        var node = new FilterCondition { Field = "age", Operator = FilterOperator.Eq, Value = "abc" };

        var result = QueryFilterCompiler.Compile<Person>(node, registry, QueryContext.Anonymous);

        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.specification.value.invalid");
    }

    #endregion

    #region Empty

    [Fact]
    public void Compile_NullFilter_ShouldMatchEverything()
    {
        var registry = Registry.With<Person>("age", x => x.Age);

        var predicate = QueryFilterCompiler.Compile<Person>(null, registry, QueryContext.Anonymous).Value.Compile();

        predicate(new Person { Age = 99 }).Should().BeTrue();
        predicate(new Person()).Should().BeTrue();
    }

    #endregion
}
