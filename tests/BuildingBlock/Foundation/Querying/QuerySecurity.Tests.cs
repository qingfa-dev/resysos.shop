namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QuerySecurity"/>.</summary>
public sealed class QuerySecurityTests
{
    #region Operator

    [Fact]
    public void Validate_DisallowedOperator_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq);
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.Gte, Value = "18" };

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.operator.not_allowed");
    }

    [Fact]
    public void Validate_AllowedOperator_ShouldPass()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Gte);
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.Gte, Value = "18" };

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Field

    [Fact]
    public void Validate_UnknownFilterField_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq);
        var filter = new FilterCondition { Field = "passwordHash", Operator = FilterOperator.Eq, Value = "x" };

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Depth

    [Fact]
    public void Validate_FilterDeeperThanCap_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq, maxFilterDepth: 2);
        var filter = Nest(Condition(), levels: 2);

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.filter.too_deep");
    }

    [Fact]
    public void Validate_FilterAtCap_ShouldPass()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq, maxFilterDepth: 3);
        var filter = Nest(Condition(), levels: 2);

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Conditions

    [Fact]
    public void Validate_TooManyConditions_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq, maxFilterConditions: 2);
        var filter = new FilterGroup { Children = [Condition(), Condition(), Condition()] };

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.filter.too_many_conditions");
    }

    #endregion

    #region NullChildren

    [Fact]
    public void Validate_NullChildren_ShouldNotThrow()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq);
        var filter = new FilterGroup { Children = null! };

        var result = QuerySecurity.Validate(filter, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Sort

    [Fact]
    public void Validate_SortOnNonSortableField_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq, sortable: false);
        var sorts = new[] { new SortDescriptor { Field = "age" } };

        var result = QuerySecurity.Validate(null, sorts, policy, typeof(Person), QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.sort.not_allowed");
    }

    [Fact]
    public void Validate_SortOnUnknownField_ShouldFail()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq, sortable: true);
        var sorts = new[] { new SortDescriptor { Field = "passwordHash" } };

        var result = QuerySecurity.Validate(null, sorts, policy, typeof(Person), QueryContext.Anonymous);

        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Success

    [Fact]
    public void Validate_ValidFilterAndSort_ShouldSucceed()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq | FilterOperators.Gte, sortable: true);
        var filter = new FilterCondition { Field = "age", Operator = FilterOperator.Gte, Value = "18" };
        var sorts = new[] { new SortDescriptor { Field = "age", Direction = SortDirection.Descending } };

        var result = QuerySecurity.Validate(filter, sorts, policy, typeof(Person), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_NoFilterAndNoSorts_ShouldSucceed()
    {
        var policy = Policy.WithField<Person>("age", FilterOperators.Eq);

        var result = QuerySecurity.Validate(null, [], policy, typeof(Person), QueryContext.Anonymous);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Helpers

    private static FilterCondition Condition()
        => new() { Field = "age", Operator = FilterOperator.Eq, Value = "30" };

    private static FilterNode Nest(FilterNode node, int levels)
    {
        for (int level = 0; level < levels; level++)
        {
            node = new FilterGroup { Children = [node] };
        }

        return node;
    }

    #endregion
}
