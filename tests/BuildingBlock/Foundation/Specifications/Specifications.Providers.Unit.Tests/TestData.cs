using System.Linq.Expressions;
using BuildingBlock.Domain.Modeling.Foundation.Identifiable;

namespace BuildingBlock.Specifications.Providers.Unit.Tests;

#region Entities

internal sealed class Person : IIdentifiable<Guid>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public int Age { get; init; }

    public bool IsDeleted { get; init; }
}

internal sealed class PersonSpecification : Specification<Person>
{
    private readonly Expression<Func<Person, bool>> _criteria;

    public PersonSpecification(Expression<Func<Person, bool>> criteria)
    {
        _criteria = criteria;
    }

    public override Expression<Func<Person, bool>> Criteria => _criteria;
}

#endregion

#region Factories

internal static class People
{
    public static Person Adult(int age = 30, bool deleted = false)
        => new() { Id = Guid.NewGuid(), Name = "Adult", Age = age, IsDeleted = deleted };

    public static Person Child(int age = 10)
        => new() { Id = Guid.NewGuid(), Name = "Child", Age = age };

    public static IQueryable<Person> Query(params Person[] people)
        => people.AsQueryable();
}

#endregion
