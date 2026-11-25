namespace BuildingBlock.Querying.Dapper.Unit.Tests;

/// <summary>Tests for the Dapper connection and engine extensions over SQLite.</summary>
public sealed class DapperConnectionTests
{
    #region Fixture

    private static SqliteConnection Open()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE people (id INTEGER PRIMARY KEY, name TEXT NOT NULL, age INTEGER NOT NULL);
            INSERT INTO people (id, name, age) VALUES
                (1, 'Ada', 30),
                (2, 'Grace', 45),
                (3, 'Alan', 17),
                (4, 'Noah', 22);
            """;
        command.ExecuteNonQuery();

        return connection;
    }

    #endregion

    #region QueryDynamic

    [Fact]
    public void QueryDynamic_AppliesFilterSortAndPagingOverDapperRows()
    {
        using SqliteConnection connection = Open();
        var engine = Policies.Engine();
        var request = Requests.With(
            filter: Requests.Condition("age", FilterOperator.Gte, "18"),
            sorts: [new SortDescriptor { Field = "age", Direction = SortDirection.Descending }],
            paging: new PagedRequest { Page = 1, PageSize = 2 });

        var result = connection.QueryDynamic<Person>("SELECT * FROM people", request, engine);

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value.Items).Should().Equal("Grace", "Ada");
        result.Value.TotalCount.Should().Be(3);
    }

    [Fact]
    public void QueryDynamic_UnregisteredField_ShouldReturnSecurityError()
    {
        using SqliteConnection connection = Open();
        var engine = Policies.Engine();
        var request = Requests.With(filter: Requests.Condition("secret", FilterOperator.Eq, "x"));

        var result = connection.QueryDynamic<Person>("SELECT * FROM people", request, engine);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.security.field.not_allowed");
    }

    [Fact]
    public async Task QueryDynamicAsync_ShouldMatchSync()
    {
        using SqliteConnection connection = Open();
        var engine = Policies.Engine();
        var request = Requests.With(
            sorts: [new SortDescriptor { Field = "name", Direction = SortDirection.Ascending }]);

        var result = await connection.QueryDynamicAsync<Person>("SELECT * FROM people", request, engine);

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value.Items).Should().Equal("Ada", "Alan", "Grace", "Noah");
    }

    #endregion

    #region ApplyDynamicQuery

    [Fact]
    public void ApplyDynamicQuery_OverPlainSequence_ShouldPage()
    {
        var engine = Policies.Engine();
        var request = Requests.With(paging: new PagedRequest { Page = 2, PageSize = 2 });

        var result = People.All.ApplyDynamicQuery(request, engine);

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value.Items).Should().Equal("Alan", "Noah");
    }

    [Fact]
    public async Task ApplyDynamicQueryAsync_OverPlainSequence_ShouldPage()
    {
        var engine = Policies.Engine();
        var request = Requests.With(paging: new PagedRequest { Page = 1, PageSize = 2 });

        var result = await People.All.ApplyDynamicQueryAsync(request, engine);

        result.IsSuccess.Should().BeTrue();
        People.Names(result.Value.Items).Should().Equal("Ada", "Grace");
    }

    #endregion

    #region Failures

    [Fact]
    public void QueryDynamic_NullConnection_ShouldReturnError()
    {
        IDbConnection connection = null!;

        var result = connection.QueryDynamic<Person>("SELECT 1", Requests.With(), Policies.Engine());

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Code.Should().Be("query.dapper.source.required");
    }

    #endregion
}
