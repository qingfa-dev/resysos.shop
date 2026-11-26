namespace BuildingBlock.Querying.InMemory.Unit.Tests;

#region Entities

internal sealed class Address
{
    public string? Country { get; init; }

    public string? City { get; init; }
}

internal sealed class Person
{
    public int Id { get; init; }

    public int Age { get; init; }

    public string Name { get; init; } = string.Empty;

    public Address? Address { get; init; }
}

#endregion

#region Data

internal static class People
{
    public static readonly Person Ada = new()
    {
        Id = 1,
        Age = 30,
        Name = "Ada",
        Address = new Address { City = "London", Country = "UK" }
    };

    public static readonly Person Grace = new()
    {
        Id = 2,
        Age = 45,
        Name = "Grace",
        Address = new Address { City = "New York", Country = "US" }
    };

    public static readonly Person Alan = new()
    {
        Id = 3,
        Age = 17,
        Name = "Alan",
        Address = new Address { City = "London", Country = null }
    };

    public static IReadOnlyList<Person> All { get; } = [Ada, Grace, Alan];
}

#endregion
