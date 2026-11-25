using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlock.Querying.Unit.Tests;

/// <summary>Tests for <see cref="QueryEngine"/> and its extensions.</summary>
public sealed class QueryEngineTests
{
    #region Create

    [Fact]
    public void Create_NullPolicy_ShouldFail()
    {
        var result = QueryEngine.Create(null!, new StubQueryPlanExecutor());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.engine.policy.required");
    }

    [Fact]
    public void Create_NullExecutor_ShouldFail()
    {
        var result = QueryEngine.Create(new QueryPolicy(), null!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.engine.executor.required");
    }

    [Fact]
    public void Create_ShouldSucceed()
    {
        var result = QueryEngine.Create(new QueryPolicy(), new StubQueryPlanExecutor());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeAssignableTo<IQueryEngine>();
    }

    #endregion

    #region Page

    [Fact]
    public void Page_InvalidField_ShouldFailBeforeExecuting()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.WithField<Person>("age", FilterOperators.All), executor).Value;
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "nope", Operator = FilterOperator.Eq, Value = "1" }
        };

        var result = engine.Page(People.All.AsQueryable(), request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
        executor.ExecuteCalls.Should().Be(0);
    }

    [Fact]
    public void Page_ValidRequest_ShouldDelegateToExecutor()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.For<Person>(), executor).Value;

        var result = engine.Page(People.All.AsQueryable(), new QueryRequest());

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteCalls.Should().Be(1);
    }

    #endregion

    #region PageAsync

    [Fact]
    public async Task PageAsync_ValidRequest_ShouldDelegateToExecutor()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.For<Person>(), executor).Value;

        var result = await engine.PageAsync(People.All.AsQueryable(), new QueryRequest());

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteAsyncCalls.Should().Be(1);
    }

    [Fact]
    public async Task PageAsync_InvalidField_ShouldFailBeforeExecuting()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.WithField<Person>("age", FilterOperators.All), executor).Value;
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "nope", Operator = FilterOperator.Eq, Value = "1" }
        };

        var result = await engine.PageAsync(People.All.AsQueryable(), request);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
        executor.ExecuteAsyncCalls.Should().Be(0);
    }

    #endregion

    #region ApplyDynamicQuery

    [Fact]
    public void ApplyDynamicQuery_WithEngine_ShouldDelegate()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.For<Person>(), executor).Value;

        var result = People.All.AsQueryable().ApplyDynamicQuery(new QueryRequest(), engine);

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteCalls.Should().Be(1);
    }

    [Fact]
    public async Task ApplyDynamicQueryAsync_WithEngine_ShouldDelegate()
    {
        var executor = new StubQueryPlanExecutor();
        QueryEngine engine = QueryEngine.Create(Policy.For<Person>(), executor).Value;

        var result = await People.All.AsQueryable().ApplyDynamicQueryAsync(new QueryRequest(), engine);

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteAsyncCalls.Should().Be(1);
    }

    [Fact]
    public void ApplyDynamicQuery_UnregisteredServices_ShouldFail()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var result = People.All.AsQueryable().ApplyDynamicQuery(new QueryRequest(), services);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.engine.not_registered");
    }

    [Fact]
    public void ApplyDynamicQuery_RegisteredServices_ShouldDelegate()
    {
        var executor = new StubQueryPlanExecutor();
        var services = new ServiceCollection();
        services.AddSingleton<IQueryPlanExecutor>(executor);
        services.AddDynamicQuery(policy =>
            policy.Registry.Add<Person>("age", x => x.Age, FilterOperators.All, sortable: true));

        using var provider = services.BuildServiceProvider();

        var result = People.All.AsQueryable().ApplyDynamicQuery(new QueryRequest(), provider);

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteCalls.Should().Be(1);
    }

    [Fact]
    public async Task ApplyDynamicQueryAsync_UnregisteredServices_ShouldFail()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        var result = await People.All.AsQueryable().ApplyDynamicQueryAsync(new QueryRequest(), services);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.engine.not_registered");
    }

    [Fact]
    public async Task ApplyDynamicQueryAsync_RegisteredServices_ShouldDelegate()
    {
        var executor = new StubQueryPlanExecutor();
        var services = new ServiceCollection();
        services.AddSingleton<IQueryPlanExecutor>(executor);
        services.AddDynamicQuery(policy =>
            policy.Registry.Add<Person>("age", x => x.Age, FilterOperators.All, sortable: true));

        using var provider = services.BuildServiceProvider();

        var result = await People.All.AsQueryable().ApplyDynamicQueryAsync(new QueryRequest(), provider);

        result.IsSuccess.Should().BeTrue();
        executor.ExecuteAsyncCalls.Should().Be(1);
    }

    #endregion

    #region DependencyInjection

    [Fact]
    public void AddDynamicQuery_NullServices_ShouldFail()
    {
        IServiceCollection services = null!;

        var result = services.AddDynamicQuery();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.engine.services.required");
    }

    [Fact]
    public void AddDynamicQuery_ShouldRegisterPolicyAndEngine()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IQueryPlanExecutor>(new StubQueryPlanExecutor());

        var result = services.AddDynamicQuery(policy =>
            policy.Registry.Add<Person>("age", x => x.Age, FilterOperators.All, sortable: true));

        result.IsSuccess.Should().BeTrue();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<QueryPolicy>().Registry.Resolve(typeof(Person), "age").IsSuccess.Should().BeTrue();
        provider.GetRequiredService<QueryEngine>().Should().NotBeNull();
        provider.GetRequiredService<IQueryEngine>().Should().BeSameAs(provider.GetRequiredService<QueryEngine>());
    }

    [Fact]
    public void AddDynamicQuery_ConfiguredMaxPageSize_ShouldClamp()
    {
        var services = new ServiceCollection();
        var executor = new StubQueryPlanExecutor();
        services.AddSingleton<IQueryPlanExecutor>(executor);

        services.AddDynamicQuery(policy => policy.MaxPageSize = 100);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<QueryPolicy>().MaxPageSize.Should().Be(100);
        QueryEngine engine = provider.GetRequiredService<QueryEngine>();
        var request = new QueryRequest { Paging = new PagedRequest { Page = 1, PageSize = 500 } };

        var result = engine.Page(People.All.AsQueryable(), request);

        result.IsSuccess.Should().BeTrue();
        executor.LastPlan.Should().BeOfType<QueryPlan<Person>>()
            .Which.Paging.Should().BeOfType<PagingPlan.Offset>()
            .Which.Take.Should().Be(100);
    }

    #endregion

    #region Stub

    private sealed class StubQueryPlanExecutor : IQueryPlanExecutor
    {
        public int ExecuteCalls { get; private set; }

        public int ExecuteAsyncCalls { get; private set; }

        public object? LastPlan { get; private set; }

        public Result<IQueryable<T>> Build<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
            => Result.Success(source);

        public Result<PagedResult<T>> Execute<T>(IQueryable<T> source, QueryPlan<T> plan) where T : class
        {
            ExecuteCalls++;
            LastPlan = plan;
            return Result.Success(new PagedResult<T>());
        }

        public ValueTask<Result<PagedResult<T>>> ExecuteAsync<T>(
            IQueryable<T> source,
            QueryPlan<T> plan,
            CancellationToken cancellationToken = default) where T : class
        {
            ExecuteAsyncCalls++;
            LastPlan = plan;
            return ValueTask.FromResult(Result.Success(new PagedResult<T>()));
        }
    }

    #endregion
}
