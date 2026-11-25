namespace BuildingBlock.Specifications.Query.Unit.Tests;

#region Entities

internal enum AccountStatus
{
    Inactive,
    Active,
    Suspended
}

internal sealed class Address
{
    public string City { get; init; } = string.Empty;

    public string? Country { get; init; }
}

internal sealed class Person
{
    public Guid Id { get; init; }

    public int Age { get; init; }

    public int? Score { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Salary { get; init; }

    public bool IsVerified { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public AccountStatus Status { get; init; }

    public Address? Address { get; init; }
}

#endregion

#region Data

internal static class People
{
    public static readonly DateTimeOffset Start = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static readonly Person Ada = new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Age = 30,
        Score = 90,
        Name = "Ada",
        Salary = 1200m,
        IsVerified = true,
        CreatedAt = Start,
        Status = AccountStatus.Active,
        Address = new Address { City = "London", Country = "UK" }
    };

    public static readonly Person Grace = new()
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Age = 45,
        Score = null,
        Name = "Grace",
        Salary = 2400m,
        IsVerified = true,
        CreatedAt = Start.AddMonths(5),
        Status = AccountStatus.Suspended,
        Address = new Address { City = "New York", Country = "US" }
    };

    public static readonly Person Alan = new()
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Age = 17,
        Score = 50,
        Name = "Alan",
        Salary = 800m,
        IsVerified = false,
        CreatedAt = Start.AddYears(-1),
        Status = AccountStatus.Inactive,
        Address = new Address { City = "London", Country = null }
    };

    public static readonly Person Noah = new()
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Age = 22,
        Score = 10,
        Name = "Noah",
        Salary = 1000m,
        IsVerified = true,
        CreatedAt = Start.AddYears(1),
        Status = AccountStatus.Active,
        Address = null
    };

    public static IReadOnlyList<Person> All { get; } = [Ada, Grace, Alan, Noah];

    public static string[] Names(IEnumerable<Person> people)
        => people.Select(person => person.Name).ToArray();
}

#endregion

#region Mapping Helpers

internal static class Mapping
{
    public static FilterDescriptor Filter(
        string field,
        FilterOperator @operator,
        string? value = null,
        params string[] values)
        => new()
        {
            Field = field,
            Operator = @operator,
            Value = value,
            Values = values
        };

    public static QueryRequest Request(
        IReadOnlyList<FilterDescriptor>? filters = null,
        IReadOnlyList<SortDescriptor>? sorts = null,
        PagedRequest? paging = null)
        => new()
        {
            Filters = filters ?? [],
            Sorts = sorts ?? [],
            Paging = paging ?? new PagedRequest()
        };

    public static Result<ISpecification<Person>> Specification(QueryRequest request)
        => request.ToSpecification<Person>();

    public static Result<IEnumerable<Person>> Evaluate(QueryRequest request)
        => Evaluate(request, People.All);

    public static Result<IEnumerable<Person>> Evaluate(
        QueryRequest request,
        IEnumerable<Person> source)
        => request.ToSpecification<Person>()
            .Bind(specification => InMemorySpecificationEvaluator.Create()
                .Bind(evaluator => evaluator.Apply(source, specification)));

    public static Result<IEnumerable<Person>> Evaluate(FilterDescriptor filter)
        => Evaluate(Request(filters: [filter]));
}

#endregion
