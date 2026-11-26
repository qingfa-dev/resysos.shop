namespace BuildingBlock.Querying.InMemory.Unit.Tests;

/// <summary>Tests for <see cref="InMemoryQueryPlanExecutor"/>.</summary>
public sealed class InMemoryQueryPlanExecutorTests
{
    #region Build

    [Fact]
    public void Build_AppliesFilterSortAndPaging()
    {
        var plan = new QueryPlan<Person>
        {
            Filter = (x => x.Age >= 18),
            Sorts = [new QuerySort<Person>(x => x.Age, Descending: true)],
            Paging = new PagingPlan.Offset(0, 2)
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan).Value;

        result.Select(p => p.Name).Should().Equal("Grace", "Ada");
    }

    [Fact]
    public void Build_NoFilterSortOrPaging_ReturnsSourceInOrder()
    {
        var plan = new QueryPlan<Person>();

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan).Value;

        result.Select(p => p.Name).Should().Equal("Ada", "Grace", "Alan");
    }

    [Fact]
    public void Build_MultipleSorts_AppliesThenBy()
    {
        var people = new[]
        {
            new Person { Id = 1, Age = 30, Name = "Ada" },
            new Person { Id = 2, Age = 30, Name = "Grace" },
            new Person { Id = 3, Age = 20, Name = "Alan" }
        };
        var plan = new QueryPlan<Person>
        {
            Sorts =
            [
                new QuerySort<Person>(x => x.Age, Descending: false),
                new QuerySort<Person>(x => x.Name, Descending: true)
            ]
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(people, plan).Value;

        result.Select(p => p.Name).Should().Equal("Alan", "Grace", "Ada");
    }

    [Fact]
    public void Build_AscendingSort_OrdersAscending()
    {
        var plan = new QueryPlan<Person>
        {
            Sorts = [new QuerySort<Person>(x => x.Age, Descending: false)]
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan).Value;

        result.Select(p => p.Name).Should().Equal("Alan", "Ada", "Grace");
    }

    [Fact]
    public void Build_OffsetPaging_SkipsAndTakes()
    {
        var plan = new QueryPlan<Person>
        {
            Sorts = [new QuerySort<Person>(x => x.Age, Descending: false)],
            Paging = new PagingPlan.Offset(1, 1)
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan).Value;

        result.Select(p => p.Name).Should().Equal("Ada");
    }

    [Fact]
    public void Build_NullSource_ShouldFail()
    {
        var result = InMemoryQueryPlanExecutor.Create().Value
            .Build<Person>(null!, new QueryPlan<Person>());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.in_memory.source.required");
    }

    [Fact]
    public void Build_NullPlan_ShouldFail()
    {
        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, null!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.in_memory.plan.required");
    }

    [Fact]
    public void Build_NullSorts_ShouldNotThrow()
    {
        var plan = new QueryPlan<Person> { Sorts = null! };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(p => p.Name).Should().Equal("Ada", "Grace", "Alan");
    }

    [Fact]
    public void Build_NullKeySelector_ShouldSkipSort()
    {
        var plan = new QueryPlan<Person>
        {
            Sorts = [new QuerySort<Person>(null!, Descending: false)]
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(People.All, plan);

        result.IsSuccess.Should().BeTrue();
        result.Value.Select(p => p.Name).Should().Equal("Ada", "Grace", "Alan");
    }

    #endregion

    #region EndToEnd

    [Fact]
    public void Build_AliasedSortField_OrdersByRegisteredMember()
    {
        var registry = new QueryFieldRegistry();
        registry.Add<Person>("age_rank", x => x.Age, FilterOperators.All, sortable: true);
        var policy = new QueryPolicy { Registry = registry };
        var request = new QueryRequest
        {
            Sorts = [new SortDescriptor { Field = "age_rank", Direction = SortDirection.Descending }]
        };

        var plan = QueryCompiler.Compile<Person>(request, policy, QueryContext.Anonymous).Value;

        var source = new[]
        {
            new Person { Id = 1, Age = 30, Name = "Ada" },
            new Person { Id = 2, Age = 20, Name = "Grace" },
            new Person { Id = 3, Age = 40, Name = "Alan" }
        };

        var result = InMemoryQueryPlanExecutor.Create().Value.Build(source, plan).Value;

        result.Select(p => p.Name).Should().Equal("Alan", "Ada", "Grace");
    }

    #endregion

    #region Factory

    [Fact]
    public void Create_ShouldSucceed()
    {
        var result = InMemoryQueryPlanExecutor.Create();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<InMemoryQueryPlanExecutor>();
    }

    #endregion
}
