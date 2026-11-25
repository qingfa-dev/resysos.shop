namespace BuildingBlock.Querying.EfCore.Unit.Tests;

/// <summary>Tests for <see cref="EfQueryPlanExecutor"/>.</summary>
public sealed class EfQueryPlanExecutorTests
{
    #region Build

    [Fact]
    public async Task Build_TranslatedToSql_ForFilterAndPaging()
    {
        await using TestDb db = TestDb.Create();
        var plan = new QueryPlan<Product>
        {
            Filter = x => x.Price >= 100,
            Sorts = [new QuerySort<Product>(x => x.Price, Descending: true)],
            Paging = new PagingPlan.Offset(0, 10)
        };

        string sql = EfQueryPlanExecutor.Create().Value
            .Build(db.Products.AsNoTracking(), plan).Value
            .ToQueryString();

        sql.Should().Contain("WHERE").And.Contain("ORDER BY").And.Contain("LIMIT");
    }

    [Fact]
    public async Task Build_AppliesFilterSortAndPaging()
    {
        await using TestDb db = TestDb.Create(products: 5);
        var plan = new QueryPlan<Product>
        {
            Filter = x => x.Price >= 102,
            Sorts = [new QuerySort<Product>(x => x.Price, Descending: true)],
            Paging = new PagingPlan.Offset(0, 2)
        };

        List<Product> items = await EfQueryPlanExecutor.Create().Value
            .Build(db.Products, plan).Value
            .ToListAsync();

        items.Select(product => product.Name).Should().Equal("Product 5", "Product 4");
    }

    [Fact]
    public async Task Build_MultipleSorts_AppliesThenBy()
    {
        await using TestDb db = TestDb.Create();
        db.Products.AddRange(
            new Product { Id = 1, Name = "Beta", Price = 10m },
            new Product { Id = 2, Name = "Alpha", Price = 10m },
            new Product { Id = 3, Name = "Gamma", Price = 5m });
        await db.SaveChangesAsync();
        var plan = new QueryPlan<Product>
        {
            Sorts =
            [
                new QuerySort<Product>(x => x.Price, Descending: false),
                new QuerySort<Product>(x => x.Name, Descending: false)
            ]
        };

        List<Product> items = await EfQueryPlanExecutor.Create().Value
            .Build(db.Products, plan).Value
            .ToListAsync();

        items.Select(product => product.Name).Should().Equal("Gamma", "Alpha", "Beta");
    }

    [Fact]
    public void Build_NullSource_ShouldFail()
    {
        var result = EfQueryPlanExecutor.Create().Value
            .Build<Product>(null!, new QueryPlan<Product>());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.ef_core.source.required");
    }

    [Fact]
    public void Build_NullPlan_ShouldFail()
    {
        var result = EfQueryPlanExecutor.Create().Value
            .Build<Product>(Array.Empty<Product>().AsQueryable(), null!);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.ef_core.plan.required");
    }

    [Fact]
    public async Task Build_NullSorts_ShouldNotThrow()
    {
        await using TestDb db = TestDb.Create();
        var plan = new QueryPlan<Product> { Sorts = null! };

        string sql = EfQueryPlanExecutor.Create().Value
            .Build(db.Products, plan).Value
            .ToQueryString();

        sql.Should().NotContain("ORDER BY");
    }

    [Fact]
    public async Task Build_NullKeySelector_ShouldSkipSort()
    {
        await using TestDb db = TestDb.Create();
        var plan = new QueryPlan<Product>
        {
            Sorts = [new QuerySort<Product>(null!, Descending: false)]
        };

        string sql = EfQueryPlanExecutor.Create().Value
            .Build(db.Products, plan).Value
            .ToQueryString();

        sql.Should().NotContain("ORDER BY");
    }

    #endregion

    #region Execute

    [Fact]
    public async Task ExecuteAsync_CountExcludesSortingAndPaging()
    {
        await using TestDb db = TestDb.Create(products: 25);
        var plan = new QueryPlan<Product>
        {
            Paging = new PagingPlan.Offset(0, 10),
            IncludeTotalCount = true
        };

        PagedResult<Product> page = (await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, plan, CancellationToken.None)).Value;

        page.Items.Should().HaveCount(10);
        page.TotalCount.Should().Be(25);
    }

    [Fact]
    public async Task ExecuteAsync_FilterApplied_CountReflectsFilter()
    {
        await using TestDb db = TestDb.Create(products: 25);
        var plan = new QueryPlan<Product>
        {
            Filter = x => x.Price >= 110,
            Paging = new PagingPlan.Offset(0, 5),
            IncludeTotalCount = true
        };

        PagedResult<Product> page = (await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, plan, CancellationToken.None)).Value;

        page.Items.Should().HaveCount(5);
        page.Items.Should().OnlyContain(product => product.Price >= 110m);
        page.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutTotalCount_ReturnsZero()
    {
        await using TestDb db = TestDb.Create(products: 25);
        var plan = new QueryPlan<Product>
        {
            Paging = new PagingPlan.Offset(0, 10)
        };

        PagedResult<Product> page = (await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, plan, CancellationToken.None)).Value;

        page.Items.Should().HaveCount(10);
        page.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_OffsetPaging_MapsPageAndPageSize()
    {
        await using TestDb db = TestDb.Create(products: 25);
        var plan = new QueryPlan<Product>
        {
            Sorts = [new QuerySort<Product>(x => x.Id, Descending: false)],
            Paging = new PagingPlan.Offset(10, 10),
            IncludeTotalCount = true
        };

        PagedResult<Product> page = (await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, plan, CancellationToken.None)).Value;

        page.Page.Should().Be(2);
        page.PageSize.Should().Be(10);
        page.Items.Select(product => product.Id).Should().Equal(11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
    }

    [Fact]
    public async Task ExecuteAsync_CancelledToken_ShouldCancel()
    {
        await using TestDb db = TestDb.Create(products: 25);
        var plan = new QueryPlan<Product> { IncludeTotalCount = true };
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        Func<Task> act = () => EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, plan, source.Token).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ExecuteAsync_NullSource_ShouldFail()
    {
        var result = await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync<Product>(null!, new QueryPlan<Product>(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.ef_core.source.required");
    }

    [Fact]
    public async Task ExecuteAsync_NullPlan_ShouldFail()
    {
        await using TestDb db = TestDb.Create();

        var result = await EfQueryPlanExecutor.Create().Value
            .ExecuteAsync(db.Products, null!, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.ef_core.plan.required");
    }

    #endregion

    #region Factory

    [Fact]
    public void Create_ShouldSucceed()
    {
        var result = EfQueryPlanExecutor.Create();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<EfQueryPlanExecutor>();
    }

    #endregion
}
