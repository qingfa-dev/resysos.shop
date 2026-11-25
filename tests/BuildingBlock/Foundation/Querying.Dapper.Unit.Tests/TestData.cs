namespace BuildingBlock.Querying.Dapper.Unit.Tests;

#region Entities

internal sealed class Person
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Age { get; init; }
}

#endregion

#region Data

internal static class People
{
    public static IReadOnlyList<Person> All { get; } =
    [
        new Person { Id = 1, Name = "Ada", Age = 30 },
        new Person { Id = 2, Name = "Grace", Age = 45 },
        new Person { Id = 3, Name = "Alan", Age = 17 },
        new Person { Id = 4, Name = "Noah", Age = 22 }
    ];

    public static string[] Names(IEnumerable<Person> people)
        => people.Select(person => person.Name).ToArray();
}

#endregion

#region Policies

internal static class Policies
{
    public static QueryPolicy For(Action<QueryFieldRegistry>? configure = null)
    {
        var policy = new QueryPolicy();
        configure?.Invoke(policy.Registry);
        return policy;
    }

    public static QueryPolicy PeoplePolicy()
    {
        var policy = new QueryPolicy();
        policy.Registry.Add<Person>("age", x => x.Age, FilterOperators.All, sortable: true);
        policy.Registry.Add<Person>("name", x => x.Name, FilterOperators.All, sortable: true);
        policy.Registry.Add<Person>("id", x => x.Id, FilterOperators.Eq, sortable: true);
        return policy;
    }

    public static QueryEngine Engine(QueryPolicy? policy = null)
        => QueryEngine.Create(
            policy ?? PeoplePolicy(),
            DapperQueryPlanExecutor.Create().Value).Value;
}

#endregion

#region Requests

internal static class Requests
{
    public static QueryRequest With(
        FilterNode? filter = null,
        IReadOnlyList<SortDescriptor>? sorts = null,
        PagedRequest? paging = null)
        => new()
        {
            Filter = filter,
            Sorts = sorts ?? [],
            Paging = paging ?? new PagedRequest()
        };

    public static FilterCondition Condition(string field, FilterOperator @operator, string? value = null)
        => new() { Field = field, Operator = @operator, Value = value };
}

#endregion
