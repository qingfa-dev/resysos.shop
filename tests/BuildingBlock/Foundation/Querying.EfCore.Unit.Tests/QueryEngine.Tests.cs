using System.Reflection;

using BuildingBlock.Contracts.Filtering;
using BuildingBlock.Contracts.Querying;
using BuildingBlock.Contracts.Sorting;

namespace BuildingBlock.Querying.EfCore.Unit.Tests;

/// <summary>End-to-end tests for the <see cref="QueryEngine"/> facade over the EF Core executor.</summary>
public sealed class QueryEngineTests
{
    #region Page

    [Fact]
    public async Task PageAsync_EndToEnd_AppliesFilterSortAndCount()
    {
        await using TestDb db = TestDb.Create(products: 25);
        QueryEngine engine = QueryEngine.Create(PolicyFor<Product>(), EfQueryPlanExecutor.Create().Value).Value;
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "price", Operator = FilterOperator.Gte, Value = "100" },
            Sorts = [new SortDescriptor { Field = "price", Direction = SortDirection.Descending }],
            Paging = new PagedRequest { Page = 1, PageSize = 10 }
        };

        var result = await engine.PageAsync(db.Products, request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
    }

    #endregion

    #region ApplyDynamicQuery

    [Fact]
    public async Task ApplyDynamicQueryAsync_InvalidField_ShouldReturnError()
    {
        await using TestDb db = TestDb.Create();
        QueryEngine engine = QueryEngine.Create(PolicyFor<Product>(), EfQueryPlanExecutor.Create().Value).Value;
        var request = new QueryRequest
        {
            Filter = new FilterCondition { Field = "nope", Operator = FilterOperator.Eq, Value = "x" }
        };

        var result = await db.Products.ApplyDynamicQueryAsync(request, engine);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("query.security.field.not_allowed");
    }

    #endregion

    #region Policy

    private static QueryPolicy PolicyFor<T>()
    {
        var registry = new QueryFieldRegistry();

        foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is null)
            {
                continue;
            }

            if (property.PropertyType != typeof(string) && property.PropertyType.IsClass)
            {
                continue;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var accessor = Expression.Lambda(Expression.Property(parameter, property), parameter);
            registry.Add(typeof(T), property.Name, accessor, FilterOperators.All, sortable: true);
        }

        return new QueryPolicy { Registry = registry };
    }

    #endregion
}
